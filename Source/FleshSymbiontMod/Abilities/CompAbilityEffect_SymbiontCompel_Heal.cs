using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Simplified symbiont compel ability
    public class CompAbilityEffect_SymbiontCompel : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            
            var caster = parent.pawn;
            var targetPawn = target.Pawn;
            
            if (targetPawn == null || targetPawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
                return;
                
            // Check if caster is bonded
            if (!caster.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
            {
                Messages.Message($"{caster.Name.ToStringShort} must be bonded to a symbiont to use this ability.", 
                    caster, MessageTypeDefOf.RejectInput);
                return;
            }
            
            // Play psycast activation sound
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                var psycastSound = SoundDef.Named("PsycastActivate");
                psycastSound?.PlayOneShot(new TargetInfo(caster));
            }
            
            // Find nearest symbiont
            var symbiont = targetPawn.Map?.listerBuildings?.AllBuildingsColonistOfClass<Building_FleshSymbiont>()
                ?.OrderBy(s => s.Position.DistanceTo(targetPawn.Position))
                ?.FirstOrDefault();
                
            if (symbiont != null)
            {
                // Apply power multiplier from settings
                float powerMultiplier = FleshSymbiontSettings.psycastPowerMultiplier;
                float resistanceChance = Mathf.Max(0.05f, 0.3f / powerMultiplier);
                
                // Check for resistance based on target's traits and psychic sensitivity
                if (CanResistCompulsion(targetPawn, resistanceChance))
                {
                    // Play resistance static
                    if (FleshSymbiontSettings.enableAtmosphericEffects)
                    {
                        var staticSound = SoundDef.Named("CompulsionStatic");
                        staticSound?.PlayOneShot(new TargetInfo(targetPawn));
                    }
                    
                    ShowResistanceMessage(targetPawn, caster);
                    return;
                }
                
                // Play compulsion whispers
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    var whisperSound = SoundDef.Named("SymbiontWhisper");
                    whisperSound?.PlayOneShot(new TargetInfo(targetPawn));
                }
                
                // Successfully compel the target
                targetPawn.mindState.mentalStateHandler.TryStartMentalState(
                    FleshSymbiontDefOf.SymbiontCompulsion,
                    "Psychically compelled by symbiont",
                    true,
                    causedByMood: false);
                    
                // Atmospheric effects
                CreateCompulsionEffects(caster, targetPawn, symbiont);
                
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    ShowCompulsionMessage(targetPawn, caster);
                }
            }
            else
            {
                Messages.Message("No symbiont found to channel compulsion through.", 
                    caster, MessageTypeDefOf.RejectInput);
            }
        }
        
        private bool CanResistCompulsion(Pawn target, float baseResistanceChance)
        {
            float resistance = baseResistanceChance;
            
            // Trait-based resistance
            if (target.story?.traits != null)
            {
                if (target.story.traits.HasTrait(TraitDefOf.PsychicSensitivity))
                {
                    var sensitivityDegree = target.story.traits.GetTrait(TraitDefOf.PsychicSensitivity).Degree;
                    resistance += sensitivityDegree * -0.1f; // More sensitive = less resistance
                }
                
                if (target.story.traits.HasTrait(TraitDefOf.Nerves))
                {
                    var nervesDegree = target.story.traits.GetTrait(TraitDefOf.Nerves).Degree;
                    resistance += nervesDegree * 0.15f; // Iron will = more resistance
                }
                
                if (target.story.traits.HasTrait(TraitDefOf.Psychopath))
                    resistance += 0.2f; // Psychopaths resist alien influence
            }
            
            // Psychic sensitivity stat
            if (ModsConfig.RoyaltyActive)
            {
                float psychicSensitivity = target.GetStatValue(StatDefOf.PsychicSensitivity);
                resistance -= (psychicSensitivity - 1f) * 0.2f;
            }
            
            // Faction loyalty
            if (target.Faction != Faction.OfPlayer)
                resistance += 0.3f; // Outsiders resist more
            
            // Royal titles provide mental fortification
            if (ModsConfig.RoyaltyActive && target.royalty?.AllTitlesForReading?.Any() == true)
                resistance += 0.15f;
            
            return Rand.Chance(Mathf.Clamp(resistance, 0.02f, 0.8f));
        }
        
        private void ShowResistanceMessage(Pawn target, Pawn caster)
        {
            if (!FleshSymbiontSettings.enableHorrorMessages) return;
            
            string message = "";
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                message = $"{target.Name.ToStringShort}'s mind blazes with defiant will as {caster.Name.ToStringShort}'s alien " +
                         "whispers crash against their mental barriers. Their eyes flare with righteous fury as they throw off " +
                         "the symbiont's influence, standing tall against the encroaching darkness of alien corruption.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                message = $"{target.Name.ToStringShort} resists {caster.Name.ToStringShort}'s psychic compulsion through " +
                         "sheer force of will, shaking off the alien whispers.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                message = $"{target.Name.ToStringShort} resists the psychic compulsion.";
            }
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, target, MessageTypeDefOf.PositiveEvent);
            }
        }
        
        private void ShowCompulsionMessage(Pawn target, Pawn caster)
        {
            string message = "";
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                message = $"{caster.Name.ToStringShort}'s eyes glow with alien light as they channel the symbiont's will " +
                         $"through the psychic link. {target.Name.ToStringShort} staggers as otherworldly whispers flood " +
                         "their mind, their pupils dilating unnaturally as the compulsion takes hold. They turn toward " +
                         "the flesh symbiont with the vacant hunger of a predator sensing prey.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                message = $"{caster.Name.ToStringShort} channels alien power to compel {target.Name.ToStringShort}. " +
                         "Otherworldly whispers fill their mind as they turn toward the symbiont with vacant eyes.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                message = $"{target.Name.ToStringShort} is compelled by {caster.Name.ToStringShort}'s psychic influence.";
            }
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, target, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        private void CreateCompulsionEffects(Pawn caster, Pawn target, Building_FleshSymbiont symbiont)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            // Energy beam from caster to target
            var beamStart = caster.DrawPos;
            var beamEnd = target.DrawPos;
            var beamMid = Vector3.Lerp(beamStart, beamEnd, 0.5f);
            
            for (int i = 0; i < 5; i++)
            {
                MoteMaker.ThrowMicroSparks(beamMid + Gen.RandomHorizontalVector(0.5f), caster.Map);
            }
            
            // Caster's eyes glow
            MoteMaker.ThrowLightningGlow(caster.DrawPos, caster.Map, 0.6f);
            
            // Target disorientation effect
            MoteMaker.ThrowDustPuff(target.DrawPos, target.Map, 1.2f);
            
            // Symbiont pulses in response
            MoteMaker.ThrowHeatGlow(symbiont.Position, symbiont.Map, 1.0f);
        }
        
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!FleshSymbiontSettings.enableRoyaltyFeatures || !FleshSymbiontSettings.enableSymbiontPsycasts)
            {
                if (throwMessages)
                    Messages.Message("Symbiont psycasts are disabled in mod settings.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            if (!parent.pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
            {
                if (throwMessages)
                    Messages.Message("Must be bonded to a symbiont to use this ability.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            if (target.Pawn?.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) == true)
            {
                if (throwMessages)
                    Messages.Message("Target is already bonded to a symbiont.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            return base.Valid(target, throwMessages);
        }
    }
    
    // Simplified symbiont heal ability
    public class CompAbilityEffect_SymbiontHeal : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            
            var caster = parent.pawn;
            var targetPawn = target.Pawn;
            
            if (targetPawn == null)
                return;
                
            // Check if caster is bonded
            if (!caster.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
            {
                Messages.Message($"{caster.Name.ToStringShort} must be bonded to a symbiont to use this ability.", 
                    caster, MessageTypeDefOf.RejectInput);
                return;
            }
            
            // Play psycast activation sound
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                var psycastSound = SoundDef.Named("PsycastActivate");
                psycastSound?.PlayOneShot(new TargetInfo(caster));
                
                // Play healing-specific sound
                var healSound = SoundDef.Named("SymbiontHeal");
                healSound?.PlayOneShot(new TargetInfo(targetPawn));
            }
            
            // Apply healing based on power multiplier
            float powerMultiplier = FleshSymbiontSettings.psycastPowerMultiplier;
            ApplySymbiontHealing(targetPawn, caster, powerMultiplier);
            
            // Create atmospheric effects
            CreateHealingEffects(caster, targetPawn, powerMultiplier);
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                ShowHealingMessage(targetPawn, caster, powerMultiplier);
            }
        }
        
        private void ApplySymbiontHealing(Pawn target, Pawn caster, float powerMultiplier)
        {
            // Create regeneration hediff
            var regenHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontRegeneration, target);
            regenHediff.Severity = 0.8f * powerMultiplier;
            target.health.AddHediff(regenHediff);
            
            // Immediate healing boost
            var injuries = target.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(h => h.CanHealNaturally())
                .OrderByDescending(h => h.Severity)
                .Take(Mathf.RoundToInt(3 * powerMultiplier));
            
            foreach (var injury in injuries)
            {
                float immediateHeal = Rand.Range(5f, 15f) * powerMultiplier;
                injury.Heal(immediateHeal);
            }
            
            // Remove some negative hediffs
            if (Rand.Chance(0.3f * powerMultiplier))
            {
                var removableHediffs = target.health.hediffSet.hediffs
                    .Where(h => h.def.isBad && h.def.everCurableByItem && h.CurStage?.lifeThreatening != true)
                    .Take(1);
                
                foreach (var hediff in removableHediffs.ToList())
                {
                    if (Rand.Chance(0.5f))
                    {
                        target.health.RemoveHediff(hediff);
                        
                        if (FleshSymbiontSettings.enableHorrorMessages)
                        {
                            Messages.Message($"The symbiont's power purges {hediff.def.label} from {target.Name.ToStringShort}.", 
                                target, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }
            
            // Restore some blood if lost
            if (target.health.hediffSet.BloodLoss > 0.1f)
            {
                var bloodLoss = target.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                if (bloodLoss != null)
                {
                    bloodLoss.Severity = Mathf.Max(0f, bloodLoss.Severity - 0.2f * powerMultiplier);
                }
            }
        }
        
        private void CreateHealingEffects(Pawn caster, Pawn target, float powerMultiplier)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            // Caster channeling effects
            MoteMaker.ThrowLightningGlow(caster.DrawPos, caster.Map, 0.8f * powerMultiplier);
            
            // Target healing effects
            for (int i = 0; i < Mathf.RoundToInt(8 * powerMultiplier); i++)
            {
                var effectPos = target.DrawPos + Gen.RandomHorizontalVector(1f);
                MoteMaker.ThrowHeatGlow(target.Position, target.Map, 0.6f);
                MoteMaker.ThrowMicroSparks(effectPos, target.Map);
            }
            
            // Energy connection between caster and target
            if (caster != target)
            {
                var connectionPoint = Vector3.Lerp(caster.DrawPos, target.DrawPos, 0.5f);
                for (int i = 0; i < 5; i++)
                {
                    MoteMaker.ThrowDustPuff(connectionPoint + Gen.RandomHorizontalVector(0.5f), caster.Map, 1.2f);
                }
            }
            
            // Pulsing effect on target
            for (int i = 0; i < 3; i++)
            {
                MoteMaker.ThrowLightningGlow(target.DrawPos, target.Map, 0.4f + (i * 0.2f));
            }
        }
        
        private void ShowHealingMessage(Pawn target, Pawn caster, float powerMultiplier)
        {
            string targetName = target.Name.ToStringShort;
            string casterName = caster.Name.ToStringShort;
            bool isSelfHeal = caster == target;
            
            string message = "";
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                if (isSelfHeal)
                {
                    message = $"{casterName}'s eyes roll back as they channel the symbiont's regenerative power through their own body. " +
                             "Alien tendrils writhe visibly beneath their skin, knitting flesh and bone with wet, organic sounds. " +
                             "Their wounds close in ways that defy human biology, leaving behind tissue that pulses with unnatural life.";
                }
                else
                {
                    message = $"{casterName} extends their hands toward {targetName}, crimson energy flowing between them as the symbiont's " +
                             "healing power transfers from host to host. {targetName}'s wounds close with sickening efficiency as alien " +
                             "biology overwrites human limitations.";
                }
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                if (isSelfHeal)
                {
                    message = $"{casterName} channels the symbiont's regenerative power, their wounds closing with unnatural speed.";
                }
                else
                {
                    message = $"{casterName} channels symbiont energy to heal {targetName}'s injuries with alien efficiency.";
                }
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                if (isSelfHeal)
                {
                    message = $"{casterName} regenerates using symbiont power.";
                }
                else
                {
                    message = $"{casterName} heals {targetName} using symbiont abilities.";
                }
            }
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, target, MessageTypeDefOf.PositiveEvent);
            }
        }
        
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!FleshSymbiontSettings.enableRoyaltyFeatures || !FleshSymbiontSettings.enableSymbiontPsycasts)
            {
                if (throwMessages)
                    Messages.Message("Symbiont psycasts are disabled in mod settings.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            if (!parent.pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
            {
                if (throwMessages)
                    Messages.Message("Must be bonded to a symbiont to use this ability.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            // Must target a living pawn
            if (target.Pawn?.Dead != false)
            {
                if (throwMessages)
                    Messages.Message("Can only heal living creatures.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            // Check if target actually needs healing
            var targetPawn = target.Pawn;
            bool hasInjuries = targetPawn.health.hediffSet.hediffs.Any(h => h is Hediff_Injury && h.CanHealNaturally());
            bool hasBloodLoss = targetPawn.health.hediffSet.BloodLoss > 0.1f;
            bool hasCurableConditions = targetPawn.health.hediffSet.hediffs.Any(h => h.def.isBad && h.def.everCurableByItem);
            
            if (!hasInjuries && !hasBloodLoss && !hasCurableConditions)
            {
                if (throwMessages)
                    Messages.Message($"{targetPawn.Name.ToStringShort} does not need healing.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            return base.Valid(target, throwMessages);
        }
    }
}