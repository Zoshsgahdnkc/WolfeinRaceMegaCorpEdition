using RimWorld;
using RimWorld.SketchGen;
using Verse;

namespace WRMegaCorp;

public class LayoutWorker_RuinedOutpost(LayoutDef def): LayoutWorker_Structure(def)
{

    protected override StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect)
    {
        return RoomLayoutGenerator.GenerateRandomLayout(
            parms.sketch,
            rect,
            Def.minRoomWidth,
            Def.minRoomHeight,
            Def.areaPrunePercent,
            false,
            true,
            Def.corridorDef,
            new IntRange(2,3).RandomInRange,
            maxMergeRoomsRange: IntRange.One,
            canDisconnectRooms: Def.canDisconnectRooms);
    }
    
    public LayoutStructureSketch GenerateStructureSketch(StructureGenParams parms)
    {
        var toReturn = base.GenerateStructureSketch(parms);
        SketchResolveParams parms1 = new SketchResolveParams()
        {
            sketch = (Sketch) parms.sketch.layoutSketch,
            destroyChanceExp = new float?(1.5f)
        };
        SketchResolverDefOf.DamageBuildings.Resolve(parms1);
        return toReturn;
    }
}