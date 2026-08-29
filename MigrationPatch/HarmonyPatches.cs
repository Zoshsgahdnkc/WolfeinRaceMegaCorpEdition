using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Wolfein;

namespace MCMigrationPatch
{
    [StaticConstructorOnStartup]
    public class StartUp
    {
        static StartUp()
        {
            Log.Warning("[军企版兼容包] 您现在正在使用军企版兼容包。这个兼容包用于兼容“沃芬族：军企版模组”的改动。此兼容包允许曾使用低于2.0版本的军企版的存档加载高于2.0版本的军企版模组。");
            Log.Warning("[军企版兼容包] 此兼容包将停止维护。如果您要开始新游戏，请不要在新游戏中使用这个兼容包！请前往创意工坊订阅 “沃芬族额外背景故事” 和 “沃芬族额外基因”以继续体验完整内容。");
            Log.Warning("[MegaCorp Edition Migration Patch] You are now using Mega Corp Migration Patch. This patch is designed to support the change of mod \"Wolfein Race: Mega Corp Edition\". This patch allow saves that once use Mega Corp Edition mod version <2.0 to load Mega Corp Edition mod version >2.0.");
            Log.Warning("[MegaCorp Edition Migration Patch] This patch will no longer be maintained! Please DO NOT USE THIS PATCH IN A NEW GAME! Subscribe mod \"Wolfein Race Extra Backstories\" and \"Wolfein Race Extra Genes\" to continue to experience full content.");
            var harmony = new Harmony("com.zoshsgahdnkc.MCMigrationPatch");
            harmony.PatchAll();
        }
    }
    
    // 对原版的技能学习打补丁，应用艺术技能学习系数
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.LearnRateFactor))]
    public static class Patch_SkillRecord_LearnRateFactor
    {
        static Patch_SkillRecord_LearnRateFactor() => DefOfHelper.EnsureInitializedInCtor(typeof (WRMC_DefOfs));
        public static void Postfix(SkillRecord __instance,bool direct, ref float __result)
        {
            if (direct) return;
            if (__instance.def != SkillDefOf.Artistic) return;

            __result *= __instance.Pawn.GetStatValue(WRMC_DefOfs.WRMC_ArtisticLearningFactor);
        }
    }

    // 对原版工艺过程打补丁，应用艺术品品质偏移
    [HarmonyPatch]
    public static class Patch_GenRecipe_PostProcessProduct
    {
        static MethodBase TargetMethod() => AccessTools.Method(typeof(GenRecipe), "PostProcessProduct");

        public static void Postfix(ref Thing __result, Thing product, RecipeDef recipeDef, Pawn worker)
        {
            int offset = (int)worker.GetStatValue(WRMC_DefOfs.WRMC_ArtisticQualityOffset);
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
            DefOfHelper.EnsureInitializedInCtor(typeof (WRMC_DefOfs));
            DefOfHelper.EnsureInitializedInCtor(typeof (InspirationDefOf));
        } 
        public static void Postfix(InspirationHandler __instance, ref InspirationDef __result)
        {
            if (__instance.pawn.genes.HasActiveGene(WRMC_DefOfs.WRMC_GeniusArtist))
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
            if (p == null || p.GetStatValue(WRMC_DefOfs.WRMC_InspirationMTBFactor) == null) return;
            __result *= __instance.pawn.GetStatValue(WRMC_DefOfs.WRMC_InspirationMTBFactor);
        }
    }
    
    [HarmonyPatch(typeof(Window), nameof(Window.PostOpen))]
    public static class Patch_Window_PostOpen
    {
        public static void Postfix(Window __instance)
        {
            if (!(__instance is Page_SelectScenario)) return;
            
            var text = "MCMigrationPatchWarningWindow.Text".Translate();

            var dlg = new Dialog_MessageBox(
                text,
                "MCMigrationPatchWarningWindow.Continue".Translate(), // A
                () => {},
                "MCMigrationPatchWarningWindow.GoBack".Translate(), // B
                () => __instance.Close(),  // 关掉当前页面，回主菜单
                "MCMigrationPatchWarningWindow".Translate(),
                true
            )
            {
                doCloseX = false,
                absorbInputAroundWindow = true,
                forcePause = true
            };

            Find.WindowStack.Add(dlg);
            Log.Message("Opened");
        }
    }
    
}