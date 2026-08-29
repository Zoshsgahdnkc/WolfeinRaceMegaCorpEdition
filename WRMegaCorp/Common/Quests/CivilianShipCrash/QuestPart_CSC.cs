using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    /*
     * 信号延后处理，负责处理接受任务后的逻辑
     */
    public class QuestPart_CSC : QuestPart
    {
        public int shipChunkCount;
        public string signalTag;
        public Map map;
        public IntVec3 pos;
        
        public QuestPart_CSC(string signalAccept, Map map, int shipChunkCount, IntVec3 pos, Quest quest)
        {
            this.shipChunkCount = shipChunkCount;
            this.signalTag = signalAccept;
            this.map = map;
            this.pos = pos;
            this.quest = quest;
        }

        // 接受信号后处理逻辑的部分
        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == this.signalTag)
            {
                spawnShipWreck(map, pos);
                Messages.Message((string) "WRMC.QuestShipCrash.Message".Translate(), 
                    (LookTargets) new TargetInfo(pos, map), MessageTypeDefOf.NeutralEvent);
            }
        }
        
        public void spawnShipWreck(Map map, IntVec3 position)
        {
            SkyfallerMaker.SpawnSkyfaller(ThingDefOf.ShipChunkIncoming, ThingDefOf.ShipChunk, position, map);
            // 随机再生成0~2个碎块
            int count = shipChunkCount - 1;
            int RANGE = 5;
            for (int i = 0; i < count; i++)
            {
                IntVec3 pos;
                if (TryFindShipChunkDropCell(position, map, RANGE, out pos))
                {
                    SkyfallerMaker.SpawnSkyfaller(ThingDefOf.ShipChunkIncoming, ThingDefOf.ShipChunk, pos, map);
                }
            }
        }
        
        // 抄
        private bool TryFindShipChunkDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
        {
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.ShipChunkIncoming, map, ThingDefOf.ShipChunk.terrainAffordanceNeeded, out pos, nearLoc: nearLoc, nearLocMaxDist: maxDist);
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shipChunkCount, "shipChunkCount");
            Scribe_Values.Look(ref signalTag, "signalTag");
            Scribe_References.Look(ref map, "map");
        }
        
    }
}