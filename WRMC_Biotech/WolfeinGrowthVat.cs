using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace WRMegaCorp
{
    public class WolfeinGrowthVat : 
        Building_Enterable,
        IStoreSettingsParent,
        IThingHolderWithDrawnPawn,
        IThingHolder
    {
  public HumanEmbryo 
    selectedEmbryo;
  private float embryoStarvation;
  protected float containedNutrition;
  private StorageSettings allowedNutritionSettings;
  [Unsaved(false)]
  private CompPowerTrader cachedPowerComp;
  [Unsaved(false)]
  private Graphic cachedTopGraphic;
  [Unsaved(false)]
  private Graphic fetusEarlyStageGraphic;
  [Unsaved(false)]
  private Graphic fetusLateStageGraphic;
  [Unsaved(false)]
  private Sustainer sustainerWorking;
  [Unsaved(false)]
  private Hediff cachedVatLearning;
  [Unsaved(false)]
  private Effecter bubbleEffecter;
  private static readonly Texture2D CancelLoadingIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
  public static readonly CachedTexture InsertPawnIcon = new CachedTexture("UI/Gizmos/InsertPawn");
  public static readonly CachedTexture InsertEmbryoIcon = new CachedTexture("UI/Gizmos/InsertEmbryo");
  private const float BiostarvationGainPerDayNoFood = 0.35f;
  private const float BiostarvationFallPerDayFed = -0.07f;
  private const float BasePawnConsumedNutritionPerDay = 3f;
  private const float BaseEmbryoConsumedNutritionPerDay = 6f;
  private const float AgeToEject = 18f;
  public const float NutritionBuffer = 10f;
  public const int AgeTicksPerTickInGrowthVat = 20;
  // CHANGE FROM .7 TO .85
  private const float EmbryoBirthQuality = 0.85f;
  public const int EmbryoGestationTicks = 540000;
  private const int EmbryoLateStageGraphicTicksRemaining = 540000;
  private const float FetusMinSize = 0.4f;
  private const float FetusMaxSize = 0.95f;
  private const int GlowIntervalTicks = 132;
  private static Dictionary<Rot4, ThingDef> GlowMotePerRotation;
  private static Dictionary<Rot4, EffecterDef> BubbleEffecterPerRotation;

  public bool StorageTabVisible => true;

  public float HeldPawnDrawPos_Y => this.DrawPos.y + 0.03658537f;

  public float HeldPawnBodyAngle => this.Rotation.Opposite.AsAngle;

  public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

  public bool PowerOn => this.PowerTraderComp.PowerOn;

  public override Vector3 PawnDrawOffset
  {
    get
    {
      Vector3 v1 = CompBiosculpterPod.FloatingOffset((float) Find.TickManager.TicksGame);
      Vector3 v2 =IntVec3.West.RotatedBy(this.Rotation).ToVector3() / (float) this.def.size.x;
      return new Vector3(v1.x - v2.x, v1.y, v1.z - v2.z);
    } 
  }

  private CompPowerTrader PowerTraderComp
  {
    get
    {
      if (this.cachedPowerComp == null)
        this.cachedPowerComp = this.TryGetComp<CompPowerTrader>();
      return this.cachedPowerComp;
    }
  }

  public float BiostarvationDailyOffset
  {
    get
    {
      if (!this.Working)
        return 0.0f;
      return containedNutrition <= 0.0 ? BiostarvationGainPerDayNoFood : BiostarvationFallPerDayFed;
    }
  }

  private float BiostarvationSeverityPercent
  {
    get
    {
      if (this.selectedEmbryo != null)
        return this.embryoStarvation;
      if (this.selectedPawn != null)
      {
        Hediff firstHediffOfDef = this.selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BioStarvation);
        if (firstHediffOfDef != null)
          return firstHediffOfDef.Severity / HediffDefOf.BioStarvation.maxSeverity;
      }
      return 0.0f;
    }
  }

  public float NutritionConsumedPerDay
  {
    get
    {
      float nutritionConsumedPerDay = this.selectedEmbryo != null ? 6f : 3f;
      if ((double) this.BiostarvationSeverityPercent > 0.0)
      {
        float num = 1.1f;
        nutritionConsumedPerDay *= num;
      }
      return nutritionConsumedPerDay;
    }
  }

  public float NutritionStored
  {
    get
    {
      float containedNutrition = this.containedNutrition;
      for (int index = 0; index < this.innerContainer.Count; ++index)
      {
        Thing thing = this.innerContainer[index];
        containedNutrition += (float) thing.stackCount * thing.GetStatValue(StatDefOf.Nutrition);
      }
      return containedNutrition;
    }
  }

  public float NutritionNeeded
  {
    get
    {
      return this.selectedPawn == null && this.selectedEmbryo == null ? 0.0f : 10f - this.NutritionStored;
    }
  }

  public int EmbryoGestationTicksRemaining => this.startTick - Find.TickManager.TicksGame;

  public float EmbryoGestationPct
  {
    get => 1f - Mathf.Clamp01((float) this.EmbryoGestationTicksRemaining / 540000f);
  }

  private Graphic TopGraphic
  {
    get
    {
      if (this.cachedTopGraphic == null)
        this.cachedTopGraphic = GraphicDatabase.Get<Graphic_Multi>("Wolfein/Buildings/WolfeinGrowthVat/WolfeinGrowthVatTop", ShaderDatabase.Transparent, this.def.graphicData.drawSize, Color.white);
      return this.cachedTopGraphic;
    }
  }

  private Graphic FetusEarlyStage
  {
    get
    {
      if (this.fetusEarlyStageGraphic == null)
        this.fetusEarlyStageGraphic = GraphicDatabase.Get<Graphic_Single>("Other/VatGrownFetus_EarlyStage", ShaderDatabase.Cutout, Vector2.one, Color.white);
      return this.fetusEarlyStageGraphic;
    }
  }

  private Graphic FetusLateStage
  {
    get
    {
      if (this.fetusLateStageGraphic == null)
        this.fetusLateStageGraphic = GraphicDatabase.Get<Graphic_Single>("Other/VatGrownFetus_LateStage", ShaderDatabase.Cutout, Vector2.one, Color.white);
      return this.fetusLateStageGraphic;
    }
  }

  private Hediff VatLearning
  {
    get
    {
      if (this.cachedVatLearning == null)
        this.cachedVatLearning = this.selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("WRMC_WolfeinVatLearning")) ?? this.selectedPawn.health.AddHediff(HediffDef.Named("WRMC_WolfeinVatLearning"));
      return this.cachedVatLearning;
    }
  }

  public override void PostMake()
  {
    base.PostMake();
    this.allowedNutritionSettings = new StorageSettings((IStoreSettingsParent) this);
    if (this.def.building.defaultStorageSettings == null)
      return;
    this.allowedNutritionSettings.CopyFrom(this.def.building.defaultStorageSettings);
  }

  public override void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (!respawningAfterLoad || this.selectedEmbryo == null || !this.innerContainer.Contains((Thing) this.selectedEmbryo))
      return;
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      Color color = this.EmbryoColor();
      this.fetusEarlyStageGraphic = this.FetusEarlyStage.GetColoredVersion(ShaderDatabase.Cutout, color, color);
      this.fetusLateStageGraphic = this.FetusLateStage.GetColoredVersion(ShaderDatabase.Cutout, color, color);
    }));
  }

  public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
  {
    if (mode != DestroyMode.WillReplace)
    {
      if (this.selectedPawn != null && this.innerContainer.Contains((Thing) this.selectedPawn))
        this.Notify_PawnRemoved();
      this.DestroyEmbryo();
    }
    this.sustainerWorking = (Sustainer) null;
    this.cachedVatLearning = (Hediff) null;
    base.DeSpawn(mode);
  }

  protected override void TickInterval(int delta)
  {
    base.TickInterval(delta);
    if (!this.Working || this.selectedPawn == null || !this.innerContainer.Contains((Thing) this.selectedPawn))
      return;
    this.VatLearning?.TickInterval(delta);
    this.VatLearning?.PostTickInterval(delta);
  }

  protected override void Tick()
  {
    base.Tick();
    if (this.IsHashIntervalTick(250))
    {
      this.PowerTraderComp.PowerOutput =
        this.Working ? -this.PowerComp.Props.PowerConsumption : -this.PowerComp.Props.idlePowerDraw;
      drawFromPipeIfVNPELoaded();
    }
    Pawn selectedPawn = this.selectedPawn;
    if ((selectedPawn != null ? (selectedPawn.Destroyed ? 1 : 0) : 0) == 0)
    {
      HumanEmbryo selectedEmbryo = this.selectedEmbryo;
      if ((selectedEmbryo != null ? (selectedEmbryo.Destroyed ? 1 : 0) : 0) == 0)
        goto label_5;
    }
    this.OnStop();
label_5:
    foreach (Thing thing in (IEnumerable<Thing>) this.innerContainer)
    {
      if (thing is HumanEmbryo humanEmbryo && humanEmbryo != this.selectedEmbryo)
        this.innerContainer.TryDrop(thing, this.InteractionCell, this.Map, ThingPlaceMode.Near, 1, out Thing _);
    }
    if (this.Working)
    {
      if (this.selectedPawn != null)
      {
        if ((double) this.selectedPawn.ageTracker.AgeBiologicalYearsFloat >= 18.0)
        {
          Messages.Message((string) ("OccupantEjectedFromGrowthVat".Translate(this.selectedPawn.Named("PAWN")) + ": " + "PawnIsTooOld".Translate(this.selectedPawn.Named("PAWN"))), (LookTargets) (Thing) this.selectedPawn, MessageTypeDefOf.NeutralEvent);
          this.Finish();
          return;
        }
        if (this.innerContainer.Contains((Thing) this.selectedPawn))
        {
          this.selectedPawn.ageTracker.Notify_TickedInGrowthVat(Mathf.RoundToInt(20f * 0.25f * this.selectedPawn.GetStatValue(StatDefOf.GrowthVatOccupantSpeed)));
          this.VatLearning?.Tick();
          this.VatLearning?.PostTick();
        }
        float num = this.BiostarvationDailyOffset / 60000f * HediffDefOf.BioStarvation.maxSeverity;
        Hediff firstHediffOfDef = this.selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BioStarvation);
        if (firstHediffOfDef != null)
        {
          firstHediffOfDef.Severity += num;
          if (firstHediffOfDef.ShouldRemove)
            this.selectedPawn.health.RemoveHediff(firstHediffOfDef);
        }
        else if ((double) num > 0.0)
        {
          Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BioStarvation, this.selectedPawn);
          hediff.Severity = num;
          this.selectedPawn.health.AddHediff(hediff);
        }
      }
      else if (this.selectedEmbryo != null)
      {
        if (this.EmbryoGestationTicksRemaining <= 0)
        {
          this.Finish();
          return;
        }
        this.embryoStarvation = Mathf.Clamp01(this.embryoStarvation + this.BiostarvationDailyOffset / 60000f);
      }
      if ((double) this.BiostarvationSeverityPercent >= 1.0)
      {
        this.Fail();
      }
      else
      {
        if (this.sustainerWorking == null || this.sustainerWorking.Ended)
          this.sustainerWorking = SoundDefOf.GrowthVat_Working.TrySpawnSustainer(SoundInfo.InMap((TargetInfo) (Thing) this, MaintenanceType.PerTick));
        else
          this.sustainerWorking.Maintain();
        this.containedNutrition = Mathf.Clamp(this.containedNutrition - this.NutritionConsumedPerDay / 60000f, 0.0f, (float) int.MaxValue);
        // drawFromPipeIfVNPELoaded();
        if ((double) this.containedNutrition <= 0.0)
          this.TryAbsorbNutritiousThing();
        if (WolfeinGrowthVat.GlowMotePerRotation == null)
        {
          WolfeinGrowthVat.GlowMotePerRotation = new Dictionary<Rot4, ThingDef>()
          {
            {
              Rot4.South,
              ThingDef.Named("WRMC_Mote_VatGlowSouth")
            },
            {
              Rot4.East,
              ThingDef.Named("WRMC_Mote_VatGlowEast")
            },
            {
              Rot4.West,
              ThingDef.Named("WRMC_Mote_VatGlowWest")
            },
            {
              Rot4.North,
              ThingDef.Named("WRMC_Mote_VatGlowNorth")
            }
          };
          WolfeinGrowthVat.BubbleEffecterPerRotation = new Dictionary<Rot4, EffecterDef>()
          {
            {
              Rot4.South,
              WRMC_Biotech_DefOfs.WRMC_Vat_Bubbles_South
            },
            {
              Rot4.East,
              WRMC_Biotech_DefOfs.WRMC_Vat_Bubbles_East
            },
            {
              Rot4.West,
              WRMC_Biotech_DefOfs.WRMC_Vat_Bubbles_West
            },
            {
              Rot4.North,
              WRMC_Biotech_DefOfs.WRMC_Vat_Bubbles_North
            }
          };
        }
        if (this.IsHashIntervalTick(132))
          MoteMaker.MakeStaticMote(this.DrawPos, this.MapHeld, WolfeinGrowthVat.GlowMotePerRotation[this.Rotation]);
        if (this.bubbleEffecter == null)
          this.bubbleEffecter = WolfeinGrowthVat.BubbleEffecterPerRotation[this.Rotation].SpawnAttached((Thing) this, this.MapHeld);
        this.bubbleEffecter.EffectTick((TargetInfo) (Thing) this, (TargetInfo) (Thing) this);
      }
    }
    else
    {
      this.TryGrowEmbryo();
      this.bubbleEffecter?.Cleanup();
      this.bubbleEffecter = (Effecter) null;
    }
  }

  protected virtual void drawFromPipeIfVNPELoaded() {}

  public override AcceptanceReport CanAcceptPawn(Pawn pawn)
  {
    if (this.Working)
      return (AcceptanceReport) "Occupied".Translate();
    if (!this.PowerOn)
      return (AcceptanceReport) "NoPower".Translate().CapitalizeFirst();
    if (this.selectedEmbryo != null)
      return (AcceptanceReport) "EmbryoSelected".Translate();
    if ((double) pawn.ageTracker.AgeBiologicalYearsFloat >= 18.0)
      return (AcceptanceReport) "TooOld".Translate(pawn.Named("PAWN"), 18f.Named("AGEYEARS"));
    if (this.selectedPawn != null && this.selectedPawn != pawn)
      return (AcceptanceReport) "WaitingForPawn".Translate(this.selectedPawn.Named("PAWN"));
    return pawn.health.hediffSet.HasHediff(HediffDefOf.BioStarvation) ? (AcceptanceReport) "PawnBiostarving".Translate(pawn.Named("PAWN")) : (AcceptanceReport) (pawn.IsColonist && !pawn.IsQuestLodger());
  }

  public override void TryAcceptPawn(Pawn pawn)
  {
    if (this.selectedPawn == null || !(bool) this.CanAcceptPawn(pawn))
      return;
    this.selectedPawn = pawn;
    int num = pawn.DeSpawnOrDeselect() ? 1 : 0;
    if (this.innerContainer.TryAddOrTransfer((Thing) pawn))
    {
      SoundDefOf.GrowthVat_Close.PlayOneShot(SoundInfo.InMap((TargetInfo) (Thing) this));
      this.startTick = Find.TickManager.TicksGame;
      if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("WRMC_WolfeinVatLearning")))
        pawn.health.AddHediff(HediffDef.Named("WRMC_WolfeinVatLearning"));
      if (!pawn.health.hediffSet.HasHediff(HediffDefOf.VatGrowing))
        pawn.health.AddHediff(HediffDefOf.VatGrowing);
    }
    if (num == 0)
      return;
    Find.Selector.Select((object) pawn, false, false);
  }

  private void TryGrowEmbryo()
  {
    if (this.Working || !this.PowerOn || this.selectedEmbryo == null || !this.innerContainer.Contains((Thing) this.selectedEmbryo))
      return;
    SoundDefOf.GrowthVat_Close.PlayOneShot(SoundInfo.InMap((TargetInfo) (Thing) this));
    this.startTick = Find.TickManager.TicksGame + 540000;
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      Color color = this.EmbryoColor();
      this.fetusEarlyStageGraphic = this.FetusEarlyStage.GetColoredVersion(ShaderDatabase.Cutout, color, color);
      this.fetusLateStageGraphic = this.FetusLateStage.GetColoredVersion(ShaderDatabase.Cutout, color, color);
    }));
    if (this.selectedPawn == null)
      return;
    Log.Error("Growing embryo while pawn was somehow marked as selected");
    this.selectedPawn = (Pawn) null;
  }

  private void TryAbsorbNutritiousThing()
  {
    for (int index = 0; index < this.innerContainer.Count; ++index)
    {
      if (this.innerContainer[index] != this.selectedPawn && this.innerContainer[index].def != ThingDefOf.Xenogerm && this.innerContainer[index].def != ThingDefOf.HumanEmbryo)
      {
        float statValue = this.innerContainer[index].GetStatValue(StatDefOf.Nutrition);
        if ((double) statValue > 0.0)
        {
          this.containedNutrition += statValue;
          this.innerContainer[index].SplitOff(1).Destroy();
          break;
        }
      }
    }
  }

  private void Finish()
  {
    if (this.selectedPawn != null)
    {
      this.FinishPawn();
    }
    else
    {
      if (this.selectedEmbryo == null)
        return;
      this.FinishEmbryo();
    }
  }

  private void FinishEmbryo()
  {
    this.EmbryoBirth();
    this.DestroyEmbryo();
    this.OnStop();
  }

  private void FinishPawn()
  {
    if (this.selectedPawn == null || !this.innerContainer.Contains((Thing) this.selectedPawn))
      return;
    this.Notify_PawnRemoved();
    this.innerContainer.TryDrop((Thing) this.selectedPawn, this.InteractionCell, this.Map, ThingPlaceMode.Near, 1, out Thing _);
    this.OnStop();
  }

  private void Fail()
  {
    if (this.innerContainer.Contains((Thing) this.selectedPawn))
    {
      this.Notify_PawnRemoved();
      this.innerContainer.TryDrop((Thing) this.selectedPawn, this.InteractionCell, this.Map, ThingPlaceMode.Near, 1, out Thing _);
      this.selectedPawn.Kill(new DamageInfo?(), this.selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BioStarvation));
    }
    this.DestroyEmbryo(true);
    this.OnStop();
  }

  private void OnStop()
  {
    this.selectedPawn = (Pawn) null;
    this.selectedEmbryo = (HumanEmbryo) null;
    this.startTick = -1;
    this.embryoStarvation = 0.0f;
    this.sustainerWorking = (Sustainer) null;
    this.cachedVatLearning = (Hediff) null;
  }

  private void DestroyEmbryo(bool biostarvation = false)
  {
    if (this.startTick < 0 || this.selectedEmbryo == null || !this.innerContainer.Contains((Thing) this.selectedEmbryo))
      return;
    if (this.startTick > Find.TickManager.TicksGame)
    {
      if (biostarvation)
        Messages.Message((string) "EmbryoEjectedFromGrowthVatBiostarvation".Translate((NamedArgument) this.selectedEmbryo.Label), (LookTargets) (Thing) this, MessageTypeDefOf.NegativeEvent);
      else
        Messages.Message((string) "EmbryoEjectedFromGrowthVat".Translate((NamedArgument) this.selectedEmbryo.Label), (LookTargets) (Thing) this, MessageTypeDefOf.NegativeEvent);
    }
    this.innerContainer.Remove((Thing) this.selectedEmbryo);
    this.selectedEmbryo.Destroy(DestroyMode.Vanish);
    this.selectedEmbryo = (HumanEmbryo) null;
  }

  private void EmbryoBirth()
  {
    if (this.selectedEmbryo == null || !this.innerContainer.Contains((Thing) this.selectedEmbryo) || this.startTick > Find.TickManager.TicksGame)
      return;
    Precept_Ritual precept = Faction.OfPlayer.ideos.PrimaryIdeo.GetPrecept(PreceptDefOf.ChildBirth) as Precept_Ritual;
    Thing thing = PregnancyUtility.ApplyBirthOutcome(((RitualOutcomeEffectWorker_FromQuality) RitualOutcomeEffectDefOf.ChildBirth.GetInstance()).GetOutcome(EmbryoBirthQuality, (LordJob_Ritual) null), EmbryoBirthQuality, precept, this.selectedEmbryo?.GeneSet?.GenesListForReading, this.selectedEmbryo.Mother, (Thing) this, this.selectedEmbryo.Father);
    if (thing == null || (double) this.embryoStarvation <= 0.0)
      return;
    Pawn pawn = thing is Corpse corpse ? corpse.InnerPawn : (Pawn) thing;
    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BioStarvation, pawn);
    hediff.Severity = Mathf.Lerp(0.0f, HediffDefOf.BioStarvation.maxSeverity, this.embryoStarvation);
    pawn.health.AddHediff(hediff);
  }

  private void Notify_PawnRemoved()
  {
    SoundDefOf.GrowthVat_Open.PlayOneShot(SoundInfo.InMap((TargetInfo) (Thing) this));
  }

  public bool CanAcceptNutrition(Thing thing)
  {
    return this.allowedNutritionSettings.AllowedToAccept(thing);
  }

  public StorageSettings GetStoreSettings() => this.allowedNutritionSettings;

  public StorageSettings GetParentStoreSettings() => this.def.building.fixedStorageSettings;

  public void Notify_SettingsChanged()
  {
  }

  public override IEnumerable<Gizmo> GetGizmos()
{
    foreach (Gizmo gizmo in base.GetGizmos())
        yield return gizmo;

    foreach (Gizmo gizmo in StorageSettingsClipboard.CopyPasteGizmosFor(allowedNutritionSettings))
        yield return gizmo;

    if (Working)
    {
        var cancelGrowth = new Command_Action
        {
            defaultLabel = "CommandCancelGrowth".Translate(),
            defaultDesc = "CommandCancelGrowthDesc".Translate(),
            icon = CancelLoadingIcon,
            activateSound = SoundDefOf.Designate_Cancel,
            action = delegate
            {
                void DoCancel()
                {
                    Finish();
                    innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
                }

                if (startTick > Find.TickManager.TicksGame &&
                    selectedEmbryo != null &&
                    innerContainer.Contains(selectedEmbryo))
                {
                    var window = Dialog_MessageBox.CreateConfirmation(
                        "ImplantEmbryoCancelVat".Translate(selectedEmbryo.Label),
                        DoCancel,
                        destructive: true);
                    Find.WindowStack.Add(window);
                }
                else
                {
                    DoCancel();
                }
            }
        };
        yield return cancelGrowth;

        if (selectedEmbryo != null)
        {
            yield return new Command_Action
            {
                defaultLabel = "InspectGenes".Translate() + "...",
                defaultDesc = "InspectGenesEmbryoDesc".Translate(),
                icon = GeneSetHolderBase.GeneticInfoTex.Texture,
                action = () => InspectPaneUtility.OpenTab(typeof(ITab_Genes))
            };
        }

        if (DebugSettings.ShowDevGizmos)
        {
            if (selectedPawn != null && innerContainer.Contains(selectedPawn))
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Advance 1 year",
                    action = () => selectedPawn.ageTracker.Notify_TickedInGrowthVat(900000)
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Learn",
                    action = ((Hediff_WolfeinVatLearning)VatLearning).Learn
                };
            }

            if (selectedEmbryo != null && innerContainer.Contains(selectedEmbryo))
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Advance gestation 1 day",
                    action = () => startTick = Mathf.Max(0, startTick - 60000)
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Embryo birth now",
                    action = delegate
                    {
                        startTick = Find.TickManager.TicksGame;
                        Finish();
                    }
                };
            }
        }
    }
    else
    {
        if (selectedPawn != null || selectedEmbryo != null)
        {
            var cancelLoad = new Command_Action
            {
                defaultLabel = "CommandCancelLoad".Translate(),
                defaultDesc = "CommandCancelLoadDesc".Translate(),
                icon = CancelLoadingIcon,
                activateSound = SoundDefOf.Designate_Cancel,
                action = delegate
                {
                    DestroyEmbryo();
                    innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);

                    if (innerContainer.Contains(selectedPawn))
                        Notify_PawnRemoved();

                    if (selectedPawn?.CurJobDef == JobDefOf.EnterBuilding)
                        selectedPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);

                    OnStop();
                }
            };
            yield return cancelLoad;
        }

        if (selectedPawn == null)
        {
            var insertPerson = new Command_Action
            {
                defaultLabel = "InsertPerson".Translate() + "...",
                defaultDesc = "InsertPersonGrowthVatDesc".Translate(),
                icon = InsertPawnIcon.Texture,
                action = delegate
                {
                    var options = new List<FloatMenuOption>();
                    foreach (Pawn p in Map.mapPawns.AllPawnsSpawned)
                    {
                        Pawn pawn = p; // 闭包安全副本
                        if (CanAcceptPawn(p).Accepted)
                        {
                            options.Add(new FloatMenuOption(
                                p.LabelCap,
                                () => SelectPawn(pawn),
                                pawn,
                                Color.white));
                        }
                    }

                    if (!options.Any())
                        options.Add(new FloatMenuOption("NoViablePawns".Translate(), null));

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };

            if (selectedEmbryo != null)
                insertPerson.Disable("EmbryoSelected".Translate().CapitalizeFirst());
            else if (!PowerOn)
                insertPerson.Disable("NoPower".Translate().CapitalizeFirst());
            else if (!AnyAcceptablePawns)
                insertPerson.Disable("NoPawnsCanEnterGrowthVat".Translate(18f).ToString());

            yield return insertPerson;
        }

        if (selectedEmbryo == null && Find.Storyteller.difficulty.ChildrenAllowed)
        {
            List<Thing> embryos = Map.listerThings.ThingsOfDef(ThingDefOf.HumanEmbryo);

            var implantEmbryo = new Command_Action
            {
                defaultLabel = "ImplantEmbryo".Translate() + "...",
                defaultDesc = "InsertEmbryoGrowthVatDesc".Translate(540000.ToStringTicksToPeriod()).Resolve(),
                icon = InsertEmbryoIcon.Texture,
                action = delegate
                {
                    var options = new List<FloatMenuOption>();
                    foreach (Thing t in embryos)
                    {
                        Thing embryoThing = t; // 闭包安全副本
                        options.Add(new FloatMenuOption(
                            embryoThing.LabelCap,
                            () => SelectEmbryo(embryoThing as HumanEmbryo),
                            embryoThing,
                            Color.white));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };

            if (embryos.NullOrEmpty())
                implantEmbryo.Disable("ImplantNoEmbryos".Translate().CapitalizeFirst());
            else if (selectedPawn != null)
                implantEmbryo.Disable("PersonSelected".Translate().CapitalizeFirst());
            else if (!PowerOn)
                implantEmbryo.Disable("NoPower".Translate().CapitalizeFirst());

            yield return implantEmbryo;
        }
    }

    if (DebugSettings.ShowDevGizmos)
    {
        yield return new Command_Action
        {
            defaultLabel = "DEV: Fill nutrition",
            action = () => containedNutrition = 10f
        };
        yield return new Command_Action
        {
            defaultLabel = "DEV: Empty nutrition",
            action = () => containedNutrition = 0f
        };
    }
}

  public void SelectEmbryo(HumanEmbryo embryo)
  {
    this.selectedEmbryo = embryo;
    embryo.implantTarget = (Thing) this;
  }

  public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
  {
    if (this.Working && this.selectedPawn != null && this.innerContainer.Contains((Thing) this.selectedPawn))
      this.selectedPawn.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc + this.PawnDrawOffset, neverAimWeapon: true);
    base.DynamicDrawPhaseAt(phase, drawLoc, flip);
  }

  protected override void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    base.DrawAt(drawLoc, flip);
    if (this.Working && this.selectedPawn == null && this.selectedEmbryo != null && this.innerContainer.Contains((Thing) this.selectedEmbryo))
    {
      Vector2 vector2 = Vector2.one * Mathf.Lerp(0.4f, 0.95f, this.EmbryoGestationPct);
      if (this.EmbryoGestationTicksRemaining > 540000)
      {
        this.FetusEarlyStage.drawSize = vector2;
        this.FetusEarlyStage.DrawFromDef(this.DrawPos + this.PawnDrawOffset + Altitudes.AltIncVect * 0.25f, this.Rotation, (ThingDef) null);
      }
      else
      {
        this.FetusLateStage.drawSize = vector2;
        this.FetusLateStage.DrawFromDef(this.DrawPos + this.PawnDrawOffset + Altitudes.AltIncVect * 0.25f, this.Rotation, (ThingDef) null);
      }
    }
    this.TopGraphic.Draw(this.DrawPos + Altitudes.AltIncVect * 2f, this.Rotation, (Thing) this);
  }

  private Color EmbryoColor()
  {
    Color skinColor = PawnSkinColors.GetSkinColor(0.5f);
    if (this.selectedEmbryo?.GeneSet != null)
    {
      foreach (GeneDef geneDef in this.selectedEmbryo.GeneSet.GenesListForReading)
      {
        if (geneDef.skinColorOverride.HasValue)
          return geneDef.skinColorOverride.Value;
        if (geneDef.skinColorBase.HasValue)
          skinColor = geneDef.skinColorBase.Value;
      }
    }
    return skinColor;
  }

  public override void DrawExtraSelectionOverlays()
  {
    base.DrawExtraSelectionOverlays();
    if (this.selectedEmbryo == null || this.selectedEmbryo.Map != this.Map)
      return;
    GenDraw.DrawLineBetween(this.TrueCenter(), this.selectedEmbryo.TrueCenter());
  }

  public override string GetInspectString()
  {
    StringBuilder sb = new StringBuilder();
    sb.Append(base.GetInspectString());
    if (this.Working)
    {
      if (this.selectedPawn != null && this.innerContainer.Contains((Thing) this.selectedPawn))
      {
        StringBuilder stringBuilder = sb.AppendLineIfNotEmpty();
        TaggedString taggedString = "CasketContains".Translate();
        string str1 = taggedString.ToString();
        taggedString = this.selectedPawn.NameShortColored;
        string str2 = taggedString.Resolve();
        // ISSUE: variable of a boxed type
        int ageBiologicalYears = this.selectedPawn.ageTracker.AgeBiologicalYears;
        string str3 = $"{str1}: {str2}, {ageBiologicalYears}";
        stringBuilder.Append(str3);
      }
      if (this.selectedEmbryo != null && this.innerContainer.Contains((Thing) this.selectedEmbryo))
      {
        sb.AppendLineIfNotEmpty().AppendLine((string) ("Gestating".Translate() + ": " + this.selectedEmbryo.Label.CapitalizeFirst()));
        sb.AppendLineTagged("EmbryoTimeUntilBirth".Translate() + ": " + this.EmbryoGestationTicksRemaining.ToStringTicksToDays().Colorize(ColoredText.DateTimeColor));
        sb.Append((string) ("EmbryoBirthQuality".Translate() + ": " + EmbryoBirthQuality.ToStringPercent()));
      }
      float biostarvationSeverityPercent = this.BiostarvationSeverityPercent;
      if ((double) biostarvationSeverityPercent > 0.0)
      {
        string str = (double) this.BiostarvationDailyOffset >= 0.0 ? "+" : string.Empty;
        sb.AppendLineIfNotEmpty().Append($"{"Biostarvation".Translate()}: {biostarvationSeverityPercent.ToStringPercent()} ({"PerDay".Translate((NamedArgument) (str + this.BiostarvationDailyOffset.ToStringPercent()))})");
      }
    }
    else if (this.selectedPawn != null)
      sb.AppendLineIfNotEmpty().Append("WaitingForPawn".Translate(this.selectedPawn.Named("PAWN")).Resolve());
    sb.AppendLineIfNotEmpty().Append((string) "Nutrition".Translate()).Append(": ").Append(this.NutritionStored.ToStringByStyle(ToStringStyle.FloatMaxOne));
    if (this.Working)
      sb.Append(" (-").Append((string) "PerDay".Translate((NamedArgument) this.NutritionConsumedPerDay.ToString("F1"))).Append(")");
    return sb.ToString();
  }

  public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
  {
    foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn))
      yield return option;

    if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
    {
      yield return new FloatMenuOption(
        "CannotEnterBuilding".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(),
        null);
      yield break;
    }

    AcceptanceReport report = CanAcceptPawn(selPawn);
    if (report.Accepted)
    {
      yield return FloatMenuUtility.DecoratePrioritizedTask(
        new FloatMenuOption(
          "EnterBuilding".Translate(this),
          () => SelectPawn(selPawn)),
        selPawn,
        this);
    }
    else if (!report.Reason.NullOrEmpty())
    {
      yield return new FloatMenuOption(
        "CannotEnterBuilding".Translate(this) + ": " + report.Reason.CapitalizeFirst(),
        null);
    }
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<HumanEmbryo>(ref this.selectedEmbryo, "selectedEmbryo");
    Scribe_Values.Look<float>(ref this.embryoStarvation, "embryoStarvation");
    Scribe_Values.Look<float>(ref this.containedNutrition, "containedNutrition");
    Scribe_Deep.Look<StorageSettings>(ref this.allowedNutritionSettings, "allowedNutritionSettings", (object) this);
    if (this.allowedNutritionSettings != null)
      return;
    this.allowedNutritionSettings = new StorageSettings((IStoreSettingsParent) this);
    if (this.def.building.defaultStorageSettings == null)
      return;
    this.allowedNutritionSettings.CopyFrom(this.def.building.defaultStorageSettings);
  }

  public void SetStartTick(int startTick)
  {
    this.startTick = startTick;
  }
  
  public void SetSomeNutrition(float nutrition)
  {
    this.containedNutrition += nutrition;
  }
    }
}