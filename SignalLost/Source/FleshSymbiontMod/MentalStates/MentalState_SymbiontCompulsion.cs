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
        
        public override void MentalStateTick()
        {
            base.MentalStateTick();
            
            // Find the nearest symbiont if we don't have one
            if (targetSymbiont == null || targetSymbiont.Destroyed)
            {
                FindNearestSymbiont();
            }
            
            if (targetSymbiont != null)
            {
                // Check if pawn is making progress toward the symbiont
                if (pawn.CurJob?.def != FleshSymbiontDefOf.CompelledToSymbiont)
                {
                    ticksWithoutProgress++;
                    
                    // Try to give the compulsion job
                    var job = JobMaker.MakeJob(FleshSymbiontDefOf.CompelledToSymbiont, targetSymbiont);
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
                else
                {
                    ticksWithoutProgress = 0;
                }
                
                // Give up if stuck for too long
                if (ticksWithoutProgress > MAX_TICKS_WITHOUT_PROGRESS)
                {
                    if (FleshSymbiontSettings.enableHorrorMessages)
                    {
                        Messages.Message($"{pawn.Name.ToStringShort} breaks free from the symbiont's compulsion after struggling against the alien whispers.", 
                            pawn, MessageTypeDefOf.PositiveEvent);
                    }
                    RecoverFromState();
                }
                
                // Add some atmospheric effects
                if (Rand.Chance(0.1f) && FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    MoteMaker.ThrowAirPuffUp(pawn.DrawPos, pawn.Map);
                    
                    if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 3)
                    {
                        var messages = new[]
                        {
                            $"{pawn.Name.ToStringShort}'s eyes glaze over as alien whispers fill their mind...",
                            $"{pawn.Name.ToStringShort} walks with jerky, unnatural movements toward the symbiont.",
                            $"{pawn.Name.ToStringShort} seems to be fighting against some invisible force pulling them forward.",
                            $"Strange shadows seem to dance around {pawn.Name.ToStringShort} as they approach the symbiont."
                        };
                        
                        if (Rand.Chance(0.05f)) // 5% chance per tick when atmospheric messages are on
                        {
                            Messages.Message(messages.RandomElement(), pawn, MessageTypeDefOf.NeutralEvent);
                        }
                    }
                }
            }
            else
            {
                // No symbiont found, end the mental state
                RecoverFromState();
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
        
        public override string GetBeginLetterText()
        {
            string baseText = base.GetBeginLetterText();
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                string intensity = FleshSymbiontSettings.messageFrequency switch
                {
                    3 => $"{pawn.Name.ToStringShort}'s mind has been invaded by alien whispers that claw at their consciousness. " +
                         "Their pupils dilate unnaturally as they turn toward the flesh symbiont with vacant, predatory hunger. " +
                         "Each step forward is accompanied by the wet sound of something moving beneath their skin.",
                    2 => $"{pawn.Name.ToStringShort} has been overwhelmed by psychic whispers from the flesh symbiont. " +
                         "They are walking toward it in a trance-like state, unable to resist its call.",
                    1 => $"{pawn.Name.ToStringShort} is being compelled by the symbiont.",
                    _ => baseText
                };
                
                return intensity;
            }
            
            return baseText;
        }
    }
}