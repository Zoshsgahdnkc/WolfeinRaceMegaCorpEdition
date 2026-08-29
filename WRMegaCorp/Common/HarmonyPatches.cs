using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    
    [StaticConstructorOnStartup]
    public class StartUp
    {
        static StartUp()
        {
            var harmony = new Harmony("com.zoshsgahdnkc.WRMegaCorpEdition");
            harmony.PatchAll();
            Log.Message("[WRMegaCorp] 本游戏进程已被沃芬军工联合体监视。");
        }
    }
    
    // 对派系通讯台对话打补丁，添加沃芬复兴基金的对话
    [HarmonyPatch(typeof (FactionDialogMaker), nameof(FactionDialogMaker.FactionDialogFor))]
    public static class Patch_FactionDialogMaker_FactionDialogFor
    {
        [HarmonyPostfix]
        public static void Postfix(ref DiaNode __result, Pawn negotiator, Faction faction)
        {
            Map map = negotiator.Map;
            if (__result == null || faction.def.defName != "Wolfein_Faction_MegaCorp")
                return;
            var foundationDialog = FactionDialog.WolfeinRevivalFoundationDialogOption(map, faction, negotiator);
            var onlineStoreDialog = FactionDialog.MegaCorpOnlineStoreOption(map, faction, negotiator);
            __result.options.Insert(2, foundationDialog);
            __result.options.Insert(3, onlineStoreDialog);
        }
    }
    
    // 对WolfeinRace的通讯台补丁打补丁，橄榄瓜鸽的皮炎子
    [HarmonyPatch(typeof (Wolfein_FactionDialogMaker_FactionDialogFor_Patch), "Postfix")]
    public static class Patch_WolfeinDialogMaker_Postfix
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
    
    // 对单位受伤patch，使得心脏受伤时伤害变为原先的10%
    // 这个能生效吗？有待测试
    [HarmonyPatch(typeof (Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_Pawn_PreApplyDamage
    {
        [HarmonyPrefix]
        public static void Prefix(ref Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            // WRMC_Utils.LogDebugMessage("Checking...");
            if (dinfo.HitPart != null && __instance.health != null  && dinfo.HitPart.def == BodyPartDefOf.Heart && __instance.health.hediffSet.HasHediff(WRMC_DefOfs.WRMC_PrototypeBionicHeart_Hediff))
            {
                WRMC_Utils.LogDebugMessage($"Multiplying 0.1f to damage {dinfo.Amount} for pawn {__instance.LabelShort}");
                dinfo.SetAmount(dinfo.Amount * 0.1f);
            }
        }
    }
    
    // 对换装工作patch，使得试制型动力甲在有和没有重型改造时有不同的装备优先级。
    [HarmonyPatch(typeof (JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreRaw))]
    public static class Patch_JobGiver_OptimizeApparel_ApparelScoreRaw
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, Pawn pawn, Apparel ap)
        {
            if (!ap.GetComps<Comp_PreferableWithAugmentation>().EnumerableNullOrEmpty())
            {
                __result += WRMC_Utils.hasHeavyGearAugmentation(pawn) ? 0.4f * __result : -10;
            }
        }
    }
    
    // 对据点打补丁，使得军企派系生成特殊据点地图
    [HarmonyPatch(typeof(Settlement), "MapGeneratorDef", MethodType.Getter)]
    public static class Patch_Settlement_MapGeneratorDef
    {
        [HarmonyPostfix]
        public static void Postfix(Settlement __instance, ref MapGeneratorDef __result)
        {
            try
            {
                if (__instance?.Faction == null) return;
                if (Find.FactionManager?.OfMegaCorp() == null) return;
                if (__instance.Faction == Find.FactionManager.OfMegaCorp())
                {
                    __result = WRMC_DefOfs.WRMC_Faction;
                }
            }
            catch (Exception e)
            {
                Log.Error("[WRMC Debug] Patching Settlement MapGeneratorDef has unexpected error: " + e);
                Log.Error("Settlement is null:" + (__instance == null));
                Log.Error("MapGeneratorDef is null:" + (WRMC_DefOfs.WRMC_Faction?.defName??"NULL!"));
                Log.Error(e.StackTrace);
            }
            
        }
    }

    // [HarmonyPatch(typeof(Window), nameof(Window.PostOpen))]
    // public static class Patch_Window_PostOpen
    // {
    //     public static void Postfix(Window __instance)
    //     {
    //         if (__instance is not Dialog_ModMismatch dialog) return;
    //         // if (CompatWarningState.confirmedForThisSession) return;
    //         if (!NeedMigrationPatch(dialog)) return;
    //         if (WRMegaCorpEdition.modSettings?.CloseWarningForever ?? false) return;
    //
    //         var dlg = new Dialog_MessageBox(
    //             "WRMC.LoadWithoutPatch.Text".Translate(),
    //             "WRMC.LoadWithoutPatch.Continue".Translate(), // A
    //             null,
    //             "WRMC.LoadWithoutPatch.Acknowledged".Translate(), // B
    //             () => __instance.Close(), // 关掉当前页面，回主菜单
    //             "WRMC.LoadWithoutPatch".Translate(),
    //             true
    //         )
    //         {
    //             doCloseX = false,
    //             absorbInputAroundWindow = true,
    //             forcePause = true
    //         };
    //
    //         Find.WindowStack.Add(dlg);
    //     }
    //
    //     private static bool NeedMigrationPatch(Dialog_ModMismatch dialog)
    //     {
    //         // 存档里没有军企版时，肯定不需要提示
    //         // 存档里有军企版的话，要么存档里已有新模组，要么带了兼容包，不然就会提示
    //         bool megaCorpLoaded = dialog.loadedModNamesList.Contains("Wolfein Race MegaCorp Edition");
    //         bool newModLoaded = dialog.loadedModNamesList.ContainsAny(mod => mod is "Wolfein Race Extra Backstories" or "Wolfein Race Extra Genes");
    //         bool migrationPatchRunning = LoadedModManager.RunningModsListForReading.Any(m => m.PackageId is "com.zoshsgahdnkc.mcmigrationpatch");
    //         return megaCorpLoaded && (!newModLoaded && !migrationPatchRunning);
    //     }
    //
    //     // private static class CompatWarningState
    //     // {
    //     //     public static bool confirmedForThisSession = false;
    //     // }
    // }
    //
    // [HarmonyPatch(typeof(GameDataSaveLoader), nameof(GameDataSaveLoader.CheckVersionAndLoadGame), typeof(string))]
    // public static class Patch_LoadGame_Warn
    // {
    //     static bool Prefix(string saveFileName)
    //     {
    //         Log.Message("Loading " + saveFileName);
    //         if (CompatWarnState.bypassOnce) return true;
    //         // if (CompatWarnState.confirmedSaves.Contains(saveFileName)) return;
    //         if (MigrationPatchRunning()) return true;
    //         if (WRMegaCorpEdition.modSettings?.CloseWarningForever ?? false) return true;
    //
    //         var dlg = new Dialog_MessageBox(
    //             "WRMC.OpenSaveList.Text".Translate(),
    //             "WRMC.LoadWithoutPatch.DontShowAgain".Translate(), // A
    //             () => CompatWarnState.bypassOnce = true,
    //             "WRMC.LoadWithoutPatch.Acknowledged".Translate(),
    //             null,
    //             title:"WRMC.OpenSaveList".Translate()
    //         )
    //         {
    //             doCloseX = false,
    //             absorbInputAroundWindow = true,
    //             forcePause = true
    //         };
    //
    //         dlg.doCloseX = false;
    //         dlg.absorbInputAroundWindow = true;
    //         dlg.forcePause = true;
    //         Find.WindowStack.Add(dlg);
    //         
    //         return false;
    //     }
    //     
    //     public static class CompatWarnState
    //     {
    //         public static bool bypassOnce = false;
    //     }
    //     
    //     private static bool MigrationPatchRunning()
    //     {
    //         // LoadedModManager.RunningModsListForReading.ConvertAll(m => m.PackageId).ForEach(Log.Message);
    //         bool migrationPatchRunning = LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == "com.zoshsgahdnkc.mcmigrationpatch");
    //         return migrationPatchRunning;
    //     }
    // }
}