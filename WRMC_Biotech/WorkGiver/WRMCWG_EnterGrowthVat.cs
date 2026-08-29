using RimWorld;
using Verse;

namespace WRMegaCorp
{
    public class WRMCWG_EnterGrowthVat: WorkGiver_EnterBuilding
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get => ThingRequest.ForDef(WRMC_Biotech_DefOfs.WRMC_WolfeinGrowthVat);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false) => !ModsConfig.BiotechActive;
    }
}