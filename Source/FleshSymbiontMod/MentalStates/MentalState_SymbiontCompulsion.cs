using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FleshSymbiontMod
{
    public class MentalState_SymbiontCompulsion : MentalState
    {
        private Building_FleshSymbiont targetSymbiont;
        private int ticksActive = 0;
        private int lastProgressTick = 0;
        private const int MAX_DURATION = 5000; // ~5 minutes maximum
        private const int PROGRESS_CHECK_INTERVAL = 250; // Check progress every ~10 seconds
        
        public Building_FleshSymbiont TargetSymbiont => targetSymbiont;
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref targetSymbiont, "targetSymbiont");
            Scribe_Values.Look(ref ticksActive, "ticksActive", 0);
            Scribe_Values.Look(ref lastProgressTick, "lastProgressTick", 0);
        }
        
        public override void PostStart(string reason)
        {
            base.PostStart(reason);
            
            FindNearestSymbiont();
            
            if (targetSymbiont != null)
            {
                // Immediately start the compulsion job
                var job = JobMaker.MakeJob(FleshSymbiontDefOf.CompelledToSymbiont, targetSymbiont);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.MentalState);
                
                // Show compulsion start message
                ShowCompulsionStartMessage();
            }
            else
            {
                // No symbiont found, end the mental state
                RecoverFromState();
            }
        }
        
        public override void MentalStateTick()
        {
            base.MentalStateTick();
            
            ticksActive++;
            
            // Check if we should continue or abort
            if (ShouldEndCompulsion())
            {
                RecoverFromState();
                return;
            }
            
            // Periodic progress checks
            if (ticksActive - lastProgressTick > PROGRESS_CHECK_INTERVAL)
            {
                CheckCompulsionProgress();
                lastProgressTick = ticksActive;
            }
            
            // Atmospheric effects
            if (FleshSymbiontSettings.enableAtmosphericEffects && IsHashIntervalTick(120))
            {
                CreateCompulsionAtmosphere();
            }
        }
        
        private void FindNearestSymbiont()
        {
            if (pawn.Map == null) return;
            
            targetSymbiont = pawn.Map.GetAllSymbionts()
                .Where(s => s.Spawned && !s.Destroyed)
                .Where(s => s.Position.DistanceTo(pawn.Position) <= FleshSymbiontSettings.maxCompulsionRange)
                .OrderBy(s => s.Position.DistanceTo(pawn.Position))
                .FirstOrDefault();
        }
        
        private bool ShouldEndCompulsion()
        {
            // Maximum duration exceeded
            if (ticksActive > MAX_DURATION)
                return true;
            
            // Target symbiont is gone
            if (targetSymbiont == null || !targetSymbiont.Spawned || targetSymbiont.Destroyed)
                return true;
            
            // Pawn is incapacitated
            if (pawn.Downed || pawn.Dead)
                return true;
            
            // Pawn is already bonded
            if (pawn.IsSymbiontBonded())
                return true;
            
            // Too far from symbiont
            if (pawn.Position.DistanceTo(targetSymbiont.Position) > FleshSymbiontSettings.maxCompulsionRange * 1.5f)
                return true;
            
            return false;
        }
        
        private void CheckCompulsionProgress()
        {
            if (targetSymbiont == null) return;
            
            float distance = pawn.Position.DistanceTo(targetSymbiont.Position);
            
            // If pawn is close enough, try to complete bonding
            if (distance <= 2f && pawn.CanReach(targetSymbiont, PathEndMode.InteractionCell, Danger.Deadly))
            {
                TryCompleteBonding();
            }
            // If pawn seems stuck, give them a new job
            else if (pawn.CurJob?.def != FleshSymbiontDefOf.CompelledToSymbiont)
            {
                RefreshCompulsionJob();
            }
        }
        
        private void TryCompleteBonding()
        {
            if (pawn.IsSymbiontBonded()) return;
            
            // Small chance of last-minute resistance
            if (Rand.Chance(0.05f) && FleshSymbiontSettings.allowCompulsionResistance)
            {
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    Messages.Message($"{pawn.Name.ToStringShort} breaks free from the compulsion at the last moment!", 
                        pawn, MessageTypeDefOf.PositiveEvent);
                }
                RecoverFromState();
                return;
            }
            
            // Complete the bonding
            SymbiontUtility.TryBondPawnToSymbiont(pawn, targetSymbiont, voluntary: false);
            RecoverFromState();
        }
        
        private void RefreshCompulsionJob()
        {
            if (targetSymbiont?.Spawned == true)
            {
                var job = JobMaker.MakeJob(FleshSymbiontDefOf.CompelledToSymbiont, targetSymbiont);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.MentalState);
            }
        }
        
        private void CreateCompulsionAtmosphere()
        {
            if (pawn.Map == null) return;
            
            // Subtle effects around the compelled pawn
            if (Rand.Chance(0.3f))
            {
                MoteMaker.ThrowMicroSparks(pawn.DrawPos + Gen.RandomHorizontalVector(0.5f), pawn.Map);
            }
            
            // More dramatic effects if close to symbiont
            if (targetSymbiont != null && pawn.Position.DistanceTo(targetSymbiont.Position) < 8f)
            {
                if (Rand.Chance(0.1f))
                {
                    pawn.CreatePsychicEffect(0.5f);
                }
            }
        }
        
        private void ShowCompulsionStartMessage()
        {
            if (!FleshSymbiontSettings.enableHorrorMessages) return;
            
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{pawn.Name.ToStringShort}'s mind has been invaded by alien whispers that claw at their consciousness. " +
                     "Their pupils dilate unnaturally as they turn toward the flesh symbiont with vacant, predatory hunger. " +
                     "Each step forward is guided by an intelligence that is not their own.",
                2 => $"{pawn.Name.ToStringShort} has been overwhelmed by psychic whispers from the flesh symbiont. " +
                     "They are walking toward it in a trance-like state, unable to resist its call.",
                1 => $"{pawn.Name.ToStringShort} is being compelled by the symbiont.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, pawn, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        public override void PostEnd()
        {
            base.PostEnd();
            
            if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 2)
            {
                if (!pawn.IsSymbiontBonded()) // Only show if they didn't get bonded
                {
                    Messages.Message($"{pawn.Name.ToStringShort} shakes their head as if waking from a nightmare, " +
                                    "the alien whispers fading from their mind.", 
                        pawn, MessageTypeDefOf.NeutralEvent);
                }
            }
            
            // Clear any compulsion jobs
            if (pawn.CurJob?.def == FleshSymbiontDefOf.CompelledToSymbiont)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
        
        public override bool ForceHostileTo(Thing t)
        {
            // Don't become hostile to the symbiont we're compelled toward
            if (t == targetSymbiont) return false;
            
            // Don't become hostile to other symbionts either
            if (t is Building_FleshSymbiont) return false;
            
            return base.ForceHostileTo(t);
        }
        
        public override bool ForceHostileTo(Faction f)
        {
            // Don't become hostile to player faction while compelled
            return f != Faction.OfPlayer && base.ForceHostileTo(f);
        }
        
        public override TaggedString GetBeginLetterText()
        {
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                return GetHorrorBeginText();
            }
            
            return base.GetBeginLetterText();
        }
        
        private TaggedString GetHorrorBeginText()
        {
            return FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{pawn.Name.ToStringShort}'s mind has been seized by otherworldly intelligence. " +
                     "Alien whispers override their will as they move with unnatural purpose toward " +
                     "the flesh symbiont. Their humanity flickers like a dying candle in an alien wind.",
                2 => $"{pawn.Name.ToStringShort} has been overwhelmed by psychic whispers from the flesh symbiont. " +
                     "They are walking toward it in a trance-like state, unable to resist its call.",
                1 => $"{pawn.Name.ToStringShort} is being compelled by the symbiont.",
                _ => base.GetBeginLetterText()
            };
        }
        
        public override string GetInspectLine()
        {
            if (targetSymbiont == null)
                return "Compelled (no target)";
            
            float distance = pawn.Position.DistanceTo(targetSymbiont.Position);
            string distanceText = distance > 10f ? "far from" : distance > 5f ? "approaching" : "near";
            
            return $"Compelled ({distanceText} symbiont)";
        }
        
        // Prevent certain jobs while compelled
        public override bool AllowRestingInBed => false;
        
        public override RandomSocialMode SocialModeMax() => RandomSocialMode.Off;
        
        // Allow breaking out of compulsion under extreme circumstances
        public override bool CanBreakoutNow()
        {
            // Can break out if severely injured
            if (pawn.health.summaryHealth.SummaryHealthPercent < 0.3f)
                return true;
            
            // Can break out if in extreme pain
            if (pawn.health.hediffSet.PainTotal > 0.8f)
                return true;
            
            // Small chance to break out on their own over time
            if (ticksActive > MAX_DURATION * 0.8f && Rand.Chance(0.01f))
                return true;
            
            return false;
        }
        
        public override void Notify_AttackedTarget(LocalTargetInfo target)
        {
            base.Notify_AttackedTarget(target);
            
            // If they attack someone while compelled, end the compulsion
            if (target.Pawn != null)
            {
                RecoverFromState();
            }
        }
        
        public override void Notify_TookDamage(DamageInfo dinfo)
        {
            base.Notify_TookDamage(dinfo);
            
            // Chance to break compulsion when taking damage
            if (dinfo.Amount > 10f && Rand.Chance(0.3f))
            {
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    Messages.Message($"The pain breaks {pawn.Name.ToStringShort} free from the symbiont's compulsion!", 
                        pawn, MessageTypeDefOf.PositiveEvent);
                }
                RecoverFromState();
            }
        }
    }
}
