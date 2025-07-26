using RimWorld;
using Verse;

namespace FleshSymbiontMod
{
    // Static references to our defs for easy access in code
    [DefOf]
    public static class FleshSymbiontDefOf
    {
        // Mental States
        public static MentalStateDef SymbiontCompulsion;
        public static MentalStateDef SymbiontMadness;
        
        // Jobs
        public static JobDef CompelledToSymbiont;
        public static JobDef FeedSymbiont;
        public static JobDef TouchFleshSymbiont;
        
        // Hediffs
        public static HediffDef SymbiontBond;
        public static HediffDef SymbiontRegeneration;
        public static HediffDef SymbiontFury;
        public static HediffDef PsychicAmplifierSymbiont;
        
        // Things
        public static ThingDef FleshSymbiont;
        public static ThingDef XenogermSymbiont;
        
        // Incidents
        public static IncidentDef FleshSymbiontDiscovery;
        
        // Thoughts
        public static ThoughtDef NearFleshSymbiont;
        public static ThoughtDef SymbiontBonded;
        public static ThoughtDef FedSymbiont;
        public static ThoughtDef AttendedSymbiontCommunion;
        public static ThoughtDef SymbiontHarvested;
        public static ThoughtDef FleshSymbiontRevered;
        public static ThoughtDef FleshSymbiontAbhorred;
        public static ThoughtDef BondedColonistDisgust;
        public static ThoughtDef RoyaltySymbiontHorror;
        public static ThoughtDef RoyaltyBondedColonist;
        public static ThoughtDef MeditatedOnSymbiont;
        public static ThoughtDef SymbiontFeedingEuphoria;
        
        // Research
        public static ResearchProjectDef FleshSymbiontStudy;
        public static ResearchProjectDef SymbiontGenetics;
        
        // Recipes
        public static RecipeDef ExtractSymbiontXenogerm;
        
        // Abilities (Royalty)
        public static AbilityDef SymbiontCompel;
        public static AbilityDef SymbiontHeal;
        public static AbilityDef SymbiontRage;
        public static AbilityDef SymbiontTrackPrey;
        public static AbilityDef SymbiontDrainLife;
        public static AbilityDef SymbiontDevour;
        public static AbilityDef SymbiontNetworkLink;
        public static AbilityDef SymbiontPsychicScream;
        public static AbilityDef SymbiontShareSkill;
        public static AbilityDef SymbiontNetworkCommand;
        
        // Ability Groups
        public static AbilityGroupDef Symbiont;
        
        // Genes (Biotech)
        public static GeneDef SymbiontEnhancedStrength;
        public static GeneDef SymbiontPredatorVision;
        public static GeneDef SymbiontParasiticRegeneration;
        public static GeneDef SymbiontVoracity;
        public static GeneDef SymbiontNeuralChaos;
        public static GeneDef SymbiontAdaptiveImmunity;
        public static GeneDef SymbiontHiveMind;
        
        // Xenotypes (Biotech)
        public static XenotypeDef SymbiontHybrid;
        
        // Precepts (Ideology)
        public static PreceptDef FleshSymbiosis;
        public static PreceptDef FleshSymbiosisAbhorrent;
        
        // Issues (Ideology)
        public static IssueDef FleshSymbiontAttitude;
        
        // Memes (Ideology)
        public static MemeDef FleshTranscendence;
        
        // Rituals (Ideology)
        public static RitualPatternDef SymbiontCommunion;
        public static RitualBehaviorDef SymbiontCommunion;
        
        // Duties
        public static DutyDef SymbiontCommunion_Priest;
        
        // Meditation Focus (Royalty)
        public static MeditationFocusDef SymbiontFocus;
        
        // Room Requirements (Royalty)
        public static RoomRequirementDef NoSymbiontPresence;
        
        // Quest Scripts (Royalty)
        public static QuestScriptDef RoyalSymbiontPurge;
        
        // Periodic Hediffs
        public static HediffDef SymbiontMutation;
        
        // Sound Effects
        public static SoundDef SymbiontPulse;
        public static SoundDef SymbiontWhisper;
        public static SoundDef CompulsionStatic;
        public static SoundDef BondingScream;
        public static SoundDef SymbiontFeed;
        public static SoundDef PsycastActivate;
        public static SoundDef SymbiontHeal;
        public static SoundDef SymbiontRage;
        public static SoundDef RitualChant;
        
        static FleshSymbiontDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FleshSymbiontDefOf));
        }
    }
}