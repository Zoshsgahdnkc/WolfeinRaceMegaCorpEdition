using RimWorld;
using Verse;

namespace WRMegaCorp;

public class StatPart_SwitchModeRangedWeapon: StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!TryGetComp(req, out var comp, out var mode)) return;
        if (mode.newStat == null) return;

        if (parentStat == StatDefOf.RangedWeapon_Cooldown)
            val = mode.newStat?.Cooldown ?? val;
        else if (parentStat == StatDefOf.AccuracyTouch)
            val = mode.newStat?.AccuracyTouch ?? val;
        else if (parentStat == StatDefOf.AccuracyShort)
            val = mode.newStat?.AccuracyShort ?? val;
        else if (parentStat == StatDefOf.AccuracyMedium)
            val = mode.newStat?.AccuracyMedium ?? val;
        else if (parentStat == StatDefOf.AccuracyLong)
            val = mode.newStat?.AccuracyLong ?? val;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!TryGetComp(req, out var comp, out var mode)) return null;
        // if (parentStat == StatDefOf.RangedWeapon_Cooldown) return null;
        // if (parentStat == StatDefOf.RangedWeapon_Cooldown && mode.newStat?.Cooldown != null)
        //     return "WRMC.SwitchModeWeapon.StatExplanation".Translate(mode.newStat.Cooldown.ToString("F2"));
        // if (parentStat == StatDefOf.AccuracyTouch && mode.newStat?.AccuracyTouch != null)
        //     return "WRMC.SwitchModeWeapon.StatExplanation".Translate(mode.newStat.AccuracyTouch.ToStringPercent());
        // if (parentStat == StatDefOf.AccuracyShort && mode.newStat?.AccuracyShort != null)
        //     return "WRMC.SwitchModeWeapon.StatExplanation".Translate(mode.newStat.AccuracyShort.ToStringPercent());
        // if (parentStat == StatDefOf.AccuracyMedium && mode.newStat?.AccuracyMedium != null)
        //     return "WRMC.SwitchModeWeapon.StatExplanation".Translate(mode.newStat.AccuracyMedium.ToStringPercent());
        // if (parentStat == StatDefOf.AccuracyLong && mode.newStat?.AccuracyLong != null)
        //     return "WRMC.SwitchModeWeapon.StatExplanation".Translate(mode.newStat.AccuracyLong.ToStringPercent());
        return "WRMC.SwitchModeWeapon.StatCurrentMode".Translate(mode.label);
    }

    private bool TryGetComp(StatRequest req, out Comp_SwitchModeRangedWeapon comp, out RangedMode mode)
    {
        comp = null; mode = null;
        if (!req.HasThing) return false;
        if (req.Thing is not ThingWithComps twc) return false;
        comp = twc.GetComp<Comp_SwitchModeRangedWeapon>();
        if (comp == null || comp.CurrentMode == null) return false;
        mode = comp.CurrentMode;
        return true;
    }
}