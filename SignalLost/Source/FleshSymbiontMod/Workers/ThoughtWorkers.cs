using System.Linq;
using RimWorld;
using Verse;

namespace FleshSymbiontMod
{
    // Thought worker for checking if flesh symbiont exists
    public class ThoughtWorker_Precept_HasFleshSymbiont : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.Map == null) return false;
            
            var symbionts = p.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>();
            return symbionts.Any();
        }
    }
    
    // Thought worker for checking if bonded colonists are nearby
    public class ThoughtWorker_Precept_BondedColonist : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.Map == null || p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond)) 
                return false;
            
            var bondedNearby = p.Map.mapPawns.FreeColonistsSpawned
                .Where(pawn => pawn != p && 
                              pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) &&
                              pawn.Position.DistanceTo(p.Position) < 12f)
                .Any();
                
            return bondedNearby;
        }
    }
    
    // Thought worker for royal reactions to symbionts
    public class ThoughtWorker_RoyalSymbiontHorror : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!FleshSymbiontSettings.enableRoyaltyFeatures || !FleshSymbiontSettings.royalsHateSymbionts)
                return false;
                
            if (p.Map == null || !HasRoyalTitle(p))
                return false;
            
            var symbionts = p.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>();
            return symbionts.Any();
        }
        
        private bool HasRoyalTitle(Pawn p)
        {
            if (!ModsConfig.RoyaltyActive) return false;
            
            // Check if pawn has any royal title
            return p.royalty?.AllTitlesForReading?.Any() ?? false;
        }
    }
    
    public class ThoughtWorker_RoyalBondedColonist : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!FleshSymbiontSettings.enableRoyaltyFeatures || !FleshSymbiontSettings.royalsHateSymbionts)
                return false;
                
            if (p.Map == null || !HasRoyalTitle(p) || 
                p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
                return false;
            
            var bondedNearby = p.Map.mapPawns.FreeColonistsSpawned
                .Where(pawn => pawn != p && 
                              pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond) &&
                              pawn.Position.DistanceTo(p.Position) < 15f)
                .Any();
                
            return bondedNearby;
        }
        
        private bool HasRoyalTitle(Pawn p)
        {
            if (!ModsConfig.RoyaltyActive) return false;
            
            // Check if pawn has any royal title
            return p.royalty?.AllTitlesForReading?.Any() ?? false;
        }
    }
    
    // Thought worker for being near symbiont (general unease)
    public class ThoughtWorker_IsNearFleshSymbiont : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.Map == null || p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
                return false;
            
            // Check for nearby symbionts
            var nearbySymbionts = p.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>()
                .Where(s => s.Position.DistanceTo(p.Position) < 8f);
            
            if (!nearbySymbionts.Any())
                return false;
            
            // Stronger negative reaction if multiple symbionts nearby
            int symbiontCount = nearbySymbionts.Count();
            if (symbiontCount >= 3)
                return ThoughtState.ActiveAtStage(2); // Overwhelming dread
            else if (symbiontCount >= 2)
                return ThoughtState.ActiveAtStage(1); // Strong unease
            else
                return ThoughtState.ActiveAtStage(0); // Basic unease
        }
    }
    
    // Thought worker for ideology-based symbiont reverence
    public class ThoughtWorker_SymbiontReverence : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!FleshSymbiontSettings.enableIdeologyFeatures || !ModsConfig.IdeologyActive)
                return false;
                
            if (p.Map == null || p.Ideo == null)
                return false;
            
            // Check if pawn's ideology reveres flesh symbiosis
            bool reveresSymbionts = p.Ideo.PreceptsListForReading
                .Any(precept => precept.def.defName == "FleshSymbiosis");
            
            if (!reveresSymbionts)
                return false;
            
            // Check for nearby symbionts
            var nearbySymbionts = p.Map.listerBuildings.AllBuildingsColonistOfClass<Building_FleshSymbiont>()
                .Where(s => s.Position.DistanceTo(p.Position) < 12f);
            
            if (!nearbySymbionts.Any())
                return false;
            
            // Positive mood based on symbiont count and bonding status
            int symbiontCount = nearbySymbionts.Count();
            bool isBonded = p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond);
            
            if (isBonded && symbiontCount >= 2)
                return ThoughtState.ActiveAtStage(3); // Transcendent bliss
            else if (isBonded)
                return ThoughtState.ActiveAtStage(2); // Symbiotic harmony
            else if (symbiontCount >= 2)
                return ThoughtState.ActiveAtStage(1); // Sacred presence
            else
                return ThoughtState.ActiveAtStage(0); // Blessed sight
        }
    }
    
    // Thought worker for hybrid genetic pride/shame
    public class ThoughtWorker_HybridIdentity : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!FleshSymbiontSettings.enableBiotechFeatures || !ModsConfig.BiotechActive)
                return false;
            
            // Check if pawn is a symbiont hybrid
            if (p.genes?.Xenotype?.defName != "SymbiontHybrid")
                return false;
            
            // Check ideology attitude toward hybrids
            if (p.Ideo != null && ModsConfig.IdeologyActive)
            {
                bool reveresSymbionts = p.Ideo.PreceptsListForReading
                    .Any(precept => precept.def.defName == "FleshSymbiosis");
                
                if (reveresSymbionts)
                    return ThoughtState.ActiveAtStage(1); // Proud of evolution
                else
                    return ThoughtState.ActiveAtStage(0); // Ashamed of corruption
            }
            
            // Default neutral state for hybrids without strong ideology
            return false;
        }
    }
    
    // Thought worker for witnessing symbiont feeding
    public class ThoughtWorker_WitnessedFeeding : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.Map == null)
                return false;
            
            // This would be triggered by specific events rather than constant checking
            // Implementation would involve tracking recent feeding events and proximity
            // For now, returning false as this requires event-based triggering
            return false;
        }
    }
    
    // Thought worker for being in the same room as a symbiont
    public class ThoughtWorker_SymbiontInRoom : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.Map == null || p.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond))
                return false;
            
            var room = p.GetRoom();
            if (room == null)
                return false;
            
            // Check if there's a symbiont in the same room
            var symbiontInRoom = room.ContainedAndAdjacentThings
                .OfType<Building_FleshSymbiont>()
                .Any();
            
            if (!symbiontInRoom)
                return false;
            
            // Check room size for intensity
            if (room.CellCount < 50) // Small room
                return ThoughtState.ActiveAtStage(1); // Claustrophobic dread
            else
                return ThoughtState.ActiveAtStage(0); // Uncomfortable presence
        }
    }
    
    // Thought worker for social interactions with bonded colonists
    public class ThoughtWorker_SocialWithBonded : ThoughtWorker_Social
    {
        protected override float OpinionOffset(Pawn pawn, Pawn other)
        {
            if (!FleshSymbiontSettings.enableHorrorMessages)
                return 0f;
            
            bool pawnBonded = pawn.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond);
            bool otherBonded = other.health.hediffSet.HasHediff(FleshSymbiontDefOf.SymbiontBond);
            
            // Bonded pawns like each other more
            if (pawnBonded && otherBonded)
                return 15f; // Symbiotic kinship
            
            // Non-bonded pawns are disturbed by bonded ones
            if (!pawnBonded && otherBonded)
                return -10f; // Uncanny valley effect
            
            // Bonded pawns see non-bonded as inferior
            if (pawnBonded && !otherBonded)
                return -5f; // Evolutionary superiority
            
            return 0f;
        }
    }
}