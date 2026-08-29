using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;
using Wolfein;

namespace WRMegaCorp
{
public static class WRMC_Utils
{
    // 用于检测pawn是否有重型改造
    public static bool hasHeavyGearAugmentation(Pawn pawn)
    {
        var hediffSet = pawn.health.hediffSet;
        if (hediffSet != null) 
        {
            bool b = hediffSet.GetFirstHediffOfDef(HediffDef.Named("Wolfein_HeavyGearAugmentationInstalled")) != null || hediffSet.GetFirstHediffOfDef(HediffDef.Named("Wolfein_HeavyGearAugmentationLightInstalled")) != null;
            return b;
        }
        return false;
    }
    
    // 用于从rulepack直接生成string
    public static TaggedString genTextFromRulePack(RulePackDef rulePack, List<Rule> rules = null, Dictionary<string, string> constants = null)
    {
        var request = new GrammarRequest();
        request.Includes.Add(rulePack);
        if (rules != null)
        {
            request.Rules.AddRange(rules);
        }

        if (constants != null)
        {
            request.Constants.AddRange(constants);
        }
        return GrammarResolver.Resolve("root", request, capitalizeFirstSentence: true);
    }

    public static Faction OfMegaCorp(this FactionManager manager)
    {
        return manager.FirstFactionOfDef(WolfeinDefOf.Wolfein_Faction_MegaCorp);
    }
    
    public static Faction OfResistance(this FactionManager manager)
    {
        return manager.FirstFactionOfDef(WolfeinDefOf.Wolfein_Faction_Rebel);
    }

    public static bool isFromWolfein(ThingDef thingDef)
    {
        if (thingDef == null)
        {
            Log.Warning("thingDef is null: " + thingDef.label);
            return false;
        }
        if (thingDef.modContentPack == null)
        {
            // Log.Warning("modContentPack is null: " + thingDef.label);
            return false;
        }

        // if (thingDef.modContentPack.PackageId != "melondove.wolfeinrace")
        // {
        //     Log.Message($"{thingDef.label} has a packageID of {thingDef.modContentPack.PackageId}, not \"melondove.wolfeinrace\", thus excepting.");
        // }
        return thingDef.modContentPack.PackageId == "melondove.wolfeinrace";
    }

    public static IntVec3 rotateWithRect(IntVec3 pos, CellRect rect, Rot4 rot)
    {
        int relativeX = pos.x - rect.minX;
        int relativeZ = pos.z - rect.minZ;
        if (rot == Rot4.East)
        {
            (relativeX, relativeZ) = (rect.Width - relativeZ - 1, relativeX);
            pos.x = rect.minX + relativeX;
            pos.z = rect.minZ + relativeZ;
        }
        if (rot == Rot4.West)
        {
            (relativeX, relativeZ) = (relativeZ, rect.Height - relativeX - 1);
            pos.x = rect.minX + relativeX;
            pos.z = rect.minZ + relativeZ;
        }
        if (rot == Rot4.North)
        {
            (relativeX, relativeZ) = (rect.Width - relativeX - 1, rect.Height - relativeZ - 1);
            pos.x = rect.minX + relativeX;
            pos.z = rect.minZ + relativeZ;
        }
        return pos;
    }

    public static bool isNonRebelApparel(this ThingDef thingDef)
    {
        return thingDef.IsApparel && !thingDef.apparel.tags.ContainsAny(str => 
            str is "Wolfein_OnSkin_RebelCombat" or "Wolfein_OverHead_RebelPowerArmor" or "Wolfein_Shell_RebelCombatII" or "Wolfein_Shell_RebelPowerArmor" );
    }
    
    public static bool isNonRebelWeapon(this ThingDef thingDef)
    {
        return thingDef.IsWeapon && !thingDef.weaponTags.ContainsAny(str => 
            str is "Wolfein_RebelAlloyDagger" or "Wolfein_RebelPowerHammer" or "Wolfein_RebelLight" or "Wolfein_RebelStandard" ) && thingDef != ThingDef.Named("W_Weapon_RebelMissileLauncher");
    }
    
    public static QualityCategory GenerateQualityWithMultiplier(float valueMultiplier)
    {
        switch (valueMultiplier)
        {
            case < 1.0f:
            case > 10.0f:
                throw new Exception("valueMultiplier must be between 1.0f and 10.0f, get" + valueMultiplier);
            case < 1.9f:
                return QualityUtility.GenerateFromGaussian(1f,  min: QualityCategory.Poor);
            case < 3.9f:
                return QualityUtility.GenerateFromGaussian(1f,  min: QualityCategory.Normal, center: QualityCategory.Good, max: QualityCategory.Masterwork);
            case < 5.9f:
                return QualityUtility.GenerateFromGaussian(1f,  min: QualityCategory.Good, center: QualityCategory.Excellent);
            default:
                return QualityUtility.GenerateFromGaussian(1f,  min: QualityCategory.Excellent, center: QualityCategory.Excellent);
        }
    }
    
    // Sorry Ancot I copied it from your code ;)
    public static int AmountSendableSilver(Map map)
    {
        return TradeUtility.AllLaunchableThingsForTrade(map).Where<Thing>((Func<Thing, bool>) (t => t.def == ThingDefOf.Silver)).Sum<Thing>((Func<Thing, int>) (t => t.stackCount));
    }

    public static bool DebugMode => WRMegaCorpEdition.modSettings?.DebugMode ?? false;

    public static void LogDebugMessage(string message)
    {
        if (!DebugMode) return;
        Log.Message("[WRMC_DEBUG]" + message);
    }
    
    public static void LogError(string message)
    {
        try
        {
            Log.Error("[WRMC_DEBUG]" + message);
        }
        catch (Exception e)
        {
            Log.Error("[WRMC_DEBUG] LOGGING ERROR LOG WENT WRONG!");
            Log.Error(e.StackTrace);
        }
    }
    
    public static void LogWarning(string message)
    {
        try
        {
            Log.Warning("[WRMC_DEBUG]" + message);
        }
        catch (Exception e)
        {
            Log.Warning("[WRMC_DEBUG] LOGGING WARNING LOG WENT WRONG!");
            Log.Warning(e.StackTrace);
        }
    }
} 
// AI代码
public static class SilverUtility
{
    /// <summary>
    /// 统计地图上可用白银总数（全图）。
    /// extraFilter 可用于附加限制（例如只算可达、只算某区域）。
    /// </summary>
    public static int CountSilver(Map map, Predicate<Thing> extraFilter = null)
    {
        if (map == null) return 0;

        List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.Silver);
        int total = 0;

        // 直接 for 循环，避免 LINQ 分配
        for (int i = 0; i < list.Count; i++)
        {
            Thing t = list[i];
            if (!IsValidSilverThing(t)) continue;
            if (extraFilter != null && !extraFilter(t)) continue;

            total += t.stackCount;
        }

        return total;
    }

    /// <summary>
    /// 尝试扣除指定数量白银。成功返回 true，失败不扣除并返回 false。
    /// extraFilter 与 CountSilver 一致。
    /// </summary>
    public static bool TryConsumeSilver(Map map, int amount, Predicate<Thing> extraFilter = null)
    {
        if (map == null) return false;
        if (amount <= 0) return true;

        List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.Silver);

        // 第一遍：先检查是否足够（避免扣一半失败）
        int available = 0;
        for (int i = 0; i < list.Count; i++)
        {
            Thing t = list[i];
            if (!IsValidSilverThing(t)) continue;
            if (extraFilter != null && !extraFilter(t)) continue;

            available += t.stackCount;
            if (available >= amount) break; // 够了就提前退出
        }

        if (available < amount) return false;

        // 第二遍：实际扣除
        int remaining = amount;

        // 倒序遍历，防止 Destroy 影响当前列表迭代
        for (int i = list.Count - 1; i >= 0 && remaining > 0; i--)
        {
            Thing t = list[i];
            if (!IsValidSilverThing(t)) continue;
            if (extraFilter != null && !extraFilter(t)) continue;

            int take = t.stackCount <= remaining ? t.stackCount : remaining;
            if (take <= 0) continue;

            if (take == t.stackCount)
            {
                // 整堆扣除
                t.Destroy(DestroyMode.Vanish);
            }
            else
            {
                // 部分扣除
                Thing split = t.SplitOff(take);
                split.Destroy(DestroyMode.Vanish);
            }

            remaining -= take;
        }

        // 理论上不会 >0（前面已检查）
        return remaining <= 0;
    }

    private static bool IsValidSilverThing(Thing t)
    {
        return t != null
               && !t.Destroyed
               && t.Spawned
               && t.stackCount > 0;
    }
}
}