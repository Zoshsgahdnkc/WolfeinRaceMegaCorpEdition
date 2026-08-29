using RimWorld;
using Verse;

namespace WRMegaCorp;

public class RoomContents_Gap: RoomContentsWorker
{
    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
    {
        if (this.RoomDef == null)
            return;
        foreach (CellRect rect in room.rects)
        {
            foreach (IntVec3 edgeCell in rect.EdgeCells)
            {
                if (this.CanRemoveWall(edgeCell, map, room))
                {
                    bool flag = true;
                    for (int index = 0; index < 4; ++index)
                    {
                        if ((edgeCell + GenAdj.CardinalDirections[index]).GetDoor(map) != null)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        foreach (LayoutRoom room1 in room.sketch.structureLayout.Rooms)
                        {
                            if (!CanRemoveWalls(room1) && room1.Contains(edgeCell))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            Building building = map.edificeGrid[edgeCell];
                            if (building != null && building.def.IsWall)
                            {
                                building.Destroy(DestroyMode.Vanish);
                                map.roofGrid.SetRoof(edgeCell, (RoofDef) null);
                            }
                        }
                    }
                }
            }
        }
    }
    
    private static bool CanRemoveWalls(LayoutRoom room)
    {
        foreach (LayoutRoomDef def in room.defs)
        {
            if (!def.canRemoveBorderWalls)
                return false;
        }
        return true;
    }
}