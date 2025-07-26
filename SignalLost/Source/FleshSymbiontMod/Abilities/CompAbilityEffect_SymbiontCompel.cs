using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Symbiont psycast ability components
    public class CompProperties_AbilitySymbiontCompel : CompProperties_AbilityEffect
    {
        public CompProperties_AbilitySymbiontCompel()
        {
            compClass = typeof(CompAbilityEffect_SymbiontCompel);
        }
    }
    
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
            
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{target.Name.ToStringShort}'s mind blazes with defiant will as {caster.Name.ToStringShort}'s alien " +
                     "whispers crash against their mental barriers. Their eyes flare with righteous fury as they throw off " +
                     "the symbiont's influence, standing tall against the encroaching darkness of alien corruption.",
                     
                2 => $"{target.Name.ToStringShort} resists {caster.Name.ToStringShort}'s psychic compulsion through " +
                     "sheer force of will, shaking off the alien whispers.",
                     
                1 => $"{target.Name.ToStringShort} resists the psychic compulsion.",
                
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, target, MessageTypeDefOf.PositiveEvent);
            }
        }
        
        private void ShowCompulsionMessage(Pawn target, Pawn caster)
        {
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{caster.Name.ToStringShort}'s eyes glow with alien light as they channel the symbiont's will " +
                     $"through the psychic link. {target.Name.ToStringShort} staggers as otherworldly whispers flood " +
                     "their mind, their pupils dilating unnaturally as the compulsion takes hold. They turn toward " +
                     "the flesh symbiont with the vacant hunger of a predator sensing prey.",
                     
                2 => $"{caster.Name.ToStringShort} channels alien power to compel {target.Name.ToStringShort}. " +
                     "Otherworldly whispers fill their mind as they turn toward the symbiont with vacant eyes.",
                     
                1 => $"{target.Name.ToStringShort} is compelled by {caster.Name.ToStringShort}'s psychic influence.",
                
                _ => ""
            };
            
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
        
        public override float PowerMultiplier => FleshSymbiontSettings.psycastPowerMultiplier;
        
        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (target.Pawn != null)
            {
                var resistanceChance = Mathf.Max(0.05f, 0.3f / FleshSymbiontSettings.psycastPowerMultiplier);
                if (CanResistCompulsion(target.Pawn, resistanceChance))
                {
                    return "High resistance to compulsion";
                }
            }
            
            return base.ExtraLabelMouseAttachment(target);
        }
    }
    
    // Meditation focus worker for symbiont
    public class MeditationFocusDefWorker_Symbiont : MeditationFocusDefWorker
    {
        public override bool CanPawnUse(Pawn p, FocusStrengthOffset_Thing thing)
        {
            if (!FleshSymbiontSettings.enableRoyaltyFeatures || !FleshSymbiontSettings.allowSymbiontMeditation)
                return false;
                
            return p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) && 
                   thing.parent is Building_FleshSymbiont;
        }
        
        public override float FocusStrengthFromThing(Thing thing, Pawn user)
        {
            if (!(thing is Building_FleshSymbiont symbiont))
                return 0f;
                
            // Base focus strength
            float strength = 0.34f;
            
            // Bonus if user is specifically bonded to this symbiont
            // (This would require tracking which symbiont each pawn is bonded to)
            
            // Modify by settings
            strength *= FleshSymbiontSettings.psycastPowerMultiplier;
            
            return Mathf.Clamp(strength, 0.1f, 1.0f);
        }
        
        public override string ExplanationString(Thing thing)
        {
            if (thing is Building_FleshSymbiont)
            {
                return "The alien whispers guide your meditation, opening pathways to transcendent power.";
            }
            
            return base.ExplanationString(thing);
        }
    }
    
    // Room requirement checker for throne rooms
    public class RoomRequirement_NoSymbiontPresence : RoomRequirement
    {
        public override bool Met(Room room, Pawn p = null)
        {
            if (!FleshSymbiontSettings.enableRoyaltyFeatures || !FleshSymbiontSettings.royalsHateSymbionts)
                return true;
                
            return !room.ContainedAndAdjacentThings.Any(t => t is Building_FleshSymbiont);
        }
        
        public override string Label(Room room = null)
        {
            return "No alien symbiont presence";
        }
        
        public override string LabelCap(Room room = null)
        {
            return Label(room).CapitalizeFirst();
        }
    }
}