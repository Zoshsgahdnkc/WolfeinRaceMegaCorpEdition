using RimWorld;
using Verse;

namespace MCMigrationPatch
{
    [DefOf]
    [StaticConstructorOnStartup]
    public static class WRMC_DefOfs
    {
        // 集群战术Hediff
        [MayRequire("ancot.wolfeinracegenepatch")]
        public static HediffDef WRMC_ClusterTacticsEffect;
        // 天才艺术家基因
        [MayRequire("ancot.wolfeinracegenepatch")]
        public static GeneDef WRMC_GeniusArtist;
        // 艺术技能学习系数
        public static StatDef WRMC_ArtisticLearningFactor;
        // 艺术品品质偏移
        public static StatDef WRMC_ArtisticQualityOffset;
        // 灵感触发间隔乘数
        public static StatDef WRMC_InspirationMTBFactor;
        
        static WRMC_DefOfs() => DefOfHelper.EnsureInitializedInCtor(typeof (WRMC_DefOfs));
    }
}