using System.Collections.Generic;
using Verse;

namespace WRMegaCorp;

public class CompProps_SwitchModeRangedWeapon: CompProperties
{
    public List<RangedMode> rangedModes;
    
    public CompProps_SwitchModeRangedWeapon() => this.compClass = typeof(Comp_SwitchModeRangedWeapon);
}

public class RangedMode
{
    [MustTranslate]
    public string label = "";
    [MustTranslate]
    public string desc = "";

    public VerbProperties verb;
    public RangedModeNewStat newStat;
    
    public string iconPath;
}

public class RangedModeNewStat
{
    public float Cooldown;
    public float AccuracyTouch;
    public float AccuracyShort;
    public float AccuracyMedium;
    public float AccuracyLong;
}