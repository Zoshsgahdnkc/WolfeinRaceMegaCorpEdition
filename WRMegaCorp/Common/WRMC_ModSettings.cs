using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    public enum WRMC_ModSettingsPreset
    {
        DEFAULT,
        EASY,
        HARD,
        EXTREME
    }
    public class WRMC_ModSettings: ModSettings
    {
        //总设置
        public WRMC_ModSettingsPreset preset = WRMC_ModSettingsPreset.DEFAULT;
        public bool DebugMode = false;
        public bool CloseWarningForever = false;
        // 杂项设置
        public float HoneyGlazedHamEffectMultiplier_NonWolfein = 0.15f;
        public float HoneyGlazedHamEffectMultiplier_Wolfein = 1f;
        // 通讯台部分设置
        public float DonateSilverMultiplier = 1f;
        public float DonateRelationMultiplier = 1f;
        public float AidCostMultiplier = 1f;
        public float AidStrengthMultiplier = 1f;
        public float AidCooldown = 24f;
        public float PrototypeBionicCostMultiplier = 1f;
        // 据点生成部分设置
        public float SettlementLootValueMultiplier = 2f;
        public float SettlementDefenseStrength = 1f;
        // 任务部分设置
        public bool QCivilianShipCrash_Activated = true;
        public bool QMeatOrder_Activated = true;
        public float QMeatOrder_RewardMul = 1f;
        public float QMeatOrder_RequestAmountMul = 1f;
        public bool QArmisticeDay_Activated = true;
        public bool QProjectLiquidation_Activated = true;
        public float QProjectLiquidation_RebelStrMul = 1f;
        public float QProjectLiquidation_RewardMul = 1f;
        public bool QDepositExploitation_Activated = true;
        public float QDepositExploitation_RewardMul = 1f;
        public float QDepositExploitation_ShareCountMultiplier = 1f;
        public int QDepositExploitation_ShareIntervalDays = 30;
        public bool QDebtSlave_Activated = true;
        public float QDebtSlave_debtAmountMultiplier = 1f;
        public bool QImplantExperiment_Activated = true;
        public int QImplantExperiment_RequiredSurgeryCount = 4;
        public float QImplantExperiment_SurgeryInterval = 36;
        public bool IBlackMarketTradeShip_Activated = true;
        public bool ResistanceReducingFoodRemoveUnwavering = false;
        public bool QTheRescue_Activated = true;
        
        
        private Vector2 scrollPosition = Vector2.zero;


        // 应用预设
        private void ApplyPreset(WRMC_ModSettingsPreset p)
        {
            preset = p;
            switch (p)
            {
                case WRMC_ModSettingsPreset.DEFAULT:
                    DonateSilverMultiplier = 1f;
                    DonateRelationMultiplier = 1f;
                    AidCostMultiplier = 1f;
                    AidStrengthMultiplier = 1f;
                    AidCooldown = 24f;
                    PrototypeBionicCostMultiplier = 1f;
                    SettlementLootValueMultiplier = 2f;
                    SettlementDefenseStrength = 1f;
                    QMeatOrder_RequestAmountMul = 1f;
                    QMeatOrder_RewardMul = 1f;
                    QProjectLiquidation_RebelStrMul = 1f;
                    QProjectLiquidation_RewardMul = 1f;
                    QDepositExploitation_RewardMul = 1f;
                    QDepositExploitation_ShareCountMultiplier = 1f;
                    QDepositExploitation_ShareIntervalDays = 30;
                    QDebtSlave_debtAmountMultiplier = 1f;
                    HoneyGlazedHamEffectMultiplier_Wolfein = 1f;
                    HoneyGlazedHamEffectMultiplier_NonWolfein = 0.15f;
                    QImplantExperiment_SurgeryInterval = 36f;
                    break;
                case WRMC_ModSettingsPreset.EASY:
                    DonateSilverMultiplier = 0.75f;
                    DonateRelationMultiplier = 1.25f;
                    AidCostMultiplier = 0.6f;
                    AidStrengthMultiplier = 1f;
                    AidCooldown = 24f;
                    PrototypeBionicCostMultiplier = 1f;
                    SettlementLootValueMultiplier = 2f;
                    SettlementDefenseStrength = 0.75f;
                    QMeatOrder_RequestAmountMul = 0.5f;
                    QMeatOrder_RewardMul = 0.9f;
                    QProjectLiquidation_RebelStrMul = 0.5f;
                    QProjectLiquidation_RewardMul = 0.75f;
                    QDepositExploitation_ShareCountMultiplier = 1f;
                    QDepositExploitation_ShareIntervalDays = 15;
                    HoneyGlazedHamEffectMultiplier_Wolfein = 1f;
                    QDebtSlave_debtAmountMultiplier = 0.5f;
                    break;
                case WRMC_ModSettingsPreset.HARD:
                    DonateSilverMultiplier = 1.5f;
                    DonateRelationMultiplier = 0.75f;
                    AidCostMultiplier = 2.5f;
                    AidStrengthMultiplier = 2f;
                    AidCooldown = 12f;
                    PrototypeBionicCostMultiplier = 1f;
                    SettlementLootValueMultiplier = 4f;
                    SettlementDefenseStrength = 3f;
                    QMeatOrder_RequestAmountMul = 2f;
                    QMeatOrder_RewardMul = 2f;
                    QProjectLiquidation_RebelStrMul = 3f;
                    QProjectLiquidation_RewardMul = 4f;
                    QDepositExploitation_ShareCountMultiplier = 3f;
                    QDepositExploitation_ShareIntervalDays = 30;
                    QDebtSlave_debtAmountMultiplier = 1.25f;
                    HoneyGlazedHamEffectMultiplier_Wolfein = 1f;
                    HoneyGlazedHamEffectMultiplier_NonWolfein = 0.175f;
                    break;
                case WRMC_ModSettingsPreset.EXTREME:
                    DonateSilverMultiplier = 3f;
                    DonateRelationMultiplier = 0.5f;
                    AidCostMultiplier = 7.5f;
                    AidStrengthMultiplier = 5f;
                    AidCooldown = 6f;
                    PrototypeBionicCostMultiplier = 1f;
                    SettlementLootValueMultiplier = 6f;
                    SettlementDefenseStrength = 5f;
                    QMeatOrder_RequestAmountMul = 4f;
                    QMeatOrder_RewardMul = 4f;
                    QProjectLiquidation_RebelStrMul = 5f;
                    QProjectLiquidation_RewardMul = 6f;
                    QDepositExploitation_ShareCountMultiplier = 6f;
                    QDepositExploitation_ShareIntervalDays = 30;
                    QDebtSlave_debtAmountMultiplier = 1.5f;
                    HoneyGlazedHamEffectMultiplier_Wolfein = 1f;
                    HoneyGlazedHamEffectMultiplier_NonWolfein = 0.2f;
                    break;
            }
        }
        
        // 序列化
        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref DebugMode, "WRMC_Debug", false);
            Scribe_Values.Look<float>(ref DonateSilverMultiplier, "DonateSilverMultiplier", 1f);
            Scribe_Values.Look<float>(ref DonateRelationMultiplier, "DonateRelationMultiplier", 1f);
            Scribe_Values.Look<float>(ref AidCostMultiplier, "AidCostMultiplier", 1f);
            Scribe_Values.Look<float>(ref AidStrengthMultiplier, "AidStrengthMultiplier", 1f);
            Scribe_Values.Look<float>(ref AidCooldown, "AidCooldown", 24f);
            Scribe_Values.Look<float>(ref SettlementLootValueMultiplier, "SettlementLootValueMultiplier", 2f);
            Scribe_Values.Look<float>(ref SettlementDefenseStrength, "SettlementDefenseStrength", 1f);
            Scribe_Values.Look<bool>(ref QArmisticeDay_Activated, "QArmisticeDay_Activated", true);
            Scribe_Values.Look<bool>(ref QCivilianShipCrash_Activated, "QCivilianShipCrash_Activated", true);
            Scribe_Values.Look<bool>(ref QMeatOrder_Activated, "QMeatOrder_Activated", true);
            Scribe_Values.Look<bool>(ref QProjectLiquidation_Activated, "QProjectLiquidation_Activated", true);
            Scribe_Values.Look<float>(ref QProjectLiquidation_RebelStrMul, "QProjectLiquidation_RebelStrMul", 1f);
            Scribe_Values.Look<float>(ref QProjectLiquidation_RewardMul, "QProjectLiquidation_RewardMul", 1f);
            Scribe_Values.Look<float>(ref QMeatOrder_RequestAmountMul, "QMeatOrder_RequestAmountMul", 1f);
            Scribe_Values.Look<bool>(ref QDepositExploitation_Activated, "QDepositExploitation_Activated", true);
            Scribe_Values.Look<float>(ref QDepositExploitation_RewardMul, "QDepositExploitation_RewardMul", 1f);
            Scribe_Values.Look<float>(ref QDepositExploitation_ShareCountMultiplier, "QDepositExploitation_ShareCountMultiplier", 1f);
            Scribe_Values.Look<int>(ref QDepositExploitation_ShareIntervalDays, "QDepositExploitation_ShareIntervalDays", 30);
            Scribe_Values.Look<float>(ref QDebtSlave_debtAmountMultiplier, "QDebtSlave_debtAmountMultiplier", 1f);
            Scribe_Values.Look<bool>(ref QDebtSlave_Activated, "QDebtSlave_Activated", true);
            Scribe_Values.Look<float>(ref HoneyGlazedHamEffectMultiplier_Wolfein, "HoneyGlazedHamEffectMultiplier_Wolfein", 1f);
            Scribe_Values.Look<float>(ref HoneyGlazedHamEffectMultiplier_NonWolfein, "HoneyGlazedHamEffectMultiplier_NonWolfein", 0.15f);
            Scribe_Values.Look<bool>(ref QImplantExperiment_Activated, "QImplantExperiment_Activated", true);
            Scribe_Values.Look<int>(ref QImplantExperiment_RequiredSurgeryCount, "QImplantExperiment_RequiredSurgeryCount", 4);
            Scribe_Values.Look<float>(ref QImplantExperiment_SurgeryInterval, "QImplantExperiment_SurgeryInterval", 36);
            Scribe_Values.Look<bool>(ref IBlackMarketTradeShip_Activated, "IBlackMarketTradeShip_Activated", true);
            Scribe_Values.Look<bool>(ref ResistanceReducingFoodRemoveUnwavering, "ResistanceReducingFoodRemoveUnwavering", false);
            Scribe_Values.Look<bool>(ref CloseWarningForever, "CloseWarningForever", false);
            Scribe_Values.Look<bool>(ref QTheRescue_Activated, "QTheRescue_Activated", true);
        }
        
        // 绘制模组设置界面的主方法
        public void DoWindowContents(Rect inRect)
        {
            float height = 1600f;
            Rect rect1 = new Rect(inRect.x, inRect.y, inRect.width - 20f, height);
            Widgets.BeginScrollView(inRect, ref this.scrollPosition, rect1);
            var listing = new Listing_Standard();
            listing.Begin(rect1);
            listing.Gap(5f);
            
            GeneralAndPresetSettings(listing);
            MiscSettings(listing);
            FactionDialogSettings(listing);
            SettlementGenerationSettings(listing);
            QuestSettings(listing);
            IncidentSettings(listing);
            
            listing.End();
            Widgets.EndScrollView();
        }

        private void GeneralAndPresetSettings(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            // 调试模式
            listing.CheckboxLabeled("WRMC.Settings.Debug".Translate(), ref DebugMode);
            listing.Gap(2f);
            listing.CheckboxLabeled("WRMC.Settings.CloseWarningForever".Translate(), ref CloseWarningForever);
            listing.Gap(2f);
            
            // 预设按钮
            Rect presetBtnRect = listing.GetRect(30f);
            if (Widgets.ButtonText(presetBtnRect, "WRMC.Settings.Preset".Translate()))
            {
                var opts = new List<FloatMenuOption>
                {
                    new FloatMenuOption("WRMC.Settings.Preset.Default".Translate(), () => ApplyPreset(WRMC_ModSettingsPreset.DEFAULT)),
                    new FloatMenuOption("WRMC.Settings.Preset.Easy".Translate(), () => ApplyPreset(WRMC_ModSettingsPreset.EASY)),
                    new FloatMenuOption("WRMC.Settings.Preset.Hard".Translate(), () => ApplyPreset(WRMC_ModSettingsPreset.HARD)),
                    new FloatMenuOption("WRMC.Settings.Preset.Extreme".Translate(), () => ApplyPreset(WRMC_ModSettingsPreset.EXTREME))
                };
                Find.WindowStack.Add(new FloatMenu(opts));
            }
            listing.GapLine();
            listing.Gap(3f);
            Text.Font = GameFont.Small;
        }
        private void FactionDialogSettings(Listing_Standard listing)
        {
            // 通讯台设置
            Headline(listing, "WRMC.Settings.Comms");
            listing.Gap(2f);
            DonateSilverMultiplier = listing.SliderLabeled("WRMC.Settings.Comms.DonateSilverMultiplier".Translate() + "\t" + DonateSilverMultiplier.ToStringPercent(), DonateSilverMultiplier, 0.01f, 10f);
            DonateRelationMultiplier = listing.SliderLabeled("WRMC.Settings.Comms.DonateSilverMultiplier".Translate() + "\t" + DonateSilverMultiplier.ToStringPercent(), DonateRelationMultiplier, 0.01f, 10f);
            AidCostMultiplier = listing.SliderLabeled("WRMC.Settings.Comms.AidCostMultiplier".Translate() + "\t" + AidCostMultiplier.ToStringPercent(), AidCostMultiplier, 0.01f, 10f);
            AidStrengthMultiplier = listing.SliderLabeled("WRMC.Settings.Comms.AidStrengthMultiplier".Translate() + "\t" + AidStrengthMultiplier.ToStringPercent(), AidStrengthMultiplier, 0.01f, 10f);
            AidCooldown = listing.SliderLabeled("WRMC.Settings.Comms.AidCooldown".Translate() + "\t" + $"{AidCooldown:F1}", AidCooldown, 1f, 720f);
            PrototypeBionicCostMultiplier = listing.SliderLabeled("WRMC.Settings.Comms.PrototypeBionicCostMultiplier".Translate() + "\t" + PrototypeBionicCostMultiplier.ToStringPercent(), PrototypeBionicCostMultiplier, 0.01f, 10f);
            listing.GapLine();
            listing.Gap(3f);
        }

        private void SettlementGenerationSettings(Listing_Standard listing)
        {
            // 据点生成设置
            Headline(listing, "WRMC.Settings.SettlementGen");
            listing.Gap(2f);
            SettlementLootValueMultiplier = listing.SliderLabeled("WRMC.Settings.SettlementGen.LootValueMultiplier".Translate() + "\t\t" + SettlementLootValueMultiplier.ToStringPercent(), SettlementLootValueMultiplier, 1.001f, 10f);
            SettlementDefenseStrength = listing.SliderLabeled("WRMC.Settings.SettlementGen.DefenseStrength".Translate() + "\t" + SettlementDefenseStrength.ToStringPercent(), SettlementDefenseStrength, 0.01f, 10f);
            listing.GapLine();
            listing.Gap(3f);
        }

        private void QuestSettings(Listing_Standard listing)
        {
            // 任务
            Headline(listing, "WRMC.Settings.Quest");
            listing.Gap(2f);
            listing.Label("WRMC.Settings.Quest.CivilianShipCrash".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Quest.CivilianShipCrash.Activated".Translate(), ref QCivilianShipCrash_Activated);
            listing.GapLine();
            listing.Label("WRMC.Settings.Quest.DebtSlave".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Quest.DebtSlave.Activated".Translate(), ref QDebtSlave_Activated);
            QDebtSlave_debtAmountMultiplier = listing.SliderLabeled("WRMC.Settings.Quest.DebtSlave.debtAmountMultiplier".Translate() + "\t" + QDebtSlave_debtAmountMultiplier.ToStringPercent(), QDebtSlave_debtAmountMultiplier, 0.01f, 10f);
            listing.GapLine();
            listing.Label("WRMC.Settings.Quest.ArmisticeDay".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Quest.ArmisticeDay.Activated".Translate(), ref QArmisticeDay_Activated);
            listing.GapLine();
            listing.Label("WRMC.Settings.Quest.MeatOrder".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Quest.MeatOrder.Activated".Translate(), ref QMeatOrder_Activated);
            QMeatOrder_RewardMul = listing.SliderLabeled("WRMC.Settings.Quest.MeatOrder.RewardMul".Translate() + "\t" + QMeatOrder_RewardMul.ToStringPercent(), QMeatOrder_RewardMul, 0.01f, 10f);
            QMeatOrder_RequestAmountMul = listing.SliderLabeled("WRMC.Settings.Quest.MeatOrder.RequestAmountMul".Translate() + "\t" + QMeatOrder_RequestAmountMul.ToStringPercent(), QMeatOrder_RequestAmountMul, 0.01f, 10f);
            listing.GapLine();
            listing.Label("WRMC.Settings.Quest.ProjectLiquidation".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Quest.ProjectLiquidation.Activated".Translate(), ref QProjectLiquidation_Activated);
            QProjectLiquidation_RewardMul = listing.SliderLabeled("WRMC.Settings.Quest.ProjectLiquidation.RewardMul".Translate() + "\t" + QProjectLiquidation_RewardMul.ToStringPercent(), QProjectLiquidation_RewardMul, 0.01f, 10f);
            QProjectLiquidation_RebelStrMul = listing.SliderLabeled("WRMC.Settings.Quest.ProjectLiquidation.RebelStrMul".Translate() + "\t" + QProjectLiquidation_RebelStrMul.ToStringPercent(), QProjectLiquidation_RebelStrMul, 0.01f, 10f);
            listing.GapLine();
            listing.Label("WRMC.Settings.Quest.DepositExploitation".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Quest.DepositExploitation.Activated".Translate(), ref QDepositExploitation_Activated);
            QDepositExploitation_RewardMul = listing.SliderLabeled("WRMC.Settings.Quest.DepositExploitation.RewardMul".Translate() + "\t" + QDepositExploitation_RewardMul.ToStringPercent(), QDepositExploitation_RewardMul, 0.01f, 10f);
            QDepositExploitation_ShareCountMultiplier = listing.SliderLabeled("WRMC.Settings.Quest.DepositExploitation.ShareCountMultiplier".Translate() + "\t" + QDepositExploitation_ShareCountMultiplier.ToStringPercent(), QDepositExploitation_ShareCountMultiplier, 0.01f, 10f);
            QDepositExploitation_ShareIntervalDays = (int) Math.Round(listing.SliderLabeled("WRMC.Settings.Quest.DepositExploitation.ShareIntervalDays".Translate() + "\t" + QDepositExploitation_ShareIntervalDays, QDepositExploitation_ShareIntervalDays, 1, 120));
            listing.GapLine();
            listing.Label("WRMC.Settings.Quest.ImplantExperiment".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Quest.ImplantExperiment.Activated".Translate(), ref QImplantExperiment_Activated);
            QImplantExperiment_RequiredSurgeryCount = (int) Math.Round(listing.SliderLabeled("WRMC.Settings.Quest.ImplantExperiment.RequiredSurgeryCount".Translate() + "\t" + QImplantExperiment_RequiredSurgeryCount, QImplantExperiment_RequiredSurgeryCount, 1f, 10f));
            QImplantExperiment_SurgeryInterval = listing.SliderLabeled("WRMC.Settings.Quest.ImplantExperiment.SurgeryInterval".Translate() + "\t" + $"{QImplantExperiment_SurgeryInterval:F1}", QImplantExperiment_SurgeryInterval, 0.1f, 144f);
            listing.GapLine();
            listing.Label("WRMC.Settings.Quest.TheRescue".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Quest.TheRescue.Activated".Translate(), ref QTheRescue_Activated);
            listing.Gap(3f);
        }

        private void IncidentSettings(Listing_Standard listing)
        {
            Headline(listing, "WRMC.Settings.Incident");
            listing.Gap(2f);
            listing.Label("WRMC.Settings.Incident.BlackMarketTradeShip".Translate());
            listing.CheckboxLabeled("WRMC.Settings.Incident.BlackMarketTradeShip.Activated".Translate(), ref IBlackMarketTradeShip_Activated);
            listing.GapLine();
            listing.Gap(3f);
        }
        
        private void MiscSettings(Listing_Standard listing)
        {
            // 杂项生成设置
            Headline(listing, "WRMC.Settings.Misc");
            listing.Gap(2f);
            HoneyGlazedHamEffectMultiplier_Wolfein = listing.SliderLabeled("WRMC.Settings.Misc.HoneyGlazedHamEffectMultiplier_Wolfein".Translate() + "\t\t" + HoneyGlazedHamEffectMultiplier_Wolfein.ToStringPercent(), HoneyGlazedHamEffectMultiplier_Wolfein, 0f, 10f);
            HoneyGlazedHamEffectMultiplier_NonWolfein = listing.SliderLabeled("WRMC.Settings.Misc.HoneyGlazedHamEffectMultiplier_NonWolfein".Translate() + "\t" + HoneyGlazedHamEffectMultiplier_NonWolfein.ToStringPercent(), HoneyGlazedHamEffectMultiplier_NonWolfein, 0f, 10f);
            listing.CheckboxLabeled("WRMC.Settings.Misc.ResistanceReducingFoodRemoveUnwavering".Translate(), ref ResistanceReducingFoodRemoveUnwavering);
            listing.GapLine();
            listing.Gap(3f);
        }

        private void Headline(Listing_Standard listing, string headline)
        {
            Text.Font = GameFont.Medium;
            listing.Label(headline.Translate());
            listing.Gap(3f);
            Text.Font = GameFont.Small;
        }
        
    }
}