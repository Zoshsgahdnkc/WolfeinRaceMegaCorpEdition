using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WRMegaCorp;

public class Verb_DragonBreath: Verb
{
  protected override bool TryCastShot()
  {
    if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
      return false;
    if (this.EquipmentSource != null)
    {
      this.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
      this.EquipmentSource.GetComp<CompApparelReloadable>()?.UsedOnce();
    }
    
    IntVec3 position = this.caster.Position;
    float num = Mathf.Atan2((float) -(this.currentTarget.Cell.z - position.z), (float) (this.currentTarget.Cell.x - position.x)) * 57.29578f;
    FloatRange floatRange = new FloatRange(num - 13f, num + 13f);
    IntVec3 center = position;
    Map mapHeld = this.caster.MapHeld;
    double effectiveRange = (double) this.EffectiveRange;
    DamageDef flame = WRMC_DefOfs.WRMC_NonIgnitionFlame;
    Thing caster = this.caster;
    // 不生成胆汁地形
    // ThingDef filthFlammableBile = ThingDefOf.Filth_FlammableBile;
    FloatRange? nullable = new FloatRange?(floatRange);
    GasType? postExplosionGasType = new GasType?();
    float? postExplosionGasRadiusOverride = new float?();
    float? direction = new float?();
    FloatRange? affectedAngle = nullable;
    GenExplosion.DoExplosion(center, mapHeld, (float) effectiveRange, flame, caster, 13, 0.2f, postExplosionSpawnChance: 1f, postExplosionGasType: postExplosionGasType, postExplosionGasRadiusOverride: postExplosionGasRadiusOverride, chanceToStartFire: 0f, direction: direction, affectedAngle: affectedAngle, doVisualEffects: false, propagationSpeed: 0.6f, doSoundEffects: false);
    // this.AddEffecterToMaintain(WRMC_DefOfs.WRMC_DragonBreath.Spawn(this.caster.Position, this.currentTarget.Cell, this.caster.Map), this.caster.Position, this.currentTarget.Cell, 14, this.caster.Map);
    SpawnDragonBreathVfx(this.caster.Position, this.currentTarget.Cell, this.caster.Map);
    SoundDef.Named("Flamethrower_Firing_Resolve").PlayOneShot(SoundInfo.InMap(caster));
    this.lastShotTick = Find.TickManager.TicksGame;
    if (CasterIsPawn) CasterPawn.records.Increment(RecordDefOf.ShotsFired);
    return true;
  }

  public override bool Available()
  {
    if (!base.Available())
      return false;
    if (this.CasterIsPawn)
    {
      Pawn casterPawn = this.CasterPawn;
      if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
        return false;
    }
    return true;
  }
  private const int NumStreams = 15;
  private const float VisualRange = 8f;
  private const float ConeSizeDegrees = 13f;
  private const float BarrelOffsetDistance = 3.0f; // 注意：一般是“格”的小数，不建议 6f
  private const float SizeReductionDistanceThreshold = 8f;
  private const int LifespanNoise = 40;
  private const float RangeNoise = 0.4f;
  
    private void SpawnDragonBreathVfx(IntVec3 fromCell, IntVec3 targetCell, Map map)
    {
        if (map == null) return;

        // 承载器：每次施放一个
        var spray = GenSpawn.Spawn(ThingDefOf.IncineratorSpray, fromCell, map) as IncineratorSpray;
        if (spray == null) return;

        // 基础方向：由施法者 -> 目标
        Vector3 casterPos = CasterPawn?.DrawPos ?? fromCell.ToVector3Shifted();
        Vector3 baseDir = (targetCell.ToVector3Shifted() - casterPos);
        if (baseDir == Vector3.zero) baseDir = Vector3.forward;
        baseDir.y = 0f;
        baseDir.Normalize();

        for (int i = 0; i < NumStreams; i++)
        {
            // 1) 锥形随机偏转
            float angle = Rand.Range(-ConeSizeDegrees, ConeSizeDegrees);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;
            dir.y = 0f;
            dir.Normalize();

            // 2) 枪口前移
            Vector3 worldSource = casterPos + dir * BarrelOffsetDistance;
            IntVec3 sourceCell = worldSource.ToIntVec3();

            // 3) 距离噪声
            float noisyRange = VisualRange + Rand.Value * RangeNoise;
            Vector3 unclampedTarget = casterPos + dir * noisyRange;
            IntVec3 destCell = unclampedTarget.ToIntVec3();

            // 防止出界
            if (!destCell.InBounds(map)) continue;

            // 4) 近距离缩放衰减
            float dist = (unclampedTarget - worldSource).MagnitudeHorizontal();
            float t = Mathf.Clamp01(dist / Mathf.Max(0.01f, SizeReductionDistanceThreshold)); // 0~1
            float startScale = Mathf.Lerp(0.35f, 1f, t);
            float endScale = Mathf.Lerp(0.6f, 1.2f, t);

            // 5) 建立 mote
            MoteDualAttached mote = MoteMaker.MakeInteractionOverlay(
                ThingDefOf.Mote_IncineratorBurst,
                new TargetInfo(sourceCell, map),
                new TargetInfo(destCell, map)
            );
            if (mote == null) continue;

            // 6) 生命周期（含噪声）
            int life = Mathf.FloorToInt(dist * 5f) + Rand.Range(-LifespanNoise, LifespanNoise);
            life = (int) (life * 0.5f);
            if (life < 8) life = 8;

            // 7) 挂到 IncineratorProjectileMotion
            spray.Add(new IncineratorProjectileMotion
            {
                mote = mote,
                targetDest = destCell, // 记录
                worldSource = worldSource,
                worldTarget = unclampedTarget,
                moveVector = dir,
                startScale = startScale,
                endScale = endScale,
                lifespanTicks = life
            });

            // 8) 落点 effecter（可选，类似 BurnerUsed）
            // map.effecterMaintainer.AddEffecterToMaintain(
            //     EffecterDefOf.BurnerUsed.Spawn(destCell, map),
            //     destCell,
            //     100
            // );
        }
    }
}