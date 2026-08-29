using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    public class QuestNode_GetMegaCorpSettlement: QuestNode_GetNearbySettlement
    {
      
      public static readonly int MaxTileDistance = 25;

      private Settlement RandomNearbyMegaCorpSettlement(PlanetTile originTile, Slate slate)
      {
        var settlements = Find.WorldObjects.SettlementBases.Where(Validator);
        return settlements == null ? null: settlements.RandomElement();

        bool Validator(Settlement settlement)
        {
          if (!settlement.Visitable || !this.canBeSpace.GetValue(slate) && settlement.Tile.LayerDef.isSpace)
          {
            return false;
          }
          // 加入一条判断，检测是否为军企派系
          if (settlement.Faction.def != WolfeinDefOf.Wolfein_Faction_MegaCorp)
          {
            return false;
          }

          List<PlanetLayerDef> list1 = this.layerWhitelist.GetValue(slate);
          List<PlanetLayerDef> list2 = this.layerBlacklist.GetValue(slate);
          PlanetTile tile;
          if (!list1.NullOrEmpty<PlanetLayerDef>() && settlement.Tile.Valid)
          {
            List<PlanetLayerDef> planetLayerDefList = list1;
            tile = settlement.Tile;
            PlanetLayerDef layerDef = tile.LayerDef;
            if (!planetLayerDefList.Contains(layerDef))
              return false;
          }
          if (!list2.NullOrEmpty<PlanetLayerDef>())
          {
            tile = settlement.Tile;
            if (tile.Valid)
            {
              List<PlanetLayerDef> planetLayerDefList = list2;
              tile = settlement.Tile;
              PlanetLayerDef layerDef = tile.LayerDef;
              if (planetLayerDefList.Contains(layerDef))
                return false;
            }
          }
          if (this.requireSameOrAdjacentLayer.GetValue(slate))
          {
            tile = settlement.Tile;
            if (tile.Valid && originTile.Valid)
            {
              tile = settlement.Tile;
              if (tile.Layer != originTile.Layer)
              {
                tile = settlement.Tile;
                if (!tile.Layer.DirectConnectionTo(originTile.Layer))
                  return false;
              }
            }
          }
          if (!this.allowActiveTradeRequest.GetValue(slate))
          {
            if (settlement.GetComponent<TradeRequestComp>() != null &&
                settlement.GetComponent<TradeRequestComp>().ActiveRequest)
              return false;
            List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
            for (int index1 = 0; index1 < questsListForReading.Count; ++index1)
            {
              if (!questsListForReading[index1].Historical)
              {
                List<QuestPart> partsListForReading = questsListForReading[index1].PartsListForReading;
                for (int index2 = 0; index2 < partsListForReading.Count; ++index2)
                {
                  if (partsListForReading[index2] is QuestPart_InitiateTradeRequest initiateTradeRequest &&
                      initiateTradeRequest.settlement == settlement)
                    return false;
                }
              }
            }
          }

          int maxDistance = MaxTileDistance;
          // if (GravshipUtility.PlayerHasGravEngine()) maxDistance = (int) (maxDistance * 1.3f);
          if (hasShuttle()) maxDistance = (int) (maxDistance * 1.6f);
          bool canReach = Find.WorldReachability.CanReach(originTile, settlement.Tile);
          var distance = Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile);
          return canReach && distance < maxDistance;
        }
      }

      private bool hasShuttle()
      {
        Map map = QuestGen.slate.Get<Map>("map");
        if (map == null)
          return false;
        var shuttles = map.listerBuildings.AllBuildingsColonistOfClass<Building_PassengerShuttle>();
        shuttles.Where(shuttle => shuttle.Faction == Find.FactionManager.OfPlayer);
        return !shuttles.EnumerableNullOrEmpty();
      }

      protected override void RunInt()
      {
        Slate slate = RimWorld.QuestGen.QuestGen.slate;
        Map map = RimWorld.QuestGen.QuestGen.slate.Get<Map>("map");
        Settlement var1 = this.RandomNearbyMegaCorpSettlement(map.Tile, slate);
        RimWorld.QuestGen.QuestGen.slate.Set<Settlement>(this.storeAs.GetValue(slate), var1);
        if (!string.IsNullOrEmpty(this.storeFactionAs.GetValue(slate)))
          RimWorld.QuestGen.QuestGen.slate.Set<Faction>(this.storeFactionAs.GetValue(slate), var1.Faction);
        if (!this.storeFactionLeaderAs.GetValue(slate).NullOrEmpty())
          RimWorld.QuestGen.QuestGen.slate.Set<Pawn>(this.storeFactionLeaderAs.GetValue(slate), var1.Faction.leader);
        if (this.storeCanCaravanAs.GetValue(slate).NullOrEmpty())
          return;
        PlanetTile tile = var1.Tile;
        int num;
        if (tile.Valid)
        {
          tile = map.Tile;
          if (tile.Valid)
          {
            tile = var1.Tile;
            PlanetLayer layer1 = tile.Layer;
            tile = map.Tile;
            PlanetLayer layer2 = tile.Layer;
            if (layer1 == layer2)
            {
              tile = var1.Tile;
              num = tile.LayerDef.SurfaceTiles ? 1 : 0;
              goto label_10;
            }
          }
        }
        num = 0;
        label_10:
        bool var2 = num != 0;
        RimWorld.QuestGen.QuestGen.slate.Set<bool>(this.storeCanCaravanAs.GetValue(slate), var2);
      }
      protected override bool TestRunInt(Slate slate)
      {
        Map map = slate.Get<Map>("map");
        if (map == null)
          return false;
        Settlement var = this.RandomNearbyMegaCorpSettlement(map.Tile, slate);
        if (var == null)
        {
          Log.Message("[WRMegaCorp Debug]: Tried to generate grilled meat mission but no nearby settlement found.");
          return false;
        }
        slate.Set<Settlement>(this.storeAs.GetValue(slate), var);
        if (!string.IsNullOrEmpty(this.storeFactionAs.GetValue(slate)))
          slate.Set<Faction>(this.storeFactionAs.GetValue(slate), var.Faction);
        if (!string.IsNullOrEmpty(this.storeFactionLeaderAs.GetValue(slate)))
          slate.Set<Pawn>(this.storeFactionLeaderAs.GetValue(slate), var.Faction.leader);
        return true;
      }
    }
    
}