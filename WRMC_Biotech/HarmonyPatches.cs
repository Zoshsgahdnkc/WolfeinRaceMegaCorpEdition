using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace WRMegaCorp
{
    [StaticConstructorOnStartup]
    public class StartUp
    {
        static StartUp()
        {
            var harmony = new Harmony("com.zoshsgahdnkc.WRMegaCorpEdition_Biotech");
            harmony.PatchAll();
        }
    }
    
    // Patch胚胎进入培育舱的判断
    [HarmonyPatch(typeof(HumanEmbryo), "EnsureImplantTargetValid")]
    public static class Patch_HumanEmbryo_EnsureImplantTargetValid
    {
        [HarmonyPrefix]
        static bool Prefix(HumanEmbryo __instance)
        {
            // Harmony 2.0+ 可以使用 __runOriginal 控制原始方法执行
            Thing implantTarget = __instance.implantTarget;
            
            // 只有当 implantTarget 不是 Pawn 时才检查 WolfeinGrowthVat
            if (implantTarget != null && !(implantTarget is Pawn))
            {
                if (implantTarget is WolfeinGrowthVat vat && vat.selectedEmbryo == __instance)
                { // 跳过原始方法
                    return false;
                }
            }
            
            return true; // 继续执行原始方法
        }
    }
}