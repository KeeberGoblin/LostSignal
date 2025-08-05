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
        private int hungerLevel = 0;
        private List<Pawn> bondedPawns = new List<Pawn>();
        private int pulseTimer = 0;
        private CompGlower glowerComp;
        
        private const int COMPULSION_BASE_INTERVAL = 45000;
        private const int FEED_INTERVAL = 30000;
        private const int HUNGER_INCREASE_INTERVAL = 60000;
        private const int PULSE_INTERVAL = 120;
        
        public int HungerLevel => hungerLevel;
        public IReadOnlyList<Pawn> BondedPawns => bondedPawns.AsReadOnly();
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            glowerComp = GetComp<CompGlower>();
            
            if (!respawningAfterLoad)
            {
                // Initial spawn effects
                CreateSpawnEffects();
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksSinceLastBond, "ticksSinceLastBond", 0);
            Scribe_Values.Look(ref ticksSinceLastFeed, "ticksSinceLastFeed", 0);
            Scribe_Values.Look(ref hungerLevel, "hungerLevel", 0);
            Scribe_Values.Look(ref pulseTimer, "pulseTimer", 0);
            Scribe_Collections.Look(ref bondedPawns, "bondedPawns", LookMode.Reference);
            
            if (bondedPawns == null)
                bondedPawns = new List<Pawn>();
        }
        
        public override void Tick()
        {
            base.Tick();
            
            if (!Spawned) return;
            
            ticksSinceLastBond++;
            ticksSinceLastFeed++;
            pulseTimer++;
            
            // Clean up bonded pawns list
            CleanupBondedPawns();
            
            // Update atmospheric effects
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                UpdatePulsingGlow();
                
                if (IsHashIntervalTick(300))
                    PlaySymbiontPulseSound();
            }
            
            // Hunger progression
            UpdateHungerState();
            
            // Try compulsion
            if (Rand.MTBEventOccurs(GetCompulsionMTB(), GenDate.TicksPerDay, 1f))
            {
                TryCompelNearbyPawn();
            }
            
            // Make bonded pawns feed
            if (bondedPawns.Count > 0 && ticksSinceLastFeed > FEED_INTERVAL)
            {
                TryMakeBondedPawnFeed();
            }
            
            // Decay if enabled
            if (FleshSymbiontSettings.enableSymbiontDecay && ticksSinceLastFeed > FEED_INTERVAL * 2)
            {
                ApplyDecayDamage();
            }
        }
        
        private void CleanupBondedPawns()
        {
            bondedPawns.RemoveAll(p => p == null || p.Dead || !p.IsSymbiontBonded());
        }
        
        private void UpdatePulsingGlow()
        {
            if (glowerComp?.Props == null) return;
            
            float pulseIntensity = Mathf.Sin(pulseTimer * 0.05f) * 0.3f + 0.7f;
            float hungerMultiplier = 1f + (hungerLevel * 0.5f);
            pulseIntensity *= hungerMultiplier;
            
            Color baseColor = hungerLevel switch
            {
                0 => new Color(0.6f, 0.2f, 0.2f),
                1 => new Color(0.8f, 0.3f, 0.2f),
                2 => new Color(1.0f, 0.4f, 0.1f),
                3 => new Color(1.0f, 0.2f, 0.2f),
                _ => new Color(0.6f, 0.2f, 0.2f)
            };
            
            glowerComp.Props.glowColor = baseColor * pulseIntensity;
            
            if (pulseTimer > 1000)
                pulseTimer = 0;
        }
        
        private void PlaySymbiontPulseSound()
        {
            if (Map?.info?.parent?.def?.defName == "World") return;
            
            FleshSymbiontDefOf.SymbiontPulse?.PlayOneShot(new TargetInfo(this));
        }
        
        private void UpdateHungerState()
        {
            float hungerInterval = HUNGER_INCREASE_INTERVAL / FleshSymbiontSettings.hungerGrowthMultiplier;

            if (ticksSinceLastFeed > hungerInterval)
            {
                int previousHunger = hungerLevel;
                hungerLevel = Mathf.Min(3, hungerLevel + 1);
                ticksSinceLastFeed = 0;

                if (hungerLevel > previousHunger && hungerLevel >= 2)
                {
                    ShowHungerMessage();
                }
            }
        }
        
        private void ShowHungerMessage()
        {
            if (!FleshSymbiontSettings.enableHorrorMessages) return;
            
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => "The flesh symbiont writhes in ravenous hunger, its alien whispers growing into screams that claw at your colonists' minds. The very air around it seems to pulse with malevolent need...",
                2 => "The flesh symbiont pulses hungrily, its whispers growing stronger and more insistent...",
                1 => "The flesh symbiont seems increasingly restless.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, this, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        private float GetCompulsionMTB()
        {
            float baseMTB = COMPULSION_BASE_INTERVAL / GenDate.TicksPerDay;
            baseMTB /= FleshSymbiontSettings.compulsionFrequencyMultiplier;
            baseMTB /= (1f + hungerLevel * 0.5f);
            
            if (bondedPawns.Count == 0)
                baseMTB *= 0.5f;
            else if (bondedPawns.Count >= 3)
                baseMTB *= 2f;
                
            return baseMTB;
        }
        
        private void TryCompelNearbyPawn()
        {
            var validTargets = Map.mapPawns.FreeColonistsSpawned
                .Where(p => p.Position.DistanceTo(Position) <= FleshSymbiontSettings.maxCompulsionRange && 
                           p.IsValidSymbiontTarget())
                .OrderBy(p => p.Position.DistanceTo(Position));
                
            var target = validTargets.FirstOrDefault();
            if (target == null) return;
            
            // Play compulsion sound
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                FleshSymbiontDefOf.SymbiontWhisper?.PlayOneShot(new TargetInfo(target));
            }
            
            // Check resistance
            float resistanceChance = target.GetSymbiontCompulsionResistance();
            if (FleshSymbiontSettings.allowCompulsionResistance && Rand.Chance(resistanceChance))
            {
                HandleCompulsionResistance(target);
                return;
            }
            
            // Apply compulsion
            bool success = target.mindState.mentalStateHandler.TryStartMentalState(
                FleshSymbiontDefOf.SymbiontCompulsion,
                "Compelled by flesh symbiont",
                true,
                causedByMood: false);
                
            if (success && FleshSymbiontSettings.enableHorrorMessages)
            {
                ShowCompulsionMessage(target);
            }
        }
        
        private void HandleCompulsionResistance(Pawn target)
        {
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                FleshSymbiontDefOf.CompulsionStatic?.PlayOneShot(new TargetInfo(target));
            }
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                Messages.Message($"{target.Name.ToStringShort} resists the symbiont's psychic whispers through sheer willpower!", 
                    target, MessageTypeDefOf.PositiveEvent);
            }
        }
        
        private void ShowCompulsionMessage(Pawn target)
        {
            string message = Extensions.GetHorrorIntensityMessage(
                $"{target.Name.ToStringShort} is being compelled by the symbiont.",
                $"{target.Name.ToStringShort} staggers as alien whispers flood their mind, compelling them toward the flesh symbiont...",
                $"{target.Name.ToStringShort}'s pupils dilate unnaturally as otherworldly whispers claw at their consciousness. They turn toward the symbiont with vacant, predatory hunger."
            );
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, target, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        private void TryMakeBondedPawnFeed()
        {
            var availableFeeders = bondedPawns.Where(p => 
                p?.Spawned == true && 
                p.Awake() && 
                p.CurJob?.def != FleshSymbiontDefOf.FeedSymbiont).ToList();
                
            if (!availableFeeders.Any()) return;
            
            var feeder = availableFeeders.RandomElement();
            var feedJob = JobMaker.MakeJob(FleshSymbiontDefOf.FeedSymbiont, this);
            feeder.jobs.TryTakeOrderedJob(feedJob, JobTag.Misc);
            ticksSinceLastFeed = 0;
        }
        
        private void ApplyDecayDamage()
        {
            float damagePerTick = FleshSymbiontSettings.symbiontDecayRate / GenDate.TicksPerDay;
            TakeDamage(new DamageInfo(DamageDefOf.Deterioration, damagePerTick));
        }
        
        private void CreateSpawnEffects()
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            for (int i = 0; i < 12; i++)
            {
                var effectPos = DrawPos + Gen.RandomHorizontalVector(3f);
                MoteMaker.ThrowDustPuff(effectPos, Map, 1.5f);
            }
            
            MoteMaker.ThrowHeatGlow(Position, Map, 2f);
            
            for (int i = 0; i < 6; i++)
            {
                MoteMaker.ThrowMicroSparks(DrawPos + Gen.RandomHorizontalVector(1f), Map);
            }
        }
        
        public void OnPawnBonded(Pawn pawn)
        {
            if (pawn == null || bondedPawns.Contains(pawn)) return;
            
            bondedPawns.Add(pawn);
            hungerLevel = Mathf.Max(0, hungerLevel - 1);
            ticksSinceLastBond = 0;
            
            // Effects and sounds
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                FleshSymbiontDefOf.BondingScream?.PlayOneShot(new TargetInfo(pawn));
                SymbiontUtility.CreateBondingEffects(pawn, this);
            }
            
            // Grant abilities if Royalty is active
            GrantSymbiontAbilities(pawn);
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                ShowBondingMessage(pawn);
            }
        }
        
        private void GrantSymbiontAbilities(Pawn pawn)
        {
            if (!ModsConfig.RoyaltyActive || !FleshSymbiontSettings.enableRoyaltyFeatures || 
                !FleshSymbiontSettings.enableSymbiontPsycasts || pawn.abilities == null) return;
            
            var abilities = new[]
            {
                FleshSymbiontDefOf.SymbiontCompel,
                FleshSymbiontDefOf.SymbiontHeal,
                FleshSymbiontDefOf.SymbiontRage
            };
            
            foreach (var ability in abilities)
            {
                if (ability != null && !pawn.abilities.HasAbility(ability))
                {
                    pawn.abilities.GainAbility(ability);
                }
            }
        }
        
        private void ShowBondingMessage(Pawn pawn)
        {
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"The flesh symbiont's tendrils pierce deep into {pawn.Name.ToStringShort}'s spine with a wet, tearing sound. Their screams echo as alien flesh melds with human nervous tissue. The symbiont purrs with satisfaction as another soul is claimed...",
                2 => $"The flesh symbiont has bonded with {pawn.Name.ToStringShort}. It seems... satisfied, for now.",
                1 => $"{pawn.Name.ToStringShort} has bonded with the symbiont.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, this, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        public void OnSymbiontFed()
        {
            ticksSinceLastFeed = 0;
            hungerLevel = Mathf.Max(0, hungerLevel - 1);
        }
        
        public void OnXenogermExtracted()
        {
            // Apply extraction damage
            if (FleshSymbiontSettings.xenogermExtractionDamage > 0)
            {
                float damage = MaxHitPoints * FleshSymbiontSettings.xenogermExtractionDamage;
                TakeDamage(new DamageInfo(DamageDefOf.Cut, damage));
            }
            
            // Upset bonded pawns
            foreach (var pawn in bondedPawns.Where(p => p?.Dead == false))
            {
                var thought = ThoughtMaker.MakeThought(FleshSymbiontDefOf.SymbiontHarvested);
                pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
                
                // Chance of madness
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
                Messages.Message("The flesh symbiont shudders as genetic material is extracted. Its bonded servants sense the violation...", 
                    this, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (FleshSymbiontSettings.bondedDefendsSymbiont && FleshSymbiontSettings.allowSymbiontDestruction)
            {
                foreach (var pawn in bondedPawns.Where(p => p?.Dead == false))
                {
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
            
            string hungerDesc = Extensions.GetSymbiontStateDescription(hungerLevel);
            str += $"\nState: {hungerDesc}";
            
            if (Prefs.DevMode)
            {
                str += $"\nDEV: Hunger {hungerLevel}, Ticks since bond: {ticksSinceLastBond}, Ticks since feed: {ticksSinceLastFeed}";
            }
            
            return str;
        }
    }
}
