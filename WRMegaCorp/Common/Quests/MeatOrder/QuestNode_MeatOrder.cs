using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public class QuestNode_MeatOrder: QuestNode
{
    protected override void RunInt()
    {
    }

    protected override bool TestRunInt(Slate slate)
    {
        if (WRMegaCorpEdition.modSettings?.QMeatOrder_Activated != true)
        {
            if (WRMC_Utils.DebugMode)
            {
                Log.Message("Mission Meat Order is deactivated due to WRMC mod settings");
            }
            return false;
        }
        return true;
    }
}