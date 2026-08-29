using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace WRMegaCorp;

public class QuestNode_StartTrackingBionic: QuestNode
{
    protected override void RunInt()
    {
        var quest = QuestGen.quest;
        var part = new QuestPart_TrackingBionic();
        part.map = QuestGen.slate.Get<Map>("map");
        part.thingDef = QuestGen.slate.Get<ThingDef>("bionic");
        // part.outSignalsCompleted = new List<string>() {$"Quest{quest.id.ToString()}.ExperimentAllComplete"};
        part.outSignalsCompleted = new List<string>() {QuestGenUtility.HardcodedSignalWithQuestID("ExperimentAllComplete")};
        quest.AddPart(part);
    }

    protected override bool TestRunInt(Slate slate) => true;
}

public class QuestPart_TrackingBionic : QuestPartActivable
{
    public Map map;
    public ThingDef thingDef;
    public Pawn pawn;
    public int LastExaminationTick;
    public bool isExaminationReady;
    // 这应该是Lazy吧，不太明白这个语法糖，是想要tick的时候减少一点开销
    public static int ExaminationTickInterval
    {
        get
        {
            if (field == 0) field = (int)Math.Round((WRMegaCorpEdition.modSettings?.QImplantExperiment_SurgeryInterval ?? 36) * GenDate.TicksPerHour);
            return field;
        }
    }

    public static int MaxExaminationNeeded => WRMegaCorpEdition.modSettings?.QImplantExperiment_RequiredSurgeryCount??4;
    public int ExecutedExamination = 0;
    
    // 检查冷却结束时，发送消息
    public override void QuestPartTick()
    {
        if (isExaminationReady) return;
        if ((Find.TickManager.TicksAbs > (LastExaminationTick + ExaminationTickInterval)) && pawn != null)
        {
            isExaminationReady = true;
            WRMC_Utils.LogDebugMessage("Quest Part Examination Ready!");
            Messages.Message("WRMC.QuestImplantExperiment.ExaminationReady.Message".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
        };
    }
    
    // 检测到安装试制型仿生体后调用这个方法
    public void NotifyPrototypeBodyPartInstalled(Pawn pawn, List<Thing> ingredients)
    {
        if (!ingredients.Any(t => t.def == thingDef)) return;
        WRMC_Utils.LogDebugMessage("Prototype Body Part Installed! Quest Part notified.");
        Enable(new SignalArgs());
        this.pawn = pawn;
        ExecutedExamination = 0;
        Hyperlinks.AddItem(new Dialog_InfoCard.Hyperlink(pawn));
        LastExaminationTick = Find.TickManager.TicksAbs;
    }

    // 每次检查手术后调用该方法，如果在殖民地内没有检测到之前的pawn
    public void NotifyExamination(Pawn pawn, Pawn billDoer)
    {
        WRMC_Utils.LogDebugMessage($"NOTIFY EXAMINATION=========================");
        WRMC_Utils.LogDebugMessage($"PartTrackingPawn: {this.pawn.LabelShort}");
        WRMC_Utils.LogDebugMessage($"Pawn: {pawn.LabelShort}");
        WRMC_Utils.LogDebugMessage($"BillDoer: {billDoer.LabelShort}");
        ExecutedExamination += 1;
        WRMC_Utils.LogDebugMessage($"1-Examination Executed: {ExecutedExamination - 1} -> {ExecutedExamination}");
        LastExaminationTick = Find.TickManager.TicksAbs;
        isExaminationReady = false;
        // 为了避免可能的null，加一条检测
        if (thingDef == null) thingDef = WRMC_DefOfs.WRMC_PrototypeBionicHeart_Thing;
        try
        {
            Find.LetterStack.ReceiveLetter(MakeExaminationLetter(quest, pawn, billDoer, thingDef));
        }
        catch (Exception e)
        {
            WRMC_Utils.LogWarning("Failed to make examination letter");
            WRMC_Utils.LogWarning(e.ToString());
        }
        // 全部完成后，进入下面的分支，完成任务并解锁仿生体购买
        if (ExecutedExamination >= MaxExaminationNeeded) finishQuest();
    }

    public void finishQuest()
    {
        WRMC_Utils.LogDebugMessage($"EnteringFinishQuestMethod");
        
        if (UnlockBionicGameComponent.Get()?.UnlockThing(thingDef) != true) WRMC_Utils.LogError($"Could not unlock thingDef {thingDef}");

        try
        {
            var label = "WRMC.QuestImplantExperiment.Complete.Letter".Translate();
            var text = "WRMC.QuestImplantExperiment.Complete.Text".Translate(quest.name, thingDef.label);
            var let = LetterMaker.MakeLetter(label, text, LetterDefOf.PositiveEvent, LookTargets.Invalid,
                Find.FactionManager.OfMegaCorp(), quest);
            Find.LetterStack.ReceiveLetter(let, delayTicks: 450);
        }
        catch (Exception e)
        {
            WRMC_Utils.LogWarning("Failed to make completion letter");
            WRMC_Utils.LogWarning(e.ToString());
        }
        WRMC_Utils.LogDebugMessage($"FinishLetterSentToLetterStack");
        // Complete方法会发送part.outSignalsCompleted中设置的signal，完成任务
        Complete();
    }

    private Letter MakeExaminationLetter(Quest quest, Pawn pawn, Pawn billDoer, ThingDef bionic)
    {
        string label = "WRMC.QuestImplantExperiment.Examination.Letter".Translate();
        
        // 描述部分的rule
        string id()
        {
            if (pawn.IsColonist) return "Colonist".Translate();
            if (pawn.IsPrisoner) return "Prisoner".Translate();
            if (pawn.IsSlave) return "Slave".Translate();
            return "Unknown".Translate();
        }
        
        List<Rule> rules = new List<Rule>
        {
            new Rule_String("examinationCount", ExecutedExamination.ToString()),
            new Rule_String("Identity", id())
        };
        rules.AddRange(GrammarUtility.RulesForPawn("PAWN", pawn));
        rules.AddRange(GrammarUtility.RulesForPawn("DOCTOR", billDoer));
        
        // 描述部分的constant
        Dictionary<string, string> constants = new Dictionary<string, string>();
        constants["part"] = bionic.defName;
        constants["pawnIsPrisoner"] = pawn.IsPrisoner.ToString();
        
        // 描述
        string desc = WRMC_Utils.genTextFromRulePack(RulePackDef.Named("WRMC_QuestImplantExperiment_ExaminationText"), rules, constants);

        var let = LetterMaker.MakeLetter(label, desc, LetterDefOf.NeutralEvent, pawn, quest: quest);
        return let;
    }
    
    public override string DescriptionPart
    {
        get
        {
            var examString = "WRMC.QuestImplantExperiment.ExecutedExamination".Translate(ExecutedExamination, MaxExaminationNeeded);
            if (isExaminationReady) return examString + "\n" + "WRMC.QuestImplantExperiment.ExaminationReady".Translate(pawn.Named("PAWN"));
            var timeLeftString = "WRMC.QuestImplantExperiment.TimeLeft".Translate((ExaminationTickInterval + LastExaminationTick - Find.TickManager.TicksAbs).ToStringTicksToPeriod());
            return examString + "\n" + timeLeftString;
        }
    }

    public override void ExposeData()
    {
        Scribe_References.Look(ref map, "map");
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Defs.Look(ref thingDef, "thingDef");
        Scribe_Values.Look(ref isExaminationReady, "isExaminationReady");
        Scribe_Values.Look(ref ExecutedExamination, "ExecutedExamination");
        Scribe_Values.Look(ref LastExaminationTick, "LastExaminationTick");
        base.ExposeData();
    }
}

public class ExaminationSurgery : Recipe_Surgery
{
    // 检测任务是否存在，是否是正在追踪的pawn，以及pawn是否有仿生体
    public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
    {
        if (base.AvailableOnNow(thing, part))
        {
            Pawn pawn = thing as Pawn;
            var quest = FindQuest();
            var questPart = FindQuestPart(quest);
            return (questPart != null) &&
                   (questPart.pawn == pawn) &&
                   (questPart.isExaminationReady) &&
                   (pawn.health.hediffSet.HasHediff(WRMC_DefOfs.WRMC_PrototypeBionicHeart_Hediff));
        }
        return false;
    }

    // 在执行前检测是否可执行
    public override bool CompletableEver(Pawn surgeryTarget)
    {
        if (!base.CompletableEver(surgeryTarget)) return false;
        var questPart = FindQuestPart(FindQuest());
        return surgeryTarget.health.hediffSet.HasHediff(WRMC_DefOfs.WRMC_PrototypeBionicHeart_Hediff) && questPart.isExaminationReady;
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        var quest = FindQuest();
        var questPart = FindQuestPart(quest);
        if (questPart != null)
        {
            questPart.NotifyExamination(pawn, billDoer);
        }
        base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
    }

    private Quest FindQuest()
    {
        var ongoingQuests = Find.QuestManager.ActiveQuestsListForReading.Where(q => q.State == QuestState.Ongoing);
        try
        {
            return ongoingQuests.First(q => q.tags.Contains("WRMC_bionic"));
        } 
        catch 
        {
            return null;
        }
    }

    private QuestPart_TrackingBionic FindQuestPart(Quest quest)
    {
        if (quest == null) return null; 
        foreach (var questPart in quest.PartsListForReading)
        {
            if (questPart is QuestPart_TrackingBionic questPart_TrackingBionic)
            {
                return questPart_TrackingBionic;
            }
        }
        return null;
    }
}

// 用于持久化已解锁的仿生体的GameComponent
public class UnlockBionicGameComponent : GameComponent
{
    public Dictionary<ThingDef, bool> PossibleBionics = new ()
    {
        { WRMC_DefOfs.WRMC_PrototypeBionicHeart_Thing, false }
    };
        
    public bool UnlockThing(ThingDef thingDef)
    {
        if (!PossibleBionics.TryGetValue(thingDef, out var bionic))
        {
            WRMC_Utils.LogWarning($"ThingDef {thingDef} doesn't exist!");
            return false;
        }
        if (bionic)
        {
            WRMC_Utils.LogWarning($"ThingDef {thingDef} already unlocked!");
            return false;
        }
        PossibleBionics[thingDef] = true;
        return true;
    }

    public bool isUnlocked(ThingDef thingDef)
    {
        PossibleBionics.TryGetValue(thingDef, out var value);
        return value;
    }

    public bool hasLockedBionic()
    {
        return PossibleBionics.Values.Any(value => !value);
    }
    
    [return: MaybeNull]
    public ThingDef selectRandomLockedBionic()
    {
        var lockedPairs = PossibleBionics.Where(pair => !pair.Value).ToList();
        if (lockedPairs.EnumerableNullOrEmpty()) return null;
        return lockedPairs.RandomElement().Key;
    }

    public static UnlockBionicGameComponent Get() => Current.Game?.GetComponent<UnlockBionicGameComponent>();

    public UnlockBionicGameComponent(Game game) {}
        
    public override void ExposeData()
    {
        Scribe_Collections.Look(ref PossibleBionics, "PossibleBionics", LookMode.Def, LookMode.Value);
        base.ExposeData();
    }

    public override void LoadedGame()
    {
        base.LoadedGame();
        if (PossibleBionics.NullOrEmpty())
        {
            PossibleBionics = new Dictionary<ThingDef, bool>
            {
                { WRMC_DefOfs.WRMC_PrototypeBionicHeart_Thing, false }
            };
        }
    }
}