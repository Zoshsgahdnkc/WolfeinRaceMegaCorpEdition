using AncotLibrary;
using Verse;

namespace WRMegaCorp;

public class Projectile_AblationBeam: Beam
{
    public const float severityOffset = 0.1f;
    
    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        if (hitThing is Pawn pawn)
        {
            pawn.stances?.stunner?.StunFor(100, this.WeaponOwner());
            if (pawn.health != null && pawn.RaceProps.IsFlesh)
            {
                if (pawn.health.hediffSet.HasHediff(WRMC_DefOfs.WRMC_AblationBeamHediff))
                {
                    var hd = pawn.health.hediffSet.GetFirstHediffOfDef(WRMC_DefOfs.WRMC_AblationBeamHediff);
                    if (hd.Severity + severityOffset <= hd.def.maxSeverity) hd.Severity += severityOffset;
                    else hd.Severity = hd.def.maxSeverity;
                }
                else
                {
                    pawn.health.AddHediff(WRMC_DefOfs.WRMC_AblationBeamHediff);
                }
            }
        }
        base.Impact(hitThing, blockedByShield);
    }
}