using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    public class QuestNode_CSC : RimWorld.QuestGen.QuestNode
    {
        private const int TimeoutTicks = 20000;
        protected virtual bool CanBeSpace => false;
        public SlateRef<PawnKindDef> pawnKind;
        public SlateRef<IntRange> pawnCount;
        public SlateRef<IntRange> shipChunkCount;
        private string signalAccept;
        private string signalReject;
        
        
        
        protected override void RunInt()
        {
            // 初始任务板和地图
            // 说到底我真的需要这个任务板吗？
            Quest quest = QuestGen.quest;
            Slate slate = QuestGen.slate;
            Map map;
            if (!slate.TryGet<Map>("map", out map))
                map = QuestGen_Get.GetMap(canBeSpace: (this.CanBeSpace ? 1 : 0) != 0);
            if (!this.CanBeSpace)
                quest.AcceptanceRequirementNotSpace(map.Parent);
            slate.Set<Map>("map", map);
            slate.Set("pawnKind", pawnKind);
            slate.Set("pawnCount", pawnCount);
            
            Faction faction = generateFaction();
            slate.Set("faction", faction);

            List<Pawn> pawns = GenerateInjuredPawns(faction, slate);
            slate.Set("pawns", pawns);
            
            IntVec3 pos = CellFinderLoose.TryGetRandomCellWith((Predicate<IntVec3>) (x => x.Standable(map) && !x.Roofed(map) && !x.Fogged(map) && map.reachability.CanReachColony(x)), map, 1000, out pos) ? pos : DropCellFinder.RandomDropSpot(map);
            slate.Set("pos", pos);
            
            // 设置信号
            this.signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
            this.signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");
            
            // 发送信件
            SendChoiceLetter(quest, map);
            
            QuestPart_CSC part = new QuestPart_CSC(signalAccept, map, shipChunkCount.GetValue(slate).RandomInRange, pos, quest);
            quest.AddPart(part);
            quest.Signal(signalAccept, action: () =>
            {
                quest.DropPods(map.Parent, pawns, sendStandardLetter: false, dropSpot: pos, faction: faction);
            });
            
            quest.Delay(TimeoutTicks, (Action) (() => QuestGen_End.End(quest, QuestEndOutcome.Fail)));
        }

        // 严肃抄代码
        protected override bool TestRunInt(Slate slate)
        {
            if (WRMegaCorpEdition.modSettings?.QCivilianShipCrash_Activated != true)
            {
                if (WRMC_Utils.DebugMode)
                {
                    Log.Message("Mission Civilian Ship Crash is deactivated due to WRMC mod settings");
                }
                return false;
            }
            return !slate.TryGet<Map>("map", out Map _) ? QuestGen_Get.GetMap(canBeSpace: (this.CanBeSpace ? 1 : 0) != 0) != null : (!this.CanBeSpace ? Find.AnyPlayerHomeMap != null : Find.RandomSurfacePlayerHomeMap != null);
        }
        
        public void SendChoiceLetter(Quest quest, Map map)
        {
            TaggedString title = "WRMC.QuestShipCrash.Letter".Translate();
            TaggedString text = "WRMC.QuestShipCrash.Text".Translate();
            
            // 直接实例化自定义 ChoiceLetter_CSC 类
            ChoiceLetter_CSC let = new ChoiceLetter_CSC();
            let.def = LetterDefOf.NegativeEvent;
            let.Label = title;
            let.title = title;
            let.Text = text;
            let.quest = quest;
            let.signalAccept = this.signalAccept;
            let.signalReject = this.signalReject;
            let.StartTimeout(TimeoutTicks);
            
            Find.LetterStack.ReceiveLetter(let);
            
        }

        public Faction generateFaction()
        {
            // 创建临时派系的部分，抄原版代码
            List<FactionRelation> relations = new List<FactionRelation>();
            foreach (Faction faction1 in Find.FactionManager.AllFactionsListForReading)
            {
                if (!faction1.def.PermanentlyHostileTo(WolfeinDefOf.Wolfein_OutlanderRefugee))
                    relations.Add(new FactionRelation()
                    {
                        other = faction1,
                        kind = FactionRelationKind.Neutral
                    });
            }
            Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(WolfeinDefOf.Wolfein_OutlanderRefugee, relations, true);
            faction.temporary = true;
            Find.FactionManager.Add(faction);
            return faction;
        }

        // 用于生成落难者的方法
        protected List<Pawn> GenerateInjuredPawns(Faction faction, Slate slate)
        {
            PawnKindDef pawnKind = slate.Get<PawnKindDef>("pawnKind");
            IntRange pawnCount = slate.Get<IntRange>("pawnCount");
            var pawns = new List<Pawn>();
            // 随机生成pawns
            int pawnsCount = pawnCount.RandomInRange;
            for (int i = 0; i < pawnsCount; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, Find.FactionManager.FirstFactionOfDef(WolfeinDefOf.Wolfein_Faction_MegaCorp), forceGenerateNewPawn: true, allowDead: true));
                // 添加伤势
                HealthUtility.DamageUntilDowned(pawn);
                // 添加重伤奴隶的重伤状态
                Hediff hediff = pawn.health.AddHediff(WolfeinDefOf.Wolfein_Abasia);
                if (hediff.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears disappearsComp)
                {
                    disappearsComp.SetDuration(15000);
                }
                if (!pawn.IsWorldPawn())
                    Find.WorldPawns.PassToWorld(pawn);
                pawn.SetFaction(faction);
                pawns.Add(pawn);
            }
            return pawns;
        }
    }
}