using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Wolfein;

namespace WRMegaCorp;

public class QuestNode_TheRescue: QuestNode
{
    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        var faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(WolfeinDefOf.Wolfein_OutlanderRefugee));
        var map = Find.RandomRootSurfacePlayerHomeMap;
        
        slate.Set("refugeeFaction", faction);
        slate.Set("sitePartDefs", new List<SitePartDef>{WRMC_DefOfs.WRMC_RuinedOutpost});
        slate.Set("map", map);
        slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(map) / 3);
    }

    protected override bool TestRunInt(Slate slate)
    {
        return WRMegaCorpEdition.modSettings?.QTheRescue_Activated ?? true;
    }
}