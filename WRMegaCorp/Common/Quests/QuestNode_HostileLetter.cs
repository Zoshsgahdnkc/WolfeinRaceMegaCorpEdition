using System;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public class QuestNode_HostileLetter: QuestNode_Letter
{
    protected override void RunInt()
  {
    Slate slate = RimWorld.QuestGen.QuestGen.slate;
    var quest = QuestGen.quest;
    QuestPart_Letter part = new QuestPart_Letter();
    part.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(this.inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
    LetterDef def = this.letterDef.GetValue(slate) ?? LetterDefOf.NeutralEvent;
    if (typeof (ChoiceLetter).IsAssignableFrom(def.letterClass))
    {
      ChoiceLetter choiceLetter = LetterMaker.MakeLetter("WRMC.HostileInQuest.Letter".Translate(quest.name), "WRMC.HostileInQuest.Text".Translate(slate.Get<Faction>("faction").Name), def, QuestGenUtility.ToLookTargets(this.lookTargets, slate), this.relatedFaction.GetValue(slate), RimWorld.QuestGen.QuestGen.quest);
      part.letter = (Letter) choiceLetter;
    }
    else
    {
      part.letter = LetterMaker.MakeLetter(def);
      part.letter.lookTargets = QuestGenUtility.ToLookTargets(this.lookTargets, slate);
      part.letter.relatedFaction = this.relatedFaction.GetValue(slate);
    }
    part.chosenPawnSignal = QuestGenUtility.HardcodedSignalWithQuestID(this.chosenPawnSignal.GetValue(slate));
    part.useColonistsOnMap = this.useColonistsOnMap.GetValue(slate);
    part.useColonistsFromCaravanArg = this.useColonistsFromCaravanArg.GetValue(slate);
    part.acceptedVisitorsSignal = QuestGenUtility.HardcodedSignalWithQuestID(this.acceptedVisitorsSignal.GetValue(slate));
    part.visitors = this.visitors.GetValue(slate);
    part.signalListenMode = this.signalListenMode.GetValue(slate).GetValueOrDefault();
    part.filterDeadPawnsFromLookTargets = this.filterDeadPawnsFromLookTargets.GetValue(slate);
    RimWorld.QuestGen.QuestGen.quest.AddPart((QuestPart) part);
  }
}