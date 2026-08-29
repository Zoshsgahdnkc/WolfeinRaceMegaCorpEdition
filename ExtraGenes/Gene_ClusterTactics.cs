using RimWorld;
using Verse;

namespace WRExtraGenes
{
    public class Gene_ClusterTactics : Gene
    
    {
        // 检测的时间间隔，降低开销
        public int intervalTicks = 45;

        // 检测参数p是否是基因携带者的友军
        private bool isAlly(Pawn other)
        {
            // 需要符合：p非自身，存活，未倒地，有派系且派系为自身派系或者友好
            if (other == null || pawn == null) return false;
            if (other == pawn) return false;
            if (other.health == null || other.Dead || other.Downed) return false;
            if (other.Faction == null || pawn.Faction == null) return false;
            return other.Faction == pawn.Faction || other.Faction.RelationKindWith(pawn.Faction) == FactionRelationKind.Ally;
        }

        // 实现功能
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            
            if (!Active || pawn == null || !pawn.Spawned) return;
            if (!pawn.IsHashIntervalTick(45, delta)) return;
            
            bool hasAlly = false;
            
            // 遍历角色周围格子，对其上的pawn应用友军判断
            foreach (var pos in GenAdj.CellsAdjacent8Way(pawn))
            {
                if (!pos.InBounds(pawn.Map)) continue;

                var other = pos.GetFirstPawn(pawn.Map);
                if (isAlly(other))
                {
                    hasAlly = true;
                    break;
                }
            }
            
            // 应用Hediff
            var existing = pawn.health?.hediffSet?.GetFirstHediffOfDef(WREG_DefOfs.WREG_ClusterTacticsEffect);
            if (hasAlly)
            {
                if (existing == null) pawn.health.AddHediff(WREG_DefOfs.WREG_ClusterTacticsEffect);
            }
            // else
            // {
            //     if (existing != null) pawn.health.RemoveHediff(existing);
            // }
        }
    }
}