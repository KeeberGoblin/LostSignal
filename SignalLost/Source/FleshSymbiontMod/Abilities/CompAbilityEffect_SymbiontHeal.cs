using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Symbiont healing psycast ability
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
            caster.CreatePsychicEffect(powerMultiplier);
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
            
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => isSelfHeal ?
                    $"{casterName}'s eyes roll back as they channel the symbiont's regenerative power through their own body. " +
                    "Alien tendrils writhe visibly beneath their skin, knitting flesh and bone with wet, organic sounds. " +
                    "Their wounds close in ways that defy human biology, leaving behind tissue that pulses with unnatural life." :
                    $"{casterName} extends their hands toward {targetName}, crimson energy flowing between them as the symbiont's " +
                    "healing power transfers from host to host. {targetName}'s wounds close with sickening efficiency as alien " +
                    "biology overwrites human limitations.",
                    
                2 => isSelfHeal ?
                    $"{casterName} channels the symbiont's regenerative power, their wounds closing with unnatural speed." :
                    $"{casterName} channels symbiont energy to heal {targetName}'s injuries with alien efficiency.",
                    
                1 => isSelfHeal ?
                    $"{casterName} regenerates using symbiont power." :
                    $"{casterName} heals {targetName} using symbiont abilities.",
                
                _ => ""
            };
            
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
            
            if (!parent.pawn.IsSymbiontBonded())
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
        
        public override float PowerMultiplier => FleshSymbiontSettings.psycastPowerMultiplier;
        
        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (target.Pawn != null)
            {
                var injuries = target.Pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().Count();
                if (injuries > 0)
                {
                    return $"Will heal {Mathf.Min(injuries, 3)} injuries";
                }
            }
            
            return base.ExtraLabelMouseAttachment(target);
        }
    }
}