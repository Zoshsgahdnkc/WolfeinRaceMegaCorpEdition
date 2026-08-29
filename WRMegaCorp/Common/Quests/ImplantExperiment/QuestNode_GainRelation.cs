using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public class QuestNode_GainRelation: QuestNode
{
    public SlateRef<Faction> faction;
    public SlateRef<int> goodWill;
    public SlateRef<HistoryEventDef> historyEventDef;
    public SlateRef<string> inSignal;
    
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Quest quest = QuestGen.quest;
        QuestPart_GainRelation part = new QuestPart_GainRelation();
        part.faction = faction.GetValue(slate);
        part.goodWill = goodWill.GetValue(slate);
        part.historyEventDef = historyEventDef.GetValue(slate);
        part.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate))??slate.Get<string>("inSignal");
        quest.AddPart(part);
    }

    protected override bool TestRunInt(Slate slate) => true;
}

public class QuestPart_GainRelation : QuestPart
{
    public Faction faction;
    public int goodWill;
    public HistoryEventDef historyEventDef;
    public string inSignal;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        if (signal.tag != inSignal) return; 
        if (!faction.TryAffectGoodwillWith(Faction.OfPlayer, goodWill, reason: historyEventDef))
        {
            WRMC_Utils.LogError($"Cannot Affect Goodwill with faction {faction.def?.defName??"NULL"}! Value: {goodWill}. Reason: {historyEventDef.label}.");
        }
    }

    public override void ExposeData()
    {
        Scribe_References.Look(ref this.faction, "faction");
        Scribe_Defs.Look(ref this.historyEventDef, "historyEventDef");
        Scribe_Values.Look(ref this.goodWill, "goodWill");
        Scribe_Values.Look(ref this.inSignal, "inSignal");
        base.ExposeData();
    }
}