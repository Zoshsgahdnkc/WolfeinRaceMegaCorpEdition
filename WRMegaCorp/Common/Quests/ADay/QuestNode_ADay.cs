using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    public class QuestNode_ADay: QuestNode
    {
        
        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var megaCorp = Find.FactionManager.OfMegaCorp();
            slate.Set("faction", megaCorp);
            slate.Set("resistanceID", Find.FactionManager.OfResistance().GetUniqueLoadID());
            if (megaCorp.PlayerRelationKind == FactionRelationKind.Hostile)
            {
                slate.Set("leader", "WRMC.QuestArmisticeDay.TheWolfeins".Translate());
            }
            else
            {
                slate.Set("leader", megaCorp.leader.Label);
            }
            
            var involvedFactions = new QuestPart_InvolvedFactions();
            involvedFactions.factions.Add(megaCorp);
            QuestGen.quest.AddPart(involvedFactions);
        }
        // 当军企派系存在且非敌对时可以激活
        protected override bool TestRunInt(Slate slate)
        {
            if (WRMegaCorpEdition.modSettings?.QArmisticeDay_Activated != true)
            {
                WRMC_Utils.LogDebugMessage("Mission Armistice Day is deactivated due to WRMC mod settings");
                return false;
            }
            var megaCorp = Find.FactionManager.OfMegaCorp();
            if (megaCorp == null || megaCorp.deactivated || megaCorp.defeated ) return false;
            return true;
        }
    }
}