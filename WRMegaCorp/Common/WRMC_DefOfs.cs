using RimWorld;
using Verse;

namespace WRMegaCorp
{
    [DefOf]
    [StaticConstructorOnStartup]
    public static class WRMC_DefOfs
    {
        // 试制型重甲Hediff
        public static HediffDef WRMC_HeavyGearHediff;
        // 试制心脏Hediff
        public static HediffDef WRMC_PrototypeBionicHeart_Hediff;
        // 烧蚀激光Hediff
        public static HediffDef WRMC_AblationBeamHediff;
        // 伊丽莎白圈衣物
        public static ThingDef WRMC_ConeCollar;
        // 试制型心脏物品
        public static ThingDef WRMC_PrototypeBionicHeart_Thing;
        // 参与停战日
        public static ThoughtDef WRMC_AttendADayMood;
        // 捐赠小额
        public static HistoryEventDef WRMC_DonateSmallHistory;
        // 捐赠大额
        public static HistoryEventDef WRMC_DonateBigHistory;
        // 参与停战日
        public static HistoryEventDef WRMC_AttendArmisticeDay;
        // 沃芬军企地面据点生成器
        public static MapGeneratorDef WRMC_Faction;
        // 沃芬军企地面据点名字规则集
        public static RulePackDef WRMC_OutpostName;
        // 清算任务生成SitePart
        public static SitePartDef WRMC_RogueSettlement;
        // 矿脉开发生成SitePart
        public static SitePartDef WRMC_OverrunMine;
        // 救援行动生成SitePart
        public static SitePartDef WRMC_RuinedOutpost;
        // 清算任务后生成的待建据点WorldObject
        public static WorldObjectDef WRMC_ReconstructionSite;
        // 矿脉开采任务后生成的采矿据点WorldObject
        public static WorldObjectDef WRMC_MiningOutpost;
        // 债务奴隶
        public static PawnKindDef Wolfein_DebtSlave;
        // 抵抗军轨道商
        public static TraderKindDef WRMC_BlackMarketOrbitalTrader;
        // 可切换武器Stat分类
        public static StatCategoryDef WRMC_Weapon_SwitchMode;
        // 龙息弹Damage，和原版flame一模一样，只是为了避免引火
        public static DamageDef WRMC_NonIgnitionFlame;
        
        
        static WRMC_DefOfs() => DefOfHelper.EnsureInitializedInCtor(typeof (WRMC_DefOfs));
    }
}