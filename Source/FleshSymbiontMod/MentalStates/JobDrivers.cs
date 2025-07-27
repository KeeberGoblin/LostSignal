using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FleshSymbiontMod
{
    // Job driver for being compelled to the symbiont
    public class JobDriver_CompelledToSymbiont : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true; // No reservations needed for compulsion
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            
            // Go to the symbiont
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
                .FailOnDespawnedOrNull(TargetIndex.A);
            
            // Touch the symbiont
            var touchToil = new Toil();
            touchToil.initAction = delegate
            {
                var symbiont = TargetA.Thing as Building_FleshSymbiont;
                if (symbiont != null)
                {
                    // Add the symbiont bond
                    var hediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontBond, pawn);
                    pawn.health.AddHediff(hediff);
                    
                    symbiont.OnPawnBonded(pawn);
                    
                    // End the mental state
                    pawn.mindState.mentalStateHandler.CurState?.RecoverFromState();
                    
                    // Atmospheric effect
                    if (FleshSymbiontSettings.enableAtmosphericEffects)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MoteMaker.ThrowDustPuff(pawn.DrawPos + Verse.Gen.RandomHorizontalVector(1f), pawn.Map, 1.5f);
                        }
                    }
                }
            };
            touchToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return touchToil;
        }
        
        public override string GetReport()
        {
            return FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 2 
                ? "Walking toward the flesh symbiont with vacant eyes..."
                : "Approaching symbiont";
        }
    }
    
    // Job driver for bonded pawns feeding the symbiont
    public class JobDriver_FeedSymbiont : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true; // No reservations needed
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            
            // Go to the symbiont
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
                .FailOnDespawnedOrNull(TargetIndex.A);
            
            // Feeding animation
            var feedToil = Toils_General.Wait(120); // 2 second feeding animation
            feedToil.WithProgressBarToilDelay(TargetIndex.A);
            feedToil.initAction = delegate
            {
                // Play feeding sound
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    var feedSound = SoundDef.Named("SymbiontFeed");
                    feedSound?.PlayOneShot(new TargetInfo(pawn));
                }
                
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    string message = FleshSymbiontSettings.messageFrequency switch
                    {
                        3 => $"{pawn.Name.ToStringShort} approaches the flesh symbiont with glazed eyes. " +
                             "Their skin takes on a sickly pallor as the symbiont's tendrils visibly pulse beneath the surface, " +
                             "drawing sustenance directly from their life force...",
                        2 => $"{pawn.Name.ToStringShort} approaches the flesh symbiont with glazed eyes...",
                        1 => $"{pawn.Name.ToStringShort} feeds the symbiont.",
                        _ => ""
                    };
                    
                    if (!string.IsNullOrEmpty(message))
                    {
                        Messages.Message(message, pawn, MessageTypeDefOf.NeutralEvent);
                    }
                }
            };
            
            feedToil.AddFinishAction(delegate
            {
                var symbiont = TargetA.Thing as Building_FleshSymbiont;
                symbiont?.OnSymbiontFed();
                
                // Small mood buff for bonded pawns when feeding
                var thought = ThoughtMaker.MakeThought(FleshSymbiontDefOf.FedSymbiont);
                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                
                // Atmospheric effects
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    MoteMaker.ThrowHeatGlow(pawn.Position, pawn.Map, 0.8f);
                    
                    // Small chance of temporary stat boost
                    if (Rand.Chance(0.3f))
                    {
                        var boostHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontRegeneration, pawn);
                        boostHediff.Severity = 0.5f;
                        pawn.health.AddHediff(boostHediff);
                    }
                }
            });
            
            yield return feedToil;
        }
        
        public override string GetReport()
        {
            return FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 2
                ? "Offering sustenance to the symbiont..."
                : "Feeding symbiont";
        }
    }
    
    // Job driver for voluntary symbiont touching
    public class JobDriver_TouchFleshSymbiont : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);
            
            // Go to the symbiont
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            
            // Contemplation phase
            var contemplateToil = Toils_General.Wait(180); // 3 seconds of hesitation
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
            
            // Touch the symbiont
            var touchToil = new Toil();
            touchToil.initAction = delegate
            {
                var symbiont = TargetA.Thing as Building_FleshSymbiont;
                if (symbiont != null)
                {
                    // Check if voluntary bonding is allowed
                    if (!FleshSymbiontSettings.allowVoluntaryBonding)
                    {
                        Messages.Message($"{pawn.Name.ToStringShort} reaches toward the symbiont but pulls back at the last moment, " +
                                        "some primal instinct warning them away.", 
                            pawn, MessageTypeDefOf.NeutralEvent);
                        return;
                    }
                    
                    // Small chance to resist even voluntary bonding
                    if (Rand.Chance(0.15f))
                    {
                        Messages.Message($"{pawn.Name.ToStringShort} touches the symbiont but nothing happens. " +
                                        "Perhaps their will is too strong, or perhaps the symbiont simply isn't hungry...", 
                            pawn, MessageTypeDefOf.NeutralEvent);
                        return;
                    }
                    
                    // Add the symbiont bond
                    var hediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontBond, pawn);
                    pawn.health.AddHediff(hediff);
                    
                    symbiont.OnPawnBonded(pawn);
                    
                    // Atmospheric effects for voluntary bonding
                    if (FleshSymbiontSettings.enableAtmosphericEffects)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            MoteMaker.ThrowDustPuff(pawn.DrawPos + Verse.Gen.RandomHorizontalVector(1.5f), pawn.Map, 2f);
                        }
                        MoteMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 2f);
                    }
                }
            };
            touchToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return touchToil;
        }
        
        public override string GetReport()
        {
            return FleshSymbiontSettings.enableHorrorMessages && FleshSymbiontSettings.messageFrequency >= 1
                ? "Contemplating the symbiont..."
                : "Examining symbiont";
        }
    }
}