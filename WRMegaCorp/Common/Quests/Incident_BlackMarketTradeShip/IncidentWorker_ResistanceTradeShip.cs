using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Wolfein;

namespace WRMegaCorp;

public class IncidentWorker_BlackMarketTradeShip: IncidentWorker
// 照抄OrbitalTraderArrival代码
{
  protected override bool TryExecuteWorker(IncidentParms parms)
  {
    if (WRMegaCorpEdition.modSettings?.IBlackMarketTradeShip_Activated != true) return false;
    Map map = (Map) parms.target;
    if (!CanSpawn(map, WRMC_DefOfs.WRMC_BlackMarketOrbitalTrader)) return false;
    Faction faction = generateFaction();
    TradeShip vis = new TradeShip(WRMC_DefOfs.WRMC_BlackMarketOrbitalTrader, faction);
    vis.WasAnnounced = false;
    if (map.listerBuildings.allBuildingsColonist.Any<Building>((Predicate<Building>) (b =>
    {
      if (!b.def.IsCommsConsole)
        return false;
      return b.GetComp<CompPowerTrader>() == null || b.GetComp<CompPowerTrader>().PowerOn;
    })))
    {
      SendStandardLetter("WRMC.ResistanceTradeShip.Letter".Translate(), "WRMC.ResistanceTradeShip.Text".Translate(), LetterDefOf.PositiveEvent, parms, LookTargets.Invalid);
      vis.WasAnnounced = true;
    }
    map.passingShipManager.AddShip((PassingShip) vis);
    vis.GenerateThings();
    return true;
  }
    
  public Faction generateFaction()
  {
    // 创建临时派系的部分，抄原版代码
    List<FactionRelation> relations = new List<FactionRelation>();
    foreach (Faction faction1 in Find.FactionManager.AllFactionsListForReading)
    {
      if (!faction1.def.PermanentlyHostileTo(WolfeinDefOf.Wolfein_OutlanderRefugee))
        relations.Add(new FactionRelation()
        {
          other = faction1,
          kind = FactionRelationKind.Neutral
        });
    }
    Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(WolfeinDefOf.Wolfein_OutlanderRefugee, relations, true);
    faction.temporary = true;
    Find.FactionManager.Add(faction);
    return faction;
  }
    
  private bool CanSpawn(Map map, TraderKindDef trader)
  {
    foreach (Pawn freeColonist in map.mapPawns.FreeColonists)
    {
      if (freeColonist.skills != null && !freeColonist.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        return true;
    }
    return false;
  }
}
