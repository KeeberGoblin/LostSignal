using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    public static class SymbiontUtility
    {
        // Compulsion and mental state utilities
        public static bool TryCompelPawn(Pawn target, Building_FleshSymbiont symbiont, Pawn caster = null)
        {
            if (!IsValidCompulsionTarget(target, symbiont)) return false;
            
            // Check resistance
            float resistanceChance = target.GetSymbiontCompulsionResistance();
            if (FleshSymbiontSettings.allowCompulsionResistance && Rand.Chance(resistanceChance))
            {
                HandleCompulsionResistance(target, caster);
                return false;
            }
            
            // Apply compulsion
            bool success = target.mindState.mentalStateHandler.TryStartMentalState(
                FleshSymbiontDefOf.SymbiontCompulsion,
                GetCompulsionReason(caster),
                true,
                causedByMood: false);
                
            if (success)
            {
                ShowCompulsionMessage(target, caster, symbiont);
                CreateCompulsionEffects(target, symbiont);
            }
            
            return success;
        }
        
        private static bool IsValidCompulsionTarget(Pawn target, Building_FleshSymbiont symbiont)
        {
            if (target == null || symbiont == null) return false;
            if (!target.IsValidSymbiontTarget()) return false;
            if (target.Position.DistanceTo(symbiont.Position) > FleshSymbiontSettings.maxCompulsionRange) return false;
            
            return true;
        }
        
        private static void HandleCompulsionResistance(Pawn target, Pawn caster)
        {
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                FleshSymbiontDefOf.CompulsionStatic?.PlayOneShot(new TargetInfo(target));
                target.CreatePsychicEffect(0.8f);
            }
            
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                string casterText = caster != null ? $" {caster.Name.ToStringShort()}'s" : "";
                string message = Extensions.GetHorrorIntensityMessage(
                    $"{target.Name.ToStringShort} resists{casterText} psychic compulsion.",
                    $"{target.Name.ToStringShort} resists{casterText} psychic compulsion through sheer willpower!",
                    $"{target.Name.ToStringShort}'s mind blazes with defiant will as{casterText} alien whispers crash against their mental barriers. Their eyes flare with righteous fury as they throw off the symbiont's influence."
                );
                
                if (!string.IsNullOrEmpty(message))
                {
                    Messages.Message(message, target, MessageTypeDefOf.PositiveEvent);
                }
            }
        }
        
        private static string GetCompulsionReason(Pawn caster)
        {
            return caster != null ? 
                $"Compelled by {caster.Name.ToStringShort}" : 
                "Compelled by flesh symbiont";
        }
        
        private static void ShowCompulsionMessage(Pawn target, Pawn caster, Building_FleshSymbiont symbiont)
        {
            if (!FleshSymbiontSettings.enableHorrorMessages) return;
            
            string casterName = caster?.Name.ToStringShort ?? "the symbiont";
            string message = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"{casterName} channels otherworldly power as {target.Name.ToStringShort}'s pupils dilate unnaturally. " +
                     $"Alien whispers claw at their consciousness, overwhelming their will with predatory hunger. " +
                     $"They turn toward the flesh symbiont with the vacant stare of the consumed.",
                2 => $"{target.Name.ToStringShort} staggers as alien whispers flood their mind, compelling them toward the flesh symbiont...",
                1 => $"{target.Name.ToStringShort} is being compelled by {casterName}.",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Message(message, target, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        private static void CreateCompulsionEffects(Pawn target, Building_FleshSymbiont symbiont)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            // Play compulsion sound
            FleshSymbiontDefOf.SymbiontWhisper?.PlayOneShot(new TargetInfo(target));
            
            // Visual effects
            target.CreatePsychicEffect(1.0f);
            symbiont.Position.CreateSymbiontAtmosphere(symbiont.Map, 1.2f);
        }
        
        // Bonding and unbonding utilities
        public static bool TryBondPawnToSymbiont(Pawn pawn, Building_FleshSymbiont symbiont, bool voluntary = false)
        {
            if (!IsValidBondingTarget(pawn, symbiont)) return false;
            
            // Create the bond hediff
            var bondHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontBond, pawn);
            pawn.health.AddHediff(bondHediff);
            
            // Notify the symbiont
            symbiont.OnPawnBonded(pawn);
            
            // Create bonding effects
            CreateBondingEffects(pawn, symbiont);
            
            // Play bonding sound
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                FleshSymbiontDefOf.BondingScream?.PlayOneShot(new TargetInfo(pawn));
            }
            
            return true;
        }
        
        private static bool IsValidBondingTarget(Pawn pawn, Building_FleshSymbiont symbiont)
        {
            if (pawn == null || symbiont == null) return false;
            if (pawn.IsSymbiontBonded()) return false;
            if (pawn.Dead || pawn.Downed) return false;
            
            return true;
        }
        
        public static void CreateBondingEffects(Pawn pawn, Building_FleshSymbiont symbiont)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            // Dramatic visual effects around both pawn and symbiont
            pawn.Position.CreateSymbiontAtmosphere(pawn.Map, 2.0f);
            symbiont.Position.CreateSymbiontAtmosphere(symbiont.Map, 1.5f);
            
            // Energy connection effect
            var connectionPoint = Vector3.Lerp(pawn.DrawPos, symbiont.DrawPos, 0.5f);
            for (int i = 0; i < 8; i++)
            {
                MoteMaker.ThrowMicroSparks(connectionPoint + Gen.RandomHorizontalVector(1f), pawn.Map);
            }
            
            // Dramatic lighting effects
            MoteMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.5f);
            MoteMaker.ThrowHeatGlow(symbiont.Position, symbiont.Map, 2.0f);
            
            // Additional bonding effects
            for (int i = 0; i < 12; i++)
            {
                var effectPos = pawn.DrawPos + Gen.RandomHorizontalVector(2f);
                MoteMaker.ThrowDustPuff(effectPos, pawn.Map, 1.8f);
            }
        }
        
        public static bool TryRemoveBond(Pawn pawn, bool forced = false)
        {
            if (!IsValidUnbondingTarget(pawn)) return false;
            
            var bondHediff = pawn.health.hediffSet.GetFirstHediffOfDef(FleshSymbiontDefOf.SymbiontBond);
            if (bondHediff == null) return false;
            
            // Create removal effects before removing hediff
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                CreateUnbondingEffects(pawn, forced);
            }
            
            // Remove the hediff
            pawn.health.RemoveHediff(bondHediff);
            
            // Apply trauma for forced removal
            if (forced)
            {
                ApplyUnbondingTrauma(pawn);
            }
            
            return true;
        }
        
        private static bool IsValidUnbondingTarget(Pawn pawn)
        {
            return pawn?.IsSymbiontBonded() == true;
        }
        
        private static void CreateUnbondingEffects(Pawn pawn, bool forced)
        {
            float intensity = forced ? 2.0f : 1.0f;
            
            pawn.CreatePsychicEffect(intensity);
            
            for (int i = 0; i < Mathf.RoundToInt(15 * intensity); i++)
            {
                MoteMaker.ThrowDustPuff(pawn.DrawPos + Gen.RandomHorizontalVector(2f), pawn.Map, 1.8f);
            }
            
            if (forced)
            {
                // More dramatic effects for forced removal
                MoteMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 2.0f);
                for (int i = 0; i < 8; i++)
                {
                    MoteMaker.ThrowMicroSparks(pawn.DrawPos + Gen.RandomHorizontalVector(1.5f), pawn.Map);
                }
            }
        }
        
        private static void ApplyUnbondingTrauma(Pawn pawn)
        {
            // Immediate psychic shock
            var traumaHediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
            traumaHediff.Severity = Rand.Range(0.6f, 0.9f);
            pawn.health.AddHediff(traumaHediff);
            
            // Chance of permanent brain damage
            if (Rand.Chance(0.2f))
            {
                var brainInjury = HediffMaker.MakeHediff(HediffDefOf.TBI, pawn, pawn.health.hediffSet.GetBrain());
                brainInjury.Severity = Rand.Range(0.1f, 0.4f);
                pawn.health.AddHediff(brainInjury);
                
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    Messages.Message($"The forced unbonding has left permanent damage to {pawn.Name.ToStringShort}'s brain!", 
                        pawn, MessageTypeDefOf.NegativeEvent);
                }
            }
            
            // Temporary mood debuff
            var traumaThought = ThoughtMaker.MakeThought(ThoughtDef.Named("ForcedSymbiontUnbonding"));
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(traumaThought);
        }
        
        // Spawning and discovery utilities
        public static IntVec3? FindSymbiontSpawnLocation(Map map, float minDistanceFromBuildings = 15f)
        {
            var potentialSpots = new List<(IntVec3 cell, float score)>();
            
            // Evaluate all cells on the map
            foreach (var cell in map.AllCells)
            {
                if (!IsValidSpawnCell(cell, map, minDistanceFromBuildings)) continue;
                
                float score = CalculateSpawnLocationScore(cell, map);
                if (score > 0.3f) // Minimum threshold
                {
                    potentialSpots.Add((cell, score));
                }
            }
            
            if (!potentialSpots.Any())
            {
                // Fallback to basic spawn location
                return FindFallbackSpawnLocation(map);
            }
            
            // Weight by score and select
            return potentialSpots.RandomElementByWeight(spot => spot.score).cell;
        }
        
        private static bool IsValidSpawnCell(IntVec3 cell, Map map, float minDistance)
        {
            if (!cell.Standable(map) || cell.Fogged(map)) return false;
            if (!map.reachability.CanReachColony(cell)) return false;
            if (cell.GetEdifice(map) != null) return false;
            if (cell.GetThingList(map).Any(t => t.def.category == ThingCategory.Pawn)) return false;
            
            // Check distance from existing buildings
            var nearbyBuildings = GenRadial.RadialDistinctThingsAround(cell, map, minDistance, true)
                .Where(t => t is Building && !(t is Plant));
            
            return !nearbyBuildings.Any();
        }
        
        private static float CalculateSpawnLocationScore(IntVec3 cell, Map map)
        {
            float score = 0.5f; // Base score
            
            // Prefer natural rock formations (caves, mountains)
            var nearbyRock = GenRadial.RadialDistinctThingsAround(cell, map, 5f, true)
                .Where(t => t.def.building?.isNaturalRock == true);
            score += nearbyRock.Count() * 0.15f;
            
            // Prefer darker, less trafficked areas
            var room = cell.GetRoom(map);
            if (room?.PsychologicallyOutdoors == true)
                score += 0.2f;
            
            if (room?.Role == RoomRoleDefOf.None)
                score += 0.15f;
            
            // Prefer areas away from high-value buildings
            var nearbyImportantBuildings = GenRadial.RadialDistinctThingsAround(cell, map, 20f, true)
                .Where(t => t is Building b && (b.def.building?.ai_chillDestination == true || 
                                               b.def.defName.Contains("Bed") || 
                                               b.def.defName.Contains("Table")));
            score -= nearbyImportantBuildings.Count() * 0.1f;
            
            // Prefer map edges for mysterious arrival
            float distanceFromEdge = Mathf.Min(cell.x, cell.z, map.Size.x - cell.x, map.Size.z - cell.z);
            if (distanceFromEdge < 15f)
                score += 0.25f;
            
            // Avoid existing symbionts
            var nearbySymbionts = map.GetAllSymbionts()
                .Where(s => s.Position.DistanceTo(cell) < 25f);
            score -= nearbySymbionts.Count() * 0.8f;
            
            return Mathf.Clamp01(score);
        }
        
        private static IntVec3? FindFallbackSpawnLocation(Map map)
        {
            // Simple fallback: find any reachable outdoor cell
            if (CellFinder.TryFindRandomCellNear(map.Center, map, 
                Mathf.RoundToInt(map.Size.x * 0.3f),
                cell => cell.Standable(map) && !cell.Fogged(map) && map.reachability.CanReachColony(cell),
                out IntVec3 fallbackCell))
            {
                return fallbackCell;
            }
            
            return null;
        }
        
        // Research and progression utilities
        public static bool HasRequiredResearch(ResearchProjectDef research)
        {
            return Find.ResearchManager.IsFinished(research);
        }
        
        public static bool CanExtractXenogerm(Building_FleshSymbiont symbiont)
        {
            if (!Extensions.CanUseBiotechFeatures()) return false;
            if (!FleshSymbiontSettings.allowXenogermExtraction) return false;
            if (symbiont?.Spawned != true) return false;
            
            // Check research requirements
            if (FleshSymbiontSettings.requireResearchForExtraction && 
                !HasRequiredResearch(FleshSymbiontDefOf.SymbiontGenetics))
                return false;
                
            // Check symbiont condition
            if (symbiont.HitPoints < symbiont.MaxHitPoints * 0.4f)
                return false;
                
            return true;
        }
        
        // Ideology and ritual utilities
        public static bool CanPerformSymbiontRitual(Map map)
        {
            if (!Extensions.CanUseIdeologyFeatures()) return false;
            if (!FleshSymbiontSettings.enableRituals) return false;
            
            // Need at least one symbiont
            if (!map.GetAllSymbionts().Any()) return false;
            
            // Need at least 2 bonded colonists
            var bondedCount = map.GetBondedColonists().Count();
            return bondedCount >= 2;
        }
        
        // Debug utilities
        public static void DebugSpawnSymbiont(Map map, IntVec3? location = null)
        {
            var spawnLoc = location ?? FindSymbiontSpawnLocation(map) ?? map.Center;
            
            var symbiont = (Building_FleshSymbiont)ThingMaker.MakeThing(FleshSymbiontDefOf.FleshSymbiont);
            symbiont.SetFaction(Faction.OfPlayer);
            
            // Apply health multiplier
            if (FleshSymbiontSettings.symbiontHealthMultiplier != 1.0f)
            {
                symbiont.HitPoints = Mathf.RoundToInt(symbiont.MaxHitPoints * FleshSymbiontSettings.symbiontHealthMultiplier);
            }
            
            GenSpawn.Spawn(symbiont, spawnLoc, map);
            
            Messages.Message($"DEBUG: Spawned symbiont at {spawnLoc}", symbiont, MessageTypeDefOf.NeutralEvent);
        }
        
        public static void DebugBondPawn(Pawn pawn)
        {
            if (pawn?.Map == null) return;
            
            var symbiont = pawn.Map.GetNearestSymbiont(pawn.Position);
            if (symbiont == null)
            {
                DebugSpawnSymbiont(pawn.Map, pawn.Position);
                symbiont = pawn.Map.GetNearestSymbiont(pawn.Position);
            }
            
            if (symbiont != null)
            {
                TryBondPawnToSymbiont(pawn, symbiont, voluntary: true);
                Messages.Message($"DEBUG: Bonded {pawn.Name.ToStringShort} to symbiont", pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
        
        public static string GetModStatusReport()
        {
            var report = "Signal Lost Mod Status:\n";
            report += $"- Anomaly Active: {ModsConfig.AnomalyActive}\n";
            report += $"- Biotech Features: {Extensions.CanUseBiotechFeatures()}\n";
            report += $"- Ideology Features: {Extensions.CanUseIdeologyFeatures()}\n";
            report += $"- Royalty Features: {Extensions.CanUseRoyaltyFeatures()}\n";
            report += $"- Event Frequency: {FleshSymbiontSettings.eventFrequencyMultiplier:F1}x\n";
            report += $"- Horror Messages: {FleshSymbiontSettings.enableHorrorMessages}\n";
            report += $"- Atmospheric Effects: {FleshSymbiontSettings.enableAtmosphericEffects}\n";
            
            var currentMaps = Find.Maps?.Where(m => m.IsPlayerHome);
            if (currentMaps?.Any() == true)
            {
                foreach (var map in currentMaps)
                {
                    int symbiontCount = map.GetSymbiontCount();
                    int bondedCount = map.GetBondedColonists().Count();
                    report += $"\n- {map.info.parent.Label}: {symbiontCount} symbionts, {bondedCount} bonded colonists";
                }
            }
            
            return report;
        }
    }
}
