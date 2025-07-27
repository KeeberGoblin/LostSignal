using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FleshSymbiontMod
{
    // Job driver for being compelled to the symbiont
    public class JobDriver_CompelledToSymbiont : JobDriver
    {
        private Building_FleshSymbiont Symbiont => (Building_FleshSymbiont)TargetA.Thing;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true; // No reservations needed for compulsion
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => pawn.IsSymbiontBonded()); // Stop if already bonded
            this.FailOn(() => !pawn.mindState.mentalStateHandler.InMentalState); // Stop if mental state ends
            
            // Go to the symbiont with determination
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
                .FailOnDespawnedOrNull(TargetIndex.A)
                .FailOn(() => pawn.Position.DistanceTo(TargetA.Thing.Position) > FleshSymbiontSettings.maxCompulsionRange);
            
            // Touch and bond with the symbiont
            var bondingToil = new Toil();
            bondingToil.initAction = delegate
            {
                if (Symbiont?.Spawned != true) return;
                if (pawn.IsSymbiontBonded()) return;
                
                // Create dramatic bonding effects
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    SymbiontUtility.CreateBondingEffects(pawn, Symbiont);
                    FleshSymbiontDefOf.BondingScream?.PlayOneShot(new TargetInfo(pawn));
                }
                
                // Complete the bonding
                SymbiontUtility.TryBondPawnToSymbiont(pawn, Symbiont, voluntary: false);
                
                // End the mental state
                pawn.mindState.mentalStateHandler.CurState?.RecoverFromState();
            };
            bondingToil.defaultCompleteMode = ToilCompleteMode.Instant;
            bondingToil.socialMode = RandomSocialMode.Off;
            yield return bondingToil;
        }
        
        public override string GetReport()
        {
            if (!FleshSymbiontSettings.enableHorrorMessages) 
                return "Approaching symbiont";
            
            return FleshSymbiontSettings.messageFrequency >= 2 
                ? "Walking toward the flesh symbiont with vacant eyes..."
                : "Approaching symbiont";
        }
        
        public override bool IsContinuation(Job j)
        {
            return j.def == FleshSymbiontDefOf.CompelledToSymbiont;
        }
    }
    
    // Job driver for bonded pawns feeding the symbiont
    public class JobDriver_FeedSymbiont : JobDriver
    {
        private Building_FleshSymbiont Symbiont => (Building_FleshSymbiont)TargetA.Thing;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true; // No reservations needed
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !pawn.IsSymbiontBonded()); // Only bonded pawns can feed
            
            // Go to the symbiont
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
                .FailOnDespawnedOrNull(TargetIndex.A);
            
            // Feeding ritual
            var feedToil = Toils_General.Wait(180); // 3 second feeding animation
            feedToil.WithProgressBarToilDelay(TargetIndex.A);
            feedToil.initAction = delegate
            {
                // Play feeding sounds
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    FleshSymbiontDefOf.SymbiontFeed?.PlayOneShot(new TargetInfo(pawn));
                }
                
                // Show feeding message
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    ShowFeedingMessage();
                }
            };
            
            feedToil.AddFinishAction(delegate
            {
                if (Symbiont?.Spawned != true) return;
                
                // Notify symbiont that it has been fed
                Symbiont.OnSymbiontFed();
                
                // Give mood buff to bonded pawn
                var thought = ThoughtMaker.MakeThought(FleshSymbiontDefOf.FedSymbiont);
                pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
                
                // Atmospheric effects and benefits
                CreateFeedingEffects();
                ApplyFeedingBenefits();
            });
            
            yield return feedToil;
        }
        
        private void ShowFeedingMessage()
        {
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{pawn.Name.ToStringShort} approaches the flesh symbiont with glazed eyes. " +
                     "Their skin takes on a sickly pallor as the symbiont's tendrils visibly pulse beneath the surface, " +
                     "drawing sustenance directly from their life force. The air fills with wet, organic sounds...",
                2 => $"{pawn.Name.ToStringShort} approaches the flesh symbiont with glazed eyes, " +
                     "offering themselves to nourish their alien master...",
                1 => $"{pawn.Name.ToStringShort} feeds the symbiont.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
        
        private void CreateFeedingEffects()
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            // Glowing connection between pawn and symbiont
            MoteMaker.ThrowHeatGlow(pawn.Position, pawn.Map, 0.8f);
            MoteMaker.ThrowHeatGlow(Symbiont.Position, Symbiont.Map, 1.2f);
            
            // Energy transfer effects
            for (int i = 0; i < 5; i++)
            {
                var effectPos = pawn.DrawPos + Gen.RandomHorizontalVector(1f);
                MoteMaker.ThrowMicroSparks(effectPos, pawn.Map);
            }
        }
        
        private void ApplyFeedingBenefits()
        {
            // Small chance of temporary stat boost
            if (Rand.Chance(0.4f * FleshSymbiontSettings.bondingBenefitsMultiplier))
            {
                var boostHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontRegeneration, pawn);
                boostHediff.Severity = 0.3f * FleshSymbiontSettings.bondingBenefitsMultiplier;
                pawn.health.AddHediff(boostHediff);
            }
            
            // Heal small injuries
            var minorInjuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(h => h.Severity < 5f && h.CanHealNaturally())
                .Take(2);
                
            foreach (var injury in minorInjuries)
            {
                injury.Heal(2f * FleshSymbiontSettings.bondingBenefitsMultiplier);
            }
        }
        
        public override string GetReport()
        {
            if (!FleshSymbiontSettings.enableHorrorMessages) 
                return "Feeding symbiont";
            
            return FleshSymbiontSettings.messageFrequency >= 2
                ? "Offering sustenance to the symbiont..."
                : "Feeding symbiont";
        }
    }
    
    // Job driver for voluntary symbiont touching
    public class JobDriver_TouchFleshSymbiont : JobDriver
    {
        private Building_FleshSymbiont Symbiont => (Building_FleshSymbiont)TargetA.Thing;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);
            this.FailOn(() => pawn.IsSymbiontBonded()); // Can't touch if already bonded
            
            // Go to the symbiont
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            
            // Contemplation phase - the pawn hesitates
            var contemplateToil = Toils_General.Wait(240); // 4 seconds of hesitation
            contemplateToil.WithProgressBarToilDelay(TargetIndex.A);
            contemplateToil.initAction = delegate
            {
                if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 2)
                {
                    Messages.Message($"{pawn.Name.ToStringShort} hesitates before the pulsing flesh symbiont, " +
                                    "their hand trembling as alien whispers fill their mind...", 
                        pawn, MessageTypeDefOf.NeutralEvent);
                }
            };
            yield return contemplateToil;
            
            // Decision and potential bonding
            var touchToil = new Toil();
            touchToil.initAction = delegate
            {
                if (Symbiont?.Spawned != true) return;
                
                // Check if voluntary bonding is allowed
                if (!FleshSymbiontSettings.allowVoluntaryBonding)
                {
                    ShowVoluntaryBondingDenied();
                    return;
                }
                
                // Calculate resistance chance
                float resistanceChance = CalculateVoluntaryResistance();
                
                if (Rand.Chance(resistanceChance))
                {
                    ShowVoluntaryResistance();
                    return;
                }
                
                // Complete voluntary bonding
                CompleteVoluntaryBonding();
            };
            touchToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return touchToil;
        }
        
        private void ShowVoluntaryBondingDenied()
        {
            Messages.Message($"{pawn.Name.ToStringShort} reaches toward the symbiont but pulls back at the last moment, " +
                            "some primal instinct warning them away.", 
                pawn, MessageTypeDefOf.NeutralEvent);
        }
        
        private float CalculateVoluntaryResistance()
        {
            float baseResistance = 0.2f; // 20% base chance to resist even voluntary bonding
            
            // Traits affect willingness
            if (pawn.story?.traits != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Nerves))
                {
                    var degree = pawn.story.traits.GetTrait(TraitDefOf.Nerves).Degree;
                    if (degree > 0) baseResistance += 0.3f; // Iron will = more resistance
                }
                
                if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
                    baseResistance += 0.2f;
                    
                if (pawn.story.traits.HasTrait(TraitDefOf.Masochist))
                    baseResistance -= 0.3f; // More likely to accept
            }
            
            // Mood affects decision
            if (pawn.needs?.mood != null)
            {
                float moodLevel = pawn.needs.mood.CurLevelPercentage;
                if (moodLevel < 0.3f) baseResistance -= 0.2f; // Desperate pawns more likely to bond
            }
            
            return Mathf.Clamp(baseResistance, 0.05f, 0.8f);
        }
        
        private void ShowVoluntaryResistance()
        {
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{pawn.Name.ToStringShort} touches the symbiont's alien flesh and recoils in horror. " +
                     "The creature's surface feels wrong—too warm, too alive, pulsing with malevolent intelligence. " +
                     "They stagger back, their sanity intact but forever changed by the contact.",
                2 => $"{pawn.Name.ToStringShort} touches the symbiont but nothing happens. " +
                     "Perhaps their will is too strong, or perhaps the symbiont simply isn't hungry...",
                1 => $"{pawn.Name.ToStringShort} resists the symbiont's influence.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
        
        private void CompleteVoluntaryBonding()
        {
            // Create bonding effects
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                SymbiontUtility.CreateBondingEffects(pawn, Symbiont);
                
                // More dramatic effects for voluntary bonding
                for (int i = 0; i < 10; i++)
                {
                    MoteMaker.ThrowDustPuff(pawn.DrawPos + Gen.RandomHorizontalVector(2f), pawn.Map, 2f);
                }
                MoteMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 2f);
            }
            
            // Complete the bonding
            SymbiontUtility.TryBondPawnToSymbiont(pawn, Symbiont, voluntary: true);
            
            // Show voluntary bonding message
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                ShowVoluntaryBondingMessage();
            }
        }
        
        private void ShowVoluntaryBondingMessage()
        {
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{pawn.Name.ToStringShort} extends their hand to the pulsing mass with reverent determination. " +
                     "As alien flesh pierces their skin, they smile with transcendent joy even as their humanity begins to fade. " +
                     "The symbiont purrs with satisfaction at this willing offering, and {pawn.Name.ToStringShort} " +
                     "gasps as otherworldly power floods their veins.",
                2 => $"{pawn.Name.ToStringShort} willingly touches the symbiont. Their eyes glow with alien light " +
                     "as the bonding process begins, their choice made freely but permanently.",
                1 => $"{pawn.Name.ToStringShort} has voluntarily bonded with the symbiont.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
        
        public override string GetReport()
        {
            if (!FleshSymbiontSettings.enableHorrorMessages) 
                return "Examining symbiont";
            
            return FleshSymbiontSettings.messageFrequency >= 1
                ? "Contemplating the symbiont..."
                : "Examining symbiont";
        }
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Don't allow if voluntary bonding is disabled
            if (!FleshSymbiontSettings.allowVoluntaryBonding)
                return false;
                
            return base.TryMakePreToilReservations(errorOnFailed);
        }
    }
    
    // Job driver for meditation on symbiont (Royalty)
    public class JobDriver_MeditateOnSymbiont : JobDriver
    {
        private Building_FleshSymbiont Symbiont => (Building_FleshSymbiont)TargetA.Thing;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!ModsConfig.RoyaltyActive || !FleshSymbiontSettings.enableRoyaltyFeatures)
                return false;
                
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !pawn.IsSymbiontBonded()); // Only bonded pawns can meditate
            
            // Go near the symbiont
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            
            // Meditation toil
            var meditateToil = new Toil();
            meditateToil.initAction = delegate
            {
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    CreateMeditationEffects();
                }
            };
            meditateToil.tickAction = delegate
            {
                // Gain psyfocus while meditating
                if (pawn.psychicEntropy != null && CurToilDuration % 60 == 0)
                {
                    float psyfocusGain = 0.02f * FleshSymbiontSettings.bondingBenefitsMultiplier;
                    pawn.psychicEntropy.OffsetPsyfocusDirectly(psyfocusGain);
                }
                
                // Periodic meditation effects
                if (FleshSymbiontSettings.enableAtmosphericEffects && CurToilDuration % 300 == 0)
                {
                    MoteMaker.ThrowMicroSparks(pawn.DrawPos, pawn.Map);
                }
            };
            meditateToil.defaultCompleteMode = ToilCompleteMode.Delay;
            meditateToil.defaultDuration = 2500; // ~40 seconds
            meditateToil.WithProgressBar(TargetIndex.A);
            
            meditateToil.AddFinishAction(delegate
            {
                // Complete meditation benefits
                CompleteMeditation();
            });
            
            yield return meditateToil;
        }
        
        private void CreateMeditationEffects()
        {
            if (Symbiont?.Map == null) return;
            
            // Gentle glow around both pawn and symbiont
            MoteMaker.ThrowHeatGlow(pawn.Position, pawn.Map, 0.6f);
            MoteMaker.ThrowHeatGlow(Symbiont.Position, Symbiont.Map, 0.8f);
            
            // Connection visualization
            var connectionPoint = UnityEngine.Vector3.Lerp(pawn.DrawPos, Symbiont.DrawPos, 0.5f);
            MoteMaker.ThrowMicroSparks(connectionPoint, pawn.Map);
        }
        
        private void CompleteMeditation()
        {
            // Give meditation thought
            var meditationThought = ThoughtMaker.MakeThought(FleshSymbiontDefOf.MeditatedOnSymbiont);
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(meditationThought);
            
            // Additional psyfocus gain
            if (pawn.psychicEntropy != null)
            {
                float bonusGain = 0.15f * FleshSymbiontSettings.bondingBenefitsMultiplier;
                pawn.psychicEntropy.OffsetPsyfocusDirectly(bonusGain);
            }
            
            // Show completion message
            if (FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 2)
            {
                Messages.Message($"{pawn.Name.ToStringShort} completes their meditation, their consciousness " +
                                "briefly touching the alien intelligence within the symbiont.", 
                    pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        
        public override string GetReport()
        {
            return "Meditating on symbiont";
        }
    }
    
    // Job driver for ritual participation
    public class JobDriver_ParticipateInSymbiontRitual : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!ModsConfig.IdeologyActive || !FleshSymbiontSettings.enableIdeologyFeatures)
                return false;
                
            return true; // Ritual jobs don't need reservations
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            
            // Go to ritual spot
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            
            // Participate in ritual
            var ritualToil = new Toil();
            ritualToil.initAction = delegate
            {
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    CreateRitualEffects();
                }
            };
            ritualToil.tickAction = delegate
            {
                // Periodic ritual effects
                if (FleshSymbiontSettings.enableAtmosphericEffects && CurToilDuration % 200 == 0)
                {
                    if (pawn.IsSymbiontBonded())
                    {
                        // Bonded participants glow
                        MoteMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 0.4f);
                    }
                }
            };
            ritualToil.defaultCompleteMode = ToilCompleteMode.Never; // Ritual controls completion
            ritualToil.socialMode = RandomSocialMode.Off;
            
            yield return ritualToil;
        }
        
        private void CreateRitualEffects()
        {
            if (pawn.IsSymbiontBonded())
            {
                // Bonded participants have enhanced effects
                pawn.CreatePsychicEffect(0.8f);
            }
            else
            {
                // Non-bonded participants have subtle effects
                if (Rand.Chance(0.3f))
                {
                    MoteMaker.ThrowMicroSparks(pawn.DrawPos, pawn.Map);
                }
            }
        }
        
        public override string GetReport()
        {
            return "Participating in symbiont communion";
        }
    }
}
