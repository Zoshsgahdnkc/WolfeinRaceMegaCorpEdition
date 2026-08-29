using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WRExtraGenes
{
    [StaticConstructorOnStartup]
    public class StartUp
    {
        static StartUp()
        {
            Log.Message("Now Loading Wolfein Race Extra Genes Patches.");
            var harmony = new Harmony("com.zoshsgahdnkc.MCMigrationPatch");
            harmony.PatchAll();
        }
    }
    
    // 对原版的技能学习打补丁，应用艺术技能学习系数
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.LearnRateFactor))]
    public static class Patch_SkillRecord_LearnRateFactor
    {
        static Patch_SkillRecord_LearnRateFactor() => DefOfHelper.EnsureInitializedInCtor(typeof (WREG_DefOfs));
        public static void Postfix(SkillRecord __instance,bool direct, ref float __result)
        {
            if (direct) return;
            if (__instance.def != SkillDefOf.Artistic) return;

            __result *= __instance.Pawn.GetStatValue(WREG_DefOfs.WREG_ArtisticLearningFactor);
        }
    }

    // 对原版工艺过程打补丁，应用艺术品品质偏移
    [HarmonyPatch]
    public static class Patch_GenRecipe_PostProcessProduct
    {
        static MethodBase TargetMethod() => AccessTools.Method(typeof(GenRecipe), "PostProcessProduct");

        public static void Postfix(ref Thing __result, Thing product, RecipeDef recipeDef, Pawn worker)
        {
            int offset = (int)worker.GetStatValue(WREG_DefOfs.WREG_ArtisticQualityOffset);
            if (offset == 0) return;
            // 获取即将完成的艺术品
            var thing = __result.GetInnerIfMinified();
            if (__result == null || worker == null || thing.TryGetComp<CompArt>() == null) return;
            var compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality == null) return;
            // 应用品质偏移
            int oldQuality = (int)compQuality.Quality;
            int newQuality = Mathf.Clamp(oldQuality + offset, (int) QualityCategory.Awful, (int) QualityCategory.Legendary);
            if (newQuality == oldQuality) return;
            compQuality.SetQuality((QualityCategory)newQuality, ArtGenerationContext.Colony);
            // 补发信件
            if (oldQuality < (int) QualityCategory.Masterwork && newQuality >= (int) QualityCategory.Masterwork)
            {
                QualityUtility.SendCraftNotification(thing, worker);
            }
        }
    }
    
    // 拥有特定基因时，将原版触发灵感时随机选择的灵感替换成创造灵感
    [HarmonyPatch(typeof(InspirationHandler), nameof(InspirationHandler.GetRandomAvailableInspirationDef))]
    public static class Patch_InspirationHandler_GetRandomAvailableInspirationDef
    {
        static Patch_InspirationHandler_GetRandomAvailableInspirationDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof (WREG_DefOfs));
            DefOfHelper.EnsureInitializedInCtor(typeof (InspirationDefOf));
        } 
        public static void Postfix(InspirationHandler __instance, ref InspirationDef __result)
        {
            if (__instance.pawn.genes.HasActiveGene(WREG_DefOfs.WREG_GeniusArtist))
            {
                __result = InspirationDefOf.Inspired_Creativity;
            }
        }
    }
    
    // 对灵感MTB属性打补丁，值为正时触发时间和灵感触发间隔乘数stat有关
    [HarmonyPatch]
    public static class Patch_InspirationHandler_StartInspirationMTBDays 
    {
        static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(InspirationHandler), "StartInspirationMTBDays");

        public static void Postfix(ref InspirationHandler __instance, ref float __result)
        {
            if (__result <= 0) return;
            var p = __instance.pawn;
            if (p == null || p.GetStatValue(WREG_DefOfs.WREG_InspirationMTBFactor) == null) return;
            __result *= __instance.pawn.GetStatValue(WREG_DefOfs.WREG_InspirationMTBFactor);
        }
    }
    
}