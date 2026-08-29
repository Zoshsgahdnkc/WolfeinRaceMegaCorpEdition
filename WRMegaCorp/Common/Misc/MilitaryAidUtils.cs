using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    public static class MilitaryAidUtils
    {
        // 创建支援兵种池
        // 无人机池
        public static List<PawnGenOption> dronePool = new List<PawnGenOption>
        {
            // 无人机 战力80
            new PawnGenOption { kind = WolfeinDefOf.Wolfein_Mechanoid_Drone, selectionWeight = 7.0f },
            // 武装无人机 战力140
            new PawnGenOption { kind = WolfeinDefOf.Wolfein_Mechanoid_DroneArmed, selectionWeight = 3.0f },
        };
        // 机械族池
        public static List<PawnGenOption> mechPool = new List<PawnGenOption>
        {
            // 近距 战力200
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_Mechanoid_MilitaryB"), selectionWeight = 5.0f },
            // 盾卫 战力240
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_Mechanoid_MilitaryC"), selectionWeight = 5.0f },
            // 狙击 战力200
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_Mechanoid_MilitaryD"), selectionWeight = 5.0f },
            // 重型近距 战力400
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_Mechanoid_MilitaryF"), selectionWeight = 1.0f },
        };
        // 特遣机动队混编池
        public static List<PawnGenOption> MTFPool = new List<PawnGenOption>
        {
            // 护卫 战力170
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_MegaCorpGuard"), selectionWeight = 8.0f },
            // 重装支援兵 战力260
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_MegaCorpSupport"), selectionWeight = 12.0f },
            // 黑客 战力160
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_MegaCorpHacker"), selectionWeight = 2.0f },
            // 狙击手 战力200
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_MegaCorpSniper"), selectionWeight = 4.0f },
            // 尖兵 战力240
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_MegaCorpVanguard"), selectionWeight = 4.0f },
            // 重型火力 战力400
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_Mechanoid_MilitaryE"), selectionWeight = 1.2f },
            // 重型近距 战力400
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_Mechanoid_MilitaryF"), selectionWeight = 1.5f },
            // 超重型火力平台 战力1200，需要对应约6200以上的袭击点数才会出现
            new PawnGenOption { kind = PawnKindDef.Named("Wolfein_Mechanoid_MilitaryGiant"), selectionWeight = 1.0f },
        };
        
        
        public static bool TryCallAid(Verse.Map map, Faction faction,Pawn negotiator, float points, List<PawnGenOption> pool)
        {
            var parms = new PawnGroupMakerParms
            {
                faction = faction,
                points = points,
                tile = map.Tile,
                generateFightersOnly = true,
                raidStrategy = RaidStrategyDefOf.ImmediateAttackFriendly
            };
            // 抽取Pawn
            var aidingPawns = PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(points, pool, parms);
            // 生成Pawn
            List<Pawn> pawns = new List<Pawn>();
            foreach (var c in aidingPawns)
            {
                var req = new PawnGenerationRequest(
                    c.Option.kind,
                    faction,
                    PawnGenerationContext.NonPlayer,
                    tile: map.Tile,
                    forceGenerateNewPawn: false,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: true,
                    mustBeCapableOfViolence: true,
                    biocodeWeaponChance:1f
                );
                Pawn p = PawnGenerator.GeneratePawn(req);
                if (p != null) pawns.Add(p);
            }
            if (pawns.Empty()) return false;
            // 投放
            IntVec3 cell;
            var incidentParms = new IncidentParms
            {
                target = map,
                faction = faction,
                raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop,
                raidStrategy = RaidStrategyDefOf.ImmediateAttackFriendly,
                points = points,
                spawnCenter = DropCellFinder.FindRaidDropCenterDistant(map, false, true),
                forced = true
            };
            if (!incidentParms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(incidentParms))
                return false;
            incidentParms.raidArrivalMode.Worker.Arrive(pawns, incidentParms);
            incidentParms.raidStrategy.Worker.MakeLords(incidentParms, pawns);
            faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
            IncidentWorker.SendIncidentLetter("WRMC_LetterLabelMilitaryAid".Translate(),"WRMC_LetterTextMilitaryAid".Translate(negotiator), LetterDefOf.PositiveEvent, incidentParms, pawns[0], null);
            return true;
        }
    }
}