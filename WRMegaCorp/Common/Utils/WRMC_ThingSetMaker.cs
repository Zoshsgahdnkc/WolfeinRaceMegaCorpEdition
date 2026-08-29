using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Wolfein;

namespace WRMegaCorp;

public class WRMC_ThingSetMaker_SettlementReward : ThingSetMaker
{
    public static IEnumerable<ThingDef> cachedAllThingDefs;
    public SettlementRewardOption option;
    public static float cachedValueMultiplier = 2f;

    public Thing GenerateOne(ThingSetMakerParams parms)
    {
        List<Thing> things = new List<Thing>();
        Generate(parms, things);
        return things.First();
    }
    
    protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
    {
        if (option == null)
        {
            Log.Error("option == null");
            return;
        }

        var allThingDefs = PossibleThingDefs();
        float targetMarketValue = 0;
        FloatRange randomFactor = new FloatRange(0.5f, 1.5f);
        // 资源
        if (option == SettlementRewardOption.Resource)
        {
            var thingDefs = allThingDefs.Where(td => td.thingCategories != null && (td.thingCategories.Contains(ThingCategoryDefOf.ResourcesRaw)|| td.thingCategories.Contains(ThingCategoryDefOf.Textiles)));
            var def = thingDefs.RandomElement();
            Thing thing = ThingMaker.MakeThing(def);
            targetMarketValue = 65 * (float) Math.Pow(cachedValueMultiplier, 1.2f) * randomFactor.RandomInRange;
            if (def == ThingDefOf.Silver) targetMarketValue *= 0.5f;
            int stackCount = (int) Math.Clamp(targetMarketValue/thing.GetStatValue(StatDefOf.MarketValue), 1, def.stackLimit);
            thing.stackCount = stackCount;
            outThings.Add(thing);
            return;
        }
        // 部件
        if (option == SettlementRewardOption.Component)
        {
            var thingDefs = allThingDefs.Where(td => td.thingCategories != null && td.thingCategories.Contains(ThingCategoryDefOf.Manufactured) && td != WolfeinDefOf.Wolfein_ImperialDataAnalysis);
            var def = thingDefs.RandomElement();
            Thing thing = ThingMaker.MakeThing(def);
            targetMarketValue = 140 * cachedValueMultiplier * randomFactor.RandomInRange;
            int stackCount = (int) Math.Clamp(targetMarketValue/thing.GetStatValue(StatDefOf.MarketValue), 1, def.stackLimit);
            thing.stackCount = stackCount;
            outThings.Add(thing);
            return;
        }
        // 食物
        if (option == SettlementRewardOption.Meal)
        {
            var thingDefs = allThingDefs.Where(td => td.ingestible != null && td != WolfeinDefOf.Wolfein_EclipseTrigger);
            var def = thingDefs.RandomElement();
            Thing thing = ThingMaker.MakeThing(def);
            targetMarketValue = 100 * cachedValueMultiplier * randomFactor.RandomInRange;
            int stackCount = (int) Math.Clamp(targetMarketValue/thing.GetStatValue(StatDefOf.MarketValue), 1, def.stackLimit);
            thing.stackCount = stackCount;
            outThings.Add(thing);
            return;
        }
        // 杂项
        if (option == SettlementRewardOption.Misc)
        {
            var def = Rand.Element(WRMC_ThingUtils.Medicine(), WolfeinDefOf.Wolfein_EclipseTrigger);
            Thing thing = ThingMaker.MakeThing(def);
            targetMarketValue = 75 * cachedValueMultiplier * randomFactor.RandomInRange;
            int stackCount = (int) Math.Clamp(targetMarketValue/thing.GetStatValue(StatDefOf.MarketValue), 1, def.stackLimit);
            thing.stackCount = stackCount;
            outThings.Add(thing);
            return;
        }
        // 宝藏
        if (option == SettlementRewardOption.Treasure)
        {
            var thingDefs = allThingDefs.Where(td => td == WolfeinDefOf.Wolfein_ImperialDataAnalysis);
            var def = thingDefs.RandomElement();
            Thing thing = ThingMaker.MakeThing(def);
            targetMarketValue = 500 * cachedValueMultiplier * randomFactor.RandomInRange;
            int stackCount = (int) Math.Clamp(targetMarketValue/thing.GetStatValue(StatDefOf.MarketValue), 1, def.stackLimit);
            thing.stackCount = stackCount;
            outThings.Add(thing);
            return;
        }
        // 书本
        if (option == SettlementRewardOption.Book)
        {
            Book book = BookUtility.MakeBook(ArtGenerationContext.Outsider, QualityGenerator.Reward);
            outThings.Add(book);
            return;
        }
        // 服装
        if (option == SettlementRewardOption.Apparel)
        {
            var thingDefs = allThingDefs.Where(td => td.IsApparel);
            var def = thingDefs.RandomElement();
            ThingDef stuff = GenStuff.RandomStuffByCommonalityFor(def, TechLevel.Spacer);
            Thing thing = ThingMaker.MakeThing(def, stuff);
            var compQuality = thing.TryGetComp<CompQuality>();
            compQuality?.SetQuality(WRMC_Utils.GenerateQualityWithMultiplier(cachedValueMultiplier), ArtGenerationContext.Outsider);
            outThings.Add(thing);
            return;
        }
        // 武器
        if (option == SettlementRewardOption.Weapon)
        {
            var thingDefs = allThingDefs.Where(td => td.IsWeapon);
            var def = thingDefs.RandomElement();
            Thing thing;
            if (def.MadeFromStuff)
            {
                ThingDef stuff = GenStuff.RandomStuffByCommonalityFor(def, TechLevel.Spacer);
                thing = ThingMaker.MakeThing(def, stuff);
            }
            else
            {
                thing = ThingMaker.MakeThing(def);
            }
            var compQuality = thing.TryGetComp<CompQuality>();
            compQuality?.SetQuality(WRMC_Utils.GenerateQualityWithMultiplier(cachedValueMultiplier), ArtGenerationContext.Outsider);
            outThings.Add(thing);
            return;
        }
    }

    

    protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
    {
        return PossibleThingDefs();
    }

    private static IEnumerable<ThingDef> PossibleThingDefs()
    {
        if (cachedAllThingDefs == null)
        {
            var thingDefs = new List<ThingDef>();
            // Resource
            thingDefs.Add(WRMC_ThingUtils.Alloy());
            thingDefs.Add(WRMC_ThingUtils.Fabric());
            thingDefs.Add(ThingDefOf.Plasteel);
            thingDefs.Add(ThingDefOf.Silver);
            thingDefs.Add(ThingDefOf.Gold);
        
            // Meal
            thingDefs.Add(WRMC_ThingUtils.PackageMeal());
            thingDefs.Add(WRMC_ThingUtils.CustardCake());
            thingDefs.Add(WRMC_ThingUtils.MeatBites());
            thingDefs.Add(WRMC_ThingUtils.Monster());
        
            // Component
            thingDefs.Add(ThingDefOf.Chemfuel);
            thingDefs.Add(WRMC_ThingUtils.ComponentElectro());
            thingDefs.Add(WRMC_ThingUtils.ComponentMech());
            thingDefs.Add(ThingDefOf.ComponentIndustrial);
            thingDefs.Add(ThingDefOf.ComponentSpacer);
        
            // Misc
            thingDefs.Add(WRMC_ThingUtils.Medicine());
            thingDefs.Add(WolfeinDefOf.Wolfein_EclipseTrigger);
        
            // Treasure
            thingDefs.Add(WolfeinDefOf.Wolfein_ImperialDataAnalysis);
        
            // Book
            thingDefs.Add(ThingDefOf.Novel);
            thingDefs.Add(ThingDefOf.TextBook);
            thingDefs.Add(ThingDefOf.Schematic);

            var wolfeinThings = DefDatabase<ThingDef>.AllDefs.Where(WRMC_Utils.isFromWolfein);
            
            // Apparel
            thingDefs.AddRange(wolfeinThings.Where(
                t => t.isNonRebelApparel() &&
                     t.BaseMarketValue < 125 * cachedValueMultiplier && 
                     (!t.apparel.tags?.Contains("Wolfein_Mechanitor") ?? true)));
            
            // Weapon
            // 非土制和简易武器，非机械体武器
            thingDefs.AddRange(wolfeinThings.Where(
                t => t.isNonRebelWeapon() &&
                     t.tradeability != Tradeability.None &&
                     t.BaseMarketValue < 750 * cachedValueMultiplier && 
                     (!t.weaponTags?.Contains("Wolfein_WeaponII") ?? false) &&
                     (!t.weaponTags?.Contains("Wolfein_M_WeaponI") ?? false) &&
                     (!t.weaponTags?.Contains("Wolfein_Weapon_Simple") ?? false) &&
                     (!t.weaponTags?.Contains("W_Weapon_HomemadeFlamethrower") ?? false)));    
            
            cachedAllThingDefs = thingDefs;
        }
        return cachedAllThingDefs;
    }
}

public class SettlementRewardOption
{
    // 价值因子敏感指数，会影响该种类的权重
    // 大于0时随价值因子提高而指数提高，适用于高价值物品
    // 小于0时随价值因子提高而反比降低，适用于低价值物品
    // +-1.5之内的值是比较有意义的
    private float valueMulSensitivityIndex = 0;
    private float basicWeight = 0;

    private SettlementRewardOption(float basicWeight, float valueMulSensitivityIndex)
    {
        this.valueMulSensitivityIndex = valueMulSensitivityIndex;
        this.basicWeight = basicWeight;
    }
    public static readonly SettlementRewardOption Resource = new(10f, -0.3f);
    public static readonly SettlementRewardOption Component = new(5f, 0f);
    public static readonly SettlementRewardOption Meal = new(3f, -0.1f);
    public static readonly SettlementRewardOption Misc = new(3f, 0f);
    public static readonly SettlementRewardOption Treasure = new(0.6f, 0.5f);
    public static readonly SettlementRewardOption Book = new(0.8f, 0.5f);
    public static readonly SettlementRewardOption Apparel = new(1.4f, 0.1f);
    public static readonly SettlementRewardOption Weapon = new(1.8f, 0.35f);

    private float getWeight(float valueMultiplier)
    {
        return basicWeight * (float) Math.Pow(valueMultiplier, valueMulSensitivityIndex);
    }
    public Pair<SettlementRewardOption, float> getEntry(float valueMultiplier)
    {
        return new Pair<SettlementRewardOption, float>(this, getWeight(valueMultiplier));
    }
}