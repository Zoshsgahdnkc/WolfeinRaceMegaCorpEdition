using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Wolfein;

namespace WRMegaCorp
{
    public class GenStep_RuinedOutpost: GenStep_BaseRuins
    {
        private static readonly FloatRange BlastMarksPer10K = new FloatRange(2f, 6f);
        private static readonly FloatRange RubblePilesPer10K = new FloatRange(4f, 10f);
        private static readonly IntRange RubblePileCountRange = new IntRange(3, 8);
        private static readonly IntRange RubblePileDistanceRange = new IntRange(3, 8);
        private static readonly float RandomHurtMechChance = 0.4f;
        private static readonly float RandomKillMechChance = 0f;
        protected override int RegionSize => 43;
        public override int SeedPart { get; }
        protected override LayoutDef LayoutDef => DefDatabase<StructureLayoutDef>.GetNamed("WRMC_RuinedOutpost");
        protected override Faction Faction => null;

        // protected Faction faction;

        public override void GenerateRuins(Map map, GenStepParams parms, FloatRange mapFillPercentRange)
        {
            base.GenerateRuins(map, parms, mapFillPercentRange);
            // MapGenUtility.SpawnExteriorLumps(map, ThingDefOf.RubblePile, RubblePilesPer10K, RubblePileCountRange, RubblePileDistanceRange);
            // MapGenUtility.SpawnScatter(map, ThingDefOf.Filth_BlastMark, BlastMarksPer10K);
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            base.Generate(map, parms);
            GenExplosions(map, parms);
            SpawnMechs(map, parms);
            ScatterAdditionalMechCorpses(map, parms);
            ScatterCorpses(map, parms);
        }

        private void GenExplosions(Map map, GenStepParams parms)
        {
            CellRect cellRect = CellRect.CenteredOn(map.Center, RegionSize / 2).ExpandedBy(5);
            CellRect center = CellRect.CenteredOn(map.Center, RegionSize / 4);
            for (int count = 0; count < Rand.RangeInclusive(5,8); count++)
            {
                IntVec3 pos = cellRect.RandomCell;
                if (center.Contains(pos)) continue;
                GenExplosion.DoExplosion(pos, map, 4f, DamageDefOf.Bomb, null, 0, chanceToStartFire: 0.7f);
            }
        }

        private void Scatter(IEnumerable<Thing> things, Map map)
        {
            if (things.EnumerableNullOrEmpty()) return;
            CellRect cellRect = CellRect.CenteredOn(map.Center, RegionSize / 2);
            IntVec3 random = cellRect.RandomCell;
            IntVec3 pos = IntVec3.Invalid;
            // 尝试5次，5次不成功就放弃
            for (int tries = 0; tries < 6; tries++)
            {
                if (tries == 5) return;
                pos = CellFinder.StandableCellNear(random, map, 4f);
                if (pos.IsValid) break;
            }
            // 确保第一个元素生成在可站立位置
            GenSpawn.Spawn(things.First(), pos.IsValid ? pos : random, map, WipeMode.VanishOrMoveAside);
            // 生成其余元素
            foreach (Thing thing in things.Skip(1))
            {
                if (CellFinder.TryFindRandomCellNear(pos, map, 1, vec3 => !vec3.Filled(map), out var otherPos, 4))
                {
                    GenSpawn.Spawn(thing, otherPos, map, WipeMode.VanishOrMoveAside);
                };
            }
        }

        private void ScatterCorpses(Map map, GenStepParams parms)
        {
            var pawnMakerParms = new PawnGenerationRequest(
                PawnKindDef.Named("Wolfein_MegaCorpCivilian"),
                null,
                PawnGenerationContext.NonPlayer,
                allowDead:true,
                forceRecruitable:true
                );
            var numbers = Math.Clamp(parms.sitePart.parms.threatPoints / 50, 1, 12);
            for (int i = 0; i < numbers; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(pawnMakerParms);
                pawn.inventory.DestroyAll();
                HealthUtility.DamageUntilDead(pawn);
                pawn.Corpse.SetForbidden(true);
                
                var money = ThingMaker.MakeThing(ThingDefOf.Silver);
                money.stackCount = Rand.Range(30, 50);
                money.SetForbidden(true);

                var blood1 = ThingMaker.MakeThing(ThingDefOf.Filth_Blood);
                var blood2 = ThingMaker.MakeThing(ThingDefOf.Filth_Blood);
                
                Scatter([pawn.Corpse, Rand.Chance(0.5f) ? money : blood2, blood1], map);
            }
        }
        
        private void ScatterAdditionalMechCorpses(Map map, GenStepParams parms)
        {
            Faction faction = Find.FactionManager.OfMechanoids;
            
            var pawnGroupMakerParms = new PawnGroupMakerParms()
            {
                tile = map.Tile,
                faction = faction,
                points = Math.Clamp(parms.sitePart.parms.threatPoints / 2, 50f, 3000f),
                inhabitants = true,
                groupKind =  PawnGroupKindDefOf.Combat,
                seed = parms.sitePart.parms.randomValue
            };
            var pawns = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
            foreach (var pawn in pawns)
            {
                HealthUtility.DamageUntilDead(pawn);
                pawn.Corpse.SetForbidden(true);
                Scatter([pawn.Corpse, ThingMaker.MakeThing(ThingDefOf.Filth_MachineBits)], map);
            }
        }


        private void SpawnMechs(Map map, GenStepParams parms)
        {
            List<Pawn> pawnList = new List<Pawn>();
            foreach (Pawn pawn in this.GenerateMechs(parms, map))
            {
                WRMC_Utils.LogDebugMessage($"current spawning: {pawn.Name} / {pawn.Label} / {pawn.def.defName}");
                CellRect spawnRect = CellRect.CenteredOn(map.Center, (int)(RegionSize * 0.5f));
                IntVec3 nearCell = spawnRect.ContractedBy(8).RandomCell;
                IntVec3 spawnCell;
                if (!SiteGenStepUtility.TryFindSpawnCellAroundOrNear(spawnRect, nearCell, map, out spawnCell) && !SiteGenStepUtility.TryFindSpawnCellAroundOrNear(spawnRect, map.Center, map, out spawnCell))
                {
                    Find.WorldPawns.PassToWorld(pawn);
                    WRMC_Utils.LogDebugMessage($"can't find cell for pawn {pawn.Label}");
                    break;
                }
                GenSpawn.Spawn(pawn, spawnCell, map);
                WRMC_Utils.LogDebugMessage($"spawn cell: {spawnCell}");
                pawnList.Add(pawn);
            }
            var newPawnList = pawnList.ListFullCopy();
            // 有概率随机伤害或者杀死机械族
            foreach (var pawn in  newPawnList)
            {
                if (Rand.Chance(RandomHurtMechChance))
                {
                    int dmg = Rand.RangeInclusive(1, 15);
                    var def = HealthUtility.RandomViolenceDamageType();
                    pawn.TakeDamage(new DamageInfo(def, dmg, 0.15f));
                }
                else if (Rand.Chance(RandomKillMechChance))
                {
                    HealthUtility.DamageUntilDead(pawn, HealthUtility.RandomViolenceDamageType());
                    pawnList.Remove(pawn);
                }
            }
            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_DefendBase(Faction.OfMechanoids, map.Center, 12000), map, pawnList);
        }
        
        private IEnumerable<Pawn> GenerateMechs(GenStepParams parms, Map map)
        {
            Faction faction = Find.FactionManager.OfMechanoids;
            
            var pawnGroupMakerParms = new PawnGroupMakerParms()
            {
                tile = map.Tile,
                faction = faction,
                points = Math.Clamp(parms.sitePart.parms.threatPoints, 50f, 6000f),
                inhabitants = true,
                groupKind =  PawnGroupKindDefOf.Combat,
                seed = parms.sitePart.parms.randomValue
            };
            return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
        }

        protected override LayoutStructureSketch GenerateAndSpawn(CellRect rect, Map map, GenStepParams parms, LayoutDef layoutDef)
        {
            MapGenerator.UsedRects.Add(rect.ExpandedBy(1));
            LayoutWorker worker = layoutDef.Worker;
            LayoutStructureSketch structureSketch = worker.GenerateStructureSketch(new StructureGenParams() { size = rect.Size });
            using (new RandBlock(structureSketch.id))
            {
                map.layoutStructureSketches.Add(structureSketch);
                this.structureSketches.Add(structureSketch);
                float? threatPoints = new float?();
                if (parms.sitePart != null)
                    threatPoints = new float?(parms.sitePart.parms.points);
                if (!threatPoints.HasValue && map.Parent is Site parent)
                    threatPoints = new float?(parent.ActualThreatPoints);
                worker.Spawn(structureSketch, map, rect.Min, threatPoints, faction: this.Faction);
                if (this.UseUsedRects)
                    MapGenerator.UsedRects.Add(rect.ExpandedBy(1));
                return structureSketch;
            }
        }

        protected override IEnumerable<CellRect> GetRects(CellRect area, Map map)
        {
            CellRect centerRect = new CellRect(
                area.CenterCell.x - RegionSize / 2,
                area.CenterCell.z - RegionSize / 2,
                RegionSize,
                RegionSize);
            centerRect.ClipInsideMap(map);
            return [centerRect];
        }
    }
}