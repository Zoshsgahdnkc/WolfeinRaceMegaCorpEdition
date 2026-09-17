using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AncotLibrary;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WRMegaCorp;

public class Comp_SwitchModeRangedWeapon_VEF: ThingComp, IPawnWeaponGizmoProvider
{
    private static readonly FieldInfo VerbsField = AccessTools.Field(typeof (VerbTracker), "verbs");
    public CompProps_SwitchModeRangedWeapon Props => (CompProps_SwitchModeRangedWeapon) this.props;
    public int CurrentModeIndex;
    private bool verbsInitialized = false;
    private bool pendingVerbRebuild = true;
    private int nextVerbRebuildRetryTick = 0;
    private CompEquippable compEquippable => this.parent?.GetComp<CompEquippable>();
    private readonly List<Verb> addedVerbs = new List<Verb>();
    private Pawn CurrentWielder
      {
          get
          {
              return !(this.parent?.ParentHolder is Pawn_EquipmentTracker parentHolder) ? (Pawn) null : parentHolder.pawn;
          }
      }
    public RangedMode CurrentMode 
    {
        get
        {
            if (this.Props.rangedModes.NullOrEmpty<RangedMode>())
                return (RangedMode) null;
            if (this.CurrentModeIndex >= this.Props.rangedModes.Count)
                this.CurrentModeIndex = 0;
            return this.Props.rangedModes[this.CurrentModeIndex];
        }
    }
    
    
    
    public IEnumerable<Gizmo> GetWeaponGizmos()
    {
        if (this.Props.rangedModes != null && this.Props.rangedModes.Count > 1 && this.CurrentMode != null)
        {
            if (this.ShouldRetryVerbRebuildFromGizmoPath())
                this.ApplyCurrentRangedMode();
            Command_Action commandAction = new Command_Action();
            commandAction.defaultLabel = this.CurrentMode.label;
            commandAction.defaultDesc = this.CurrentMode.desc;
            commandAction.icon = !this.CurrentMode.iconPath.NullOrEmpty() ? (Texture) ContentFinder<Texture2D>.Get(this.CurrentMode.iconPath) : (Texture) BaseContent.BadTex;
            commandAction.action = (Action) (() =>
            {
                SoundDef.Named("Wolfein_WeaponInteract").PlayOneShot(SoundInfo.InMap(parent));
                this.CurrentModeIndex = (this.CurrentModeIndex + 1) % this.Props.rangedModes.Count;
                this.ApplyCurrentRangedMode();
                SoundDefOf.Tick_High.PlayOneShotOnCamera();

                // TryGetVerbList(out var verbs);
                // StringBuilder sb = new StringBuilder($"==========\ncurrentIndex:  {this.CurrentModeIndex}\nadded: ");
                // addedVerbs?.ConvertAll(v => v.ReportLabel).ForEach(label => sb.Append(label));
                // sb.Append("\nprimary verb: " + (compEquippable.PrimaryVerb.ReportLabel.NullOrEmpty() ?
                //           (compEquippable.PrimaryVerb.tool == null ? "Not Tool": "Tool") : compEquippable.PrimaryVerb.ReportLabel));
                // sb.Append("\nweapon verbs: ");
                // foreach (var verb in verbs)
                // {
                //     sb.Append("\nlabel:" + verb.ReportLabel);
                //     sb.Append($"\nisTool: {verb.tool != null},isPrimary: {verb.verbProps.isPrimary},caster: {verb.Caster},available: {verb.Available()}");
                // }
                // WRMC_Utils.LogDebugMessage(sb.ToString());
            });
            Command_Action command = commandAction;
            yield return (Gizmo) command;
        }
    }
    
    private void ApplyCurrentRangedMode()
    {
        // WRMC_Utils.LogDebugMessage("Before Apply=====================");
        // TryGetVerbList(out var vs1);
        // foreach (var v in vs1)
        // {
        //     WRMC_Utils.LogDebugMessage($"verb: {v}, tool: {v.tool != null}, caster: {v.Caster?.Label??"null"}");
        // }
        try
        {
            if (!TryGetVerbList(out List<Verb> _))
            {
                this.MarkVerbRebuildPending();
            }
            else
            {
                this.RemoveLastRangedVerbs();
                this.RemoveDefaultRangedVerbs();
                if (!this.AddCurrentRangedVerbs())
                {
                    this.MarkVerbRebuildPending();
                }
                else
                {
                    this.verbsInitialized = true;
                    this.pendingVerbRebuild = false;
                }
            }
        }
        catch (Exception e)
        {
            WRMC_Utils.LogError(e.ToString());
        }
        TryGetVerbList(out var verbs);
        
        // 完全不知道哪里丢失了近战verb的caster，于是做一个fallback补上
        var fallbackCaster = CurrentWielder;
        foreach (var v in verbs)
        {
            if (v != null && v.caster == null)
                v.caster = fallbackCaster;
        }

        // TryGetVerbList(out var vs);
        // WRMC_Utils.LogDebugMessage($"Mode Applied: {addedVerbs.First().ReportLabel}====================");
        // foreach (var v in vs)
        // {
        //     WRMC_Utils.LogDebugMessage($"verb: {v}, tool: {v.tool != null}, caster: {v.Caster?.Label??"null"}");
        // }
    }
    
    private bool TryGetVerbList(out List<Verb> verbs)
    {
        verbs = (List<Verb>) null;
        CompEquippable compEquippable = this.compEquippable;
        if (compEquippable?.verbTracker == null || VerbsField == (FieldInfo) null)
            return false;
        verbs = VerbsField.GetValue((object) compEquippable.verbTracker) as List<Verb>;
        if (verbs != null)
            return true;
        compEquippable.verbTracker.InitVerbsFromZero();
        verbs = VerbsField.GetValue((object) compEquippable.verbTracker) as List<Verb>;
        return verbs != null;
    }
    
    public void RemoveLastRangedVerbs()
    {
        List<Verb> verbs;
        if (!this.TryGetVerbList(out verbs))
            return;
        foreach (Verb addedVerb in this.addedVerbs)
        {
            // MVCF
            addedVerb.CasterPawn.Manager().RemoveVerb(addedVerb);
        }
        this.addedVerbs.Clear();
    }
    
    public void RemoveDefaultRangedVerbs()
    {
        List<Verb> verbs;
        if (!this.TryGetVerbList(out verbs))
            return;
        ThingWithComps owner = this.parent;
        if (owner?.def?.Verbs == null)
            return;
        verbs.RemoveAll((Predicate<Verb>) (verb =>
        {
            if (verb == null || verb.EquipmentSource != owner)
                return false;
            // 如果是近战
            if (verb.tool != null)
                return false;
            foreach (VerbProperties prop in owner.def.Verbs)
            {
                if (verb.verbProps == prop)
                {
                    WRMC_Utils.LogDebugMessage($"Remove Verb {verb.ReportLabel} from Weapon {parent.Label}");
                    // MVCF
                    verb.CasterPawn.Manager().RemoveVerb(verb);
                    return true;
                }
            }
            return false;
        }));
    }
    
    public bool AddCurrentRangedVerbs()
    {
        RangedMode currentMode = this.CurrentMode;
        if (currentMode?.verb == null)
            return true;
        CompEquippable compEquippable = this.compEquippable;
        if (compEquippable?.verbTracker == null)
            return false;
        Pawn currentWielder = this.CurrentWielder;
        if (currentWielder == null)
            return false;
        List<Verb> verbs;
        if (!this.TryGetVerbList(out verbs))
        {
            WRMC_Utils.LogWarning("Verb list not ready yet.");
            return false;
        }
        
        // 添加新的verb的逻辑
        Verb instance = (Verb) Activator.CreateInstance(currentMode.verb.verbClass);
        instance.caster = (Thing) ((ThingWithComps) currentWielder ?? this.parent);
        instance.verbProps = currentMode.verb;
        instance.verbTracker = compEquippable.verbTracker;
        instance.loadID = Verb.CalculateUniqueLoadID(compEquippable.verbTracker.directOwner, CurrentModeIndex);
        
        instance.Notify_PickedUp();
        verbs.Insert(0, instance);
        this.addedVerbs.Add(instance);
        
        // MVCF兼容
        WRMC_Utils.LogDebugMessage($"Initialize MVCF Verb for verb {instance.ReportLabel}");
        MVCF.Utilities.ManagedVerbUtility.InitializeManaged(instance, compEquippable.verbTracker);
        
        return true;
    }
    
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look<int>(ref this.CurrentModeIndex, "currentModeIndex");
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;
        this.MarkVerbRebuildPending();
        this.QueueSafeApplyCurrent();
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        this.MarkVerbRebuildPending();
        this.compEquippable?.verbTracker?.InitVerbsFromZero();
        this.QueueSafeApplyCurrent();
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        this.MarkVerbRebuildPending();
        this.compEquippable?.verbTracker?.InitVerbsFromZero();
        this.ApplyCurrentRangedMode();
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        this.MarkVerbRebuildPending();
    }
    
    private void MarkVerbRebuildPending()
    {
        WRMC_Utils.LogDebugMessage("Marking Verb Rebuild Pending...");
        this.verbsInitialized = false;
        this.pendingVerbRebuild = true;
        this.nextVerbRebuildRetryTick = 0;
    }

    private bool ShouldRetryVerbRebuildFromGizmoPath()
    {
        if (!this.pendingVerbRebuild)
            return false;
        TickManager tickManager = Find.TickManager;
        if (tickManager == null)
            return true;
        int ticksGame = tickManager.TicksGame;
        if (ticksGame < this.nextVerbRebuildRetryTick)
            return false;
        this.nextVerbRebuildRetryTick = ticksGame + 60;
        return true;
    }
    
    private void QueueSafeApplyCurrent()
    {
        ThingWithComps parent = this.parent;
        if (parent == null)
            return;
        System.WeakReference<ThingWithComps> parentRef = new System.WeakReference<ThingWithComps>(parent);
        LongEventHandler.ExecuteWhenFinished((Action) (() =>
        {
            ThingWithComps target;
            if (!parentRef.TryGetTarget(out target) || target.Destroyed || target != this.parent)
                return;
            this.ApplyCurrentRangedMode();
        }));
    }
    
    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        if (CurrentMode != null && parent != null && CurrentMode.verb != null)
        {
            var statCat = WRMC_DefOfs.WRMC_Weapon_SwitchMode;
            StringBuilder damageDesc = new StringBuilder();
            damageDesc.AppendLine((string) "Stat_Thing_Damage_Desc".Translate());
            damageDesc.AppendLine();
            float damageAmount = CurrentMode.verb.defaultProjectile?.projectile?.GetDamageAmount(parent, damageDesc) ?? 0f;
            damageAmount = CurrentMode.verb.verbClass == typeof(Verb_DragonBreath) ? 13f : damageAmount;
            if (damageAmount > 0)
            {
                yield return new StatDrawEntry(statCat, (string) "Damage".Translate(), damageAmount.ToString(), damageDesc.ToString(), 5500);
            }
            // if (CurrentMode.verb.defaultProjectile?.projectile?.damageDef?.armorCategory != null)
            {
              StringBuilder explanation2 = new StringBuilder();
              float armorPenetration = CurrentMode.verb?.defaultProjectile?.projectile?.GetArmorPenetration(parent, explanation2) ?? 0f;
              armorPenetration = CurrentMode.verb?.verbClass == typeof(Verb_DragonBreath) ? 0.2f : armorPenetration;
              if (armorPenetration > 0)
              {
                  TaggedString reportText = "ArmorPenetrationExplanation".Translate();
                  if (explanation2.Length != 0)
                      reportText += "\n\n" + explanation2?.ToString();
                  yield return new StatDrawEntry(statCat, (string) "ArmorPenetration".Translate(), armorPenetration.ToStringPercent(), (string) reportText, 5400);
              }
            }
            if (CurrentMode.verb.defaultProjectile?.projectile?.damageDef != null) {
                float buildingDamageFactor = CurrentMode.verb.defaultProjectile.projectile.damageDef.buildingDamageFactor;
                var dmgBuildingsImpassable = CurrentMode.verb.defaultProjectile.projectile.damageDef.buildingDamageFactorImpassable;
                var dmgBuildingsPassable = CurrentMode.verb.defaultProjectile.projectile.damageDef.buildingDamageFactorPassable;
                if ((double) buildingDamageFactor != 1.0)
                    yield return new StatDrawEntry(statCat, (string) "BuildingDamageFactor".Translate(), buildingDamageFactor.ToStringPercent(), (string) "BuildingDamageFactorExplanation".Translate(), 5410);
                if ((double) dmgBuildingsImpassable != 1.0)
                    yield return new StatDrawEntry(statCat, (string) "BuildingDamageFactorImpassable".Translate(), dmgBuildingsImpassable.ToStringPercent(), (string) "BuildingDamageFactorImpassableExplanation".Translate(), 5420);
                if ((double) dmgBuildingsPassable != 1.0)
                    yield return new StatDrawEntry(statCat, (string) "BuildingDamageFactorPassable".Translate(), dmgBuildingsPassable.ToStringPercent(), (string) "BuildingDamageFactorPassableExplanation".Translate(), 5430);

                float burstShotCount = (float)CurrentMode.verb.burstShotCount;
                float betweenBurstShots = (float)CurrentMode.verb.ticksBetweenBurstShots;
                dmgBuildingsPassable = CurrentMode.verb?.defaultProjectile.projectile?.stoppingPower ?? 0f;
                StringBuilder stringBuilder2 =
                    new StringBuilder((string)"Stat_Thing_Weapon_BurstShotCount_Desc".Translate());
                stringBuilder2.AppendLine();
                stringBuilder2.AppendLine();
                stringBuilder2.AppendLine((string)("StatsReport_BaseValue".Translate() + ": " +
                                                   CurrentMode.verb.burstShotCount.ToString()));
                stringBuilder2.AppendLine();
                StringBuilder ticksBetweenBurstShotsExplanation =
                    new StringBuilder((string)"Stat_Thing_Weapon_BurstShotFireRate_Desc".Translate());
                ticksBetweenBurstShotsExplanation.AppendLine();
                ticksBetweenBurstShotsExplanation.AppendLine();
                ticksBetweenBurstShotsExplanation.AppendLine((string)("StatsReport_BaseValue".Translate() + ": " +
                                                                      (60f / CurrentMode.verb.ticksBetweenBurstShots
                                                                          .TicksToSeconds()).ToString("0.##") +
                                                                      " rpm"));
                ticksBetweenBurstShotsExplanation.AppendLine();
                StringBuilder stoppingPowerExplanation =
                    new StringBuilder((string)"StoppingPowerExplanation".Translate());
                stoppingPowerExplanation.AppendLine();
                stoppingPowerExplanation.AppendLine();
                stoppingPowerExplanation.AppendLine((string)("StatsReport_BaseValue".Translate() + ": " +
                                                             dmgBuildingsPassable.ToString("F1")));
                stoppingPowerExplanation.AppendLine();

                stringBuilder2.AppendLine();
                stringBuilder2.AppendLine((string)("StatsReport_FinalValue".Translate() + ": " +
                                                   Mathf.CeilToInt(burstShotCount).ToString()));
                dmgBuildingsImpassable = 60f / ((int)betweenBurstShots).TicksToSeconds();
                ticksBetweenBurstShotsExplanation.AppendLine();
                ticksBetweenBurstShotsExplanation.AppendLine((string)("StatsReport_FinalValue".Translate() + ": " +
                                                                      dmgBuildingsImpassable.ToString("0.##") +
                                                                      " rpm"));
                stoppingPowerExplanation.AppendLine();
                stoppingPowerExplanation.AppendLine((string)("StatsReport_FinalValue".Translate() + ": " +
                                                             dmgBuildingsPassable.ToString("F1")));
                if (CurrentMode.verb.showBurstShotStats && CurrentMode.verb.burstShotCount > 1)
                {
                    yield return new StatDrawEntry(statCat, (string)"BurstShotCount".Translate(),
                        Mathf.CeilToInt(burstShotCount).ToString(), stringBuilder2.ToString(), 5391);
                    yield return new StatDrawEntry(statCat, (string)"BurstShotFireRate".Translate(),
                        dmgBuildingsImpassable.ToString("0.##") + " rpm", ticksBetweenBurstShotsExplanation.ToString(),
                        5395);
                }

                if ((double)dmgBuildingsPassable > 0.0)
                    yield return new StatDrawEntry(statCat, (string)"StoppingPower".Translate(),
                        dmgBuildingsPassable.ToString("F1"), stoppingPowerExplanation.ToString(), 5402);
            }
            float a = CurrentMode.verb.range;
            StringBuilder stringBuilder3 = new StringBuilder((string) "Stat_Thing_Weapon_Range_Desc".Translate());
            stringBuilder3.AppendLine();
            stringBuilder3.AppendLine();
            stringBuilder3.AppendLine((string) ("StatsReport_BaseValue".Translate() + ": " + a.ToString("F0")));
            {
              float statValue = parent.GetStatValue(StatDefOf.RangedWeapon_RangeMultiplier);
              a *= statValue;
              if (!Mathf.Approximately(statValue, 1f))
              {
                stringBuilder3.AppendLine();
                stringBuilder3.AppendLine((string) ("Stat_Thing_Weapon_Range_Multiplier".Translate() + ": x" + statValue.ToStringPercent()));
                stringBuilder3.Append(StatUtility.GetOffsetsAndFactorsFor(StatDefOf.RangedWeapon_RangeMultiplier, parent));
              }
              Map map = parent.Map ?? parent.MapHeld;
              if ((map != null ? ((double) map.weatherManager.CurWeatherMaxRangeCap >= 0.0 ? 1 : 0) : 0) != 0)
              {
                WeatherManager weatherManager = (parent.Map ?? parent.MapHeld).weatherManager;
                int num1 = (double) a > (double) weatherManager.CurWeatherMaxRangeCap ? 1 : 0;
                float num2 = a;
                a = Mathf.Min(a, weatherManager.CurWeatherMaxRangeCap);
                if (num1 != 0)
                {
                  stringBuilder3.AppendLine();
                  stringBuilder3.AppendLine((string) ("    " + "Stat_Thing_Weapon_Range_Clamped".Translate(a.ToString("F0").Named("CAP"), num2.ToString("F0").Named("ORIGINAL"))));
                }
              }
            }
            stringBuilder3.AppendLine();
            stringBuilder3.AppendLine((string) ("StatsReport_FinalValue".Translate() + ": " + a.ToString("F0")));
            yield return new StatDrawEntry(statCat, (string) "Range".Translate(), a.ToString("F0"), stringBuilder3.ToString(), 5390);
            
            if (CurrentMode.newStat?.AccuracyTouch != null) yield return new StatDrawEntry(statCat, StatDefOf.AccuracyTouch,
                Math.Clamp(CurrentMode.newStat.AccuracyTouch * getQualityApproximateFactor(), 0f, 1f), StatRequest.ForEmpty());
            if (CurrentMode.newStat?.AccuracyShort != null) yield return new StatDrawEntry(statCat, StatDefOf.AccuracyShort,
                Math.Clamp(CurrentMode.newStat.AccuracyShort * getQualityApproximateFactor(), 0f, 1f), StatRequest.ForEmpty());
            if (CurrentMode.newStat?.AccuracyMedium != null) yield return new StatDrawEntry(statCat, StatDefOf.AccuracyMedium,
                Math.Clamp(CurrentMode.newStat.AccuracyMedium * getQualityApproximateFactor(), 0f, 1f), StatRequest.ForEmpty());
            if (CurrentMode.newStat?.AccuracyLong != null) yield return new StatDrawEntry(statCat, StatDefOf.AccuracyLong,
                Math.Clamp(CurrentMode.newStat.AccuracyLong * getQualityApproximateFactor(), 0f, 1f), StatRequest.ForEmpty());
            if (CurrentMode.newStat?.Cooldown != null) yield return new StatDrawEntry(statCat, StatDefOf.RangedWeapon_Cooldown, CurrentMode.newStat.Cooldown.ToString("F2") + " " + "LetterSecond".Translate());
            // 瞄准时间
            float warmupTime = CurrentMode.verb.warmupTime;
            StringBuilder stringBuilder1 = new StringBuilder((string) "Stat_Thing_Weapon_RangedWarmupTime_Desc".Translate());
            stringBuilder1.AppendLine();
            stringBuilder1.AppendLine();
            stringBuilder1.AppendLine((string) ("StatsReport_BaseValue".Translate() + ": " + warmupTime.ToString("0.##") + " " + "LetterSecond".Translate()));
            if ((double) warmupTime > 0.0)
            {
                {
                    float statValue = parent.GetStatValue(StatDefOf.RangedWeapon_WarmupMultiplier);
                    warmupTime *= statValue;
                    if (!Mathf.Approximately(statValue, 1f))
                    {
                        stringBuilder1.AppendLine();
                        stringBuilder1.AppendLine((string) ("Stat_Thing_Weapon_WarmupTime_Multiplier".Translate() + ": x" + statValue.ToStringPercent()));
                        stringBuilder1.Append(StatUtility.GetOffsetsAndFactorsFor(StatDefOf.RangedWeapon_WarmupMultiplier, parent));
                    }
                }
                stringBuilder1.AppendLine();
                stringBuilder1.AppendLine((string) ("StatsReport_FinalValue".Translate() + ": " + warmupTime.ToString("0.##") + " " + "LetterSecond".Translate()));
                yield return new StatDrawEntry(statCat, (string) "RangedWarmupTime".Translate(), (string) (warmupTime.ToString("0.##") + " " + "LetterSecond".Translate()), stringBuilder1.ToString(), 1000);
            }
        }
    }

    private float getQualityApproximateFactor()
    {
        if (!parent.TryGetComp<CompQuality>(out var comp)) return 1f;
        if (comp.Quality < QualityCategory.Masterwork)
        {
            return 0.8f + (int)comp.Quality * 0.1f;
        }
        if (comp.Quality == QualityCategory.Masterwork) return 1.35f;
        if (comp.Quality == QualityCategory.Legendary) return 1.5f;
        return 1f;
    }
    
}