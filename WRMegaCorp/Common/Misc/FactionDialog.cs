using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WRMegaCorp
{
    public class FactionDialog
    {
        public const int DONATE_SMALL_SILVER = 1000;
        public const int DONATE_SMALL_RELATION = 6;
        public const int DONATE_BIG_SILVER = 5000;
        public const int DONATE_BIG_RELATION = 35;
        public const int AID_COST_SMALL = 2000;
        public const int AID_STR_SMALL = 900;
        public const int AID_COST_MEDIUM = 6000;
        public const int AID_STR_MEDIUM = 3000;
        public const int AID_COST_BIG = 14000;
        public const int AID_STR_BIG = 6500;
        public const int PROTOTYPE_HEART = 3500;
        // 沃芬复兴基金会
        public static DiaOption WolfeinRevivalFoundationDialogOption(Verse.Map map, Faction faction, Pawn negotiator)
        {
            // 外层option的text
            var text = "WRMC_WolfeinRevivalFoundationOption".Translate();
            // 连接到基金会选项
            var returnOption = new DiaOption(text);
            returnOption.linkLateBind = () => WolfeinRevivalFoundationDialog(map, faction, negotiator);
            return returnOption;
        }
        public static DiaNode WolfeinRevivalFoundationDialog(Verse.Map map, Faction faction, Pawn negotiator)
        {
            float donateSilverMul = WRMegaCorpEdition.modSettings?.DonateSilverMultiplier ?? 1f;
            float donateRelationMul = WRMegaCorpEdition.modSettings?.DonateRelationMultiplier ?? 1f;
            
            // 捐赠内部Option
            int silverSmall = (int)Math.Round(DONATE_SMALL_SILVER * donateSilverMul);
            int relationSmall = (int)Math.Round(DONATE_SMALL_RELATION * donateRelationMul);
            int silverBig = (int)Math.Round(DONATE_BIG_SILVER * donateSilverMul);
            int relationBig = (int)Math.Round(DONATE_BIG_RELATION * donateRelationMul);
            var DonateSmall = new DiaOption("WRMC_DonateSmall".Translate() + getDonateText(silverSmall, relationSmall));
            var DonateBig = new DiaOption("WRMC_DonateBig".Translate() + getDonateText(silverBig, relationBig));
            // 如果白银不足则禁用捐赠
            if (WRMC_Utils.AmountSendableSilver(map) < silverSmall)
            {
                DonateSmall.Disable("NeedSilverLaunchable".Translate(silverSmall));
                DonateBig.Disable("NeedSilverLaunchable".Translate(silverBig));
            } else if (WRMC_Utils.AmountSendableSilver(map) < silverBig) DonateBig.Disable("NeedSilverLaunchable".Translate(silverBig));
            // 感谢捐赠的Node
            var Foundation = new DiaNode("WRMC_FoundationNodeText".Translate(negotiator))
            {
                options =
                {
                    DonateSmall, DonateBig,
                    new DiaOption("GoBack".Translate())
                    {
                        linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
                    }
                }
            };

            bool moneySent = false;
            // 执行捐赠行为
            DonateSmall.action = () =>
            {
                moneySent = false;
                if (VerifyAmount(map, silverSmall))
                {
                    TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, silverSmall, map, (TradeShip)null);
                    Faction.OfPlayer.TryAffectGoodwillWith(faction, relationSmall, false,
                        reason: WRMC_DefOfs.WRMC_DonateSmallHistory);
                    var letter = LetterMaker.MakeLetter(
                        "WRMC_DonateSmallThankLetterLabel".Translate(),
                        "WRMC_DonateSmallThankLetterText".Translate(),
                        LetterDefOf.PositiveEvent,
                        faction);
                    Find.LetterStack.ReceiveLetter(letter);
                    moneySent = true;
                }
            };
            DonateSmall.linkLateBind = () => ThankYouOrNotEnoughMoneyNode(map, faction, negotiator, moneySent);
            
            DonateBig.action = () =>
            {
                moneySent = false;
                if (VerifyAmount(map, silverBig))
                {
                    TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, silverBig, map, (TradeShip)null);
                    Faction.OfPlayer.TryAffectGoodwillWith(faction, relationBig, false,
                        reason: WRMC_DefOfs.WRMC_DonateBigHistory);
                    var letter = LetterMaker.MakeLetter(
                        "WRMC_DonateBigThankLetterLabel".Translate(),
                        "WRMC_DonateBigThankLetterText".Translate(negotiator.Faction, negotiator),
                        LetterDefOf.PositiveEvent,
                        faction);
                    Find.LetterStack.ReceiveLetter(letter);
                    moneySent = true;
                }
            };
            DonateBig.linkLateBind = () => ThankYouOrNotEnoughMoneyNode(map, faction, negotiator, moneySent);
            return Foundation;
        }

        private static DiaNode ThankYouOrNotEnoughMoneyNode(Map map, Faction faction, Pawn negotiator, bool isEnough)
        {
            if (isEnough)
            {
                var ThanQ = new DiaNode("WRMC_FoundationThanksText".Translate());
                var backToFoundation = new DiaOption("GoBack".Translate())
                {
                    linkLateBind = () => WolfeinRevivalFoundationDialog(map, faction, negotiator)
                };
                ThanQ.options.Add(backToFoundation);
                return ThanQ;
            }
            var Insufficient = new DiaNode("WRMC_FoundationInsufficientText".Translate());
            var backToRoot = new DiaOption("GoBack".Translate())
            {
                linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
            };
            Insufficient.options.Add(backToRoot);
            return Insufficient;
        }

        protected static TaggedString getDonateText(int amount, int relationInfluence)
        {
            return "WRMC_DonateOption".Translate(amount, relationInfluence);
        }
        
        // 军企线上商城
        public static DiaOption MegaCorpOnlineStoreOption(Map map, Faction faction, Pawn negotiator)
        {
            var text = "WRMC_MegaCorpOnlineStore".Translate();
            var returnOption = new DiaOption(text);
            if (faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Ally)
            {
                returnOption.Disable("Wolfein.AllyRequire".Translate());
            }
            // 最外层的按钮
            returnOption.link = MegaCorpOnlineStore(map, faction, negotiator);
            return returnOption;
        }
        
        public static DiaNode MegaCorpOnlineStore(Map map, Faction faction, Pawn negotiator)
        {
            float aidCostMul = WRMegaCorpEdition.modSettings?.AidCostMultiplier ?? 1f;
            float aidStrMul = WRMegaCorpEdition.modSettings?.AidStrengthMultiplier ?? 1f;
            float aidCD = WRMegaCorpEdition.modSettings?.AidCooldown ?? 2.5f;
            
            //结构：returnOption => storeNode => BuyAidSmall => aidSmallNode => BuyAidSmallConfirm => replyNode
            int silverSmall = (int)Math.Round(AID_COST_SMALL * aidCostMul);
            int silverMedium = (int)Math.Round(AID_COST_MEDIUM * aidCostMul);
            int silverBig = (int)Math.Round(AID_COST_BIG * aidCostMul);
            int silver = WRMC_Utils.AmountSendableSilver(map);
            // 购买选项的Opt
            var RequestData = Wolfein.FactionDialog.RequestDataQuest(map, faction, negotiator);
            var aidSmallNode = new DiaNode("WRMC_OnlineBuyAidSmallNode".Translate(silverSmall));
            var BuyAidSmall = new DiaOption("WRMC_OnlineBuyAidSmallOption".Translate())
            {
                link = aidSmallNode
            };
            var aidMediumNode = new DiaNode("WRMC_OnlineBuyAidMediumNode".Translate(silverMedium));
            var BuyAidMedium = new DiaOption("WRMC_OnlineBuyAidMediumOption".Translate())
            {
                link = aidMediumNode
            };
            var aidBigNode = new DiaNode("WRMC_OnlineBuyAidBigNode".Translate(silverBig));
            var BuyAidBig = new DiaOption("WRMC_OnlineBuyAidBigOption".Translate())
            {
                link = aidBigNode
            };
            // 请求后的回复
            var storeNode = new DiaNode("WRMC_OnlineStoreHomepage".Translate(negotiator));
            var replyNode = new DiaNode("WRMC_OnlineStoreReply".Translate());
            replyNode.options.Add(new DiaOption("GoBack".Translate())
            {
                linkLateBind = () => MegaCorpOnlineStore(map, faction, negotiator)
            });
            
            // 给商店页面添加选项
            storeNode.options.Add(RequestData);
            // 试制心脏任务的部分
            if (UnlockBionicGameComponent.Get()?.isUnlocked(WRMC_DefOfs.WRMC_PrototypeBionicHeart_Thing) == true)
            {
                int silverHeart = (int)Math.Round(PROTOTYPE_HEART * (WRMegaCorpEdition.modSettings?.AidCostMultiplier ?? 1f));
                var BuyPrototypeHeart = new DiaOption("WRMC_OnlineBuyPrototypeHeart".Translate(silverHeart))
                {
                    action = () =>
                    {
                        if (VerifyAmount(map, silverHeart))
                        {
                            var thing = ThingMaker.MakeThing(WRMC_DefOfs.WRMC_PrototypeBionicHeart_Thing);
                        
                            DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, new HashSet<Thing>(){thing});
                            
                            Messages.Message("WRMC_OnlineBuyPrototypeBionicMessage", thing, MessageTypeDefOf.PositiveEvent);
                        
                            TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, silverSmall, map, (TradeShip)null);
                        }
                    },
                    link = replyNode
                };
                if (silver < silverHeart) BuyPrototypeHeart.Disable("NeedSilverLaunchable".Translate(silverHeart));
                storeNode.options.Add(BuyPrototypeHeart);
            }
            storeNode.options.Add(BuyAidSmall);
            storeNode.options.Add(BuyAidMedium);
            storeNode.options.Add(BuyAidBig);
            storeNode.options.Add(new DiaOption("GoBack".Translate())
            {
                linkLateBind = FactionDialogMaker.ResetToRoot(faction, negotiator)
            });
            
            // 通用的返回商店页面的选项
            var backToStoreOpt = new DiaOption("GoBack".Translate())
            {
                linkLateBind = () => MegaCorpOnlineStore(map, faction, negotiator)
            };
            RequestData.link = replyNode;
            // 确认按钮，内含跳转回复页面和执行军事支援逻辑
            var BuyAidSmallConfirm = new DiaOption("WRMC_BuyAidSmallConfirm".Translate())
            {
                link = replyNode,
                action = () =>
                {
                    if (VerifyAmount(map, silverSmall) && MilitaryAidUtils.TryCallAid(map, faction, negotiator, AID_STR_SMALL * aidStrMul, MilitaryAidUtils.dronePool))
                    {
                        TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, silverSmall, map, (TradeShip)null);
                    }
                    else WRMC_Utils.LogError("Failed calling military aid!");
                }
            };
            aidSmallNode.options.Add(BuyAidSmallConfirm);
            aidSmallNode.options.Add(backToStoreOpt);
            var BuyAidMediumConfirm = new DiaOption("WRMC_BuyAidMediumConfirm".Translate())
            {
                link = replyNode,
                action = () =>
                {
                    if (VerifyAmount(map, silverMedium) && MilitaryAidUtils.TryCallAid(map, faction, negotiator, AID_STR_MEDIUM * aidStrMul,
                            MilitaryAidUtils.mechPool))
                    {
                        TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, silverMedium, map, (TradeShip)null);
                    }
                    else WRMC_Utils.LogError("Failed calling military aid!");
                }
            };
            aidMediumNode.options.Add(BuyAidMediumConfirm);
            aidMediumNode.options.Add(backToStoreOpt);
            var BuyAidBigConfirm = new DiaOption("WRMC_BuyAidBigConfirm".Translate())
            {
                link = replyNode,
                action = () =>
                {
                    if (VerifyAmount(map, silverBig) && MilitaryAidUtils.TryCallAid(map, faction, negotiator, AID_STR_BIG * aidStrMul,
                            MilitaryAidUtils.MTFPool))
                    {
                        TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, silverBig, map, (TradeShip)null);
                    }
                    else WRMC_Utils.LogError("Failed calling military aid!");
                }
            };
            aidBigNode.options.Add(BuyAidBigConfirm);
            aidBigNode.options.Add(backToStoreOpt);
            // 检测支援时间
            int numTicks = faction.lastMilitaryAidRequestTick + (int) aidCD * 2500 - Find.TickManager.TicksGame;
            if (numTicks > 0)
            {
                BuyAidSmallConfirm.Disable("WaitTime".Translate(numTicks.ToStringTicksToPeriod()));
                BuyAidMediumConfirm.Disable("WaitTime".Translate(numTicks.ToStringTicksToPeriod()));
                BuyAidBigConfirm.Disable("WaitTime".Translate(numTicks.ToStringTicksToPeriod()));
            }
            // 检测白银数量
            if (silver < silverSmall)
            {
                BuyAidSmallConfirm.Disable("NeedSilverLaunchable".Translate(silverSmall));
                BuyAidMediumConfirm.Disable("NeedSilverLaunchable".Translate(silverMedium));
                BuyAidBigConfirm.Disable("NeedSilverLaunchable".Translate(silverBig));
            } else if (silver < silverMedium)
            {
                BuyAidMediumConfirm.Disable("NeedSilverLaunchable".Translate(silverMedium));
                BuyAidBigConfirm.Disable("NeedSilverLaunchable".Translate(silverBig));
            } else if (silver < silverBig)
            {
                BuyAidBigConfirm.Disable("NeedSilverLaunchable".Translate(silverBig));
            }

            return storeNode;
        }

        private static bool VerifyAmount(Map map, int silverCount)
        {
            if (TradeUtility.ColonyHasEnoughSilver(map, silverCount))
            {
                return true;
            }
            WRMC_Utils.LogWarning($"Try Sending {silverCount} silver when insufficient. Required silver may be transferred to somewhere else.");
            return false;
        }

    }
}