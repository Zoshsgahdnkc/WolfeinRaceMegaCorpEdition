using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public class QuestNode_SetSiteName: QuestNode
{
    public SlateRef<Site> site;
    public SlateRef<string> name;
    
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        var site_ = site.GetValue(slate);
        // string str = name.GetValue(slate) + "WRMC.QuestProjectLiquidation.SiteSuffix".Translate();
        site_.customLabel = name.GetValue(slate);
    }

    protected override bool TestRunInt(Slate slate) => true;
}