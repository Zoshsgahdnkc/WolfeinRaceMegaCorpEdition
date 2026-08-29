using RimWorld;
using Verse;

namespace WRMegaCorp
{
    [DefOf]
    [StaticConstructorOnStartup]
    public static class WRMC_Biotech_DefOfs
    {
        public static EffecterDef WRMC_Vat_Bubbles_South;
        public static EffecterDef WRMC_Vat_Bubbles_North;
        public static EffecterDef WRMC_Vat_Bubbles_East;
        public static EffecterDef WRMC_Vat_Bubbles_West;
        public static ThingDef WRMC_WolfeinGrowthVat;
        
        
        static WRMC_Biotech_DefOfs() => DefOfHelper.EnsureInitializedInCtor(typeof (WRMC_Biotech_DefOfs));
    }
}