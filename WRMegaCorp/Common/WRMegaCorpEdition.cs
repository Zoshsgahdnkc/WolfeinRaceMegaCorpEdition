using UnityEngine;
using Verse;
using Wolfein;

namespace WRMegaCorp
{
    public class WRMegaCorpEdition: Mod
    {
        public static WRMC_ModSettings modSettings;
        public WRMegaCorpEdition(ModContentPack content) : base(content)
        {
            modSettings = GetSettings<WRMC_ModSettings>();
        }
        
        public override string SettingsCategory()
        {
            return "WRMC.SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            modSettings.DoWindowContents(inRect);
        }
    }
}