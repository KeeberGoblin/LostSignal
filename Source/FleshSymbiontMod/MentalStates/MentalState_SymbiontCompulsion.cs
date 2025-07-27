using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FleshSymbiontMod
{
    // Mental state for being compelled to touch the symbiont
    public class MentalState_SymbiontCompulsion : MentalState
    {
        private Building_FleshSymbiont targetSymbiont;
        private int ticksWithoutProgress = 0;
        private const int MAX_TICKS_WITHOUT_PROGRESS = 3000; // ~5 minutes before giving up
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref targetSymbiont, "targetSymbiont");
            Scribe_Values.Look(ref ticksWithoutProgress, "ticksWithoutProgress", 0);
        }
        
        // Remove override - use PostStart instead
        public override void PostStart(string reason)
        {
            base.PostStart(reason);
            FindNearestSymbiont();
            
            if (targetSymbiont != null)
            {
                // Try to give the compulsion job immediately
                var job = JobMaker.MakeJob(FleshSymbiontDefOf.CompelledToSymbiont, targetSymbiont);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }
        
        private void FindNearestSymbiont()
        {
            if (pawn.Map == null) return;
            
            targetSymbiont = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>()
                ?.Where(s => s.Spawned && !s.Destroyed)
                ?.OrderBy(s => s.Position.DistanceTo(pawn.Position))
                ?.FirstOrDefault();
        }
        
        public override void PostEnd()
        {
            base.PostEnd();
            
            if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 2)
            {
                Messages.Message($"{pawn.Name.ToStringShort} shakes their head as if waking from a nightmare, " +
                                "the alien whispers fading from their mind.", 
                    pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
        
        public override bool ForceHostileTo(Thing t)
        {
            // Don't become hostile to the symbiont we're compelled toward
            return t != targetSymbiont && base.ForceHostileTo(t);
        }
        
        public override bool ForceHostileTo(Faction f)
        {
            // Don't become hostile to player faction while compelled
            return f != Faction.OfPlayer && base.ForceHostileTo(f);
        }
        
        public override TaggedString GetBeginLetterText()
        {
            TaggedString baseText = base.GetBeginLetterText();
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                TaggedString intensity = GetIntensityMessage();
                return intensity;
            }
            
            return baseText;
        }
        
        private TaggedString GetIntensityMessage()
        {
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                return $"{pawn.Name.ToStringShort}'s mind has been invaded by alien whispers that claw at their consciousness. " +
                       "Their pupils dilate unnaturally as they turn toward the flesh symbiont with vacant, predatory hunger. " +
                       "Each step forward is accompanied by the wet sound of something moving beneath their skin.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                return $"{pawn.Name.ToStringShort} has been overwhelmed by psychic whispers from the flesh symbiont. " +
                       "They are walking toward it in a trance-like state, unable to resist its call.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                return $"{pawn.Name.ToStringShort} is being compelled by the symbiont.";
            }
            
            return base.GetBeginLetterText();
        }
    }
}