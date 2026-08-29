using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WRMegaCorp
{
    public class CaravanArrivalAction_ADay: CaravanArrivalAction
    {
        private WorldObject_ArmisticeDay armisticeDay;

        private CaravanArrivalAction_ADay(WorldObject_ArmisticeDay armisticeDay)
        {
            this.armisticeDay = armisticeDay;
        }
        
        public override void Arrived(Caravan caravan)
        {
            this.armisticeDay.Notify_CaravanArrived(caravan);
        }

        public override string Label
        {
            get => (string) "VisitArmisticeDay".Translate((NamedArgument) this.armisticeDay.Label);
        }

        public override string ReportString
        {
            get => (string) "CaravanVisiting".Translate((NamedArgument) this.armisticeDay.Label);
        }
        
        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
        {
            FloatMenuAcceptanceReport acceptanceReport = base.StillValid(caravan, destinationTile);
            if (!(bool) acceptanceReport)
                return acceptanceReport;
            return this.armisticeDay != null && this.armisticeDay.Tile != destinationTile ? (FloatMenuAcceptanceReport) false : CanVisit(caravan, this.armisticeDay);
        }
        
        public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, WorldObject_ArmisticeDay armisticeDay)
        {
            return (armisticeDay != null && armisticeDay.Spawned);
        }
        
        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(
            Caravan caravan,
            WorldObject_ArmisticeDay armisticeDay)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions((() => CanVisit(caravan, armisticeDay)),  (() => new CaravanArrivalAction_ADay(armisticeDay)), (string) "VisitPeaceTalks".Translate((NamedArgument) armisticeDay.Label), caravan, armisticeDay.Tile, (WorldObject) armisticeDay);
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<WorldObject_ArmisticeDay>(ref this.armisticeDay, "armisticeDay");
        }
    }
}