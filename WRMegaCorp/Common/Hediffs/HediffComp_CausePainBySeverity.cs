using Verse;

namespace WRMegaCorp;

public class HediffComp_CausePainBySeverity: HediffComp
{
    public int ticksSinceHeal;
    public HediffCompProps_CausePainBySeverity Props => (HediffCompProps_CausePainBySeverity) props;
    
    public override void CompExposeData()
    {
        Scribe_Values.Look<int>(ref this.ticksSinceHeal, "ticksSinceHeal");
    }
    
    public override void CompPostTick(ref float severityAdjustment)
    {
        ++ticksSinceHeal;
        if (ticksSinceHeal < Props.updateInterval) return;
        MakePain(severityAdjustment);
        ticksSinceHeal = 0;
    }

    private void MakePain(float severity)
    {
        float pain = Props.SeverityToPainCurve.Evaluate(severity);
        parent.CurStage.painOffset = pain;
    }
}