using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WRMegaCorp
{
    public class ChoiceLetter_CSC: Verse.ChoiceLetter
    {
        public string crashReason;
        public string signalAccept;
        public string signalReject;

        // 序列化
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.crashReason, "crashReason");
            Scribe_Values.Look(ref this.signalAccept, "signalAccept");
            Scribe_Values.Look(ref this.signalReject, "signalReject");
            // Letter 基类需要序列化 quest 引用
            Scribe_Deep.Look(ref this.quest, "quest");
        }

        public override IEnumerable<DiaOption> Choices {
            get
            {
                DiaOption accept =  new DiaOption("WRMC.QuestShipCrash.Accept".Translate())
                {
                    action = delegate 
                    {
                        Find.SignalManager.SendSignal(new Signal(signalAccept)); // signalAccept 现在是 "SpawnShip"
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };
                
                DiaOption reject =  new DiaOption("WRMC.QuestShipCrash.Reject".Translate())
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

    }
}