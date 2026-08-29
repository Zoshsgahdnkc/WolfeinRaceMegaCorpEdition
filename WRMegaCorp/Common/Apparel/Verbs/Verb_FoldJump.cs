using RimWorld;
using Verse;

namespace WRMegaCorp
{
    public class Verb_FoldJump: Verb_Jump
    {
        
        protected override bool TryCastShot()
        {
            //消耗使用次数
            var comp = this.ReloadableCompSource;
            if (comp != null && !comp.CanBeUsed(out string _))
                return false;
            comp?.UsedOnce();
            // 噪音
            GenClamor.DoClamor(CasterPawn, CasterPawn.Position, 10f, ClamorDefOf.Ability);
            // 绘制特效
            this.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(CasterPawn, CasterPawn.Map), CasterPawn.Position, 60);
            this.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(currentTarget.Cell, CasterPawn.Map), currentTarget.Cell, 60);
            // 传送和传送后处理
            CasterPawn.Position = currentTarget.Cell;
            if ((CasterPawn.Faction == Faction.OfPlayer || CasterPawn.IsPlayerControlled) && CasterPawn.Position.Fogged(CasterPawn.Map))
                FloodFillerFog.FloodUnfog(CasterPawn.Position, CasterPawn.Map);
            CasterPawn.stances.stunner.StunFor(3, CasterPawn, false, false);
            CasterPawn.Notify_Teleported();
            GenClamor.DoClamor(CasterPawn, currentTarget.Cell, 10f, ClamorDefOf.Ability);
            return true;
        }
    }
}