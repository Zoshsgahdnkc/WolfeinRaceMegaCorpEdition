using System.Drawing;
using Color = UnityEngine.Color;

namespace WRMegaCorp;

using RimWorld;
using Verse;

public class CompProps_ShowRadius : CompProperties_AbilityEffect
{
    public float radius;
    public CompProps_ShowRadius()
    {
        compClass = typeof(Comp_ShowRadius);
    }
}

public class Comp_ShowRadius : CompAbilityEffect
{
    public new CompProps_ShowRadius Props
        => (CompProps_ShowRadius)props;

    public override void DrawEffectPreview(LocalTargetInfo target)
    {
        if (target.IsValid)
            GenDraw.DrawRadiusRing(target.Cell, Props.radius, Color.cyan);
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        // 故意什么都不做：只负责预览
    }
}