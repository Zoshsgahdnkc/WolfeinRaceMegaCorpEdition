using System;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace WRMegaCorp
{
    public class GenStep_WolfeinSettlement: GenStep_Scatterer
    {
        //EXPOSED
        public bool needMissionDedicatedProcess;
        // 中央建筑部分的面积。修改的话房间生成会出问题
        private static readonly int CorePartSize = 31;
        public override int SeedPart => 1806208471;
        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            // 创建中央矩形区域
            CellRect centerRect = new CellRect(loc.x - CorePartSize / 2, loc.z - CorePartSize / 2, CorePartSize, CorePartSize);
            centerRect.ClipInsideMap(map);
            MapGenerator.UsedRects.Add(centerRect.ExpandedBy(3));
            var faction = Find.FactionManager.OfMegaCorp();
            var rebelFaction = Find.FactionManager.OfResistance();
            MapGenerator.SetVar<CellRect>("SettlementRect", centerRect);
            BaseGen.globalSettings.map = map;
            // 用于生成所有建筑和战利品的rp
            ResolveParams mainPartParams = new ResolveParams()
            {
                sitePart = parms.sitePart,
                rect = centerRect,
                faction = faction
            };
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
            
            // 用于生成守卫士兵的rp
            var strengthMultiplier = WRMegaCorpEdition.modSettings?.SettlementDefenseStrength ?? 1f;
            float pawnPoint = Rand.RangeSeeded(1150f, 1600f, SeedPart) * 2f * strengthMultiplier;
            if (needMissionDedicatedProcess) pawnPoint *= 0.4f;
            var pawnGroupMakerParms = new PawnGroupMakerParms()
            {
                tile = map.Tile,
                faction = faction,
                points = pawnPoint,
                inhabitants = true,
                seed = SeedPart
            };
            var pawnParams = mainPartParams with
            {
                singlePawnLord = LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, centerRect.CenterCell, 25000, mainPartParams.attackWhenPlayerBecameEnemy.GetValueOrDefault()), map),
                singlePawnSpawnCellExtraPredicate = (x => map.reachability.CanReachMapEdge(x, traverseParms)),
                pawnGroupMakerParams = pawnGroupMakerParms,
                pawnGroupKindDef = PawnGroupKindDefOf.Settlement
            };
            BaseGen.symbolStack.Push("pawnGroup", pawnParams);

            var reachEdgeParms = mainPartParams with
            {
                rect = centerRect.ContractedBy(1)
            };
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", reachEdgeParms);
            
            BaseGen.symbolStack.Push("wolfein_settlement", mainPartParams);
            
            BaseGen.symbolStack.Push("removeDangerousTerrain", mainPartParams);
            
            if (!ModsConfig.BiotechActive)
                return;
            ResolveParams unpolluteParms = mainPartParams with
            {
                rect = centerRect.ExpandedBy(Rand.Range(1, 4)),
                edgeUnpolluteChance = 0.5f
            };
            BaseGen.symbolStack.Push("unpollute", unpolluteParms);
            
            BaseGen.Generate();
            // map.fogGrid.Refog(centerRect);
            
            // 如果生成来自于任务，则进行后处理
            if (needMissionDedicatedProcess)
            {
                foreach (var building in map.listerBuildings.allBuildingsNonColonist.ToList())
                {
                    building.SetFaction(rebelFaction);
                }
                var rebelDefenseLord = LordMaker.MakeNewLord(rebelFaction,
                    new LordJob_DefendBase(rebelFaction, centerRect.CenterCell, 25000,
                        mainPartParams.attackWhenPlayerBecameEnemy.GetValueOrDefault()), map);
                foreach (var pawn in map.mapPawns.AllHumanlikeSpawned.Where(pawn => pawn.Faction == faction))
                {
                    pawn.SetFaction(rebelFaction);
                    rebelDefenseLord.AddPawn(pawn);
                }
                var rebelMultiplier = WRMegaCorpEdition.modSettings?.SettlementDefenseStrength ?? 1f;
                float rebelThreatPoint = parms.sitePart?.parms.threatPoints ?? 1000f;
                var rebelMakerParms = new PawnGroupMakerParms()
                {
                    tile = map.Tile,
                    faction = rebelFaction,
                    points = Math.Clamp(rebelThreatPoint * rebelMultiplier, 0f, 10000f),
                    inhabitants = true,
                    seed = SeedPart
                };
                var rebelParms = mainPartParams with
                {
                    singlePawnLord = rebelDefenseLord,
                    singlePawnSpawnCellExtraPredicate = (x => map.reachability.CanReachMapEdge(x, traverseParms)),
                    pawnGroupMakerParams = rebelMakerParms,
                    pawnGroupKindDef = PawnGroupKindDefOf.Combat
                };
                BaseGen.globalSettings.map = map;
                BaseGen.symbolStack.Push("pawnGroup", rebelParms);
                BaseGen.Generate();
                
            }
        }
    }
}