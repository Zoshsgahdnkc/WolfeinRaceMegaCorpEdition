using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public class WorldObject_ReconstructionSite: WorldObject
{
    public string StoredLabel;
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref StoredLabel, "StoredLabel");
    }
}

public class TimeoutAndTurnMegaCorpComp: TimeoutComp
{
    private bool ShouldRemoveWorldObjectNow => this.Passed && !this.ParentHasMap;
    public override void CompTickInterval(int delta)
    {
        PlanetTile tile = parent.Tile;
        string storedLabel = null;
        if (parent is WorldObject_ReconstructionSite site)
        {
            storedLabel = site.StoredLabel;
        }
        base.CompTickInterval(delta);
        if (!this.ShouldRemoveWorldObjectNow)
            return;
        Faction faction = Find.FactionManager.OfMegaCorp();
        var toSpawn = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
        toSpawn.SetFaction(faction);
        toSpawn.Tile = tile;
        if (toSpawn is Settlement set && storedLabel != null)
        {
            set.Name = storedLabel;
        }
        var let = LetterMaker.MakeLetter(
            "WRMC.SettlementConstructed.Letter".Translate(), "WRMC.SettlementConstructed.Text".Translate(storedLabel),
            LetterDefOf.PositiveEvent, parent, faction);
        Find.LetterStack.ReceiveLetter(let);
        Find.WorldObjects.Add(toSpawn);
    }
}

public class WorldObjectCompProperties_TurnMegaCorpOnTimeout: WorldObjectCompProperties
{
    public WorldObjectCompProperties_TurnMegaCorpOnTimeout() => this.compClass = typeof(TimeoutAndTurnMegaCorpComp);
}