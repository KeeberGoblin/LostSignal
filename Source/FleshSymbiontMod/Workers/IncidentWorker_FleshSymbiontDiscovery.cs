using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Custom incident worker that respects settings
    public class IncidentWorker_FleshSymbiontDiscovery : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var map = (Map)parms.target;
            
            // Check if we should limit symbionts per map
            if (FleshSymbiontSettings.limitSymbiontsPerMap)
            {
                var existingSymbionts = map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>().Count();
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
            
            return base.CanFireNowSub(parms);
        }
        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            
            // Find a suitable spawn location
            IntVec3 spawnSpot;
            if (!TryFindSpawnSpot(map, out spawnSpot))
            {
                return false;
            }
            
            // Spawn the symbiont
            var symbiont = (Building_FleshSymbiont)ThingMaker.MakeThing(FleshSymbiontDefOf.FleshSymbiont);
            symbiont.SetFaction(Faction.OfPlayer);
            
            // Apply health multiplier from settings
            if (FleshSymbiontSettings.symbiontHealthMultiplier != 1.0f)
            {
                symbiont.HitPoints = Mathf.RoundToInt(symbiont.MaxHitPoints * FleshSymbiontSettings.symbiontHealthMultiplier);
            }
            
            GenSpawn.Spawn(symbiont, spawnSpot, map);
            
            // Create atmospheric effects around the spawn
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                CreateSpawnEffects(spawnSpot, map);
            }
            
            // Send letter with appropriate intensity
            string letterText = GetDiscoveryLetterText();
            SendStandardLetter(def.letterLabel, letterText, def.letterDef, parms, symbiont);
            
            return true;
        }
        
        private bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
        {
            // Try to find a good spawn location
            // Prefer areas that are:
            // 1. Not too close to existing buildings
            // 2. Accessible to colonists
            // 3. Not in forbidden areas
            // 4. Have some cover/natural feel
            
            // First try: Find a spot near natural rock or in a cave
            if (CellFinder.TryFindRandomCellNear(
                map.Center, map, Mathf.RoundToInt(map.Size.x * 0.4f),
                cell => IsGoodSymbiontSpawnCell(cell, map) && HasNearbyRock(cell, map),
                out spawnSpot))
            {
                return true;
            }
            
            // Second try: Any accessible outdoor location
            if (CellFinder.TryFindRandomCellNear(
                map.Center, map, Mathf.RoundToInt(map.Size.x * 0.3f),
                cell => IsGoodSymbiontSpawnCell(cell, map),
                out spawnSpot))
            {
                return true;
            }
            
            // Last resort: Edge spawn
            if (CellFinder.TryFindRandomEdgeCellWith(
                cell => IsGoodSymbiontSpawnCell(cell, map),
                map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
            {
                return true;
            }
            
            spawnSpot = IntVec3.Invalid;
            return false;
        }
        
        private bool IsGoodSymbiontSpawnCell(IntVec3 cell, Map map)
        {
            // Basic requirements
            if (!cell.Standable(map) || cell.Fogged(map))
                return false;
            
            // Must be reachable by colonists
            if (!map.reachability.CanReachColony(cell))
                return false;
            
            // Don't spawn too close to existing buildings
            if (cell.GetEdifice(map) != null)
                return false;
            
            // Don't spawn in rooms if possible
            var room = cell.GetRoom(map);
            if (room != null && room.IsHall)
                return false;
            
            // Avoid spawn points too close to other symbionts
            var nearbySymbionts = map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>();
            foreach (var symbiont in nearbySymbionts)
            {
                if (cell.DistanceTo(symbiont.Position) < 20)
                    return false;
            }
            
            return true;
        }
        
        private bool HasNearbyRock(IntVec3 cell, Map map)
        {
            // Check for natural rock walls within 3 tiles
            for (int i = -3; i <= 3; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    var checkCell = cell + new IntVec3(i, 0, j);
                    if (checkCell.InBounds(map))
                    {
                        var edifice = checkCell.GetEdifice(map);
                        if (edifice != null && edifice.def.building.isNaturalRock)
                            return true;
                    }
                }
            }
            return false;
        }
        
        private void CreateSpawnEffects(IntVec3 position, Map map)
        {
            // Create dramatic spawn effects
            for (int i = 0; i < 12; i++)
            {
                var effectPos = position.ToVector3() + Gen.RandomHorizontalVector(3f);
                MoteMaker.ThrowDustPuff(effectPos, map, 1.5f);
            }
            
            // Heat glow effect
            MoteMaker.ThrowHeatGlow(position, map, 2f);
            
            // Micro sparks for alien energy
            for (int i = 0; i < 6; i++)
            {
                MoteMaker.ThrowMicroSparks(position.ToVector3() + Gen.RandomHorizontalVector(1f), map);
            }
        }
        
        private string GetDiscoveryLetterText()
        {
            if (!FleshSymbiontSettings.enableHorrorMessages)
                return def.letterText;
            
            if (FleshSymbiontSettings.messageFrequency == 3)
            {
                return "CLASSIFIED FIELD REPORT - PRIORITY ALPHA\n\n" +
                       "Excavation team has uncovered what can only be described as a biological anomaly of unknown origin. " +
                       "The artifact exhibits characteristics of both organic tissue and advanced biotechnology. " +
                       "Preliminary scans detect complex neural activity and what appears to be a form of directed intelligence.\n\n" +
                       "WARNING: Personnel report experiencing auditory hallucinations and compulsive behavior when in proximity to the specimen. " +
                       "Dr. Martinez described the entity as 'beautiful' and 'calling to her' before requiring restraint.\n\n" +
                       "Recommend immediate establishment of security perimeter. Do not allow unsupervised contact.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 2)
            {
                return "Your colonists have discovered a pulsing, organic mass buried in the ground. The ribbed, hand-like appendage seems to twitch with its own life force.\n\n" +
                       "Something about its biomechanical appearance fills your colonists with unease. The flesh appears to be waiting... hungry.\n\n" +
                       "One of your colonists feels inexplicably drawn to touch it.";
            }
            else if (FleshSymbiontSettings.messageFrequency == 1)
            {
                return "Your colonists have discovered a strange organic artifact. Some of them seem drawn to it.";
            }
            
            return def.letterText;
        }
        
        // Remove the problematic AdjustedChance override - use BaseChanceThisGame instead
        public override float BaseChanceThisGame => base.BaseChanceThisGame * FleshSymbiontSettings.eventFrequencyMultiplier;
    }
}