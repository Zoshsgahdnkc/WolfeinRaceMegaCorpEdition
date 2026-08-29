using RimWorld;
using Verse;

namespace WRMegaCorp;

// 快速获取ThingDef和Thing
public static class WRMC_ThingUtils
{
    public static TerrainDef CeramicTileTerrain()
    {
        return TerrainDef.Named("Wolfein_Floor_CeramicTile");
    }
    
    public static TerrainDef CarpetTerrain()
    {
        return TerrainDef.Named("Wolfein_Floor_CarpetTile");
    }

    public static ThingDef Alloy()
    {
        return ThingDef.Named("Wolfein_AlloyMaterials");
    }
    
    public static ThingDef Fabric()
    {
        return ThingDef.Named("Wolfein_CompositeFabric");
    }
    
    public static ThingDef Medicine()
    {
        return ThingDef.Named("Wolfein_MedicineIndustrial");
    }
    
    public static ThingDef ComponentMech()
    {
        return ThingDef.Named("Wolfein_MechanicalComponent");
    }
    
    public static ThingDef ComponentElectro()
    {
        return ThingDef.Named("Wolfein_ElectronicComponent");
    }
    
    public static ThingDef WolfeinDoor()
    {
        return ThingDef.Named("Wolfein_ReinforcedDoor");
    }
    
    public static ThingDef WolfeinWall()
    {
        return ThingDef.Named("Wolfein_DefenseWall");
    }
    
    public static Thing WolfeinWall_Steel()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_DefenseWall"), ThingDefOf.Steel);
    }
    
    public static ThingDef SmallRifleTurret()
    {
        return ThingDef.Named("Wolfein_Turret_A");
    }
    
    public static ThingDef SmallMachineGunTurret()
    {
        return ThingDef.Named("Wolfein_Turret_B");
    }
    
    public static ThingDef PackageMeal()
    {
        return ThingDef.Named("Wolfein_PackingStaffMeals");
    }
    
    public static ThingDef MeatBites()
    {
        return ThingDef.Named("Wolfein_MeatBites");
    }
    
    public static ThingDef Monster()
    {
        return ThingDef.Named("Wolfein_MoodStabilizerDrink");
    }
    
    public static ThingDef CustardCake()
    {
        return ThingDef.Named("Wolfein_CustardCake");
    }
    
    public static Thing AutoDoor_Steel()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Autodoor"), ThingDefOf.Steel);
    }

    public static Thing SingleBed()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_SingleBed"));
    }
    public static Thing DoubleBed()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_DoubleBed"));
    }
    
    public static Thing Wardrobe()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_Wardrobe"));
    }
    
    public static Thing Bookcase()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_Bookcase"));
    }
    
    public static Thing BedsideTable()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_BedsideTable"));
    }
    
    public static Thing LowCabinet()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_LowCabinet"));
    }
    
    public static Thing LongShelf()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_1x2_Shelf"));
    }
    
    public static Thing ShortShelf()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_1x1_Shelf"));
    }
    
    public static Thing Model()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_Model"));
    }
    
    public static Thing NeonPlant()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_NeonPlanter"));
    }
    
    public static Thing LunarGlobe()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_LunarGlobe"));
    }
    
    public static Thing Carpet1x1()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_Doormat"), ThingDefOf.Cloth);
    }
    
    public static Thing Carpet2x3()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_2x3_Carpet"), ThingDefOf.Cloth);
    }
    
    public static Thing Carpet3x3()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_3x3_Carpet"), ThingDefOf.Cloth);
    }
    
    public static Thing OfficeDesk()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_OfficeDesk"));
    }
    
    public static Thing OfficeChair()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_OfficeChair"));
    }
    
    public static Thing Surveillance()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_SurveillanceCamera"));
    }
    
    public static Thing FloorLamp()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_FloorLamp"));
    }
    
    public static Thing Table1x2()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_1x2_Table"));
    }
    
    public static Thing Table2x2()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_2x2_Table"));
    }
    
    public static Thing Table2x3()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_2x3_Table"));
    }
    
    public static Thing MachiningTable()
    {
        return ThingMaker.MakeThing(ThingDef.Named("Wolfein_TableMachining"), ThingDefOf.Steel);
    }
    
    public static Thing FuelGenerator()
    {
        var thing = ThingMaker.MakeThing(ThingDefOf.ChemfuelPoweredGenerator);
        thing.TryGetComp<CompRefuelable>(out var comp);
        comp.Refuel(comp.Props.fuelCapacity);
        return thing;
    }

    public static Thing Battery()
    {
        var thing = ThingMaker.MakeThing(ThingDefOf.Battery);
        thing.TryGetComp<CompPowerBattery>(out var comp);
        comp.AddEnergy(comp.AmountCanAccept);
        return thing;
    }
    
}