using Verse;

namespace WRMegaCorp;

public class HediffCompProps_CausePainBySeverity: HediffCompProperties
{
    public SimpleCurve SeverityToPainCurve;
    public int updateInterval = 60;

    public HediffCompProps_CausePainBySeverity()
    {
        this.compClass = typeof(HediffComp_CausePainBySeverity);
    }

}