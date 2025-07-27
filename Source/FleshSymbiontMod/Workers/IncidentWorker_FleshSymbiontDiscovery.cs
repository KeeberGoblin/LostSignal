using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    public class IncidentWorker_FleshSymbiontDiscovery : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var map = (Map)parms.target;
            
            // Check if we should limit symbionts per map
            if (FleshSymbiontSettings.limitSymbiontsPerMap)
            {
                var existingSymbionts = map.GetSymbiontCount();
                if (existingSymbionts >= FleshSymbiontSettings.maxSymbiontsPerMap)
                {
                    return false;
                }
            }
            
            // Check if research requirement is enabled
            if (FleshSymbiontSettings.enableResearchRequirement)
            {
                if (!Find.ResearchManager.IsFinished(FleshSymbiontDefOf.FleshSymbiontStudy))
                {
                    return false;
                }
            }
            
            // Don't spawn if no valid location exists
            if (SymbiontUtility.FindSymbiontSpawnLocation(map) == null)
            {
                return false;
            }
            
            return base.CanFireNowSub(parms);
        }
        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            
            // Find spawn location
            var spawnLocation = SymbiontUtility.FindSymbiontSpawnLocation(map);
            if (!spawnLocation.HasValue)
            {
                Log.Warning("[Signal Lost] Could not find valid spawn location for flesh symbiont");
                return false;
            }
            
            // Create and configure the symbiont
            var symbiont = CreateSymbiont(map, spawnLocation.Value);
            if (symbiont == null)
            {
                Log.Error("[Signal Lost] Failed to create flesh symbiont");
                return false;
            }
            
            // Create atmospheric spawn effects
            CreateDiscoveryEffects(spawnLocation.Value, map);
            
            // Send discovery letter
            SendDiscoveryLetter(symbiont);
            
            // Schedule follow-up events
            ScheduleFollowUpEvents(map);
            
            return true;
        }
        
        private Building_FleshSymbiont CreateSymbiont(Map map, IntVec3 spawnLocation)
        {
            var symbiont = (Building_FleshSymbiont)ThingMaker.MakeThing(FleshSymbiontDefOf.FleshSymbiont);
            if (symbiont == null) return null;
            
            // Set faction (usually player, but could be neutral for mystery)
            symbiont.SetFaction(Faction.OfPlayer);
            
            // Apply health multiplier from settings
            if (FleshSymbiontSettings.symbiontHealthMultiplier != 1.0f)
            {
                int modifiedHealth = Mathf.RoundToInt(symbiont.MaxHitPoints * FleshSymbiontSettings.symbiontHealthMultiplier);
                symbiont.HitPoints = modifiedHealth;
            }
            
            // Spawn the symbiont
            GenSpawn.Spawn(symbiont, spawnLocation, map);
            
            return symbiont;
        }
        
        private void CreateDiscoveryEffects(IntVec3 location, Map map)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            // Dramatic emergence effects
            for (int i = 0; i < 15; i++)
            {
                var effectPos = location.ToVector3() + Gen.RandomHorizontalVector(4f);
                MoteMaker.ThrowDustPuff(effectPos, map, 2.0f);
            }
            
            // Heat signature
            MoteMaker.ThrowHeatGlow(location, map, 3f);
            
            // Alien energy discharge
            for (int i = 0; i < 8; i++)
            {
                MoteMaker.ThrowMicroSparks(location.ToVector3() + Gen.RandomHorizontalVector(2f), map);
            }
            
            // Lightning effect for dramatic reveal
            MoteMaker.ThrowLightningGlow(location.ToVector3(), map, 2.5f);
            
            // Sound effect
            FleshSymbiontDefOf.SymbiontPulse?.PlayOneShot(new TargetInfo(location, map));
        }
        
        private void SendDiscoveryLetter(Building_FleshSymbiont symbiont)
        {
            string letterText = GetDiscoveryLetterText();
            string letterLabel = GetDiscoveryLetterLabel();
            
            SendStandardLetter(letterLabel, letterText, def.letterDef, symbiont, null);
        }
        
        private string GetDiscoveryLetterLabel()
        {
            return FleshSymbiontSettings.messageFrequency switch
            {
                3 => "CLASSIFIED: Signal Lost",
                2 => "Anomalous Discovery",
                1 => "Flesh Symbiont Found",
                _ => def.letterLabel ?? "Discovery"
            };
        }
        
        private string GetDiscoveryLetterText()
        {
            if (!FleshSymbiontSettings.enableHorrorMessages)
                return GetGenericDiscoveryText();
            
            return FleshSymbiontSettings.messageFrequency switch
            {
                3 => GetMaximumHorrorText(),
                2 => GetModerateHorrorText(),
                1 => GetMinimalHorrorText(),
                _ => GetGenericDiscoveryText()
            };
        }
        
        private string GetMaximumHorrorText()
        {
            return "CLASSIFIED TRANSMISSION LOG - SECTOR 7G-EPSILON\n" +
                   "PRIORITY: ALPHA-BLACK\n\n" +
                   "Our excavation team has uncovered what can only be described as a biological anomaly of unknown origin. " +
                   "The artifact exhibits characteristics of both organic tissue and advanced biotechnology that defies current understanding.\n\n" +
                   "Preliminary scans detect complex neural activity and what appears to be a form of directed intelligence. " +
                   "Personnel report experiencing auditory hallucinations and compulsive behavior when in proximity to the specimen. " +
                   "Dr. Martinez described the entity as 'beautiful' and 'calling to her' before requiring immediate sedation.\n\n" +
                   "The creature's surface pulses with an organic rhythm that seems to synchronize with nearby human heartbeats. " +
                   "Ribbed appendages emerge from its mass, twitching with predatory awareness.\n\n" +
                   "WARNING: Do not allow unsupervised contact with the entity. Maintain minimum safe distance of 50 meters. " +
                   "All personnel exhibiting unusual behavior must be quarantined immediately.\n\n" +
                   "The signal we've been tracking has gone silent. Something is awakening.";
        }
        
        private string GetModerateHorrorText()
        {
            return "Your colonists have discovered a massive, pulsing organic mass buried in the ground. " +
                   "The creature's ribbed, hand-like appendage seems to twitch with its own life force, " +
                   "while complex networks beneath its translucent flesh suggest advanced intelligence.\n\n" +
                   "Something about its biomechanical appearance fills your colonists with unease. " +
                   "The flesh appears to be waiting... hungry. Several colonists report hearing faint whispers " +
                   "when they venture too close.\n\n" +
                   "One of your colonists feels inexplicably drawn to touch it.";
        }
        
        private string GetMinimalHorrorText()
        {
            return "Your colonists have discovered a strange organic artifact of unknown origin. " +
                   "The creature exhibits signs of intelligence and several colonists seem drawn to it.\n\n" +
                   "Caution is advised when interacting with this entity.";
        }
        
        private string GetGenericDiscoveryText()
        {
            return def.letterText ?? "A flesh symbiont has been discovered.";
        }
        
        private void ScheduleFollowUpEvents(Map map)
        {
            // Increase chance of additional symbiont events if this one was successful
            if (FleshSymbiontSettings.eventFrequencyMultiplier > 1.0f)
            {
                // Schedule potential follow-up discovery in the future
                float daysUntilNext = Rand.Range(
                    FleshSymbiontSettings.minDaysBetweenEvents * 0.7f,
                    FleshSymbiontSettings.maxDaysBetweenEvents * 0.8f
                );
                
                // This would require custom event scheduling - placeholder for now
                if (Prefs.DevMode)
                {
                    Log.Message($"[Signal Lost] Next potential symbiont event in ~{daysUntilNext:F0} days");
                }
            }
        }
        
        public override float BaseChanceThisGame => 
            base.BaseChanceThisGame * FleshSymbiontSettings.eventFrequencyMultiplier;
            
        protected override float AdjustedChance
        {
            get
            {
                float baseChance = base.AdjustedChance;
                
                // Reduce chance if many symbionts already exist
                var playerMaps = Find.Maps?.Where(m => m.IsPlayerHome) ?? Enumerable.Empty<Map>();
                int totalSymbionts = playerMaps.Sum(m => m.GetSymbiontCount());
                
                if (totalSymbionts > 0)
                {
                    baseChance *= Mathf.Pow(0.7f, totalSymbionts); // Diminishing returns
                }
                
                // Increase chance based on colony wealth (late game colonies get more events)
                float wealthFactor = Find.StoryWatcher?.watcherWealth?.WealthTotal ?? 0f;
                if (wealthFactor > 100000f) // 100k+ wealth
                {
                    baseChance *= 1.2f;
                }
                
                return baseChance;
            }
        }
        
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            
            // Validate settings on load
            if (FleshSymbiontSettings.minDaysBetweenEvents > FleshSymbiontSettings.maxDaysBetweenEvents)
            {
                Log.Warning("[Signal Lost] Invalid day range settings, fixing automatically");
                FleshSymbiontSettings.maxDaysBetweenEvents = FleshSymbiontSettings.minDaysBetweenEvents + 30;
            }
        }
        
        // Debug method for testing
        public bool TryExecuteDebug(Map map, IntVec3? forceLocation = null)
        {
            if (!Prefs.DevMode) return false;
            
            var parms = StorytellerUtility.DefaultParmsNow(def.category, map);
            
            if (forceLocation.HasValue)
            {
                // Force spawn at specific location for debugging
                var symbiont = CreateSymbiont(map, forceLocation.Value);
                if (symbiont != null)
                {
                    CreateDiscoveryEffects(forceLocation.Value, map);
                    Messages.Message("DEBUG: Forced symbiont spawn", symbiont, MessageTypeDefOf.NeutralEvent);
                    return true;
                }
            }
            
            return TryExecuteWorker(parms);
        }
    }
}
