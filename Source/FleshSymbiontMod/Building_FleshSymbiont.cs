using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    public class Building_FleshSymbiont : Building
    {
        private int ticksSinceLastBond = 0;
        private int ticksSinceLastFeed = 0;
        private int hungerLevel = 0; // 0-3, increases chance of compulsion
        private List<Pawn> bondedPawns = new List<Pawn>();
        private int pulseTimer = 0;
        private CompGlower glowerComp;
        
        private const int COMPULSION_BASE_INTERVAL = 45000; // ~30 in-game hours
        private const int FEED_INTERVAL = 30000; // ~20 in-game hours
        private const int HUNGER_INCREASE_INTERVAL = 60000; // ~40 in-game hours
        private const int PULSE_INTERVAL = 120; // ~2 seconds real time
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            glowerComp = GetComp<CompGlower>();
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksSinceLastBond, "ticksSinceLastBond", 0);
            Scribe_Values.Look(ref ticksSinceLastFeed, "ticksSinceLastFeed", 0);
            Scribe_Values.Look(ref hungerLevel, "hungerLevel", 0);
            Scribe_Collections.Look(ref bondedPawns, "bondedPawns", LookMode.Reference);
            
            if (bondedPawns == null)
                bondedPawns = new List<Pawn>();
        }
        
        protected override void Tick() // Changed from public to protected
        {
            base.Tick();
            
            if (!Spawned) return;
            
            ticksSinceLastBond++;
            ticksSinceLastFeed++;
            pulseTimer++;
            
            // Update bonded pawns list (remove dead/missing pawns)
            bondedPawns.RemoveAll(p => p == null || p.Dead || !p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond));
            
            // Pulsing glow effect
            if (FleshSymbiontSettings.enableAtmosphericEffects && glowerComp != null)
            {
                UpdatePulsingGlow();
            }
            
            // Ambient pulsing sound
            if (FleshSymbiontSettings.enableAtmosphericEffects && IsHashIntervalTick(300)) // Every 5 seconds
            {
                PlaySymbiontPulseSound();
            }
            
            // Increase hunger over time
            if (ticksSinceLastBond > (HUNGER_INCREASE_INTERVAL / FleshSymbiontSettings.hungerGrowthMultiplier))
            {
                hungerLevel = Mathf.Min(3, hungerLevel + 1);
                ticksSinceLastBond = 0;
                
                if (hungerLevel >= 2 && FleshSymbiontSettings.enableHorrorMessages)
                {
                    string message = GetHungerMessage();
                    
                    if (!string.IsNullOrEmpty(message))
                    {
                        Messages.Message(message, this, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
            
            // Try to compel someone
            if (Rand.MTBEventOccurs(GetCompulsionMTB(), GenDate.TicksPerDay, 1f))
            {
                TryCompelNearbyPawn();
            }
            
            // Make bonded pawns feed the symbiont
            if (bondedPawns.Count > 0 && ticksSinceLastFeed > FEED_INTERVAL)
            {
                var feeder = bondedPawns.RandomElement();
                if (feeder != null && feeder.Spawned && feeder.Awake() && 
                    feeder.CurJob?.def != FleshSymbiontDefOf.FeedSymbiont)
                {
                    var feedJob = JobMaker.MakeJob(FleshSymbiontDefOf.FeedSymbiont, this);
                    feeder.jobs.TryTakeOrderedJob(feedJob, JobTag.Misc);
                    ticksSinceLastFeed = 0;
                }
            }
            
            // Symbiont decay if enabled
            if (FleshSymbiontSettings.enableSymbiontDecay && ticksSinceLastFeed > FEED_INTERVAL * 2)
            {
                float damagePerTick = FleshSymbiontSettings.symbiontDecayRate / GenDate.TicksPerDay;
                TakeDamage(new DamageInfo(DamageDefOf.Deterioration, damagePerTick));
            }
        }
        
        private string GetHungerMessage()
        {
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                return "The flesh symbiont writhes in ravenous hunger, its alien whispers growing into screams that claw at your colonists' minds...";
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                return "The flesh symbiont pulses hungrily, its whispers growing stronger...";
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                return "The flesh symbiont seems restless.";
            }
            
            return "";
        }
        
        private void UpdatePulsingGlow()
        {
            if (glowerComp == null) return;
            
            // Create pulsing effect based on hunger level and pulse timer
            float pulseIntensity = Mathf.Sin(pulseTimer * 0.05f) * 0.3f + 0.7f; // Oscillates between 0.4 and 1.0
            
            // Hunger affects pulse speed and intensity
            float hungerMultiplier = 1f + (hungerLevel * 0.5f);
            pulseIntensity *= hungerMultiplier;
            
            // Calculate glow color based on state
            Color baseColor = GetGlowColor();
            
            // Apply pulse intensity
            Color currentColor = baseColor * pulseIntensity;
            
            // Update the glow component
            glowerComp.Props.glowColor = currentColor;
            
            // Reset pulse timer to prevent overflow
            if (pulseTimer > 1000)
                pulseTimer = 0;
        }
        
        private Color GetGlowColor()
        {
            if (hungerLevel == 0)
                return new Color(0.6f, 0.2f, 0.2f); // Dormant: Dark red
            else if (hungerLevel == 1)
                return new Color(0.8f, 0.3f, 0.2f); // Restless: Brighter red
            else if (hungerLevel == 2)
                return new Color(1.0f, 0.4f, 0.1f); // Hungry: Orange-red
            else if (hungerLevel == 3)
                return new Color(1.0f, 0.2f, 0.2f); // Ravenous: Bright red
            else
                return new Color(0.6f, 0.2f, 0.2f);
        }
        
        private void PlaySymbiontPulseSound()
        {
            if (Map?.info?.parent?.def?.defName == "World") return; // Don't play on world map
            
            // Play pulsing sound with volume based on hunger level
            float volume = 0.3f + (hungerLevel * 0.2f);
            
            var soundDef = SoundDef.Named("SymbiontPulse");
            if (soundDef != null)
            {
                soundDef.PlayOneShot(new TargetInfo(this));
            }
        }
        
        private float GetCompulsionMTB()
        {
            float baseMTB = COMPULSION_BASE_INTERVAL / GenDate.TicksPerDay;
            
            // Apply settings multiplier
            baseMTB /= FleshSymbiontSettings.compulsionFrequencyMultiplier;
            
            // More hungry = more frequent compulsions
            baseMTB /= (1f + hungerLevel * 0.5f);
            
            // Fewer bonded pawns = more aggressive
            if (bondedPawns.Count == 0)
                baseMTB *= 0.5f;
            else if (bondedPawns.Count >= 3)
                baseMTB *= 2f;
                
            return baseMTB;
        }
        
        private void TryCompelNearbyPawn()
        {
            var pawns = Map.mapPawns.FreeColonistsSpawned
                .Where(p => p.Position.DistanceTo(Position) <= FleshSymbiontSettings.maxCompulsionRange && 
                  p.IsValidSymbiontTarget()) // Use extension method
                .OrderBy(p => p.Position.DistanceTo(Position));
                
            var target = pawns.FirstOrDefault();
            if (target != null)
            {
                // Play compulsion sound
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    var compulsionSound = SoundDef.Named("SymbiontWhisper");
                    compulsionSound?.PlayOneShot(new TargetInfo(target));
                }
                
                // Check for resistance
                if (FleshSymbiontSettings.allowCompulsionResistance && 
                    Rand.Chance(FleshSymbiontSettings.compulsionResistanceChance))
                {
                    // Play resistance static sound
                    if (FleshSymbiontSettings.enableAtmosphericEffects)
                    {
                        var staticSound = SoundDef.Named("CompulsionStatic");
                        staticSound?.PlayOneShot(new TargetInfo(target));
                    }
                    
                    if (FleshSymbiontSettings.enableHorrorMessages)
                    {
                        Messages.Message($"{target.Name.ToStringShort} resists the symbiont's psychic whispers through sheer willpower!", 
                            target, MessageTypeDefOf.PositiveEvent);
                    }
                    return;
                }
                
                target.mindState.mentalStateHandler.TryStartMentalState(
                    FleshSymbiontDefOf.SymbiontCompulsion, 
                    "Compelled by flesh symbiont", 
                    true, 
                    causedByMood: false);
            }
        }
        
        public void OnPawnBonded(Pawn pawn)
        {
            if (!bondedPawns.Contains(pawn))
            {
                bondedPawns.Add(pawn);
                hungerLevel = Mathf.Max(0, hungerLevel - 1);
                ticksSinceLastBond = 0;
                
                // Play bonding sound
                if (FleshSymbiontSettings.enableAtmosphericEffects)
                {
                    var bondingSound = SoundDef.Named("BondingScream");
                    bondingSound?.PlayOneShot(new TargetInfo(pawn));
                }
                
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    string message = GetBondingMessage(pawn);
                    
                    if (!string.IsNullOrEmpty(message))
                    {
                        Messages.Message(message, this, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
        }
        
        private string GetBondingMessage(Pawn pawn)
        {
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                return $"The flesh symbiont's tendrils pierce deep into {pawn.Name.ToStringShort}'s spine with a wet, tearing sound. " +
                       $"Their screams echo as alien flesh melds with human nervous tissue. The symbiont purrs with satisfaction...";
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                return $"The flesh symbiont has bonded with {pawn.Name.ToStringShort}. It seems... satisfied, for now.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                return $"{pawn.Name.ToStringShort} has bonded with the symbiont.";
            }
            
            return "";
        }
        
        public void OnSymbiontFed()
        {
            ticksSinceLastFeed = 0;
            hungerLevel = Mathf.Max(0, hungerLevel - 1);
        }
        
        public void OnXenogermExtracted()
        {
            // Damage the symbiont from extraction
            if (FleshSymbiontSettings.xenogermExtractionDamage > 0)
            {
                float damage = MaxHitPoints * FleshSymbiontSettings.xenogermExtractionDamage;
                TakeDamage(new DamageInfo(DamageDefOf.Cut, damage));
            }
            
            // Make bonded pawns upset
            foreach (var pawn in bondedPawns.Where(p => p != null && !p.Dead))
            {
                try
                {
                    var thought = ThoughtMaker.MakeThought(ThoughtDef.Named("SymbiontHarvested"));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                }
                catch
                {
                    // Thought doesn't exist, skip
                }
                
                // Small chance of madness from violation
                if (Rand.Chance(0.2f))
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(
                        FleshSymbiontDefOf.SymbiontMadness,
                        "Symbiont violated",
                        true,
                        causedByMood: false);
                }
            }
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                Messages.Message("The flesh symbiont shudders as genetic material is extracted. " +
                                "Its bonded servants sense the violation...", 
                    this, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Bonded pawns go berserk if symbiont is destroyed (if setting enabled)
            if (FleshSymbiontSettings.bondedDefendsSymbiont && FleshSymbiontSettings.allowSymbiontDestruction)
            {
                foreach (var pawn in bondedPawns.Where(p => p != null && !p.Dead))
                {
                    // Scale aggression based on settings
                    var madnessDef = FleshSymbiontSettings.defenseAggressionLevel > 1.5f ? 
                        FleshSymbiontDefOf.SymbiontMadness : 
                        MentalStateDefOf.Berserk;
                        
                    pawn.mindState.mentalStateHandler.TryStartMentalState(
                        madnessDef,
                        "Symbiont destroyed",
                        true,
                        causedByMood: false);
                }
            }
            
            base.Destroy(mode);
        }
        
        public override string GetInspectString()
        {
            var str = base.GetInspectString();
            
            if (bondedPawns.Count > 0)
            {
                str += $"\nBonded pawns: {bondedPawns.Count}";
            }
            
            string hungerDesc = GetHungerDescription();
            str += $"\nState: {hungerDesc}";
            
            return str;
        }
        
        private string GetHungerDescription()
        {
            if (hungerLevel == 0)
                return "Dormant";
            else if (hungerLevel == 1)
                return "Restless";
            else if (hungerLevel == 2)
                return "Hungry";
            else if (hungerLevel == 3)
                return "Ravenous";
            else
                return "Unknown";
        }
    }
}