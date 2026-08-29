using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace WRMegaCorp
{
    public class WorldObject_ArmisticeDay: WorldObject
    {
        public const float MIN_RELATION_REWARD = 4;
        public const float MAX_RELATION_REWARD = 18;

        public List<ThingDef> FOOD_LIST
        {
            get
            {
                var list = new List<ThingDef>();
                list.Add(ThingDef.Named("Wolfein_HoneyGlazedHam"));
                list.Add(ThingDef.Named("Wolfein_GrilledSteak"));
                list.Add(ThingDef.Named("Wolfein_BoneBroth"));
                list.Add(ThingDef.Named("Wolfein_BaconAndEggs"));
                list.Add(ThingDef.Named("Wolfein_CustardCake"));
                return list;
            }
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            // 生成请求加入的Pawn
            bool isCivilian = Rand.Range(0f, 1f) < 0.7;
            PawnKindDef pawnKind = isCivilian ? PawnKindDef.Named("Wolfein_MegaCorpCivilian") : PawnKindDef.Named("Wolfein_RebelDeserter");
            Pawn pawnToJoin = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, Faction.OfPlayer));
            Action acceptAction = () =>
            {
                caravan.AddPawn(pawnToJoin, true);
                if (!pawnToJoin.IsWorldPawn())
                    Find.WorldPawns.PassToWorld(pawnToJoin);
            };
            // 发送加入请求
            List<Rule> rules = new List<Rule>();
            rules.Add(new Rule_String("Name->"+pawnToJoin.Name.ToStringShort));
            rules.Add(new Rule_String("Background->"+pawnToJoin.story.Adulthood.title));
            rules.Add(new Rule_String("megaCorpID->"+Find.FactionManager.OfMegaCorp().GetUniqueLoadID()));
            rules.Add(new Rule_String("rebelID->"+Find.FactionManager.OfResistance().GetUniqueLoadID()));
            var nodeText = isCivilian
                ? WRMC_Utils.genTextFromRulePack(RulePackDef.Named("WRMC_ArmisticeDayDialogRule_Civilian"), rules)
                : WRMC_Utils.genTextFromRulePack(RulePackDef.Named("WRMC_ArmisticeDayDialogRule_Resistance"), rules);
            DiaNode node = new DiaNode(nodeText);
            DiaOption acceptOption = new DiaOption("WRMC_QuestArmisticeDay_AcceptPawn".Translate())
            { action = acceptAction };
            acceptOption.resolveTree = true;
            DiaOption rejectOption = new DiaOption("WRMC_QuestArmisticeDay_RejectPawn".Translate());
            rejectOption.resolveTree = true;
            node.options.Add(acceptOption);
            node.options.Add(rejectOption);
            Find.WindowStack.Add(new Dialog_NodeTree(node));
            // 奖励食物
            int caravanHumanCount = getCaravanHumanCount(caravan);
            float point = getFoodRewardPoint(caravanHumanCount);
            foreach (var thing in foodList(point))
            {
                caravan.AddPawnOrItem(thing, true);
            }
            // 修改心情
            foreach (var caravanPawn in caravan.PawnsListForReading)
            {
                if (caravanPawn.kindDef.RaceProps.Humanlike)
                {
                    caravanPawn.needs.mood.thoughts.memories.TryGainMemory(WRMC_DefOfs.WRMC_AttendADayMood);
                }
            }
            // 提升好感
            Faction.OfPlayer.TryAffectGoodwillWith(Faction, getGoodWillRewardPoint(caravan, caravanHumanCount), reason: WRMC_DefOfs.WRMC_AttendArmisticeDay);
            Faction.OfPlayer.TryAffectGoodwillWith(Find.FactionManager.OfResistance(), getGoodWillRewardPoint(caravan, caravanHumanCount), reason: WRMC_DefOfs.WRMC_AttendArmisticeDay);
            // 完成任务
            QuestUtility.SendQuestTargetSignals(this.questTags, "Resolved", this.Named("SUBJECT"));
            this.Destroy();
        }
        
        private float getFoodRewardPoint(int humanCount)
        {
            return humanCount * 10 + 8;
        }

        private int getGoodWillRewardPoint(Caravan caravan, int humanCount)
        {
            return (int) Math.Clamp(humanCount * 4 + hasLeaderInCaravan(caravan) * 4, MIN_RELATION_REWARD, MAX_RELATION_REWARD);
        }

        private int getCaravanHumanCount(Caravan caravan)
        {
            return caravan.PawnsListForReading.Count(pawn => pawn.kindDef.RaceProps.Humanlike);
        }
        
        private int hasLeaderInCaravan(Caravan caravan)
        {
            if (ModsConfig.IdeologyActive)
            {
                foreach (Pawn pawn in caravan.pawns)
                {
                    if (pawn == caravan.Faction.leader) return 1;
                }
            }
            return 0;
        }
        
        // 生成奖励食物列表的方法
        private List<Thing> foodList(float rewardPoint)
        {
            List<ThingDef> list = FOOD_LIST;
            if (list.NullOrEmpty()) return null;
            List<Thing> result = new List<Thing>();
            while (true)
            {
                // 随机选取一种食物
                var thingdef = list.RandomElement();
                // 如果剩余点数足够则加入食物，否则退出。
                var nutrition = thingdef.GetStatValueAbstract(StatDefOf.Nutrition);
                if (nutrition > rewardPoint) break;
                rewardPoint -= nutrition;
                result.Add(ThingMaker.MakeThing(thingdef));
            }
            return result;
        }
        
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
                yield return floatMenuOption;
            foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_ADay.GetFloatMenuOptions(caravan, this))
                yield return floatMenuOption2;
        }
    }
}