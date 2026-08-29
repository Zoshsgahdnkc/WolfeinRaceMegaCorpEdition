using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace WRMegaCorp;

public class SitePartWorker_RogueSettlement: SitePartWorker_Outpost
{
    public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules,
        Dictionary<string, string> outExtraDescriptionConstants)
    {
    }

    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return "WRMC.QuestProjectLiquidation.SiteAdditionalDesc".Translate();
    }

    private int getSiteEnemiesCount(Site site, SitePartParams parms)
    {
        int seed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms);
        float megaCorpPersonnelStr = Rand.RangeSeeded(1150f, 1600f, seed) * 0.8f * (WRMegaCorpEdition.modSettings?.SettlementDefenseStrength ?? 1f);
        int megaCorpPersonnelCount = PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms()
        {
            tile = site.Tile,
            faction = Find.FactionManager.OfMegaCorp(),
            groupKind = PawnGroupKindDefOf.Settlement,
            points = megaCorpPersonnelStr,
            inhabitants = true,
            seed = seed
        }).Count();
        int resistancePersonnelCount = PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms()
        {
            tile = site.Tile,
            faction = Find.FactionManager.OfResistance(),
            groupKind = PawnGroupKindDefOf.Combat,
            points = Math.Clamp(parms.threatPoints * (WRMegaCorpEdition.modSettings?.SettlementDefenseStrength ?? 1f), 0f, 10000f),
            inhabitants = true,
            seed = seed
        }).Count();
        return  megaCorpPersonnelCount + resistancePersonnelCount;
    }

    public override void Notify_SiteMapAboutToBeRemoved(SitePart sitePart)
    {
        base.Notify_SiteMapAboutToBeRemoved(sitePart);
    }

    public override void PostDestroy(SitePart sitePart)
    {
        PlanetTile tile = sitePart.site.Tile;
        string label = sitePart.site.customLabel;
        bool b = isSiteDefeated(sitePart);
        if (WRMC_Utils.DebugMode) Log.Message("[WRMC Debug]Initiating site post [" + label + "] process:" + b);
        if (b)
        {
            var wo = (WorldObject_ReconstructionSite) WorldObjectMaker.MakeWorldObject(WRMC_DefOfs.WRMC_ReconstructionSite);
            wo.SetFaction(Find.FactionManager.OfMegaCorp());
            wo.Tile = tile;
            wo.StoredLabel = label;
            if (wo.TryGetComponent(out TimeoutAndTurnMegaCorpComp comp))
            {
                if (WRMC_Utils.DebugMode) comp.StartTimeout(900);
                // 超时15天
                else comp.StartTimeout(900000);
            }
            else
            {
                if (WRMC_Utils.DebugMode) Log.Message("[WRMC Debug]Fail to start reconstruction timeout! This is a serious issue!");
            }
            Find.WorldObjects.Add(wo);
            if (WRMC_Utils.DebugMode) Log.Message("[WRMC Debug]Reconstruction Site generated at tile" + tile);
            var let = LetterMaker.MakeLetter(
                "WRMC.SettlementReconstructing.Letter".Translate(), "WRMC.SettlementReconstructing.Text".Translate(label),
                LetterDefOf.PositiveEvent, wo, Find.FactionManager.OfMegaCorp());
            Find.LetterStack.ReceiveLetter(let);
        }
        base.PostDestroy(sitePart);
    }

    // 使用反射获取site的私有字段。为什么要私有，泰南你坏事做尽。
    private bool isSiteDefeated(SitePart sitePart)
    {
        try
        {
            Type siteType = sitePart.site.GetType();
            bool? toReturn = (bool?) siteType.GetField("allEnemiesDefeatedSignalSent",
                BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(sitePart.site);
            return toReturn ?? false;
        }
        catch (Exception e)
        {
            Log.Warning("[WRMC Debug]Something went wrong trying to acquire the allEnemiesDefeatedSignalSent field.");
            Log.Message(e.StackTrace);
        }
        return false;
    }
}