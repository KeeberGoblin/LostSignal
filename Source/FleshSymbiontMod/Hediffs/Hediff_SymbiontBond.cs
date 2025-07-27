using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Custom hediff class for symbiont bond
    public class Hediff_SymbiontBond : HediffWithComps
    {
        private Building_FleshSymbiont bondedSymbiont;
        private int ticksSinceBonding = 0;
        private int madnessCheckInterval = 0;
        private bool hasGrantedAbilities = false;
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref bondedSymbiont, "bondedSymbiont");
            Scribe_Values.Look(ref ticksSinceBonding, "ticksSinceBonding", 0);
            Scribe_Values.Look(ref madnessCheckInterval, "madnessCheckInterval", 0);
            Scribe_Values.Look(ref hasGrantedAbilities, "hasGrantedAbilities", false);
        }
        
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            // Grant symbiont psycasts if Royalty is active
            if (ModsConfig.RoyaltyActive && FleshSymbiontSettings.enableRoyaltyFeatures && 
                FleshSymbiontSettings.enableSymbiontPsycasts && pawn.abilities != null && !hasGrantedAbilities)
            {
                GrantSymbiontAbilities();
            }
            
            // Find nearest symbiont to establish bond
            if (bondedSymbiont == null && pawn.Map != null)
            {
                bondedSymbiont = pawn.Map.GetNearestSymbiont(pawn.Position);
            }
            
            // Show bonding message
            if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.enableBodyHorrorDescriptions)
            {
                ShowBondingMessage();
            }
            
            // Create atmospheric effects
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                CreateBondingEffects();
            }
        }
        
        public override void PostRemoved()
        {
            base.PostRemoved();
            
            // Remove symbiont psycasts when bond is severed
            if (ModsConfig.RoyaltyActive && pawn.abilities != null && hasGrantedAbilities)
            {
                RemoveSymbiontAbilities();
            }
            
            // Show unbonding message
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                ShowUnbondingMessage();
            }
            
            // Create removal effects
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                CreateRemovalEffects();
            }
        }
        
        public override void Tick()
        {
            base.Tick();
            
            ticksSinceBonding++;
            madnessCheckInterval++;
            
            // Grant abilities if not yet done (for save compatibility)
            if (ModsConfig.RoyaltyActive && FleshSymbiontSettings.enableRoyaltyFeatures && 
                FleshSymbiontSettings.enableSymbiontPsycasts && pawn.abilities != null && !hasGrantedAbilities)
            {
                GrantSymbiontAbilities();
            }
            
            // Periodic madness checks
            if (FleshSymbiontSettings.enableMadnessBreaks && 
                madnessCheckInterval >= GetMadnessCheckInterval())
            {
                CheckForMadness();
                madnessCheckInterval = 0;
            }
            
            // Check for hybrid transformation in children (Biotech)
            if (ModsConfig.BiotechActive && FleshSymbiontSettings.enableBiotechFeatures && 
                FleshSymbiontSettings.allowSymbiontHybridBirth && ticksSinceBonding < 60000) // Only check in first day
            {
                CheckForHybridTransformation();
            }
            
            // Gradual mutation development
            if (FleshSymbiontSettings.enableBiotechFeatures && Rand.MTBEventOccurs(30f, GenDate.TicksPerDay, 1f))
            {
                TryDevelopMutation();
            }
            
            // Atmospheric effects
            if (FleshSymbiontSettings.enableAtmosphericEffects && Rand.Chance(0.01f))
            {
                CreateBondEffects();
            }
        }
        
        private void GrantSymbiontAbilities()
        {
            try
            {
                var compel = FleshSymbiontDefOf.SymbiontCompel;
                var heal = FleshSymbiontDefOf.SymbiontHeal;
                var rage = FleshSymbiontDefOf.SymbiontRage;
                
                if (compel != null && !pawn.abilities.HasAbility(compel))
                    pawn.abilities.GainAbility(compel);
                if (heal != null && !pawn.abilities.HasAbility(heal))
                    pawn.abilities.GainAbility(heal);
                if (rage != null && !pawn.abilities.HasAbility(rage))
                    pawn.abilities.GainAbility(rage);
                    
                hasGrantedAbilities = true;
            }
            catch
            {
                // Fail silently if abilities don't exist
            }
        }
        
        private void RemoveSymbiontAbilities()
        {
            try
            {
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
                
                hasGrantedAbilities = false;
            }
            catch
            {
                // Fail silently
            }
        }
        
        private int GetMadnessCheckInterval()
        {
            // Base interval of 15 days, modified by settings
            return Mathf.RoundToInt(GenDate.TicksPerDay * 15f / FleshSymbiontSettings.madnessFrequencyMultiplier);
        }
        
        private void CheckForMadness()
        {
            // Calculate madness chance based on various factors
            float madnessChance = 0.1f; // Base 10% chance
            
            // Modify by settings
            madnessChance *= FleshSymbiontSettings.madnessFrequencyMultiplier;
            
            // Trait modifiers
            if (pawn.story?.traits != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Nerves))
                {
                    var degree = pawn.story.traits.GetTrait(TraitDefOf.Nerves).Degree;
                    madnessChance *= (1f - (degree * 0.3f)); // Iron will reduces madness
                }
                
                if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
                    madnessChance *= 0.5f; // Psychopaths resist better
                    
                if (pawn.story.traits.HasTrait(TraitDefOf.Volatile))
                    madnessChance *= 1.5f; // Volatile pawns more prone
            }
            
            // Mood modifier
            if (pawn.needs?.mood != null)
            {
                float moodLevel = pawn.needs.mood.CurLevelPercentage;
                madnessChance *= (1.5f - moodLevel); // Lower mood = higher risk
            }
            
            // Distance from symbiont affects stability
            if (bondedSymbiont != null && pawn.Map != null)
            {
                float distance = pawn.Position.DistanceTo(bondedSymbiont.Position);
                if (distance > 20f)
                {
                    madnessChance *= 1.3f; // Being far from symbiont increases instability
                }
            }
            
            if (Rand.Chance(madnessChance))
            {
                pawn.mindState.mentalStateHandler.TryStartMentalState(
                    FleshSymbiontDefOf.SymbiontMadness,
                    "Symbiont madness",
                    true,
                    causedByMood: false);
                    
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    string message = Extensions.GetHorrorIntensityMessage(
                        $"{pawn.Name.ToStringShort} has succumbed to symbiont madness.",
                        $"{pawn.Name.ToStringShort}'s eyes blaze with alien light as the symbiont's influence overwhelms their mind.",
                        $"The symbiont's whispers become screams in {pawn.Name.ToStringShort}'s mind as alien consciousness " +
                        "floods their neural pathways. They howl with inhuman rage, their humanity drowning in the tide of alien thought."
                    );
                    
                    if (!string.IsNullOrEmpty(message))
                    {
                        Messages.Message(message, pawn, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
        }
        
        private void CheckForHybridTransformation()
        {
            // Only check for very young pawns who haven't been checked yet
            if (pawn.ageTracker.AgeBiologicalYears > 1)
                return;
                
            // Check if both parents are bonded
            var mother = pawn.GetMother();
            var father = pawn.GetFather();
            
            bool motherBonded = mother?.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) ?? false;
            bool fatherBonded = father?.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) ?? false;
            
            if ((motherBonded || fatherBonded) && Rand.Chance(FleshSymbiontSettings.hybridTransformationChance))
            {
                // Transform into symbiont hybrid
                if (ModsConfig.BiotechActive && FleshSymbiontDefOf.SymbiontHybrid != null)
                {
                    pawn.genes?.SetXenotype(FleshSymbiontDefOf.SymbiontHybrid);
                    
                    if (FleshSymbiontSettings.enableHorrorMessages)
                    {
                        string message = Extensions.GetHorrorIntensityMessage(
                            $"{pawn.Name.ToStringShort} shows signs of symbiont genetic inheritance.",
                            $"{pawn.Name.ToStringShort} develops alien traits as symbiont genes express themselves.",
                            $"The alien DNA takes root in {pawn.Name.ToStringShort} as symbiont genetics " +
                            "overwrite human chromosomes. They are no longer entirely human."
                        );
                        
                        if (!string.IsNullOrEmpty(message))
                        {
                            Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent);
                        }
                    }
                }
            }
        }
        
        private void TryDevelopMutation()
        {
            if (!pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontMutation))
            {
                var mutationHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontMutation, pawn);
                mutationHediff.Severity = 0.1f;
                pawn.health.AddHediff(mutationHediff);
            }
        }
        
        private void ShowBondingMessage()
        {
            string message = "";
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                message = $"{pawn.Name.ToStringShort} convulses as the symbiont's tendrils burrow deep into their spinal cord. " +
                         $"Bone cracks, flesh tears, and alien tissue weaves itself into their nervous system. " +
                         $"Their eyes roll back, then snap open, now glowing with an unnatural crimson light. " +
                         $"When they stand, their movements are fluid, predatory... no longer entirely human.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                message = $"{pawn.Name.ToStringShort} screams as the flesh symbiont's tendrils burrow into their spine. " +
                         $"Their eyes glow with an unnatural light...";
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                message = $"{pawn.Name.ToStringShort} has been bonded by the symbiont.";
            }
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, pawn, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        private void ShowUnbondingMessage()
        {
            string message = "";
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                message = $"The symbiont's tendrils tear free from {pawn.Name.ToStringShort}'s nervous system with a sickening wet sound. " +
                         $"They collapse, convulsing as alien flesh withdraws, leaving behind torn neural pathways and the memory of inhuman whispers.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                message = $"The symbiont bond with {pawn.Name.ToStringShort} has been severed. " +
                         $"They collapse, convulsing as alien flesh withdraws from their nervous system.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                message = $"{pawn.Name.ToStringShort} is no longer bonded to the symbiont.";
            }
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
        
        private void CreateBondingEffects()
        {
            if (pawn.Map != null)
            {
                pawn.Position.CreateSymbiontAtmosphere(pawn.Map, 2.0f);
                MoteMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.5f);
                
                // Play bonding sound
                var bondingSound = SoundDef.Named("BondingScream");
                bondingSound?.PlayOneShot(new TargetInfo(pawn));
            }
        }
        
        private void CreateRemovalEffects()
        {
            if (pawn.Map != null)
            {
                pawn.CreatePsychicEffect(1.5f);
                for (int i = 0; i < 10; i++)
                {
                    MoteMaker.ThrowDustPuff(pawn.DrawPos + Gen.RandomHorizontalVector(2f), pawn.Map, 1.8f);
                }
            }
        }
        
        private void CreateBondEffects()
        {
            if (pawn.Map != null)
            {
                MoteMaker.ThrowMicroSparks(pawn.DrawPos + Gen.RandomHorizontalVector(0.5f), pawn.Map);
            }
        }
        
        public override string Description => def.description + 
            (FleshSymbiontSettings.bondingBenefitsMultiplier != 1.0f || FleshSymbiontSettings.bondingPenaltiesMultiplier != 1.0f 
                ? $"\n\nBenefit intensity: {FleshSymbiontSettings.bondingBenefitsMultiplier:P0}" +
                  $"\nPenalty intensity: {FleshSymbiontSettings.bondingPenaltiesMultiplier:P0}"
                : "");
        
        public override string TipStringExtra
        {
            get
            {
                string tip = "";
                
                if (bondedSymbiont != null)
                {
                    float distance = pawn.Position.DistanceTo(bondedSymbiont.Position);
                    tip += $"Distance to symbiont: {distance:F0} tiles\n";
                }
                
                tip += $"Bonded for: {(ticksSinceBonding / 2500f):F1} hours\n";
                
                if (FleshSymbiontSettings.enableMadnessBreaks)
                {
                    int ticksUntilNext = GetMadnessCheckInterval() - madnessCheckInterval;
                    tip += $"Next madness check: {(ticksUntilNext / 2500f):F1} hours";
                }
                
                return tip;
            }
        }
    }
}