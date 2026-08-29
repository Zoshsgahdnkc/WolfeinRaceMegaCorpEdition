using RimWorld;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    public class HediffComp_ChangeWithAugmentation: HediffComp
    {
        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (!this.Pawn.IsHashIntervalTick(60, delta)) return;
            if (WRMC_Utils.hasHeavyGearAugmentation(this.Pawn))
            {
                this.parent.Severity = 0.1f;
            }
            else
            {
                this.parent.Severity = 1f;
            }
        }

        public override bool CompDisallowVisible()
        {
            return this.parent.Severity < 0.2f;
        }
    }
}