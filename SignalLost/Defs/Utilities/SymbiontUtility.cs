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
            if (target == null || symbiont == null) return false;
            if (!target.IsValidSymbiontTarget()) return false;
            
            // Check resistance
            float resistanceChance = target.GetSymbiontCompulsionResistance();
            if (FleshSymbiontSettings.allowCompulsionResistance && Rand.Chance(resistanceChance))
            {
                if (FleshSymbiontSettings.enableHorrorMessages)
                {
                    string casterText = caster != null ? $" {caster.Name.ToStringShort()}'s" : "";
                    Messages.Message($"{target.Name.ToStringShort} resists{casterText} psychic compulsion through sheer willpower!", 
                        target, MessageTypeDefOf.PositiveEvent);
                }
                return false;
            }
            
            // Apply compulsion
            bool success = target.mindState.mentalStateHandler.TryStartMentalState(
                FleshSymbiontDefOf.SymbiontCompulsion,
                "Compelled by flesh symbiont",
                true,
                causedByMood: false);
                
            if (success && FleshSymbiontSettings.enableHorrorMessages)
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
            
            return success;
        }
        
        public static void CreateBondingEffects(Pawn pawn, Building_FleshSymbiont symbiont)
        {
            if (!FleshSymbiontSettings.enableAtmosphericEffects) return;
            
            // Visual effects around both pawn and symbiont
            pawn.Position.CreateSymbiontAtmosphere(pawn.Map, 1.5f);
            symbiont.Position.CreateSymbiontAtmosphere(symbiont.Map, 1.0f);
            
            // Energy connection effect
            var connectionPoint = Vector3.Lerp(pawn.DrawPos, symbiont.DrawPos, 0.5f);
            for (int i = 0; i < 8; i++)
            {
                MoteMaker.ThrowMicroSparks(connectionPoint + Gen.RandomHorizontalVector(1f), pawn.Map);
            }
            
            // Dramatic lighting
            MoteMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.2f);
            MoteMaker.ThrowHeatGlow(symbiont.Position, symbiont.Map, 1.5f);
        }
        
        // Bonding and unbonding utilities
        public static bool TryBondPawnToSymbiont(Pawn pawn, Building_FleshSymbiont symbiont, bool voluntary = false)
        {
            if (pawn == null || symbiont == null) return false;
            if (pawn.IsSymbiontBonded()) return false;
            
            // Create the bond hediff
            var bondHediff = HediffMaker.MakeHediff(FleshSymbiontDefOf.SymbiontBond, pawn);
            pawn.health.AddHediff(bondHediff);
            
            // Notify the symbiont
            symbiont.OnPawnBonded(pawn);
            
            // Create effects
            CreateBondingEffects(pawn, symbiont);
            
            // Send appropriate message based on whether it was voluntary
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                string message = voluntary ? 
                    GetVoluntaryBondingMessage(pawn) : 
                    GetInvoluntaryBondingMessage(pawn);
                    
                if (!string.IsNullOrEmpty(message))
                {
                    Messages.Message(message, pawn, MessageTypeDefOf.NegativeEvent);
                }
            }
            
            return true;
        }
        
        public static bool TryRemoveBond(Pawn pawn, bool forced = false)
        {
            if (pawn == null || !pawn.IsSymbiontBonded()) return false;
            
            var bondHediff = pawn.health.hediffSet.GetFirstHediffOfDef(FleshSymbiontDefOf.SymbiontBond);
            if (bondHediff == null) return false;
            
            // Removal effects
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                pawn.CreatePsychicEffect(1.5f);
                for (int i = 0; i < 10; i++)
                {
                    MoteMaker.ThrowDustPuff(pawn.DrawPos + Gen.RandomHorizontalVector(2f), pawn.Map, 1.8f);
                }
            }
            
            // Remove the hediff
            pawn.health.RemoveHediff(bondHediff);
            
            // Trauma effects for forced removal
            if (forced)
            {
                // Add temporary debuff
                var traumaHediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                traumaHediff.Severity = 0.8f;
                pawn.health.AddHediff(traumaHediff);
                
                // Chance of permanent psychological damage
                if (Rand.Chance(0.15f))
                {
                    var brainInjury = HediffMaker.MakeHediff(HediffDefOf.TBI, pawn, pawn.health.hediffSet.GetBrain());
                    brainInjury.Severity = Rand.Range(0.1f, 0.3f);
                    pawn.health.AddHediff(brainInjury);
                }
            }
            
            return true;
        }
        
        // Message generation utilities
        private static string GetVoluntaryBondingMessage(Pawn pawn)
        {
            return Extensions.GetHorrorIntensityMessage(
                $"{pawn.Name.ToStringShort} has willingly bonded with the symbiont.",
                $"{pawn.Name.ToStringShort} gasps as the symbiont's tendrils merge with their nervous system. Their choice was made freely, but the price is permanent.",
                $"{pawn.Name.ToStringShort} extends their hand to the pulsing mass with reverent determination. As alien flesh pierces their skin, they smile with transcendent joy even as their humanity begins to fade. The symbiont purrs with satisfaction at this willing offering."
            );
        }
        
        private static string GetInvoluntaryBondingMessage(Pawn pawn)
        {
            return Extensions.GetHorrorIntensityMessage(
                $"{pawn.Name.ToStringShort} has been bonded to the symbiont.",
                $"{pawn.Name.ToStringShort} screams as the symbiont's tendrils burrow into their spine. Their struggles are futile against the alien hunger.",
                $"{pawn.Name.ToStringShort} writhes in agony as the symbiont's biomechanical appendages pierce their flesh with surgical precision. Bone cracks, blood flows, and something inhuman begins to grow within them. When the screaming stops, what remains is no longer entirely human."
            );
        }
        
        // Spawning and discovery utilities
        public static IntVec3? FindSymbiontSpawnLocation(Map map, float minDistanceFromBuildings = 10f)
        {
            // Try to find atmospheric spawn locations
            var candidates = new List<IntVec3>();
            
            // Check entire map for suitable locations
            foreach (var cell in map.AllCells)
            {
                if (!IsValidSpawnCell(cell, map, minDistanceFromBuildings)) continue;
                
                // Prefer locations with atmosphere
                float score = CalculateSpawnLocationScore(cell, map);
                if (score > 0.5f)
                {
                    candidates.Add(cell);
                }
            }
            
            if (candidates.Any())
            {
                // Weight by score and pick randomly
                return candidates.RandomElementByWeight(cell => CalculateSpawnLocationScore(cell, map));
            }
            
            // Fallback to any valid location
            if (CellFinder.TryFindRandomCellNear(map.Center, map, 
                Mathf.RoundToInt(map.Size.x * 0.4f),
                cell => IsValidSpawnCell(cell, map, minDistanceFromBuildings),
                out IntVec3 fallbackCell))
            {
                return fallbackCell;
            }
            
            return null;
        }
        
        private static bool IsValidSpawnCell(IntVec3 cell, Map map, float minDistance)
        {
            if (!cell.Standable(map) || cell.Fogged(map)) return false;
            if (!map.reachability.CanReachColony(cell)) return false;
            if (cell.GetEdifice(map) != null) return false;
            
            // Check distance from existing buildings
            var nearbyBuildings = GenRadial.RadialDistinctThingsAround(cell, map, minDistance, true)
                .Where(t => t is Building && !(t is Plant));
            
            return !nearbyBuildings.Any();
        }
        
        private static float CalculateSpawnLocationScore(IntVec3 cell, Map map)
        {
            float score = 0.5f; // Base score
            
            // Prefer natural rock formations
            var nearbyRock = GenRadial.RadialDistinctThingsAround(cell, map, 5f, true)
                .Where(t => t.def.building?.isNaturalRock == true);
            score += nearbyRock.Count() * 0.1f;
            
            // Prefer darker areas (less light)
            var room = cell.GetRoom(map);
            if (room?.PsychologicallyOutdoors == true)
                score += 0.2f;
            
            // Avoid high-traffic areas
            if (cell.GetRoom(map)?.Role == RoomRoleDefOf.None)
                score += 0.1f;
            
            // Prefer edges of the map for mysterious arrival
            float distanceFromEdge = Mathf.Min(
                cell.x, 
                cell.z, 
                map.Size.x - cell.x, 
                map.Size.z - cell.z
            );
            if (distanceFromEdge < 10f)
                score += 0.3f;
            
            return Mathf.Clamp01(score);
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
            if (symbiont == null) return false;
            
            // Check research requirements
            if (FleshSymbiontSettings.requireResearchForExtraction && 
                !HasRequiredResearch(FleshSymbiontDefOf.SymbiontGenetics))
                return false;
                
            // Check symbiont condition
            if (symbiont.HitPoints < symbiont.MaxHitPoints * 0.3f)
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
        
        // Royal and noble utilities
        public static bool DoesRoyaltyRejectSymbiont(Pawn royal)
        {
            if (!Extensions.CanUseRoyaltyFeatures()) return false;
            if (!FleshSymbiontSettings.royalsHateSymbionts) return false;
            if (!royal.HasRoyalTitle()) return false;
            
            return true;
        }
        
        // Debug and development utilities
        public static void DebugSpawnSymbiont(Map map, IntVec3? location = null)
        {
            var spawnLoc = location ?? FindSymbiontSpawnLocation(map) ?? map.Center;
            
            var symbiont = (Building_FleshSymbiont)ThingMaker.MakeThing(FleshSymbiontDefOf.FleshSymbiont);
            symbiont.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(symbiont, spawnLoc, map);
            
            Messages.Message($"DEBUG: Spawned symbiont at {spawnLoc}", symbiont, MessageTypeDefOf.NeutralEvent);
        }
        
        public static void DebugBondPawn(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null) return;
            
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
            
            return report;
        }
    }
}