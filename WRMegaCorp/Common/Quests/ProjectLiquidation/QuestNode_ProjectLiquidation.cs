using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine.Diagnostics;
using Verse;
using Verse.Grammar;
using Wolfein;

namespace WRMegaCorp;

public class QuestNode_ProjectLiquidation: QuestNode
{
    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        var megaCorp = Find.FactionManager.OfMegaCorp();
        var rebel = Find.FactionManager.OfResistance();
        slate.Set("faction", megaCorp);
        slate.Set("position", megaCorp.def.leaderTitle);
        slate.Set("leader", megaCorp.leader.LabelShort);
        var outpostName = NameGenerator.GenerateName(WRMC_DefOfs.WRMC_OutpostName, rootKeyword: "r_name");
        slate.Set("outpostName", outpostName);
        var questRewardMultiplier = WRMegaCorpEdition.modSettings?.QProjectLiquidation_RewardMul ?? 1f;
        slate.Set("questRewardMultiplier", questRewardMultiplier);
        slate.Set("sitePartDefs", new List<SitePartDef>{WRMC_DefOfs.WRMC_RogueSettlement});
        slate.Set("rebelFaction", rebel);
        slate.Set("megaCorpID", megaCorp.GetUniqueLoadID());
        slate.Set("rebelID", rebel.GetUniqueLoadID());
        
        var involvedFactions = new QuestPart_InvolvedFactions();
        involvedFactions.factions.Add(megaCorp);
        QuestGen.quest.AddPart(involvedFactions);
        
        var rebelSTR = WRMegaCorpEdition.modSettings?.SettlementDefenseStrength ?? 1f;
        var settlementSTR = WRMegaCorpEdition.modSettings?.QProjectLiquidation_RebelStrMul ?? 1f;
        int challengeRating = Math.Clamp((int)Math.Round((4000 * settlementSTR + 2000 * rebelSTR) / StorytellerUtility.DefaultThreatPointsNow(Find.World)), 1, 3);
        QuestGen.quest.challengeRating = challengeRating;
    }

    // 当军企派系存在且非敌对时可以激活
    protected override bool TestRunInt(Slate slate)
    {
        if (WRMegaCorpEdition.modSettings?.QProjectLiquidation_Activated != true)
        {
            if (WRMC_Utils.DebugMode)
            {
                Log.Message("Mission Project Liquidation is deactivated due to WRMC mod settings");
            }
            return false;
        }
        var megaCorp = Find.FactionManager.OfMegaCorp();
        if (megaCorp == null || megaCorp.deactivated || megaCorp.defeated || megaCorp.PlayerRelationKind == FactionRelationKind.Hostile) return false;
        return true;
    }
}