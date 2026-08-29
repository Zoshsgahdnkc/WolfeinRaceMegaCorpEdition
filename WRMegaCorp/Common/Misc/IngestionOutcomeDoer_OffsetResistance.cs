using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp;

public class IngestionOutcomeDoer_OffsetResistance: IngestionOutcomeDoer
{
    public float reduceResistanceValue = 0.8f;
    public float reduceWillValue = 0.6f;
    public float reduceCertaintyValue = 0.08f;
    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
    {
        if (!pawn.IsPrisonerOfColony || pawn.guest == null) return;
        
        // 应用种族乘数
        // 对非沃芬族，在加载生物科技和沃芬基因拓展时，如果有肉食基因，使用两个乘数的中间值
        float multiplier = 0.15f;
        var mul1 = WRMegaCorpEdition.modSettings?.HoneyGlazedHamEffectMultiplier_Wolfein ?? 1f;
        var mul2 = WRMegaCorpEdition.modSettings?.HoneyGlazedHamEffectMultiplier_NonWolfein ?? 0.15f;
        if (pawn.def.defName != "Wolfein_Race")
        {
            if (ModsConfig.BiotechActive && ModsConfig.IsActive("ancot.wolfeinracegenepatch") && pawn.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Wolfein_MeatLover")))
            {
                multiplier = (mul1 + mul2) * 0.5f;
            }
            else multiplier = mul2;
        }
        else multiplier = mul1;
        
        // 根据特性应用额外乘数
        // 苦行者
        if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return;
        // 贪食者
        if (pawn.story.traits.HasTrait(TraitDef.Named("Gourmand"))) multiplier += 0.5f;
        // 贪婪
        if (pawn.story.traits.HasTrait(TraitDefOf.Greedy)) multiplier *= 1.1f;
        
        // 转化死忠的部分
        if (pawn.IsPrisonerOfColony && !pawn.guest.Recruitable && WRMegaCorpEdition.modSettings.ResistanceReducingFoodRemoveUnwavering)
        {
            float baseCritChance = multiplier * 0.03f;
            if (Rand.Chance(baseCritChance))
            {
                var label = "WRMC.RemoveUnwaveringByFood.Letter".Translate(pawn.Named("PAWN"));
                TaggedString HometownOrNot = pawn.def.defName == "Wolfein_Race" ? "WRMC.RemoveUnwaveringByFood.HometownOrNot".Translate() : TaggedString.Empty;
                var text = "WRMC.RemoveUnwaveringByFood.Text".Translate(pawn.Named("PAWN"), ingested.LabelShort.Named("FOOD"), pawn.Faction.Name.Named("FACTION"), HometownOrNot.Named("HOMETOWNORNOT"));
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(label, text, LetterDefOf.PositiveEvent, pawn));
                pawn.guest.Recruitable = true;
                pawn.guest.resistance = 0;
            }
        }
        
        // 降低抵抗的部分
        if (pawn.guest.Recruitable && (pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.AttemptRecruit) || pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.ReduceResistance)))
        {
            float res = pawn.guest.resistance;
            if (res > 0)
            {
                float sub = reduceResistanceValue * Rand.Range(0.7f, 1.3f) * multiplier;
                // 如果减少后的抵抗数值在容忍值（0.1）以内，则相当于直接打破抵抗
                if (res - sub < 0.1f)
                {
                    string text = "WRMC.ResistanceReduceToZero.Message".Translate(pawn.Named("PRISONER"), ingested.LabelShort.Named("FOOD"));
                    if (pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.AttemptRecruit)) text += " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
                    Messages.Message(text, pawn, MessageTypeDefOf.PositiveEvent);
                    pawn.guest.resistance = 0;
                }
                else
                {
                    pawn.guest.resistance = res - sub;
                }
            }
            return;
        }
        
        if (!ModsConfig.IdeologyActive) return;
        // 降低意志的部分
        if (pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Enslave) || pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.ReduceWill))
        {
            float will = pawn.guest.will;
            if (will > 0)
            {
                float sub = reduceWillValue * Rand.Range(0.7f, 1.3f) * multiplier;
                // 如果减少后的意志数值在容忍值（0.1）以内，则相当于直接打破意志
                if (will - sub < 0.1f)
                {
                    string text = "WRMC.WillReduceToZero.Message".Translate(pawn.Named("PRISONER"), ingested.LabelShort.Named("FOOD"));
                    if (pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.AttemptRecruit)) text += " " + "MessagePrisonerWillBroken_RecruitAttempsWillBegin".Translate();
                    Messages.Message(text, pawn, MessageTypeDefOf.PositiveEvent);
                    pawn.guest.will = 0;
                }
                else
                {
                    pawn.guest.will = will - sub;
                }
            }
            return;
        }
        // 教化的部分
        if (pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Convert))
        {
            Ideo targetIdeo = pawn.guest.ideoForConversion;
            Ideo oldIdeo = pawn.ideo.Ideo;
            var role = oldIdeo.GetRole(pawn);
            if (targetIdeo == null || targetIdeo == oldIdeo) return;
            if (pawn.ideo.Certainty > 0f)
            {
                float sub = reduceCertaintyValue * Rand.Range(0.7f, 1.3f) * multiplier;
                if (pawn.ideo.IdeoConversionAttempt(sub, targetIdeo, true))
                {
                    var label = "LetterLabelConvertIdeoAttempt_Success".Translate();
                    var text = "WRMC.ConvertIdeo.Text".Translate(
                        pawn.Named("PRISONER"),
                        ingested.LabelShort.Named("FOOD"),
                        oldIdeo.Named("OLDIDEO"),
                        targetIdeo.Named("NEWIDEO"));
                    if (role != null)
                    {
                        var textRole = "LetterRoleLostLetterIdeoChangedPostfix".Translate(pawn.Named("PRISONER"), role.Named("ROLE"), oldIdeo.Named("OLDIDEO"));
                        text += textRole;
                    }
                    Letter let = LetterMaker.MakeLetter(label, text, LetterDefOf.PositiveEvent, pawn);
                    Find.LetterStack.ReceiveLetter(let);
                }
            }
        }
    }
}