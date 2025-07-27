using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    public static class Extensions
    {
        // Extension methods for Pawn
        public static bool IsSymbiontBonded(this Pawn pawn)
        {
            return pawn?.health?.hediffSet?.HasHediff(FleshSymbiontDefOf.SymbiontBond) ?? false;
        }
        
        public static bool IsSymbiontHybrid(this Pawn pawn)
        {
            if (!ModsConfig.BiotechActive) return false;
            return pawn?.genes?.Xenotype?.defName == "SymbiontHybrid";
        }
        
        public static bool HasRoyalTitle(this Pawn pawn)
        {
            if (!ModsConfig.RoyaltyActive) return false;
            return pawn?.royalty?.AllTitlesForReading?.Any() ?? false;
        }
        
        public static bool CanResistSymbiontCompulsion(this Pawn pawn)
        {
            if (pawn == null) return false;
            
            // Already bonded pawns can't be compelled
            if (pawn.IsSymbiontBonded()) return true;
            
            // Check traits that provide resistance
            if (pawn.story?.traits?.HasTrait(TraitDefOf.Psychopath) == true) return true;
            if (pawn.story?.traits?.HasTrait(TraitDefOf.Nerves) == true &&
                pawn.story.traits.GetTrait(TraitDefOf.Nerves).Degree > 0) return true;
            
            // Royal titles provide some resistance
            if (pawn.HasRoyalTitle()) return Rand.Chance(0.3f);
            
            return false;
        }
        
        public static float GetSymbiontCompulsionResistance(this Pawn pawn)
        {
            if (pawn == null) return 0f;
            
            float resistance = 0f;
            
            // Trait-based resistance
            if (pawn.story?.traits != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
                    resistance += 0.4f;
                    
                if (pawn.story.traits.HasTrait(TraitDefOf.Nerves))
                {
                    var degree = pawn.story.traits.GetTrait(TraitDefOf.Nerves).Degree;
                    resistance += degree * 0.2f;
                }
                
                if (pawn.story.traits.HasTrait(TraitDefOf.PsychicSensitivity))
                {
                    var degree = pawn.story.traits.GetTrait(TraitDefOf.PsychicSensitivity).Degree;
                    resistance -= degree * 0.15f; // More sensitive = less resistance
                }
            }
            
            // Psychic sensitivity stat (Royalty)
            if (ModsConfig.RoyaltyActive)
            {
                float psychicSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                resistance -= (psychicSensitivity - 1f) * 0.25f;
            }
            
            // Royal titles
            if (pawn.HasRoyalTitle())
                resistance += 0.2f;
            
            // Faction loyalty
            if (pawn.Faction != Faction.OfPlayer)
                resistance += 0.3f;
            
            return Mathf.Clamp(resistance, -0.5f, 0.8f);
        }
        
        // Extension methods for Map
        public static IEnumerable<Building_FleshSymbiont> GetAllSymbionts(this Map map)
        {
            return map?.listerBuildings?.AllBuildingsColonistOfClass<Building_FleshSymbiont>() ?? 
                   Enumerable.Empty<Building_FleshSymbiont>();
        }
        
        public static Building_FleshSymbiont GetNearestSymbiont(this Map map, IntVec3 position)
        {
            return map.GetAllSymbionts()
                .Where(s => s.Spawned)
                .OrderBy(s => s.Position.DistanceTo(position))
                .FirstOrDefault();
        }
        
        public static IEnumerable<Pawn> GetBondedColonists(this Map map)
        {
            return map.mapPawns.FreeColonistsSpawned
                .Where(p => p.IsSymbiontBonded());
        }
        
        public static int GetSymbiontCount(this Map map)
        {
            return map.GetAllSymbionts().Count();
        }
        
        // Extension methods for Building_FleshSymbiont
        public static bool IsHungry(this Building_FleshSymbiont symbiont)
        {
            // This would require accessing private fields, so we'll use reflection or make them public
            // For now, return a placeholder
            return symbiont?.HitPoints < symbiont?.MaxHitPoints * 0.8f;
        }
        
        public static float GetHungerLevel(this Building_FleshSymbiont symbiont)
        {
            if (symbiont == null) return 0f;
            
            // Calculate hunger based on health ratio (placeholder)
            float healthRatio = (float)symbiont.HitPoints / symbiont.MaxHitPoints;
            return Mathf.Clamp(1f - healthRatio, 0f, 1f);
        }
        
        // Utility methods for atmospheric effects
        public static void CreateSymbiontAtmosphere(this IntVec3 position, Map map, float intensity = 1f)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            for (int i = 0; i < Mathf.RoundToInt(5 * intensity); i++)
            {
                var offset = Gen.RandomHorizontalVector(2f * intensity);
                MoteMaker.ThrowDustPuff(position.ToVector3() + offset, map, 1.2f * intensity);
            }
            
            if (intensity > 0.5f)
            {
                MoteMaker.ThrowHeatGlow(position, map, intensity);
            }
        }
        
        public static void CreatePsychicEffect(this Pawn pawn, float intensity = 1f)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects || pawn?.Map == null) return;
            
            MoteMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 0.8f * intensity);
            
            for (int i = 0; i < Mathf.RoundToInt(3 * intensity); i++)
            {
                MoteMaker.ThrowMicroSparks(pawn.DrawPos + Gen.RandomHorizontalVector(1f), pawn.Map);
            }
        }
        
        // String utility methods
        public static string GetSymbiontStateDescription(int hungerLevel)
        {
            return hungerLevel switch
            {
                0 => "Dormant",
                1 => "Restless",
                2 => "Hungry",
                3 => "Ravenous",
                _ => "Unknown"
            };
        }
        
        public static string GetHorrorIntensityMessage(string baseMessage, string intenseMessage, string extremeMessage)
        {
            if (!FleshSymbiontSettings.enableHorrorMessages) return "";
            
            return FleshSymbiontSettings.messageFrequency switch
            {
                1 => baseMessage,
                2 => intenseMessage,
                3 => extremeMessage,
                _ => ""
            };
        }
        
        // Color utility methods
        public static Color GetSymbiontGlowColor(int hungerLevel, float pulseIntensity = 1f)
        {
            Color baseColor = hungerLevel switch
            {
                0 => new Color(0.6f, 0.2f, 0.2f), // Dormant: Dark red
                1 => new Color(0.8f, 0.3f, 0.2f), // Restless: Brighter red
                2 => new Color(1.0f, 0.4f, 0.1f), // Hungry: Orange-red
                3 => new Color(1.0f, 0.2f, 0.2f), // Ravenous: Bright red
                _ => new Color(0.6f, 0.2f, 0.2f)
            };
            
            return baseColor * pulseIntensity;
        }
        
        // Validation methods
        public static bool IsValidSymbiontTarget(this Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.Dead || pawn.Downed) return false;
            if (pawn.IsSymbiontBonded()) return false;
            if (pawn.MentalStateDef != null) return false;
            
            return true;
        }
        
        public static bool CanPerformSymbiontPsycast(this Pawn pawn)
        {
            if (!ModsConfig.RoyaltyActive) return false;
            if (!FleshSymbiontSettings.enableRoyaltyFeatures) return false;
            if (!FleshSymbiontSettings.enableSymbiontPsycasts) return false;
            if (!pawn.IsSymbiontBonded()) return false;
            
            return pawn.psychicEntropy != null;
        }
        
        // Distance and proximity methods
        public static bool IsNearSymbiont(this Pawn pawn, float maxDistance = 8f)
        {
            if (pawn?.Map == null) return false;
            
            return pawn.Map.GetAllSymbionts()
                .Any(s => s.Position.DistanceTo(pawn.Position) <= maxDistance);
        }
        
        public static float DistanceToNearestSymbiont(this Pawn pawn)
        {
            if (pawn?.Map == null) return float.MaxValue;
            
            var nearest = pawn.Map.GetNearestSymbiont(pawn.Position);
            return nearest?.Position.DistanceTo(pawn.Position) ?? float.MaxValue;
        }
        
        // Thought and mood utilities
        public static void TryGiveSymbiontThought(this Pawn pawn, ThoughtDef thoughtDef, float moodMultiplier = 1f)
        {
            if (pawn?.needs?.mood?.thoughts?.memories == null) return;
            
            var thought = ThoughtMaker.MakeThought(thoughtDef);
            if (moodMultiplier != 1f && thought is Thought_Memory memThought)
            {
                memThought.moodPowerFactor = moodMultiplier;
            }
            
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
        }
        
        // DLC check utilities
        public static bool HasRequiredDLCs()
        {
            return ModsConfig.AnomalyActive; // Anomaly is required
        }
        
        public static bool CanUseBiotechFeatures()
        {
            return ModsConfig.BiotechActive && FleshSymbiontSettings.enableBiotechFeatures;
        }
        
        public static bool CanUseIdeologyFeatures()
        {
            return ModsConfig.IdeologyActive && FleshSymbiontSettings.enableIdeologyFeatures;
        }
        
        public static bool CanUseRoyaltyFeatures()
        {
            return ModsConfig.RoyaltyActive && FleshSymbiontSettings.enableRoyaltyFeatures;
        }
    }
}