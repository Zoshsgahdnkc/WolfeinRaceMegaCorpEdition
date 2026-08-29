using RimWorld;
using Verse;

namespace WRMegaCorp
{
    public class WRMCWG_CarryToGrowthVat: WorkGiver_CarryToBuilding
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get => ThingRequest.ForDef(WRMC_Biotech_DefOfs.WRMC_WolfeinGrowthVat);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false) => !ModsConfig.BiotechActive;
    }
}