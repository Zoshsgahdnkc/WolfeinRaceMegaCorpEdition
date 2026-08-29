using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WRMegaCorp;

public class Recipe_InstallPrototypeBodyPart: Recipe_Surgery
{
  
  public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
  {
    return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, (Func<BodyPartRecord, bool>) (record =>
    {
      IEnumerable<Hediff> source = pawn.health.hediffSet.hediffs.Where<Hediff>((Func<Hediff, bool>) (x => x.Part == record));
      if (typeof (Hediff_AddedPart).IsAssignableFrom(recipe.addsHediff.hediffClass))
      {
        if (source.Count<Hediff>() == 1 && source.First<Hediff>().def == recipe.addsHediff)
          return false;
      }
      else if (source.Any<Hediff>((Func<Hediff, bool>) (hd => hd.def == recipe.addsHediff)))
        return false;
      return (record.parent == null || pawn.health.hediffSet.GetNotMissingParts().Contains<BodyPartRecord>(record.parent)) && (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record));
    }));
  }

  public override void ApplyOnPawn(
    Pawn pawn,
    BodyPartRecord part,
    Pawn billDoer,
    List<Thing> ingredients,
    Bill bill)
  {
    bool flag1 = MedicalRecipesUtility.IsClean(pawn, part);
    bool flag2 = !PawnGenerator.IsBeingGenerated(pawn) && this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
    Hediff hediff = (Hediff) null;
    if (billDoer != null)
    {
      // 安装时不失败
      // if (this.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
      //   return;
      TaleRecorder.RecordTale(TaleDefOf.DidSurgery, (object) billDoer, (object) pawn);
      hediff = pawn.health.hediffSet.GetDirectlyAddedPartFor(part);
      if (part != null)
        MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, billDoer.Position, billDoer.Map);
      if (flag1 & flag2 && part.def.spawnThingOnRemoved != null)
        ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
      if (flag2)
        this.ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
      if (ModsConfig.IdeologyActive)
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InstalledProsthetic, billDoer.Named(HistoryEventArgsNames.Doer)));
    }
    else if (pawn.Map != null)
    {
      if (part != null)
        MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, pawn.Position, pawn.Map);
    }
    else if (part != null)
      pawn.health.RestorePart(part);
    pawn.health.AddHediff(this.recipe.addsHediff, part);
    hediff?.Notify_SurgicallyReplaced(billDoer);
    
    WRMC_Utils.LogDebugMessage("Surgery Recipe Completed.");
    
    // 任务提示
    foreach (var quest in Find.QuestManager.ActiveQuestsListForReading)
    {
      foreach (var questPart in quest.PartsListForReading)
      {
        if (questPart is QuestPart_TrackingBionic questPartTrackingBionic)
        {
          questPartTrackingBionic.NotifyPrototypeBodyPartInstalled(pawn, ingredients);
        }
      }
    }
  }

  public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
  {
    return (pawn.Faction != billDoerFaction && pawn.Faction != null || pawn.IsQuestLodger()) && (this.recipe.addsHediff.addedPartProps == null || !this.recipe.addsHediff.addedPartProps.betterThanNatural) && HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;
  }
}