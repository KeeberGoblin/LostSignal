using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Custom hediff class for symbiont bond
    public class Hediff_SymbiontBond : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            // Grant symbiont psycasts if Royalty is active
            if (ModsConfig.RoyaltyActive && FleshSymbiontSettings.enableRoyaltyFeatures && 
                FleshSymbiontSettings.enableSymbiontPsycasts && pawn.abilities != null)
            {
                var compel = FleshSymbiontDefOf.SymbiontCompel;
                var heal = FleshSymbiontDefOf.SymbiontHeal;
                var rage = FleshSymbiontDefOf.SymbiontRage;
                
                if (!pawn.abilities.HasAbility(compel))
                    pawn.abilities.GainAbility(compel);
                if (!pawn.abilities.HasAbility(heal))
                    pawn.abilities.GainAbility(heal);
                if (!pawn.abilities.HasAbility(rage))
                    pawn.abilities.GainAbility(rage);
            }
            
            if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.enableBodyHorrorDescriptions)
            {
                string message = FleshSymbiontSettings.messageFrequency switch
                {
                    3 => $"{pawn.Name.ToStringShort} convulses as the symbiont's tendrils burrow deep into their spinal cord. " +
                         $"Bone cracks, flesh tears, and alien tissue weaves itself into their nervous system. " +
                         $"Their eyes roll back, then snap open, now glowing with an unnatural crimson light. " +
                         $"When they stand, their movements are fluid, predatory... no longer entirely human.",
                    2 => $"{pawn.Name.ToStringShort} screams as the flesh symbiont's tendrils burrow into their spine. " +
                         $"Their eyes glow with an unnatural light...",
                    1 => $"{pawn.Name.ToStringShort} has been bonded by the symbiont.",
                    _ => ""
                };
                
                if (!string.IsNullOrEmpty(message))
                {
                    Messages.Message(message, pawn, MessageTypeDefOf.NegativeEvent);
                }
            }
        }
        
        public override void PostRemoved()
        {
            base.PostRemoved();
            
            // Remove symbiont psycasts when bond is severed
            if (ModsConfig.RoyaltyActive && pawn.abilities != null)
            {
                var symbiontAbilities = new[]
                {
                    FleshSymbiontDefOf.SymbiontCompel,
                    FleshSymbiontDefOf.SymbiontHeal, 
                    FleshSymbiontDefOf.SymbiontRage
                };
                
                foreach (var abilityDef in symbiontAbilities)
                {
                    var ability = pawn.abilities.GetAbility(abilityDef);
                    if (ability != null)
                    {
                        pawn.abilities.RemoveAbility(abilityDef);
                    }
                }
            }
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                string message = FleshSymbiontSettings.messageFrequency switch
                {
                    3 => $"The symbiont's tendrils tear free from {pawn.Name.ToStringShort}'s nervous system with a sickening wet sound. " +
                         $"They collapse, convulsing as alien flesh withdraws, leaving behind torn neural pathways and the memory of inhuman whispers.",
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
        }
        
        // Apply settings-based multipliers to effects
        public override void Tick()
        {
            base.Tick();
            
            // Modify madness frequency based on settings
            if (FleshSymbiontSettings.enableMadnessBreaks && Rand.MTBEventOccurs(
                15f / FleshSymbiontSettings.madnessFrequencyMultiplier, GenDate.TicksPerDay, 1f))
            {
                pawn.mindState.mentalStateHandler.TryStartMentalState(
                    FleshSymbiontDefOf.SymbiontMadness,
                    "Symbiont madness",
                    true,
                    causedByMood: false);
            }
            
            // Check for hybrid transformation in children (Biotech)
            if (ModsConfig.BiotechActive && FleshSymbiontSettings.enableBiotechFeatures && 
                FleshSymbiontSettings.allowSymbiontHybridBirth)
            {
                CheckForHybridTransformation();
            }
        }
        
        private void CheckForHybridTransformation()
        {
            // Only check for very young pawns who haven't been checked yet
            if (pawn.ageTracker.AgeBiologicalYears > 1 || pawn.genes?.Xenotype?.defName == "SymbiontHybrid")
                return;
                
            // Check if both parents are bonded
            var mother = pawn.GetMother();
            var father = pawn.GetFather();
            
            bool motherBonded = mother?.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) ?? false;
            bool fatherBonded = father?.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) ?? false;
            
            if ((motherBonded || fatherBonded) && Rand.Chance(FleshSymbiontSettings.hybridTransformationChance))
            {
                // Transform into symbiont hybrid
                if (ModsConfig.BiotechActive)
                {
                    pawn.genes?.SetXenotype(FleshSymbiontDefOf.SymbiontHybrid);
                    
                    if (FleshSymbiontSettings.enableHorrorMessages)
                    {
                        Messages.Message($"{pawn.Name.ToStringShort} shows signs of symbiont genetic inheritance. " +
                                        "The alien DNA has taken root...", 
                            pawn, MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
        }
        
        // Override stat modifications to apply settings multipliers
        public override void ModifyStatValue(StatDef stat, ref float value)
        {
            base.ModifyStatValue(stat, ref value);
            
            // Apply bonding benefits multiplier
            if (FleshSymbiontSettings.bondingBenefitsMultiplier != 1.0f)
            {
                if (stat == StatDefOf.MeleeHitChance || stat == StatDefOf.MeleeDodgeChance ||
                    stat == StatDefOf.ShootingAccuracyPawn || stat == StatDefOf.WorkSpeedGlobal)
                {
                    float bonus = (value - 1f) * FleshSymbiontSettings.bondingBenefitsMultiplier;
                    value = 1f + bonus;
                }
            }
            
            // Apply bonding penalties multiplier
            if (FleshSymbiontSettings.bondingPenaltiesMultiplier != 1.0f)
            {
                if (stat == StatDefOf.HungerRateMultiplier)
                {
                    float penalty = (value - 1f) * FleshSymbiontSettings.bondingPenaltiesMultiplier;
                    value = 1f + penalty;
                }
            }
        }
        
        // Override capacity modifications to apply settings multipliers
        public override void PostCapacityModifier(PawnCapacityDef capacity, ref float value)
        {
            base.PostCapacityModifier(capacity, ref value);
            
            if (FleshSymbiontSettings.bondingBenefitsMultiplier != 1.0f)
            {
                if (capacity == PawnCapacityDefOf.Moving || capacity == PawnCapacityDefOf.Manipulation ||
                    capacity == PawnCapacityDefOf.Sight)
                {
                    // Apply multiplier to the bonus portion only
                    float baseValue = 1.0f; // Assume base capacity is 1.0
                    float bonus = (value - baseValue) * FleshSymbiontSettings.bondingBenefitsMultiplier;
                    value = baseValue + bonus;
                }
            }
        }
        
        public override string Description => def.description + 
            (FleshSymbiontSettings.bondingBenefitsMultiplier != 1.0f || FleshSymbiontSettings.bondingPenaltiesMultiplier != 1.0f 
                ? $"\n\nBenefit intensity: {FleshSymbiontSettings.bondingBenefitsMultiplier:P0}" +
                  $"\nPenalty intensity: {FleshSymbiontSettings.bondingPenaltiesMultiplier:P0}"
                : "");
    }
}