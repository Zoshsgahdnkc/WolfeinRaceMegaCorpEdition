using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace WRMegaCorp
{
    public class QuestNode_GetRequestedMeal: QuestNode
    {
  private static readonly SimpleCurve CountWantedFactorFromWealthCurve = new SimpleCurve()
  {
    {
      new CurvePoint(0.0f, 15f),
      true
    },
    {
      new CurvePoint(50000f, 100f),
      true
    },
    {
      new CurvePoint(300000f, 400f),
      true
    }
  };
  private static Dictionary<ThingDef, int> requestCountDict = new Dictionary<ThingDef, int>();
  [NoTranslate]
  public SlateRef<string> storeThingAs;
  [NoTranslate]
  public SlateRef<string> storeThingCountAs;
  [NoTranslate]
  public SlateRef<string> storeMarketValueAs;

  // 需求数量为0.8~1.2乘以需求曲线
  private static int RandomRequestCount(Map map)
  {
    int count = ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(Rand.Range(0.8f, 1.2f) * CountWantedFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal))));
    count *= (int) Math.Round(WRMegaCorpEdition.modSettings?.QMeatOrder_RequestAmountMul ?? 1f);
    return count;
  }

  // 完全重写了方法体，名字不具有参考价值
  private static bool TryFindRandomRequestedThingDef(
    Map map,
    out ThingDef thingDef,
    out int count)
  {
    requestCountDict.Clear();
    thingDef = ThingDef.Named("Wolfein_GrilledSteak");
    count = RandomRequestCount(map);
    requestCountDict.Add(thingDef, count);
    return true;
  }

  protected override void RunInt()
  {
    Slate slate = RimWorld.QuestGen.QuestGen.slate;
    ThingDef thingDef;
    int count;
    if (!TryFindRandomRequestedThingDef(slate.Get<Map>("map"), out thingDef, out count))
      return;
    slate.Set<int>(this.storeThingCountAs.GetValue(slate), count);
    slate.Set<float>(this.storeMarketValueAs.GetValue(slate), thingDef.BaseMarketValue * (float) count);
    slate.Set(storeThingAs.GetValue(slate), thingDef);
    slate.Set<float>("questRewardMultiplier", WRMegaCorpEdition.modSettings?.QMeatOrder_RewardMul ?? 1f);
  }

  protected override bool TestRunInt(Slate slate)
  {
    ThingDef thingDef;
    int count;
    if (!QuestNode_GetRequestedMeal.TryFindRandomRequestedThingDef(slate.Get<Map>("map"), out thingDef, out count))
      return false;
    slate.Set<int>(this.storeThingCountAs.GetValue(slate), count);
    slate.Set<float>(this.storeMarketValueAs.GetValue(slate), thingDef.BaseMarketValue * (float) count);
    slate.Set(storeThingAs.GetValue(slate), thingDef);
    return true;
  }
}
}