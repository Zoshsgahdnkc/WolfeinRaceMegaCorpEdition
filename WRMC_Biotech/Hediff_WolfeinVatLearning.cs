using System;
using System.Linq;
using RimWorld;
using Verse;

namespace WRMegaCorp
{
    public class Hediff_WolfeinVatLearning: HediffWithComps
    {
        private const float XPToAward = 8000f;

        public override bool ShouldRemove
        {
            get => this.pawn.Spawned || !(this.pawn.ParentHolder is WolfeinGrowthVat);
        }

        public override string LabelInBrackets => this.Severity.ToStringPercent();

        public override void PostTickInterval(int delta)
        {
            base.PostTickInterval(delta);
            if ((double) this.Severity < (double) this.def.maxSeverity)
                return;
            this.Learn();
        }

        public void Learn()
        {
            SkillRecord skillToDevelop;
            if (this.pawn.skills != null)
            {
                if (Rand.Chance(0.1f))
                {
                    pawn.skills.skills
                        .Where<SkillRecord>((Func<SkillRecord, bool>)(x => (!x.TotallyDisabled) && (x.passion != Passion.Major)))
                        .TryRandomElementByWeight(s => 1/((byte) s.passion + 0.5f), out skillToDevelop);
                    if (skillToDevelop == null) return;
                    skillToDevelop.passion = skillToDevelop.passion.IncrementPassion();
                    skillToDevelop.Learn(XPToAward, true);
                    return;
                }
                this.pawn.skills.skills
                    .Where<SkillRecord>((Func<SkillRecord, bool>)(x => !x.TotallyDisabled))
                    .TryRandomElement<SkillRecord>(out skillToDevelop);
                skillToDevelop.Learn(XPToAward, true);
            }
            this.Severity = this.def.initialSeverity;
        }
    }
}