using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WRMegaCorp
{
    public class WRMCWG_HaulToGrowthVat : WorkGiver_Scanner
    {
    private const float NutritionBuffer = 2.5f;

    public override ThingRequest PotentialWorkThingRequest
    {
      get => ThingRequest.ForDef(WRMC_Biotech_DefOfs.WRMC_WolfeinGrowthVat);
    }

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
      if (!ModLister.CheckBiotech("Growth vat") || !pawn.CanReserve((LocalTargetInfo) t, ignoreOtherReservations: forced) || pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null || t.IsBurning() || !(t is WolfeinGrowthVat vat))
        return false;
      if ((double) vat.NutritionNeeded > 2.5)
      {
        if (this.FindNutrition(pawn, vat).Thing != null)
          return true;
        JobFailReason.Is((string) "NoFood".Translate());
        return false;
      }
      return vat.selectedEmbryo != null && !vat.innerContainer.Contains((Thing) vat.selectedEmbryo) && this.CanHaulSelectedThing(pawn, (Thing) vat.selectedEmbryo);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
      if (!(t is WolfeinGrowthVat vat))
        return (Job) null;
      if ((double) vat.NutritionNeeded > 0.0)
      {
        ThingCount nutrition = this.FindNutrition(pawn, vat);
        if (nutrition.Thing != null)
        {
          Job containerJob = HaulAIUtility.HaulToContainerJob(pawn, nutrition.Thing, t);
          containerJob.count = Mathf.Min(containerJob.count, nutrition.Count);
          return containerJob;
        }
      }
      if (vat.selectedEmbryo == null || vat.innerContainer.Contains((Thing) vat.selectedEmbryo) || !this.CanHaulSelectedThing(pawn, (Thing) vat.selectedEmbryo))
        return (Job) null;
      Job containerJob1 = HaulAIUtility.HaulToContainerJob(pawn, (Thing) vat.selectedEmbryo, t);
      containerJob1.count = 1;
      return containerJob1;
    }

    private bool CanHaulSelectedThing(Pawn pawn, Thing selectedThing)
    {
      return selectedThing.Spawned && selectedThing.Map == pawn.Map && !selectedThing.IsForbidden(pawn) && pawn.CanReserveAndReach((LocalTargetInfo) selectedThing, PathEndMode.OnCell, Danger.Deadly, stackCount: 1);
    }

    private ThingCount FindNutrition(Pawn pawn, WolfeinGrowthVat vat)
    {
      Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: new Predicate<Thing>(Validator));
      if (thing == null)
        return new ThingCount();
      int b = Mathf.CeilToInt(vat.NutritionNeeded / thing.GetStatValue(StatDefOf.Nutrition));
      return new ThingCount(thing, Mathf.Min(thing.stackCount, b));

      bool Validator(Thing x)
      {
        return !x.IsForbidden(pawn) && pawn.CanReserve((LocalTargetInfo) x) && vat.CanAcceptNutrition(x) && (double) x.def.GetStatValueAbstract(StatDefOf.Nutrition) <= (double) vat.NutritionNeeded;
      }
    }
  }
}