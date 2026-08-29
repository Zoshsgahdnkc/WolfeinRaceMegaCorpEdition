using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public class QuestNode_DebtSlave: QuestNode
{
    public SlateRef<IntRange> debtAmount;
    public SlateRef<int> letterTimeoutTicks;
    protected override void RunInt()
    {
        WRMC_Utils.LogDebugMessage("Quest Debt Slave: RunInt");
        // 任务板设置
        Slate slate = QuestGen.slate;
        Quest quest = QuestGen.quest;
        
        int need = (int) Math.Round(debtAmount.GetValue(slate).RandomInRange * WRMegaCorpEdition.modSettings?.QDebtSlave_debtAmountMultiplier??1f);
        var PGR = new PawnGenerationRequest(WRMC_DefOfs.Wolfein_DebtSlave, Find.FactionManager.OfMegaCorp(),
            forceGenerateNewPawn: true, allowDead: false);
        Pawn pawn =PawnGenerator.GeneratePawn(PGR);
        slate.Set("need", need);
        slate.Set("pawns", new List<Pawn> {pawn});
        slate.Set("quest", Find.RandomSurfacePlayerHomeMap);
        Map possibleMap = slate.Get<Map>("map");
        
        // 生成选择信
        var title = "WRMC.QuestDebtSlave.Letter".Translate();
        var text = "WRMC.QuestDebtSlave.Text".Translate(pawn.Named("PAWN"), pawn.story.Adulthood.titleShort, need);
        var signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
        var signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");
        
        var let = new ChoiceLetter_DebtSlave();
        let.def = LetterDefOf.PositiveEvent;
        let.Label = title;
        let.title = title;
        let.Text = text;
        let.quest = quest;
        let.signalAccept = signalAccept;
        let.signalReject = signalReject;
        let.map = possibleMap;
        let.need = need;
        let.StartTimeout(letterTimeoutTicks.GetValue(slate));
        
        Find.LetterStack.ReceiveLetter(let);
        
        // 创建扣钱的part
        var part = new QuestPart_ConsumeSilver();
        part.map = possibleMap;
        part.insignal = signalAccept;
        part.need = need;
        quest.AddPart(part);
    }

    protected override bool TestRunInt(Slate slate)
    {
        bool isActivated = WRMegaCorpEdition.modSettings?.QDebtSlave_Activated ?? true;
        float needMax = 1.35f * debtAmount.GetValue(slate).max * (WRMegaCorpEdition.modSettings?.QDebtSlave_debtAmountMultiplier ?? 1f);
        int current = SilverUtility.CountSilver(slate.Get<Map>("map"));
        WRMC_Utils.LogDebugMessage($"Test Run Debt Slave: isActivated={isActivated}, needMax={needMax}, current={current}");
        return (isActivated && (current >= needMax));
    }
}

public class QuestPart_ConsumeSilver: QuestPart
{
    public Map map;
    public string insignal;
    public int need;
    public QuestNode runIfSuccess;

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != insignal) return;
        SilverUtility.TryConsumeSilver(map, need);
    }
}

public class QuestNode_GetMapWithMaxColonists : QuestNode
{
    protected override void RunInt()
    {
        setMap(QuestGen.slate);
    }

    protected override bool TestRunInt(Slate slate)
    {
        setMap(slate);
        return true;
    }

    private bool isMapSurface(Map map)
    {
        var tile = map.Tile;
        if (!tile.Valid) return false;
        return !tile.LayerDef.isSpace;
    }

    private void setMap(Slate slate)
    {
        Map possibleMap = Find.Maps.Where(m => m.IsPlayerHome && isMapSurface(m))
            .MaxBy(m => m.mapPawns.FreeColonistsCount);
        if (possibleMap == null)
        {
            WRMC_Utils.LogDebugMessage("No surface player map! Using random instead");
            possibleMap = Find.RandomSurfacePlayerHomeMap;
        }
        slate.Set("map", possibleMap);
    }
}
    