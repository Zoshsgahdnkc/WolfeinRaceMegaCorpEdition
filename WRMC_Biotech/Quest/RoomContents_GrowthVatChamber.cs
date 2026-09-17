using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Wolfein;

namespace WRMegaCorp;

public class RoomContents_GrowthVatChamber: RoomContentsWorker
{
    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
    {
        IntVec3 cell;
        WolfeinGrowthVat? vat = ThingMaker.MakeThing(WRMC_Biotech_DefOfs.WRMC_WolfeinGrowthVat) as WolfeinGrowthVat;
        if (!room.TryGetRandomCellInRoom(map, out cell, 3))
            cell = room.rects[0].CenterCell;
        if (vat == null)
        {
            WRMC_Utils.LogError("Cannot make wolfein growth vat");
            WRMC_Utils.LogError($"Type: {ThingMaker.MakeThing(WRMC_Biotech_DefOfs.WRMC_WolfeinGrowthVat).GetType().Name}");
        }
        else
        {
            if (vat.questTags == null)
            {
                vat.questTags = new List<string>() { "ImportantGrowthVat" };
            }
            else
            {
                vat.questTags.Add("ImportantGrowthVat");
            }
            GenSpawn.Spawn(vat, cell, map, Rot4.South);

            var req = new PawnGenerationRequest(
                PawnKindDef.Named("Wolfein_OutlierWolf"),
                null,
                allowDowned: true,
                canGeneratePawnRelations: false,
                fixedBiologicalAge:3,
                fixedChronologicalAge:3,
                forceNoIdeo: true,
                forceRecruitable: true);
            var baby = PawnGenerator.GeneratePawn(req);
            if (baby != null)
            {
                WRMC_Utils.LogDebugMessage($"Generating wolfein baby: {baby.Label}, {baby.ageTracker.AgeBiologicalYears}");
                
                float dmg = Rand.Range(0f, 4f);
                baby.TakeDamage(new DamageInfo(HealthUtility.RandomViolenceDamageType(), dmg));
                Hediff hediff = baby.health.AddHediff(WolfeinDefOf.Wolfein_Abasia);
                if (hediff.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears disappearsComp)
                {
                    disappearsComp.SetDuration(GenDate.TicksPerDay);
                }
                
                baby.inventory.DestroyAll();
                baby.apparel.DestroyAll();
                var clothes = ThingMaker.MakeThing(ThingDef.Named("Wolfein_T-shirt"), ThingDefOf.Cloth) as Apparel;
                clothes.compQuality.SetQuality(QualityCategory.Normal, null);
                baby.apparel.Wear(clothes);
                baby.mindState.WillJoinColonyIfRescued = true;

                vat.SelectedPawn = baby;
                vat.innerContainer.TryAddOrTransfer(baby);
                vat.SetSomeNutrition(Rand.Range(1f, 3f));
                vat.SetStartTick(Find.TickManager.TicksGame);

                Pawn mother = GenerateMotherFor(baby, faction);
                IntVec3? motherCell = null;
                CellRect cellRect = CellRect.CenteredOn(cell, 2);
                for (int tries = 0; tries < 20; tries++)
                {
                    IntVec3 possibleCell = cellRect.EdgeCells.RandomElement();
                    if (possibleCell.GetEdifice(map) == null)
                    {
                        motherCell = possibleCell;
                        break;
                    }
                }

                if (motherCell == null)
                {
                    for (int tries = 0; tries < 20; tries++)
                    {
                        IntVec3 possibleCell = cellRect.EdgeCells.RandomElement();
                        if (!possibleCell.Filled(map))
                        {
                            motherCell = possibleCell;
                            break;
                        }
                    }
                }
                motherCell ??= vat.InteractionCell;
                // mother.guest.Recruitable = true;
                GenSpawn.Spawn(mother, motherCell.Value, map);
                
                // float hoursSinceQuest = (Find.TickManager.TicksGame - map.Parent.creationGameTicks) / (float)GenDate.TicksPerHour;
                float hoursSinceQuest = 18;
                WRMC_Utils.LogDebugMessage($"Gen site at {map.Parent.creationGameTicks}, Gen map at {Find.TickManager.TicksGame}, hours passed :{(int)hoursSinceQuest}");
                // 6小时以内存活
                if (hoursSinceQuest < 6)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float amount = Rand.Range(5f, 10f);
                        var damDef = HealthUtility.RandomViolenceDamageType();
                        mother.TakeDamage(new DamageInfo(damDef, amount));
                        // 如果17小时内死亡则不继续
                        if (HealthUtility.TicksUntilDeathDueToBloodLoss(mother) < GenDate.TicksPerDay * 0.7) break;
                    }
                    if (mother.Dead)
                    {
                        WRMC_Utils.LogError("Mother died. This is not expected.");
                    }
                    else
                    {
                        Hediff hediff_ = mother.health.AddHediff(WolfeinDefOf.Wolfein_Abasia);
                        if (hediff_.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears dis)
                        {
                            dis.SetDuration(GenDate.TicksPerDay * 2);
                        }
                    }
                }
                // 24小时以外死亡
                else if (hoursSinceQuest > 24 && !mother.Dead)
                {
                    HealthUtility.DamageUntilDead(mother);
                }
                // 如非以上两种情况，根据到达时间额外伤害，不保证存活
                else
                {
                    int tries = (int)((hoursSinceQuest - 5)/2) + 6;
                    for (int i = 0; i < tries; i++)
                    {
                        float amount = Rand.Range(10f, 15f);
                        var damDef = HealthUtility.RandomViolenceDamageType();
                        mother.TakeDamage(new DamageInfo(damDef, amount, 0.2f));
                        WRMC_Utils.LogDebugMessage($"Randomly injuring wolfein mother - try:{i + 1}/{tries}, amount:{amount}, died:{mother.Dead}");
                        if (!mother.Dead)
                        {
                            Hediff hediff_ = mother.health.AddHediff(WolfeinDefOf.Wolfein_Abasia);
                            if (hediff_.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears dis)
                            {
                                dis.SetDuration(GenDate.TicksPerDay * 2);
                            }
                        }
                        else break;
                    }
                }
                if(!mother.Dead) mother.mindState.WillJoinColonyIfRescued = true;
                
                // map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Buildings | MapMeshFlagDefOf.Things);
                var letterProps = vat.GetComp<CompLetterOnRevealed>().Props;
                letterProps.label = "WRMC.QuestTheRescue.RevealGrowthVat.Letter".Translate();
                letterProps.text = "WRMC.QuestTheRescue.RevealGrowthVat.Text".Translate(baby.Named("PAWN")).Resolve();
                letterProps.letterDef = LetterDefOf.PositiveEvent;
                
                // Log.Message(new Signal( $"{vat.questTags[0]}.Unfogged", vat));
            }
            else
            {
                WRMC_Utils.LogDebugMessage("Cannot generate wolfein baby");
            }
        }
        
        base.FillRoom(map, room, faction, threatPoints);
    }

    private static Pawn GenerateMotherFor(Pawn baby, Faction faction)
    {
        float babyAge = 3;
        float minMotherAge = 20;
        float maxMotherAge = 40;
        float midMotherAge = 28;
        float age = Rand.GaussianAsymmetric(
            midMotherAge,
            (midMotherAge - minMotherAge) / 2f,
            (maxMotherAge - midMotherAge) / 2f);
        age = Mathf.Clamp(age, minMotherAge, maxMotherAge);

        string lastName = null;
        if (baby.Name is NameTriple nameTriple)
        {
            lastName = nameTriple.Last;
        }

        Pawn mother = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
            PawnKindDef.Named("Wolfein_MegaCorpCivilian"),
            null,
            PawnGenerationContext.NonPlayer,
            null,
            forceGenerateNewPawn: true,
            allowDead: true,
            allowDowned: true,
            canGeneratePawnRelations: false,
            mustBeCapableOfViolence: false,
            1f,
            forceAddFreeWarmLayerIfNeeded: false,
            allowGay: false,
            allowPregnant: false,
            allowFood: true,
            allowAddictions: true,
            inhabitant: false,
            certainlyBeenInCryptosleep: false,
            forceRedressWorldPawnIfFormerColonist: false,
            worldPawnFactionDoesntMatter: false,
            0f, 0f,
            null, 1f,
            null, null, null, null, null,
            age, age,
            Gender.Female, // 固定女性
            lastName, null, null, null,
            forceNoIdeo: false,
            forceNoBackstory: false,
            forbidAnyTitle: false,
            forceDead: false,
            forceRecruitable:true
        ));

        if (!Find.WorldPawns.Contains(mother))
        {
            Find.WorldPawns.PassToWorld(mother);
        }
        
        // 随机增长技能20次
        for (int tries = 0; tries < 20; tries++)
        {
            var skill = mother.skills.skills.Where(s => !s.TotallyDisabled).RandomElementByWeight(s => ((int)s.passion * 4) + s.Level);
            if (skill == null) break;
            mother.skills.Learn(skill.def, 2500, true);
        }

        baby.SetMother(mother);
        
        mother.inventory.DestroyAll();
        mother.equipment.AddEquipment(ThingMaker.MakeThing(ThingDef.Named("W_Weapon_HG_Pistol")) as ThingWithComps);
        mother.SetFaction(null);
        
        return mother;
    }
}