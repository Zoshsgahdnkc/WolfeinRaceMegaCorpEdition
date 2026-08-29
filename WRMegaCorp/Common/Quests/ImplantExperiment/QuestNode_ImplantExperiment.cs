using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public class QuestNode_ImplantExperiment: QuestNode
{
    
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Faction faction = Find.FactionManager.OfMegaCorp();

        var possibleBionic = UnlockBionicGameComponent.Get()?.selectRandomLockedBionic();
        if (possibleBionic == null)
        {
            WRMC_Utils.LogError("Quest Implant Experiment initiated but possibleBionic is null !!!");
            possibleBionic = WRMC_DefOfs.WRMC_PrototypeBionicHeart_Thing;
        }
        
        slate.Set<ThingDef>("bionic", possibleBionic);
        slate.Set("faction", faction);
        slate.Set("title", faction.LeaderTitle);
        slate.Set("leader", faction.leader.LabelShort);
        
        QuestUtility.AddQuestTag(ref QuestGen.quest.tags, "WRMC_bionic");
    }

    protected override bool TestRunInt(Slate slate)
    {
        Faction faction = Find.FactionManager.OfMegaCorp();
        bool activated = WRMegaCorpEdition.modSettings.QImplantExperiment_Activated;
        bool hasLockedBionic = UnlockBionicGameComponent.Get()?.hasLockedBionic() ?? false;
        return activated && hasLockedBionic && (!faction.defeated) && (faction.RelationKindWith(Faction.OfPlayer) >= FactionRelationKind.Ally);
    }
    
}