using RimWorld;
using Verse;

namespace WRExtraGenes
{
    [DefOf]
    [StaticConstructorOnStartup]
    public static class WREG_DefOfs
    {
        // 集群战术Hediff
        [MayRequire("ancot.wolfeinracegenepatch")]
        public static HediffDef WREG_ClusterTacticsEffect;
        // 天才艺术家基因
        [MayRequire("ancot.wolfeinracegenepatch")]
        public static GeneDef WREG_GeniusArtist;
        // 艺术技能学习系数
        public static StatDef WREG_ArtisticLearningFactor;
        // 艺术品品质偏移
        public static StatDef WREG_ArtisticQualityOffset;
        // 灵感触发间隔乘数
        public static StatDef WREG_InspirationMTBFactor;
        
        static WREG_DefOfs() => DefOfHelper.EnsureInitializedInCtor(typeof (WREG_DefOfs));
    }
}