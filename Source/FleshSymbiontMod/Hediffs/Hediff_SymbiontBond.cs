using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;

namespace FleshSymbiontMod
{
    public class Hediff_SymbiontBond : HediffWithComps
    {
        private int ticksToNextMadnessCheck = 0;
        private bool hasGrantedAbilities = false;
        
        private const int MADNESS_CHECK_INTERVAL = 90000; // ~1.5 in-game days
        
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            // Grant abilities once
            if (!hasGrantedAbilities)
            {
                GrantSymbiontAbilities();
                hasGrantedAbilities = true;
            }
            
            // Show bonding message
            if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.enableBodyHorrorDescriptions)
            {
                ShowBondingMessage();
            }
            
            // Initialize madness timer
            ticksToNextMadnessCheck = Rand.Range(MADNESS_CHECK_INTERVAL / 2, MADNESS_CHECK_INTERVAL);
        }
        
        public override void PostRemoved()
        {
            base.PostRemoved();
            
            // Remove symbiont abilities
            RemoveSymbiontAbilities();
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                ShowUnbondingMessage();
            }
        }
        
        public override void Tick()
        {
            base.Tick();
            
            if (pawn?.Dead != false || !pawn.Spawned) return;
            
            // Check for madness breaks
            if (FleshSymbiontSettings.enableMadnessBreaks)
            {
                ticksToNextMadnessCheck--;
                if (ticksToNextMadnessCheck <= 0)
                {
                    CheckForMadnessBreak();
                    ResetMadnessTimer();
                }
            }
            
            // Check for hybrid transformation in children
            if (Extensions.CanUseBiotechFeatures() && FleshSymbiontSettings.allowSymbiontHybridBirth)
            {
                CheckForHybridTransformation();
            }
            
            // Periodic enhancement effects
            if (IsHashIntervalTick(2500)) // Every ~40 seconds
            {
                ApplyPeriodicEffects();
            }
        }
        
        private void GrantSymbiontAbilities()
        {
            if (!ModsConfig.RoyaltyActive || !FleshSymbiontSettings.enableRoyaltyFeatures || 
                !FleshSymbiontSettings.enableSymbiontPsycasts || pawn.abilities == null) return;
            
            var abilities = new[]
            {
                FleshSymbiontDefOf.SymbiontCompel,
                FleshSymbiontDefOf.SymbiontHeal,
                FleshSymbiontDefOf.SymbiontRage
            };
            
            foreach (var abilityDef in abilities)
            {
                if (abilityDef != null && !pawn.abilities.HasAbility(abilityDef))
                {
                    pawn.abilities.GainAbility(abilityDef);
                }
            }
        }
        
        private void RemoveSymbiontAbilities()
        {
            if (!ModsConfig.RoyaltyActive || pawn.abilities == null) return;
            
            var symbiontAbilities = new[]
            {
                FleshSymbiontDefOf.SymbiontCompel,
                FleshSymbiontDefOf.SymbiontHeal, 
                FleshSymbiontDefOf.SymbiontRage
            };
            
            foreach (var abilityDef in symbiontAbilities)
            {
                if (abilityDef != null)
                {
                    var ability = pawn.abilities.GetAbility(abilityDef);
                    if (ability != null)
                    {
                        pawn.abilities.RemoveAbility(abilityDef);
                    }
                }
            }
        }
        
        private void ShowBondingMessage()
        {
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{pawn.Name.ToStringShort} convulses as the symbiont's tendrils burrow deep into their spinal cord. " +
                     $"Bone cracks, flesh tears, and alien tissue weaves itself into their nervous system. " +
                     $"Their eyes roll back, then snap open, now glowing with an unnatural crimson light. " +
                     $"When they stand, their movements are fluid, predatory... no longer entirely human.",
                2 => $"{pawn.Name.ToStringShort} screams as the flesh symbiont's tendrils burrow into their spine. " +
                     $"Their eyes glow with an unnatural light as the bonding completes...",
                1 => $"{pawn.Name.ToStringShort} has been bonded by the symbiont.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, pawn, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        private void ShowUnbondingMessage()
        {
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"The symbiont's tendrils tear free from {pawn.Name.ToStringShort}'s nervous system with a sickening wet sound. " +
                     $"They collapse, convulsing as alien flesh withdraws, leaving behind torn neural pathways and the fading echo of inhuman whispers.",
                2 => $"The symbiont bond with {pawn.Name.ToStringShort} has been severed. " +
                     $"They collapse, convulsing as alien flesh withdraws from their nervous system.",
                1 => $"{pawn.Name.ToStringShort} is no longer bonded to the symbiont.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
        
        private void CheckForMadnessBreak()
        {
            float baseMadnessChance = 0.08f * FleshSymbiontSettings.madnessFrequencyMultiplier;
            
            // Modify chance based on pawn's mental state
            if (pawn.needs?.mood != null)
            {
                float moodLevel = pawn.needs.mood.CurLevelPercentage;
                baseMadnessChance += (0.5f - moodLevel) * 0.15f;
            }
            
            // Trait modifiers
            if (pawn.story?.traits != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Nerves))
                {
                    var degree = pawn.story.traits.GetTrait(TraitDefOf.Nerves).Degree;
                    baseMadnessChance -= degree * 0.05f; // Iron will reduces madness
                }
                
                if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
                    baseMadnessChance -= 0.03f;
                    
                if (pawn.story.traits.HasTrait(TraitDefOf.Volatile))
                    baseMadnessChance += 0.05f;
            }
            
            // Check for nearby symbionts (calming effect)
            if (pawn.Map != null)
            {
                var nearbySymbionts = pawn.Map.GetAllSymbionts()
                    .Where(s => s.Position.DistanceTo(pawn.Position) < 8f);
                    
                if (nearbySymbionts.Any())
                    baseMadnessChance *= 0.7f; // Proximity to symbiont reduces madness
            }
            
            baseMadnessChance = Mathf.Clamp(baseMadnessChance, 0.01f, 0.25f);
            
            if (Rand.Chance(baseMadnessChance))
            {
                TriggerMadnessBreak();
            }
        }
        
        private void TriggerMadnessBreak()
        {
            bool success = pawn.mindState.mentalStateHandler.TryStartMentalState(
                FleshSymbiontDefOf.SymbiontMadness,
                "Overwhelmed by symbiont influence",
                true,
                causedByMood: false);
                
            if (success && FleshSymbiontSettings.enableHorrorMessages)
            {
                string message = Extensions.GetHorrorIntensityMessage(
                    $"{pawn.Name.ToStringShort} succumbs to symbiont madness.",
                    $"{pawn.Name.ToStringShort}'s eyes blaze with alien fury as the symbiont overwhelms their mind.",
                    $"{pawn.Name.ToStringShort} throws back their head and releases an inhuman shriek as the symbiont's consciousness floods their brain. Their humanity drowns beneath waves of alien hunger and rage."
                );
                
                if (!string.IsNullOrEmpty(message))
                {
                    Messages.Message(message, pawn, MessageTypeDefOf.NegativeEvent);
                }
            }
        }
        
        private void ResetMadnessTimer()
        {
            float baseInterval = MADNESS_CHECK_INTERVAL / FleshSymbiontSettings.madnessFrequencyMultiplier;
            ticksToNextMadnessCheck = Rand.Range((int)(baseInterval * 0.8f), (int)(baseInterval * 1.2f));
        }
        
        private void CheckForHybridTransformation()
        {
            // Only check very young pawns
            if (pawn.ageTracker.AgeBiologicalYears > 1) return;
            
            // Check if this is a child of bonded parents
            var mother = pawn.GetMother();
            var father = pawn.GetFather();
            
            bool motherBonded = mother?.IsSymbiontBonded() ?? false;
            bool fatherBonded = father?.IsSymbiontBonded() ?? false;
            
            if ((motherBonded || fatherBonded) && Rand.MTBEventOccurs(30f, GenDate.TicksPerDay, 1f))
            {
                if (Rand.Chance(FleshSymbiontSettings.hybridTransformationChance))
                {
                    TransformToHybrid();
                }
            }
        }
        
        private void TransformToHybrid()
        {
            if (!ModsConfig.BiotechActive || FleshSymbiontDefOf.SymbiontHybrid == null) return;
            
            pawn.genes?.SetXenotype(FleshSymbiontDefOf.SymbiontHybrid);
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                string message = Extensions.GetHorrorIntensityMessage(
                    $"{pawn.Name.ToStringShort} shows signs of symbiont genetic inheritance.",
                    $"{pawn.Name.ToStringShort} develops alien traits as symbiont genes express themselves.",
                    $"The alien DNA takes root in {pawn.Name.ToStringShort} as symbiont genetics overwrite human chromosomes. They are no longer entirely human."
                );
                
                if (!string.IsNullOrEmpty(message))
                {
                    Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent);
                }
            }
        }
        
        private void ApplyPeriodicEffects()
        {
            // Small chance of temporary regeneration boost
            if (Rand.Chance(0.05f))
            {
                var regenHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontRegeneration, pawn);
                regenHediff.Severity = 0.3f;
                pawn.health.AddHediff(regenHediff);
            }
            
            // Gradual mutation development (low chance)
            if (Extensions.CanUseBiotechFeatures() && Rand.Chance(0.001f))
            {
                DevelopMutation();
            }
            
            // Atmospheric effects around bonded pawns
            if (FleshSymbiontSettings.enableAtmosphericEffects && Rand.Chance(0.01f))
            {
                MoteMaker.ThrowMicroSparks(pawn.DrawPos + Gen.RandomHorizontalVector(0.5f), pawn.Map);
            }
        }
        
        private void DevelopMutation()
        {
            if (pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontMutation)) return;
            
            var mutationHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontMutation, pawn);
            mutationHediff.Severity = 0.1f;
            pawn.health.AddHediff(mutationHediff);
            
            if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 2)
            {
                Messages.Message($"{pawn.Name.ToStringShort} shows signs of gradual symbiont mutation...", 
                    pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToNextMadnessCheck, "ticksToNextMadnessCheck", 0);
            Scribe_Values.Look(ref hasGrantedAbilities, "hasGrantedAbilities", false);
        }
        
        public override string Description => 
            def.description + GetBondStatusDescription();
        
        private string GetBondStatusDescription()
        {
            if (!FleshSymbiontSettings.enableBodyHorrorDescriptions) return "";
            
            string status = "\n\n";
            
            // Describe current state
            if (pawn.Map != null)
            {
                var nearbySymbionts = pawn.Map.GetAllSymbionts()
                    .Where(s => s.Position.DistanceTo(pawn.Position) < 12f);
                    
                if (nearbySymbionts.Any())
                {
                    status += "The symbiont's presence calms the alien whispers in their mind. ";
                }
                else
                {
                    status += "Separated from their symbiont, they struggle against growing instability. ";
                }
            }
            
            // Madness timer info
            if (FleshSymbiontSettings.enableMadnessBreaks)
            {
                float hoursUntilCheck = ticksToNextMadnessCheck / 2500f;
                if (hoursUntilCheck < 12f)
                {
                    status += "The alien influence grows stronger...";
                }
            }
            
            return status;
        }
        
        public override string TipStringExtra
        {
            get
            {
                string tip = base.TipStringExtra;
                
                if (pawn.Map != null)
                {
                    var nearestSymbiont = pawn.Map.GetNearestSymbiont(pawn.Position);
                    if (nearestSymbiont != null)
                    {
                        float distance = pawn.Position.DistanceTo(nearestSymbiont.Position);
                        tip += $"\nDistance to symbiont: {distance:F0} tiles";
                    }
                }
                
                if (FleshSymbiontSettings.enableMadnessBreaks)
                {
                    float hoursUntilCheck = ticksToNextMadnessCheck / 2500f;
                    tip += $"\nNext stability check: {hoursUntilCheck:F1} hours";
                }
                
                if (hasGrantedAbilities && ModsConfig.RoyaltyActive)
                {
                    tip += "\nGranted symbiont psycasts";
                }
                
                return tip;
            }
        }
        
        // Override stat offsets to apply settings multipliers
        public override void ModifyStatValue(StatDef stat, ref float val)
        {
            base.ModifyStatValue(stat, ref val);
            
            if (CurStage?.statOffsets == null) return;
            
            foreach (var offset in CurStage.statOffsets)
            {
                if (offset.stat == stat)
                {
                    // Remove the base offset that was already applied
                    val -= offset.value;
                    
                    // Apply modified offset based on settings
                    float modifiedOffset = offset.value;
                    
                    if (offset.value > 0) // Benefit
                        modifiedOffset *= FleshSymbiontSettings.bondingBenefitsMultiplier;
                    else // Penalty
                        modifiedOffset *= FleshSymbiontSettings.bondingPenaltiesMultiplier;
                    
                    val += modifiedOffset;
                }
            }
        }
    }
}
