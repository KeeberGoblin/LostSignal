using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace FleshSymbiontMod
{
    // Recipe worker for extracting xenogerm from symbiont
    public class RecipeWorker_ExtractSymbiontXenogerm : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!FleshSymbiontSettings.enableBiotechFeatures || !FleshSymbiontSettings.allowXenogermExtraction)
                return false;
                
            if (FleshSymbiontSettings.requireResearchForExtraction && 
                !Find.ResearchManager.IsFinished(FleshSymbiontDefOf.SymbiontGenetics))
                return false;
            
            // Only available on flesh symbionts
            if (!(thing is Building_FleshSymbiont symbiont))
                return false;
            
            // Symbiont must be alive (not too damaged)
            if (symbiont.HitPoints < symbiont.MaxHitPoints * 0.3f)
                return false;
                
            return base.AvailableOnNow(thing, part);
        }
        
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            base.ConsumeIngredient(ingredient, recipe, map);
        }
        
        public override bool DoRecipeWork(RecipeDef recipe, Thing thing, List<Thing> ingredients, 
            Thing dominantIngredient, IBillGiver billGiver)
        {
            var symbiont = thing as Building_FleshSymbiont;
            if (symbiont == null) return false;
            
            // Find the doctor performing the procedure
            var doctor = ingredients.FirstOrDefault()?.ParentHolder as Pawn;
            if (doctor == null)
            {
                // Try to find nearby doctor
                doctor = symbiont.Map.mapPawns.FreeColonistsSpawned
                    .Where(p => p.Position.DistanceTo(symbiont.Position) < 5f && 
                               p.skills.GetSkill(SkillDefOf.Medicine).Level >= 8)
                    .OrderByDescending(p => p.skills.GetSkill(SkillDefOf.Medicine).Level)
                    .FirstOrDefault();
            }
            
            // Calculate success chance based on doctor skill and symbiont condition
            float successChance = CalculateSuccessChance(doctor, symbiont);
            
            // Pre-extraction warnings and reactions
            ShowPreExtractionWarnings(symbiont, doctor);
            
            // Perform the extraction
            bool success = Rand.Chance(successChance);
            
            if (success)
            {
                PerformSuccessfulExtraction(symbiont, doctor);
            }
            else
            {
                PerformFailedExtraction(symbiont, doctor);
            }
            
            // Always trigger symbiont response (damage and bonded pawn reactions)
            symbiont.OnXenogermExtracted();
            
            return success;
        }
        
        private float CalculateSuccessChance(Pawn doctor, Building_FleshSymbiont symbiont)
        {
            float baseChance = 0.7f;
            
            if (doctor != null)
            {
                // Doctor skill bonus
                int medicineSkill = doctor.skills.GetSkill(SkillDefOf.Medicine).Level;
                int intellectSkill = doctor.skills.GetSkill(SkillDefOf.Intellectual).Level;
                
                float skillBonus = (medicineSkill - 8) * 0.05f + (intellectSkill - 6) * 0.03f;
                baseChance += skillBonus;
                
                // Trait bonuses/penalties
                if (doctor.story.traits.HasTrait(TraitDefOf.Careful))
                    baseChance += 0.15f;
                if (doctor.story.traits.HasTrait(TraitDefOf.Psychopath))
                    baseChance += 0.1f; // Less disturbed by the procedure
                if (doctor.story.traits.HasTrait(TraitDefOf.Squeamish))
                    baseChance -= 0.2f;
            }
            
            // Symbiont condition affects difficulty
            float healthRatio = (float)symbiont.HitPoints / symbiont.MaxHitPoints;
            baseChance += (healthRatio - 0.5f) * 0.3f; // Healthier = easier
            
            // Settings modifier
            if (FleshSymbiontSettings.xenogermExtractionDamage > 0.5f)
                baseChance -= 0.1f; // More aggressive extraction = riskier
            
            return Mathf.Clamp(baseChance, 0.1f, 0.95f);
        }
        
        private void ShowPreExtractionWarnings(Building_FleshSymbiont symbiont, Pawn doctor)
        {
            if (!FleshSymbiontSettings.enableHorrorMessages)
                return;
            
            string warning = FleshSymbiontSettings.messageFrequency switch
            {
                3 => $"Dr. {doctor?.Name.ToStringShort ?? "Unknown"} approaches the flesh symbiont with surgical tools, " +
                     "their hands trembling as the alien entity seems to sense the impending violation. " +
                     "The symbiont's flesh pulses faster, and nearby bonded colonists begin to show signs of agitation. " +
                     "The air fills with the sound of wet, organic breathing as the procedure begins...",
                     
                2 => $"The surgical team prepares to extract genetic material from the flesh symbiont. " +
                     "The creature seems to sense their intent, its pulsing becoming more erratic.",
                     
                1 => "Beginning xenogerm extraction procedure on the flesh symbiont.",
                
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(warning))
            {
                Messages.Message(warning, symbiont, MessageTypeDefOf.NegativeEvent);
            }
        }
        
        private void PerformSuccessfulExtraction(Building_FleshSymbiont symbiont, Pawn doctor)
        {
            // Create the xenogerm
            var xenogerm = ThingMaker.MakeThing(FleshSymbiontDefOf.XenogermSymbiont);
            
            // Drop it near the symbiont
            GenPlace.TryPlaceThing(xenogerm, symbiont.Position, symbiont.Map, ThingPlaceMode.Near);
            
            // Success message
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                string message = FleshSymbiontSettings.messageFrequency switch
                {
                    3 => $"The extraction is successful, but at a terrible cost. The symbiont's agonized shriek echoes " +
                         "through the colony as Dr. {doctor?.Name.ToStringShort ?? "Unknown"} pulls free a writhing mass of " +
                         "alien genetic material. The xenogerm pulses with its own malevolent life, and the doctor's " +
                         "hands are stained with something that isn't quite blood...",
                         
                    2 => "The xenogerm extraction is successful, but the symbiont writhes in agony. " +
                         "The extracted genetic material glows with an unnatural light.",
                         
                    1 => "Xenogerm extraction successful.",
                    
                    _ => "Genetic material extracted."
                };
                
                Messages.Message(message, symbiont, MessageTypeDefOf.PositiveEvent);
            }
            
            // Doctor gains experience but potential trauma
            if (doctor != null)
            {
                doctor.skills.Learn(SkillDefOf.Medicine, 250f);
                doctor.skills.Learn(SkillDefOf.Intellectual, 150f);
                
                // Small chance of trauma for non-psychopath doctors
                if (!doctor.story.traits.HasTrait(TraitDefOf.Psychopath) && Rand.Chance(0.15f))
                {
                    var traumaThought = ThoughtMaker.MakeThought(ThoughtDef.Named("PerformedSymbiontSurgery"));
                    doctor.needs.mood.thoughts.memories.TryGainMemory(traumaThought);
                }
            }
            
            // Atmospheric effects
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                for (int i = 0; i < 8; i++)
                {
                    MoteMaker.ThrowDustPuff(symbiont.DrawPos + Gen.RandomHorizontalVector(2f), symbiont.Map, 1.5f);
                }
                MoteMaker.ThrowHeatGlow(symbiont.Position, symbiont.Map, 1.5f);
            }
        }
        
        private void PerformFailedExtraction(Building_FleshSymbiont symbiont, Pawn doctor)
        {
            // Failure message
            if (FleshSymbiontSettings.enableHorrorMessages)
            {
                string message = FleshSymbiontSettings.messageFrequency switch
                {
                    3 => $"The extraction fails catastrophically! The symbiont's defensive mechanisms activate, " +
                         "releasing a spray of corrosive alien fluid that burns through the surgical equipment. " +
                         $"Dr. {doctor?.Name.ToStringShort ?? "Unknown"} staggers back as the creature's flesh " +
                         "seals itself, leaving behind only the acrid smell of melted metal and the echo of inhuman screaming.",
                         
                    2 => "The xenogerm extraction fails! The symbiont's defensive systems activate, " +
                         "destroying the surgical equipment and injuring the medical team.",
                         
                    1 => "Xenogerm extraction failed.",
                    
                    _ => "Extraction procedure failed."
                };
                
                Messages.Message(message, symbiont, MessageTypeDefOf.NegativeEvent);
            }
            
            // Doctor may be injured
            if (doctor != null && Rand.Chance(0.4f))
            {
                var injury = HediffMaker.MakeHediff(HediffDefOf.Burn, doctor);
                injury.Severity = Rand.Range(0.1f, 0.3f);
                
                // Random body part (hands/arms most likely)
                var bodyParts = new[] { 
                    doctor.RaceProps.body.AllParts.FirstOrDefault(p => p.def.defName == "Hand"),
                    doctor.RaceProps.body.AllParts.FirstOrDefault(p => p.def.defName == "Arm")
                }.Where(p => p != null);
                
                var targetPart = bodyParts.RandomElementWithFallback();
                if (targetPart != null)
                {
                    doctor.health.AddHediff(injury, targetPart);
                }
            }
            
            // Destroy some medical supplies
            var map = symbiont.Map;
            var nearbyMedicine = GenRadial.RadialDistinctThingsAround(symbiont.Position, map, 3f, true)
                .Where(t => t.def.IsMedicine)
                .Take(Rand.Range(1, 3));
            
            foreach (var medicine in nearbyMedicine)
            {
                medicine.Destroy();
            }
            
            // More dramatic atmospheric effects for failure
            if (FleshSymbiontSettings.enableAtmosphericEffects)
            {
                for (int i = 0; i < 15; i++)
                {
                    MoteMaker.ThrowMicroSparks(symbiont.DrawPos + Gen.RandomHorizontalVector(3f), symbiont.Map);
                }
                MoteMaker.ThrowSmoke(symbiont.DrawPos, symbiont.Map, 2f);
            }
        }
        
        public override string GetLabel(RecipeDef recipe)
        {
            string baseLabel = base.GetLabel(recipe);
            
            if (FleshSymbiontSettings.requireResearchForExtraction && 
                !Find.ResearchManager.IsFinished(FleshSymbiontDefOf.SymbiontGenetics))
            {
                return baseLabel + " (requires symbiont genetics research)";
            }
            
            return baseLabel;
        }
    }
}