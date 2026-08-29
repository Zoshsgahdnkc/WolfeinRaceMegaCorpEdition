using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WRMegaCorp
{
    public class ChoiceLetter_DebtSlave: Verse.ChoiceLetter
    {
        public string signalAccept;
        public string signalReject;
        public int need;
        public Map map;

        // 序列化
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.signalAccept, "signalAccept");
            Scribe_Values.Look(ref this.signalReject, "signalReject");
        }

        public override IEnumerable<DiaOption> Choices {
            get
            {
                DiaOption accept = new DiaOption("WRMC.QuestDebtSlave.Accept".Translate())
                {
                    action = delegate 
                    {
                        Find.SignalManager.SendSignal(new Signal(signalAccept));
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };

                Slate slate = QuestGen.slate;
                WRMC_Utils.LogDebugMessage($"need: {need},  possibleMap is null: {map == null}");
                if (need > SilverUtility.CountSilver(map))
                {
                    accept.Disable("NotEnoughSilver".Translate());
                }
                
                DiaOption reject = new DiaOption("WRMC.QuestDebtSlave.Reject".Translate())
                {
                    action = delegate 
                    {
                        Find.SignalManager.SendSignal(new Signal(signalReject));
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };
                yield return accept;
                yield return reject;
            }
        }

        private bool isMapSurface(Map map)
        {
            var tile = map.Tile;
            if (!tile.Valid) return false;
            return !tile.LayerDef.isSpace;
        }
    }
}