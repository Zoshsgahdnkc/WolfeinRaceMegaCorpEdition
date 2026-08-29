using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace WRMegaCorp
{
    public class SymbolResolver_WolfeinSettlement : SymbolResolver
    {
        protected static readonly int DefenseGap = 1;
        protected static readonly float chanceSpawningAnyTurret = 0.5f;
        protected static readonly float chanceSpawningBigTurretIfAny = 0f;
        protected static readonly float chanceSpawningMachineGunIfSmall = 0.35f;
        protected static readonly int roomCount = 16;
        protected static readonly int roomLength = 7;
        protected static readonly int walkwayLength = 3;
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction;
            
            # region Making floor and ceiling
            // 铺地板，铺天花板，清空内部物品
            var floorRect = rp.rect;
            var coreRect = floorRect.ContractedBy(1, 1);
            foreach (var floorPos in floorRect.EdgeCells)
            {
                map.terrainGrid.SetTerrain(floorPos, WRMC_ThingUtils.CeramicTileTerrain());
            }
            // 赞美Ancot
            foreach (var corePos in coreRect)
            {
                map.terrainGrid.SetTerrain(corePos, WRMC_ThingUtils.CeramicTileTerrain());
                map.roofGrid.SetRoof(corePos, RoofDefOf.RoofConstructed);
                var things = map.thingGrid.ThingsListAtFast(corePos).ToList();
                foreach (var thing in things)
                {
                    if (thing.def.destroyable)
                    {
                        thing.Destroy();
                    }
                    else
                    {
                        thing.DeSpawn();
                    }
                }
            }
            # endregion
            
            # region Generating wall and doors on the outside
            // 在中央周围生成自动门
            foreach (var rotation in Rot4.AllRotations)
            {
                IntVec3 doorPos = coreRect.GetCenterCellOnEdge(rotation);
                Thing door = ThingMaker.MakeThing(WRMC_ThingUtils.WolfeinDoor(), ThingDefOf.Steel);
                door.SetFaction(faction);
                GenSpawn.Spawn(door, doorPos, map);
            }
            // 生成一圈墙
            foreach (var edgeCell in coreRect.EdgeCells)
            {
                spawnBasicWallAndSkipIfExisted(edgeCell, map, faction);
            }
            # endregion
            
            # region Generating outside defenses
            // 生成角落的墙
            var defenseWallRect = floorRect.ExpandedBy(DefenseGap);
            var quarterWallCoords = new List<(int, int)>
            {
                (0,0),(0,1),(0,2),(0,3),(1,0),(2,0),(3,0)
            };
            foreach (var quarterWallCoord in quarterWallCoords)
            {
                var coord1 = new IntVec3(defenseWallRect.minX + quarterWallCoord.Item1, defenseWallRect.CenterCell.y, defenseWallRect.minZ + quarterWallCoord.Item2);
                var coord2 = new IntVec3(defenseWallRect.minX + quarterWallCoord.Item1, defenseWallRect.CenterCell.y, defenseWallRect.maxZ - quarterWallCoord.Item2);
                var coord3 = new IntVec3(defenseWallRect.maxX - quarterWallCoord.Item1, defenseWallRect.CenterCell.y, defenseWallRect.minZ + quarterWallCoord.Item2);
                var coord4 = new IntVec3(defenseWallRect.maxX - quarterWallCoord.Item1, defenseWallRect.CenterCell.y, defenseWallRect.maxZ - quarterWallCoord.Item2);
                spawnBasicWallAndSkipIfExisted(coord1, map, faction);
                spawnBasicWallAndSkipIfExisted(coord2, map, faction);
                spawnBasicWallAndSkipIfExisted(coord3, map, faction);
                spawnBasicWallAndSkipIfExisted(coord4, map, faction);
            }
            // 生成再外圈的路障
            var barrierRect = defenseWallRect.ExpandedBy(1);
            var cachedCenterCell = barrierRect.CenterCell;
            foreach (var pos in barrierRect.EdgeCells)
            {
                int distFromCenterX = Math.Abs(pos.x - cachedCenterCell.x);
                int distFromCenterZ = Math.Abs(pos.z - cachedCenterCell.z);
                if (distFromCenterX + distFromCenterZ < 28)
                {
                    spawnUnsolidBuildingIfPossible(pos, map, ThingDefOf.Barricade, ThingDefOf.Steel, faction);
                }
            }
            // 在路障上生成炮塔
            // 在每一边的路障上找左中右三点，每点有概率生成大型或小型炮塔
            var strengthMultiplier = WRMegaCorpEdition.modSettings?.SettlementDefenseStrength ?? 1f;
            int bRectLength = barrierRect.Width - 1;
            var turretPosList = new List<(int, int)>
            {
                (0, 10), (0, 17), (0, 24),
                (bRectLength, 10), (bRectLength, 17), (bRectLength, 24),
                (10, 0), (17, 0), (24, 0),
                (10, bRectLength), (17, bRectLength), (24, bRectLength)
            };
            foreach (var turretPos in turretPosList)
            {
                if (Rand.Chance(chanceSpawningAnyTurret * (strengthMultiplier > 1f ? strengthMultiplier * 0.4f + 0.6f : strengthMultiplier)))
                {
                    var pos = new IntVec3(barrierRect.minX + turretPos.Item1,  barrierRect.CenterCell.y, barrierRect.minZ + turretPos.Item2);
                    if (!Rand.Chance(chanceSpawningBigTurretIfAny))
                    {
                        var turret = Rand.Chance(chanceSpawningMachineGunIfSmall)
                            ? WRMC_ThingUtils.SmallMachineGunTurret()
                            : WRMC_ThingUtils.SmallRifleTurret();
                        spawnUnsolidBuildingIfPossibleIgnoreBuilding(pos, map, turret, null, faction);
                        foreach (var barrierPos in getSurroundedBarrierPosForSmallTurret(pos, barrierRect))
                        {
                            spawnUnsolidBuildingIfPossibleIgnoreBuilding(barrierPos, map, ThingDefOf.Barricade, ThingDefOf.Steel, faction);
                        }
                    }
                }
            }
            # endregion
            
            # region Generating rooms and loot
            // 创建网格，分配房间
            List<int> roomStartOffset = new List<int>{0, roomLength - 1, roomLength * 2 + walkwayLength - 1, roomLength * 3 + walkwayLength - 2};
            List<SettlementRoom> roomsToGen =  new List<SettlementRoom>();
            var roomTypes = addRandomRoomTypeUntilLimit(roomCount - 2);
            // 保证一定会生成一个卧室和一个发电机室
            roomTypes.Add(SettlementRoomType.Dorm_2);
            roomTypes.Add(SettlementRoomType.GeneratorRoomA);
            roomTypes.Shuffle();
            // 确定房间的坐标和朝向，生成房间
            // 价值因子，被Clamp于(1, 10)之间，代表战利品总体质量
            float valueMultiplier = WRMegaCorpEdition.modSettings?.SettlementLootValueMultiplier ?? 2f;
            List<Pair<SettlementRewardOption, float>> optionsWithWeight = new()
            {
                SettlementRewardOption.Resource.getEntry(valueMultiplier),
                SettlementRewardOption.Component.getEntry(valueMultiplier),
                SettlementRewardOption.Meal.getEntry(valueMultiplier),
                SettlementRewardOption.Misc.getEntry(valueMultiplier),
                SettlementRewardOption.Treasure.getEntry(valueMultiplier),
                SettlementRewardOption.Book.getEntry(valueMultiplier),
                SettlementRewardOption.Apparel.getEntry(valueMultiplier),
                SettlementRewardOption.Weapon.getEntry(valueMultiplier)
            };
            WRMC_ThingSetMaker_SettlementReward.cachedAllThingDefs = null;
            for (int xCoord = 0; xCoord < roomStartOffset.Count; xCoord++)
            {
                int xOffset = roomStartOffset[xCoord];
                for (int zCoord = 0; zCoord < roomStartOffset.Count; zCoord++)
                {
                    int zOffset = roomStartOffset[zCoord];
                    Rot4 rotSelection1 = xCoord <= 1 ? Rot4.East : Rot4.West;
                    Rot4 rotSelection2 = zCoord <= 1 ? Rot4.North : Rot4.South;
                    var roomRect = new CellRect(coreRect.minX + xOffset, coreRect.minZ + zOffset, roomLength, roomLength);
                    SettlementRoom room = new SettlementRoom(roomTypes[xCoord*4+zCoord], roomRect, Rand.Element(rotSelection1, rotSelection2), map, faction);
                    room.Generate(optionsWithWeight, valueMultiplier);
                }
            }
            # endregion
        }

        private List<SettlementRoomType> addRandomRoomTypeUntilLimit(int repeatCount)
        {
            var toReturn = new List<SettlementRoomType>();
            var everyTypesWithWeight = SettlementRoomType.everyTypesWithWeight;
            if (everyTypesWithWeight == null)
            {
                Log.Error("everyTypesWithWeight is null");
                return toReturn;
            }
            for (int i = 0; i < repeatCount; i++)
            {
                toReturn.Add(everyTypesWithWeight.RandomElementByWeight(pair => pair.Second).First);
            }
            return toReturn;
        }

        public IEnumerable<IntVec3> getSurroundedBarrierPosForSmallTurret(IntVec3 turretPos, CellRect rect)
        {
            if (turretPos.x == rect.minX)
            {
                IntVec3 pos1 = turretPos with { x = rect.minX - 1 };
                IntVec3 pos2 = pos1 with { z = pos1.z + 1 };
                IntVec3 pos3 = pos1 with { z = pos1.z - 1 };
                return new[] { pos1, pos2, pos3 };
            }
            if (turretPos.x == rect.maxX)
            {
                IntVec3 pos1 = turretPos with { x = rect.maxX + 1 };
                IntVec3 pos2 = pos1 with { z = pos1.z + 1 };
                IntVec3 pos3 = pos1 with { z = pos1.z - 1 };
                return new[] { pos1, pos2, pos3 };
            }
            if (turretPos.z == rect.minZ)
            {
                IntVec3 pos1 = turretPos with { z = rect.minZ - 1 };
                IntVec3 pos2 = pos1 with { x = pos1.x + 1 };
                IntVec3 pos3 = pos1 with { x = pos1.x - 1 };
                return new[] { pos1, pos2, pos3 };
            }
            if (turretPos.z == rect.maxZ)
            {
                IntVec3 pos1 = turretPos with { z = rect.maxZ + 1 };
                IntVec3 pos2 = pos1 with { x = pos1.x + 1 };
                IntVec3 pos3 = pos1 with { x = pos1.x - 1 };
                return new[] { pos1, pos2, pos3 };
            }
            return Enumerable.Empty<IntVec3>();
        }

        private bool canSpawnUnsolid(IntVec3 pos, Map map, ThingDef thing, bool ignoreBuilding = false)
        {
            bool isInBound = pos.InBounds(map);
            bool standable = ignoreBuilding || pos.Standable(map);
            bool unroofed = !pos.Roofed(map);
            bool canTerrainSupport = pos.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy);
            bool willNotWipeThings = ignoreBuilding || !GenSpawn.WouldWipeAnythingWith(pos, Rot4.North, thing, map, (x => x.def.category == ThingCategory.Building || x.def.category == ThingCategory.Item));
            return isInBound && standable && unroofed && canTerrainSupport && willNotWipeThings;
        }

        private void spawnUnsolidBuildingIfPossible(IntVec3 pos, Map map, ThingDef thing, ThingDef stuff, Faction faction)
        {
            if (canSpawnUnsolid(pos, map, thing))
            {
                Thing t = ThingMaker.MakeThing(thing, stuff);
                t.SetFaction(faction);
                GenSpawn.Spawn(t, pos, map);
            }
        }
        
        private void spawnUnsolidBuildingIfPossibleIgnoreBuilding(IntVec3 pos, Map map, ThingDef thing, ThingDef stuff, Faction faction)
        {
            if (canSpawnUnsolid(pos, map, thing, true))
            {
                Thing t = ThingMaker.MakeThing(thing, stuff);
                t.SetFaction(faction);
                GenSpawn.Spawn(t, pos, map);
            }
        }

        public static bool spawnBasicWallAndSkipIfExisted(IntVec3 pos, Map map, Faction faction, bool alsoSkipDoor = true)
        {
            Building thingAtPos = map.thingGrid.ThingAt<Building>(pos);
            if (thingAtPos != null)
            {
                if (thingAtPos.def == WRMC_ThingUtils.WolfeinWall()) return false;
                if (alsoSkipDoor && thingAtPos is Building_Door) return false;
            }
            var thingToGen = WRMC_ThingUtils.WolfeinWall_Steel();
            thingToGen.SetFaction(faction);
            GenSpawn.Spawn(thingToGen, pos, map);
            return true;
        }
    }

    public class SettlementRoom
    {
        SettlementRoomType type;
        private CellRect roomRect;
        private Rot4 roomRot;
        private Map map;
        private Faction faction;
        
        // 尝试生成战利品的次数和概率
        private List<Pair<int, float>> getLootGenTriesAndChance(float valueMultiplier)
        {
            if (valueMultiplier < 3.9f)
            {
                return new()
                {
                    new(0, 2 - valueMultiplier * 0.5f),
                    new(1, 1.2f),
                    new(2, 0.4f * valueMultiplier),
                };
            }
            if (valueMultiplier < 5.9f)
            {
                return new()
                {
                    new(1, 2f - valueMultiplier * 0.25f),
                    new(2, 2f),
                    new(3, 1f + valueMultiplier * 0.1f),
                };
            }
            return new()
            {
                new(2, 1f),
                new(3, 1f),
            };
        }

        public void Generate(List<Pair<SettlementRewardOption, float>> optionsWithWeight, float valueMultiplier)
        {
            // 生成门
            var door = WRMC_ThingUtils.AutoDoor_Steel();
            IntVec3 doorPos = roomRect.GetCenterCellOnEdge(roomRot);
            door.SetFaction(faction);
            GenSpawn.Spawn(door, doorPos, map);
            // 铺墙
            foreach (var pos in roomRect.EdgeCells)
            {
                if (map.thingGrid.ThingAt(pos, ThingCategory.Building) is not Building_Door)
                {
                    SymbolResolver_WolfeinSettlement.spawnBasicWallAndSkipIfExisted(pos, map, faction);
                }
            }
            // 铺地板
            if (type.floor != null)
            {
                var floorRect = roomRect.ContractedBy(1);
                foreach (var pos in floorRect)
                {
                    map.terrainGrid.SetTerrain(pos, type.floor);
                }
            }
            int centerY = roomRect.CenterCell.y;
            // 生成内部建筑
            foreach (var tuple in type.buildings)
            {
                var building = tuple.Item1.Invoke();
                var coords = tuple.Item2;
                var pos = new IntVec3(roomRect.minX + coords.First, centerY, roomRect.minZ + coords.Second);
                pos = WRMC_Utils.rotateWithRect(pos, roomRect, roomRot);
                building.SetFaction(faction);
                // 如果可以旋转，应用旋转
                if (building.def.rotatable)
                {
                    bool? attachedToWall = building.def?.PlaceWorkers?.ContainsAny(pw => pw is Placeworker_AttachedToWall);
                    // 确保需要贴墙的物品贴墙，目前如果不贴墙则不生成
                    if (attachedToWall == true)
                    {
                        var r = new Rot4(roomRot.AsInt + tuple.Item3);
                        var blockPos = pos + GenAdj.CardinalDirections[r.AsInt%4];
                        if (map.thingGrid.ThingsListAt(blockPos).Exists(
                                thing => thing.def.building is { supportsWallAttachments: true } &&
                                         thing.def.Fillage == FillCategory.Full))
                        {
                            GenSpawn.Spawn(building, pos, map, r);
                        }
                    }
                    else
                    {
                        GenSpawn.Spawn(building, pos, map, new Rot4(roomRot.AsInt + tuple.Item3));
                    }
                }
                else
                {
                    // 如果建筑不为方形又不能旋转（如化合燃料发电机），应用特殊的坐标变换
                    if (building.def.size.x != building.def.size.z)
                    {
                        Log.Error("Tried to spawn a non-square building in a rotatable room: " + building.Label);
                        continue;
                    }
                    if (building.def.size.x != 1 )
                    {
                        int width = building.def.size.x;
                        if (roomRot.AsInt == 1)
                        {
                            pos.x -= width - 1;
                        }
                        if (roomRot.AsInt == 3)
                        {
                            pos.z -= width - 1;
                        }
                        if (roomRot.AsInt == 0)
                        {
                            pos.x -= width - 1;
                            pos.z -= width - 1;
                        }
                    }
                    // Fallback
                    GenSpawn.Spawn(building, pos, map);
                }
            }
            // 添加战利品
            if (!type.lootPositions.Empty())
            {
                var triesEntries = getLootGenTriesAndChance(valueMultiplier);
                foreach (var lootPos in type.lootPositions)
                {
                    int tries = triesEntries.RandomElementByWeight(p => p.Second).First;
                    if (tries == 0) continue;
                    for (int i = 0; i < tries; i++)
                    {
                        var rawP = new IntVec3(lootPos.x + roomRect.minX, centerY, lootPos.z + roomRect.minZ);
                        var p = WRMC_Utils.rotateWithRect(rawP, roomRect, roomRot);
                        var option = type.limitedOption??optionsWithWeight.RandomElementByWeight(pair => pair.Second).First;
                        WRMC_ThingSetMaker_SettlementReward.cachedValueMultiplier = valueMultiplier;
                        var maker = new WRMC_ThingSetMaker_SettlementReward()
                        {
                            option = option
                        };
                        Thing thing = maker.GenerateOne(new ThingSetMakerParams());
                        // Log.Message($"Generating {thing.stackCount} {thing.Label} at pos {p}");
                        GenSpawn.Spawn(thing, p, map);
                        thing.TrySetForbidden(true);
                    }
                }
            }
        }

        public SettlementRoom(SettlementRoomType type, CellRect roomRect, Rot4 roomRot, Map map, Faction faction)
        {
            this.type = type;
            this.roomRect = roomRect;
            this.roomRot = roomRot;
            this.map = map;
            this.faction = faction;
        }
    }
}