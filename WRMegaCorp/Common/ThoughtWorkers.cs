using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace WRMegaCorp
{
    public class ThoughtWorker_ConeCollar: ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned || p.Dead || p.IsMutant) return ThoughtState.Inactive;
            try
            {
                foreach (var apparel in p.apparel.WornApparel)
                {
                    if (apparel.def.Equals(WRMC_DefOfs.WRMC_ConeCollar))
                        return ThoughtState.ActiveAtStage(0);
                }
            }
            catch (Exception e)
            {
                WRMC_Utils.LogDebugMessage("Warning Detecting Cone Collar");
                WRMC_Utils.LogDebugMessage(e.ToString());
            }
            return ThoughtState.Inactive;
        }
    }
    
    // 修改ThoughtWorker，使其加上仅对沃芬族生效的判定
    public class ThoughtWorker_MoodOptimizer: ThoughtWorker_Hediff
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(this.def.hediff);
            if (firstHediffOfDef?.def.stages == null || p.def.defName != "Wolfein_Race")
                return ThoughtState.Inactive;
            return ThoughtState.ActiveAtStage(Mathf.Min(firstHediffOfDef.CurStageIndex, firstHediffOfDef.def.stages.Count - 1, this.def.stages.Count - 1));
        }
    }
}