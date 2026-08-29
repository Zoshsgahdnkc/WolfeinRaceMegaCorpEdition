using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace WRMegaCorp;

public class SitePartWorker_RuinedOutpost: SitePartWorker
{
    public override void Init(Site site, SitePart sitePart)
    {
        base.Init(site, sitePart);
    }

    private int GetMechanoidsCount(Site site, SitePartParams parms)
    {
        int seed = parms.randomValue;
        int count = PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms()
        {
            tile = site.Tile,
            faction = Find.FactionManager.OfMechanoids,
            groupKind = PawnGroupKindDefOf.Combat,
            // Implement threat points from mod settings
            points = Math.Clamp(parms.threatPoints, 50f, 10000f),
            inhabitants = true,
            seed = seed
        }).Count();
        // 估计机械族，数量随机变化
        count = (int) Math.Clamp(count * new FloatRange(0.9f, 1.2f).RandomInRange, 1, 65535);
        return count;
    }

    public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules,
        Dictionary<string, string> outExtraDescriptionConstants)
    {
        slate.Set("count", GetMechanoidsCount(part.site, part.parms));
        base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
    }
}