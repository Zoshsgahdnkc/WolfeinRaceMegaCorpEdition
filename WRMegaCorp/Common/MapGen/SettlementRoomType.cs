using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public enum SettlementRoomRole { Generator, Bedroom, Misc }

public class SettlementRoomType
{ 
    // 创建Thing时需要传入材料，而faction将会在SettlementRoom.Generate方法中设置
    // List<GenBuildingMethod, Pair<xOffset, zOffset>, rotationOffset>
    // rotationOffset为顺时针，朝下不变，顺时针每转90度+1
    public List<(Func<Thing>, Pair<int, int>, int)> buildings;
    public TerrainDef floor = WRMC_ThingUtils.CarpetTerrain();
    private SettlementRoomRole role;
    public List<IntVec2> lootPositions = new();
    public SettlementRewardOption limitedOption;
    private SettlementRoomType(){}

    public static readonly SettlementRoomType empty = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(2, 2), 0),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(4, 2), 0)
        }
    };

    private static List<Pair<SettlementRoomType, float>> cachedEveryTypesWithWeight = null;
    public static List<Pair<SettlementRoomType, float>> everyTypesWithWeight
    {
        get
        {
            if (cachedEveryTypesWithWeight == null)
            {
                cachedEveryTypesWithWeight = new List<Pair<SettlementRoomType, float>>()
                {
                    new (Dorm_1, 1f),
                    new (Dorm_2, 1.5f),
                    new (Dorm_3, 1f),
                    new (Dorm_4A, 1f),
                    new (Dorm_4B, 1f),
                    new (Office_1, 0.8f),   
                    new (Office_2, 1.5f),
                    new (Lounge, 1f),
                    new (ActivityRoom, 0.8f),
                    new (ConferenceRoom, 0.8f),
                    new (MachiningRoom, 1f),
                    new (StorageA, 1.5f),
                    new (StorageB, 1.5f),
                    new (GeneratorRoomA, 0.6f),
                    new (GeneratorRoomB, 1f),
                };
            }
            return cachedEveryTypesWithWeight;
        }
    }

    // 双人卧室，配有带模型的矮柜和一个置物柜
    public static readonly SettlementRoomType Dorm_2 = new SettlementRoomType()
    {
        role = SettlementRoomRole.Bedroom,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(1, 5), 0),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(5, 5), 0),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(2, 5), 2),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(4, 5), 2),
            new (WRMC_ThingUtils.LowCabinet, new Pair<int, int>(2, 2), 2),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(4, 2), 0),
            new (WRMC_ThingUtils.Model, new Pair<int, int>(2, 2), 2),
            new (WRMC_ThingUtils.Carpet3x3, new Pair<int, int>(3, 3), 0),
        },
        lootPositions =
        {
            new IntVec2(4,2)
        }, 
        limitedOption = SettlementRewardOption.Apparel
    };
    // 三人卧室
    public static readonly SettlementRoomType Dorm_3 = new SettlementRoomType()
    {
        role = SettlementRoomRole.Bedroom,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(1, 5), 0),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(3, 5), 0),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(5, 5), 0),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(2, 5), 2),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(4, 5), 2),
            new (WRMC_ThingUtils.LowCabinet, new Pair<int, int>(2, 2), 2),
            new (WRMC_ThingUtils.Wardrobe, new Pair<int, int>(4, 2), 2),
            new (WRMC_ThingUtils.Carpet3x3, new Pair<int, int>(3, 3), 0),
        }
    };
    // 四人卧室A，四张床分散
    public static readonly SettlementRoomType Dorm_4A = new SettlementRoomType()
    {
        role = SettlementRoomRole.Bedroom,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(1, 2), 3),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(1, 4), 3),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(5, 2), 1),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(5, 4), 1),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(1, 1), 1),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(1, 5), 1),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(5, 1), 3),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(5, 5), 3),
            new (WRMC_ThingUtils.Carpet3x3, new Pair<int, int>(3, 3), 0),
        }
    };
    // 四人卧室B，可以闻到舍友的狱卒
    public static readonly SettlementRoomType Dorm_4B = new SettlementRoomType()
    {
        role = SettlementRoomRole.Bedroom,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(2, 3), 0),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(3, 4), 1),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(4, 3), 2),
            new (WRMC_ThingUtils.SingleBed, new Pair<int, int>(3, 2), 3),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(3, 3), 2),
            new (WRMC_ThingUtils.NeonPlant, new Pair<int, int>(3, 3), 0),
            new (WRMC_ThingUtils.NeonPlant, new Pair<int, int>(3, 3), 1),
            new (WRMC_ThingUtils.NeonPlant, new Pair<int, int>(3, 3), 2),
            new (WRMC_ThingUtils.NeonPlant, new Pair<int, int>(3, 3), 3),
            new (WRMC_ThingUtils.Carpet3x3, new Pair<int, int>(3, 3), 0),
        }
    };
    // 单人卧室，带有一张办公桌和一个大置物架
    public static readonly SettlementRoomType Dorm_1 = new SettlementRoomType()
    {
        role = SettlementRoomRole.Bedroom,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.DoubleBed, new Pair<int, int>(4, 5), 0),
            new (WRMC_ThingUtils.BedsideTable, new Pair<int, int>(2, 5), 2),
            new (WRMC_ThingUtils.Wardrobe, new Pair<int, int>(5, 5), 3),
            new (WRMC_ThingUtils.LongShelf, new Pair<int, int>(2, 2), 1),
            new (WRMC_ThingUtils.Carpet1x1, new Pair<int, int>(3, 1), 2),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(4, 2), 2),
            new (WRMC_ThingUtils.OfficeDesk, new Pair<int, int>(4, 3), 0),
        },
        lootPositions =
        {
            new IntVec2(2,3),
            new IntVec2(2,2)
        }
    };
    // 单人办公室，带两个置物架
    public static readonly SettlementRoomType Office_1 = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(3, 4), 0),
            new (WRMC_ThingUtils.OfficeDesk, new Pair<int, int>(3, 3), 2),
            new (WRMC_ThingUtils.Carpet1x1, new Pair<int, int>(3, 1), 2),
            new (WRMC_ThingUtils.LunarGlobe, new Pair<int, int>(2, 3), 0),
            new (WRMC_ThingUtils.Surveillance, new Pair<int, int>(1, 4), 1),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(5, 5), 0),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(1, 1), 2),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(5, 1), 2),
        }
    };
    // 双人办公室，带一个大置物架
    public static readonly SettlementRoomType Office_2 = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(4, 1), 2),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(4, 4), 2),
            new (WRMC_ThingUtils.OfficeDesk, new Pair<int, int>(4, 2), 0),
            new (WRMC_ThingUtils.OfficeDesk, new Pair<int, int>(4, 5), 0),
            new (WRMC_ThingUtils.Surveillance, new Pair<int, int>(2, 1), 0),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(1, 5), 0),
            new (WRMC_ThingUtils.LongShelf, new Pair<int, int>(1, 2), 3),
        },
        lootPositions =
        {
            new IntVec2(1,1),
            new IntVec2(1,2)
        }
    };
    // 活动室，带一个大置物架
    public static readonly SettlementRoomType ActivityRoom = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(2, 3), 2),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(3, 4), 1),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(5, 3), 0),
            new (WRMC_ThingUtils.Table2x2, new Pair<int, int>(4, 1), 2),
            new (WRMC_ThingUtils.Table2x2, new Pair<int, int>(1, 4), 2),
            new (WRMC_ThingUtils.Surveillance, new Pair<int, int>(4, 5), 2),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(5, 5), 0),
            new (WRMC_ThingUtils.LongShelf, new Pair<int, int>(1, 2), 3),
        },
        lootPositions =
        {
            new IntVec2(1,1),
            new IntVec2(1,2)
        }
    };
    // 休息室，带两个大置物架
    public static readonly SettlementRoomType Lounge = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.Carpet2x3, new Pair<int, int>(3, 2), 2),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(2, 2), 3),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(2, 3), 3),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(4, 2), 1),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(4, 3), 1),
            new (WRMC_ThingUtils.Table1x2, new Pair<int, int>(3, 2), 1),
            new (WRMC_ThingUtils.NeonPlant, new Pair<int, int>(3, 2), 2),
            new (WRMC_ThingUtils.Surveillance, new Pair<int, int>(5, 4), 3),
            new (WRMC_ThingUtils.Bookcase, new Pair<int, int>(1, 1), 0),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(5, 1), 0),
            new (WRMC_ThingUtils.LongShelf, new Pair<int, int>(2, 5), 0),
            new (WRMC_ThingUtils.LongShelf, new Pair<int, int>(5, 5), 0),
        },
        lootPositions =
        {
            new IntVec2(1,5),
            new IntVec2(2,5),
            new IntVec2(4,5),
            new IntVec2(5,5)
        },
        limitedOption = SettlementRewardOption.Meal
    };
    // 会议室
    public static readonly SettlementRoomType ConferenceRoom = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.Carpet1x1, new Pair<int, int>(3, 1), 2),
            new (WRMC_ThingUtils.Table2x3, new Pair<int, int>(3, 4), 0),
            new (WRMC_ThingUtils.Bookcase, new Pair<int, int>(1, 5), 2),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(5, 5), 0),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(2, 5), 0), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(3, 5), 0), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(4, 5), 0), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(5, 3), 1), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(5, 4), 1), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(2, 2), 2), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(3, 2), 2), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(4, 2), 2), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(1, 3), 3), 
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(1, 4), 3), 
        }
    };
    // 机械加工室，带两个小置物架
    public static readonly SettlementRoomType MachiningRoom = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.MachiningTable, new Pair<int, int>(3, 4), 2),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(1, 1), 0),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(5, 1), 0),
            new (WRMC_ThingUtils.OfficeChair, new Pair<int, int>(3, 3), 2),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(2, 3), 0),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(4, 3), 0),
            new (WRMC_ThingUtils.Carpet2x3, new Pair<int, int>(3, 2), 2),
        },
        lootPositions =
        {
            new IntVec2(2,3),
            new IntVec2(4,3)
        },
        limitedOption = SettlementRewardOption.Resource
    };
    // 仓库A，6个置物架排成两排
    public static readonly SettlementRoomType StorageA = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(1, 5), 0),
            new (WRMC_ThingUtils.Wardrobe, new Pair<int, int>(2, 5), 2),
            new (WRMC_ThingUtils.Wardrobe, new Pair<int, int>(4, 5), 2),
            new (WRMC_ThingUtils.Surveillance, new Pair<int, int>(5, 5), 2),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(2, 2), 0),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(3, 2), 0),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(4, 2), 0),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(2, 3), 2),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(3, 3), 2),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(4, 3), 2),
        },
        lootPositions =
        {
            new IntVec2(2,2),
            new IntVec2(3,2),
            new IntVec2(4,2),
            new IntVec2(2,3),
            new IntVec2(3,3),
            new IntVec2(4,3)
        }
    };
    // 仓库B，8个置物架靠边摆放
    public static readonly SettlementRoomType StorageB = new SettlementRoomType()
    {
        role = SettlementRoomRole.Misc,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.WolfeinWall_Steel, new Pair<int, int>(1, 1), 0),
            new (WRMC_ThingUtils.WolfeinWall_Steel, new Pair<int, int>(1, 5), 0),
            new (WRMC_ThingUtils.WolfeinWall_Steel, new Pair<int, int>(5, 1), 0),
            new (WRMC_ThingUtils.WolfeinWall_Steel, new Pair<int, int>(5, 5), 0),
            new (WRMC_ThingUtils.Surveillance, new Pair<int, int>(2, 5), 2),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(3, 3), 0),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(2, 5), 0),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(4, 5), 0),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(5, 4), 1),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(5, 2), 1),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(2, 1), 2),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(4, 1), 2),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(1, 2), 3),
            new (WRMC_ThingUtils.ShortShelf, new Pair<int, int>(1, 4), 3),
        },
        lootPositions =
        {
            new IntVec2(2,5),
            new IntVec2(4,5),
            new IntVec2(5,4),
            new IntVec2(5,2),
            new IntVec2(2,1),
            new IntVec2(4,1),
            new IntVec2(1,2),
            new IntVec2(1,4)
        }
    };
    // 配电室A，带有一个发电机，一个大置物架和两个电池
    public static readonly SettlementRoomType GeneratorRoomA = new SettlementRoomType()
    {
        role = SettlementRoomRole.Generator,
        floor = null,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.FuelGenerator, new Pair<int, int>(4, 4), 0),
            new (WRMC_ThingUtils.Battery, new Pair<int, int>(1, 1), 2),
            new (WRMC_ThingUtils.Battery, new Pair<int, int>(1, 4), 2),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(2, 1), 0),
            new (WRMC_ThingUtils.LongShelf, new Pair<int, int>(4, 1), 2),
        },
        lootPositions =
        {
            new IntVec2(4,1),
            new IntVec2(5,1)
        },
        limitedOption = SettlementRewardOption.Component
    };
    // 配电室B，带有一个发电机，一个大置物架和一个电池
    public static readonly SettlementRoomType GeneratorRoomB = new SettlementRoomType()
    {
        role = SettlementRoomRole.Generator,
        floor = null,
        buildings = new List<(Func<Thing>, Pair<int, int>, int)>
        {
            new (WRMC_ThingUtils.FuelGenerator, new Pair<int, int>(2, 3), 0),
            new (WRMC_ThingUtils.Battery, new Pair<int, int>(1, 1), 3),
            new (WRMC_ThingUtils.FloorLamp, new Pair<int, int>(5, 1), 0),
            new (WRMC_ThingUtils.LongShelf, new Pair<int, int>(4, 4), 3),
        },
        lootPositions =
        {
            new IntVec2(4,3),
            new IntVec2(4,4)
        },
        limitedOption = SettlementRewardOption.Component
    };
    // 神圣fufu房，如果没有fufu模组则生成一个月球仪

}

