using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Symbiont rage/fury psycast ability
    public class CompAbilityEffect_SymbiontRage : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            
            var caster = parent.pawn;
            
            // Check if caster is bonded
            if (!caster.IsSymbiontBonded())
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
                
                // Play rage-specific sound
                var rageSound = SoundDef.Named("SymbiontRage");
                rageSound?.PlayOneShot(new TargetInfo(caster));
            }
            
            // Apply rage based on power multiplier
            float powerMultiplier = FleshSymbiontSettings.psycastPowerMultiplier;
            ApplySymbiontRage(caster, powerMultiplier);
            
            // Create dramatic effects
            CreateRageEffects(caster, powerMultiplier);
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                ShowRageMessage(caster, powerMultiplier);
            }
        }
        
        private void ApplySymbiontRage(Pawn caster, float powerMultiplier)
        {
            // Create fury hediff with enhanced power
            var furyHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontFury, caster);
            furyHediff.Severity = 1.0f * powerMultiplier;
            caster.health.AddHediff(furyHediff);
            
            // Immediate combat bonuses
            var combatBoostHediff = HediffMaker.MakeHediff(HediffDefOf.BattleFrenzy, caster);
            if (combatBoostHediff != null)
            {
                combatBoostHediff.Severity = 0.8f * powerMultiplier;
                caster.health.AddHediff(combatBoostHediff);
            }
            
            // Risk of madness based on power level and pawn's mental state
            float madnessChance = CalculateMadnessRisk(caster, powerMultiplier);
            
            if (Rand.Chance(madnessChance))
            {
                // Trigger symbiont madness
                caster.mindState.mentalStateHandler.TryStartMentalState(
                    FleshSymbiontDefOf.SymbiontMadness,
                    "Overwhelmed by symbiont fury",
                    true,
                    causedByMood: false);
                    
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    Messages.Message($"{caster.Name.ToStringShort} has been overwhelmed by the symbiont's primal rage!", 
                        caster, MessageTypeDefOf.NegativeEvent);
                }
            }
            
            // Temporary pain suppression
            var painHediff = caster.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pain);
            if (painHediff != null && Rand.Chance(0.7f * powerMultiplier))
            {
                painHediff.Severity = Mathf.Max(0f, painHediff.Severity - 0.3f * powerMultiplier);
            }
            
            // Enhanced adrenaline rush
            if (Rand.Chance(0.8f))
            {
                var adrenalineHediff = HediffMaker.MakeHediff(HediffDefOf.AdrenalineRush, caster);
                if (adrenalineHediff != null)
                {
                    adrenalineHediff.Severity = 1.0f;
                    caster.health.AddHediff(adrenalineHediff);
                }
            }
        }
        
        private float CalculateMadnessRisk(Pawn caster, float powerMultiplier)
        {
            float baseMadnessChance = 0.15f * powerMultiplier;
            
            // Mental break threshold affects risk
            float mentalBreakThreshold = caster.GetStatValue(StatDefOf.MentalBreakThreshold);
            baseMadnessChance += (mentalBreakThreshold - 0.35f) * 0.5f;
            
            // Traits that affect mental stability
            if (caster.story?.traits != null)
            {
                if (caster.story.traits.HasTrait(TraitDefOf.Nerves))
                {
                    var degree = caster.story.traits.GetTrait(TraitDefOf.Nerves).Degree;
                    baseMadnessChance -= degree * 0.1f; // Iron will reduces risk
                }
                
                if (caster.story.traits.HasTrait(TraitDefOf.Psychopath))
                    baseMadnessChance -= 0.1f; // Psychopaths handle rage better
                    
                if (caster.story.traits.HasTrait(TraitDefOf.Volatile))
                    baseMadnessChance += 0.2f; // Volatile pawns more prone to losing control
            }
            
            // Current mood affects risk
            if (caster.needs?.mood != null)
            {
                float moodLevel = caster.needs.mood.CurLevelPercentage;
                baseMadnessChance += (0.5f - moodLevel) * 0.3f; // Lower mood = higher risk
            }
            
            // Existing injuries increase instability
            int injuryCount = caster.health.hediffSet.hediffs.Count(h => h is Hediff_Injury);
            baseMadnessChance += injuryCount * 0.02f;
            
            return Mathf.Clamp(baseMadnessChance, 0.02f, 0.8f);
        }
        
        private void CreateRageEffects(Pawn caster, float powerMultiplier)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            // Dramatic eye glow effect
            for (int i = 0; i < 3; i++)
            {
                MoteMaker.ThrowLightningGlow(caster.DrawPos, caster.Map, 1.5f * powerMultiplier);
            }
            
            // Energy explosion around caster
            for (int i = 0; i < Mathf.RoundToInt(12 * powerMultiplier); i++)
            {
                var effectPos = caster.DrawPos + Gen.RandomHorizontalVector(2f);
                MoteMaker.ThrowMicroSparks(effectPos, caster.Map);
                MoteMaker.ThrowDustPuff(effectPos, caster.Map, 1.5f);
            }
            
            // Heat wave effect
            MoteMaker.ThrowHeatGlow(caster.Position, caster.Map, 2.0f * powerMultiplier);
            
            // Pulsing energy waves
            for (int wave = 0; wave < 3; wave++)
            {
                for (int i = 0; i < 8; i++)
                {
                    var angle = (i / 8f) * 360f;
                    var direction = Vector3.forward.RotatedBy(angle);
                    var wavePos = caster.DrawPos + direction * (1f + wave * 0.5f);
                    MoteMaker.ThrowDustPuff(wavePos, caster.Map, 0.8f);
                }
            }
            
            // Ground cracking effect (visual only)
            var nearbyPositions = GenRadial.RadialCellsAround(caster.Position, 2f, true);
            foreach (var pos in nearbyPositions.Take(6))
            {
                if (Rand.Chance(0.3f))
                {
                    MoteMaker.ThrowDustPuff(pos.ToVector3(), caster.Map, 1.0f);
                }
            }
        }
        
        private void ShowRageMessage(Pawn caster, float powerMultiplier)
        {
            string casterName = caster.Name.ToStringShort;
            
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{casterName}'s eyes blaze with alien fire as the symbiont's primal fury floods their nervous system. " +
                     "Their muscles bulge unnaturally as biomechanical fibers enhance human sinew beyond its limits. " +
                     "A bestial roar tears from their throat - a sound no human throat should make - as they surrender " +
                     "to the symbiont's predatory hunger. The air around them shimmers with barely contained violence.",
                     
                2 => $"{casterName}'s eyes glow with alien light as the symbiont's rage courses through their veins. " +
                     "Their movements become fluid and predatory, no longer entirely human.",
                     
                1 => $"{casterName} enters a symbiont-enhanced fury state.",
                
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, caster, MessageTypeDefOf.NeutralEvent);
            }
            
            // Additional warning if madness risk is high
            float madnessRisk = CalculateMadnessRisk(caster, powerMultiplier);
            if (madnessRisk > 0.4f && FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 2)
            {
                Messages.Message($"Warning: {casterName} is at high risk of losing control to the symbiont's influence!", 
                    caster, MessageTypeDefOf.CautionInput);
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
            
            if (!parent.pawn.IsSymbiontBonded())
            {
                if (throwMessages)
                    Messages.Message("Must be bonded to a symbiont to use this ability.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            // Can't use if already in fury state
            if (parent.pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontFury))
            {
                if (throwMessages)
                    Messages.Message("Already in symbiont fury state.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            // Can't use if in mental break
            if (parent.pawn.MentalStateDef != null)
            {
                if (throwMessages)
                    Messages.Message("Cannot use while in mental state.", 
                        parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            
            return base.Valid(target, throwMessages);
        }
        
        public override float PowerMultiplier => FleshSymbiontSettings.psycastPowerMultiplier;
        
        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            float madnessRisk = CalculateMadnessRisk(parent.pawn, FleshSymbiontSettings.psycastPowerMultiplier);
            if (madnessRisk > 0.3f)
            {
                return $"Madness risk: {madnessRisk:P0}";
            }
            
            return base.ExtraLabelMouseAttachment(target);
        }
    }
}