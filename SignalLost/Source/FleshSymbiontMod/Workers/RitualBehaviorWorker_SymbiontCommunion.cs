using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Ritual behavior worker for symbiont communion
    public class RitualBehaviorWorker_SymbiontCommunion : RitualBehaviorWorker
    {
        public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn organizer = null, Dictionary<string, Pawn> forcedForRole = null)
        {
            if (!FleshSymbiontSettings.enableIdeologyFeatures || !FleshSymbiontSettings.enableRituals)
                return "Symbiont rituals are disabled in mod settings";
            
            var map = target.Map ?? organizer?.Map;
            if (map == null)
                return "No valid map found";
            
            // Check if there are any symbionts on the map
            var symbionts = map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>();
            if (!symbionts.Any())
                return "No flesh symbionts available for communion";
            
            // Check if there are enough bonded colonists
            var bondedColonists = map.mapPawns.FreeColonistsSpawned
                .Where(p => p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
                .ToList();
            
            if (bondedColonists.Count < 2)
                return "Need at least 2 bonded colonists for communion ritual";
            
            return base.CanStartRitualNow(target, ritual, organizer, forcedForRole);
        }
        
        public override void PostCleanup(LordJob_Ritual ritual)
        {
            base.PostCleanup(ritual);
            
            if (!FleshSymbiontSettings.enableRituals)
                return;
            
            var map = ritual.Map;
            if (map == null) return;
            
            // Find participants and nearby symbionts
            var participants = ritual.PawnsToCountTowardsPresence.ToList();
            var nearbySymbionts = map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>()
                .Where(s => participants.Any(p => p.Position.DistanceTo(s.Position) < 15f))
                .ToList();
            
            if (!nearbySymbionts.Any())
                return;
            
            // Play ritual completion sound
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                var ritualSound = SoundDef.Named("RitualChant");
                ritualSound?.PlayOneShot(new TargetInfo(nearbySymbionts.First()));
            }
            
            // Apply ritual benefits
            ApplyRitualBenefits(participants, nearbySymbionts);
            
            // Create atmospheric effects
            CreateCommunionEffects(participants, nearbySymbionts);
            
            // Send completion message
            SendCompletionMessage(participants.Count, nearbySymbionts.Count);
        }
        
        private void ApplyRitualBenefits(List<Pawn> participants, List<Building_FleshSymbiont> symbionts)
        {
            float benefitMultiplier = FleshSymbiontSettings.ritualBenefitsMultiplier;
            
            foreach (var pawn in participants)
            {
                // Core ritual mood buff
                var communionThought = ThoughtMaker.MakeThought(FleshSymbiontDefOf.AttendedSymbiontCommunion);
                communionThought.moodPowerFactor = benefitMultiplier;
                pawn.needs.mood.thoughts.memories.TryGainMemory(communionThought);
                
                // Bonded participants get special benefits
                if (pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
                {
                    ApplyBondedParticipantBenefits(pawn, benefitMultiplier);
                }
                
                // Non-bonded participants may have mixed reactions
                else
                {
                    ApplyNonBondedParticipantEffects(pawn);
                }
            }
            
            // Empower the symbionts
            foreach (var symbiont in symbionts)
            {
                symbiont.OnSymbiontFed();
                
                // Communion reduces symbiont madness frequency for bonded pawns
                if (Rand.Chance(0.4f * benefitMultiplier))
                {
                    var bondedNearby = symbiont.Map.mapPawns.FreeColonistsSpawned
                        .Where(p => p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) &&
                                   p.Position.DistanceTo(symbiont.Position) < 12f);
                    
                    foreach (var bonded in bondedNearby)
                    {
                        // Reset madness timer by giving temporary stability
                        var stabilityHediff = HediffMaker.MakeHediff(HediffDef.Named("SymbiontStability"), bonded);
                        stabilityHediff.Severity = 1.0f;
                        bonded.health.AddHediff(stabilityHediff);
                    }
                }
            }
        }
        
        private void ApplyBondedParticipantBenefits(Pawn pawn, float multiplier)
        {
            // Enhanced psyfocus gain for psycasters
            if (ModsConfig.RoyaltyActive && pawn.psychicEntropy != null)
            {
                float psyfocusGain = 0.35f * multiplier;
                pawn.psychicEntropy.OffsetPsyfocusDirectly(psyfocusGain);
            }
            
            // Temporary regeneration boost
            if (Rand.Chance(0.6f * multiplier))
            {
                var regenHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontRegeneration, pawn);
                regenHediff.Severity = 0.8f * multiplier;
                pawn.health.AddHediff(regenHediff);
            }
            
            // Small chance of learning from the symbiont's alien knowledge
            if (Rand.Chance(0.2f * multiplier))
            {
                var randomSkill = DefDatabase<SkillDef>.AllDefsListForReading.RandomElement();
                pawn.skills.Learn(randomSkill, 500f * multiplier);
                
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    Messages.Message($"{pawn.Name.ToStringShort} gains insight into {randomSkill.label} " +
                                    "through the symbiont's alien knowledge.", 
                        pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }
        
        private void ApplyNonBondedParticipantEffects(Pawn pawn)
        {
            // Non-bonded participants may be disturbed or inspired
            if (pawn.Ideo?.PreceptsListForReading?.Any(p => p.def.defName == "FleshSymbiosis") == true)
            {
                // Believers are inspired but not bonded
                var inspirationThought = ThoughtMaker.MakeThought(ThoughtDef.Named("WitnessedTranscendence"));
                pawn.needs.mood.thoughts.memories.TryGainMemory(inspirationThought);
                
                // Small chance of voluntary bonding desire
                if (Rand.Chance(0.15f) && FleshSymbiontSettings.allowVoluntaryBonding)
                {
                    pawn.mindState.lastInvoluntaryJob = new Job(FleshSymbiontDefOf.TouchFleshSymbiont, 
                        pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>().RandomElement());
                }
            }
            else
            {
                // Non-believers are disturbed
                var disturbedThought = ThoughtMaker.MakeThought(ThoughtDef.Named("WitnessedAlienRitual"));
                pawn.needs.mood.thoughts.memories.TryGainMemory(disturbedThought);
            }
        }
        
        private void CreateCommunionEffects(List<Pawn> participants, List<Building_FleshSymbiont> symbionts)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects)
                return;
            
            foreach (var symbiont in symbionts)
            {
                // Pulsing energy effects around each symbiont
                for (int i = 0; i < 12; i++)
                {
                    var effectPos = symbiont.DrawPos + Gen.RandomHorizontalVector(4f);
                    MoteMaker.ThrowHeatGlow(symbiont.Position, symbiont.Map, 1.5f);
                    MoteMaker.ThrowMicroSparks(effectPos, symbiont.Map);
                }
                
                // Sound effect (if available)
                // SoundDefOf.PsycastPsyshockExplosion.PlayOneShot(new TargetInfo(symbiont));
            }
            
            // Effects around participants
            foreach (var participant in participants.Where(p => p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond)))
            {
                // Glowing eyes effect for bonded participants
                MoteMaker.ThrowLightningGlow(participant.DrawPos, participant.Map, 0.8f);
                
                // Energy connection visualization
                var nearestSymbiont = symbionts.OrderBy(s => s.Position.DistanceTo(participant.Position)).FirstOrDefault();
                if (nearestSymbiont != null && participant.Position.DistanceTo(nearestSymbiont.Position) < 12f)
                {
                    // Create energy beam effect (simplified)
                    MoteMaker.ThrowDustPuff(
                        Vector3.Lerp(participant.DrawPos, nearestSymbiont.DrawPos, 0.5f), 
                        participant.Map, 1.2f);
                }
            }
        }
        
        private void SendCompletionMessage(int participantCount, int symbiontCount)
        {
            if (!FleshSymbiontSettings.enableHorrorMessages)
                return;
            
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"The communion ritual reaches its crescendo as {participantCount} worshippers unite their consciousness " +
                     $"with {symbiontCount} flesh symbiont{(symbiontCount > 1 ? "s" : "")}. The air thrums with alien energy as " +
                     "human minds briefly touch something vast and incomprehensible. When the connection fades, the participants " +
                     "stagger, forever changed by their glimpse into transcendence. Some weep with joy, others with terror at what they have become.",
                     
                2 => $"The symbiont communion concludes. {participantCount} participants shared consciousness with " +
                     $"{symbiontCount} flesh symbiont{(symbiontCount > 1 ? "s" : "")}. The alien entities pulse with renewed vigor.",
                     
                1 => "Symbiont communion ritual completed successfully.",
                
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                var location = participantCount > 0 ? participants.First() : symbionts.First();
                Messages.Message(message, location, MessageTypeDefOf.RitualCompleted);
            }
        }
        
        public override float MoodMultiplier(LordJob_Ritual ritual)
        {
            // Base mood multiplier affected by settings
            float baseMood = base.MoodMultiplier(ritual);
            return baseMood * FleshSymbiontSettings.ritualBenefitsMultiplier;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            // Add any persistent data if needed
        }
    }
    
    // Job giver for communion priest
    public class JobGiver_SymbiontCommunion : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!FleshSymbiontSettings.enableRituals)
                return null;
            
            var symbiont = pawn.Map?.listerBuildings?.AllBuildingsColonistOfClass<Building_FleshSymbiont>()
                ?.OrderBy(s => s.Position.DistanceTo(pawn.Position))
                ?.FirstOrDefault();
                
            if (symbiont != null)
            {
                // Priest meditates facing the symbiont during ritual
                return JobMaker.MakeJob(JobDefOf.Meditate, symbiont);
            }
            
            return null;
        }
    }
}