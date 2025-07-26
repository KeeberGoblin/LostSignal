using UnityEngine;
using Verse;
using RimWorld;

namespace FleshSymbiontMod
{
    public class FleshSymbiontSettings : ModSettings
    {
        // Event frequency settings
        public static float eventFrequencyMultiplier = 1.0f;
        public static int minDaysBetweenEvents = 60;
        public static int maxDaysBetweenEvents = 120;
        
        // Symbiont behavior settings
        public static float compulsionFrequencyMultiplier = 1.0f;
        public static float hungerGrowthMultiplier = 1.0f;
        public static int maxCompulsionRange = 25;
        public static bool allowCompulsionResistance = true;
        public static float compulsionResistanceChance = 0.15f;
        
        // Bonding and effects settings
        public static bool allowVoluntaryBonding = true;
        public static float bondingBenefitsMultiplier = 1.0f;
        public static float bondingPenaltiesMultiplier = 1.0f;
        public static bool enableMadnessBreaks = true;
        public static float madnessFrequencyMultiplier = 1.0f;
        
        // Symbiont durability settings
        public static float symbiontHealthMultiplier = 1.0f;
        public static bool allowSymbiontDestruction = true;
        public static bool bondedDefendsSymbiont = true;
        public static float defenseAggressionLevel = 1.0f;
        
        // Ideology integration settings
        public static bool enableIdeologyFeatures = true;
        public static bool enableRituals = true;
        public static float ritualBenefitsMultiplier = 1.0f;
        public static bool enableMemePrecepts = true;
        
        // Biotech integration settings
        public static bool enableBiotechFeatures = true;
        public static bool allowXenogermExtraction = true;
        public static bool allowSymbiontHybridBirth = true;
        public static float xenogermExtractionDamage = 0.3f;
        public static bool requireResearchForExtraction = true;
        public static float hybridTransformationChance = 0.1f;
        
        // Royalty integration settings
        public static bool enableRoyaltyFeatures = true;
        public static bool enableSymbiontPsycasts = true;
        public static bool royalsHateSymbionts = true;
        public static bool allowSymbiontMeditation = true;
        public static bool enableRoyalDecrees = true;
        public static float psycastPowerMultiplier = 1.0f;
        
        // Horror/atmosphere settings
        public static bool enableHorrorMessages = true;
        public static bool enableAtmosphericEffects = true;
        public static bool enableBodyHorrorDescriptions = true;
        public static int messageFrequency = 2; // 0=None, 1=Minimal, 2=Normal, 3=Frequent
        
        // Balancing options
        public static bool enableResearchRequirement = false;
        public static bool limitSymbiontsPerMap = true;
        public static int maxSymbiontsPerMap = 2;
        public static bool enableSymbiontDecay = false;
        public static float symbiontDecayRate = 0.1f;
        
        public override void ExposeData()
        {
            base.ExposeData();
            
            // Event frequency
            Scribe_Values.Look(ref eventFrequencyMultiplier, "eventFrequencyMultiplier", 1.0f);
            Scribe_Values.Look(ref minDaysBetweenEvents, "minDaysBetweenEvents", 60);
            Scribe_Values.Look(ref maxDaysBetweenEvents, "maxDaysBetweenEvents", 120);
            
            // Symbiont behavior
            Scribe_Values.Look(ref compulsionFrequencyMultiplier, "compulsionFrequencyMultiplier", 1.0f);
            Scribe_Values.Look(ref hungerGrowthMultiplier, "hungerGrowthMultiplier", 1.0f);
            Scribe_Values.Look(ref maxCompulsionRange, "maxCompulsionRange", 25);
            Scribe_Values.Look(ref allowCompulsionResistance, "allowCompulsionResistance", true);
            Scribe_Values.Look(ref compulsionResistanceChance, "compulsionResistanceChance", 0.15f);
            
            // Bonding effects
            Scribe_Values.Look(ref allowVoluntaryBonding, "allowVoluntaryBonding", true);
            Scribe_Values.Look(ref bondingBenefitsMultiplier, "bondingBenefitsMultiplier", 1.0f);
            Scribe_Values.Look(ref bondingPenaltiesMultiplier, "bondingPenaltiesMultiplier", 1.0f);
            Scribe_Values.Look(ref enableMadnessBreaks, "enableMadnessBreaks", true);
            Scribe_Values.Look(ref madnessFrequencyMultiplier, "madnessFrequencyMultiplier", 1.0f);
            
            // Symbiont durability
            Scribe_Values.Look(ref symbiontHealthMultiplier, "symbiontHealthMultiplier", 1.0f);
            Scribe_Values.Look(ref allowSymbiontDestruction, "allowSymbiontDestruction", true);
            Scribe_Values.Look(ref bondedDefendsSymbiont, "bondedDefendsSymbiont", true);
            Scribe_Values.Look(ref defenseAggressionLevel, "defenseAggressionLevel", 1.0f);
            
            // Ideology integration
            Scribe_Values.Look(ref enableIdeologyFeatures, "enableIdeologyFeatures", true);
            Scribe_Values.Look(ref enableRituals, "enableRituals", true);
            Scribe_Values.Look(ref ritualBenefitsMultiplier, "ritualBenefitsMultiplier", 1.0f);
            Scribe_Values.Look(ref enableMemePrecepts, "enableMemePrecepts", true);
            
            // Biotech integration
            Scribe_Values.Look(ref enableBiotechFeatures, "enableBiotechFeatures", true);
            Scribe_Values.Look(ref allowXenogermExtraction, "allowXenogermExtraction", true);
            Scribe_Values.Look(ref allowSymbiontHybridBirth, "allowSymbiontHybridBirth", true);
            Scribe_Values.Look(ref xenogermExtractionDamage, "xenogermExtractionDamage", 0.3f);
            Scribe_Values.Look(ref requireResearchForExtraction, "requireResearchForExtraction", true);
            Scribe_Values.Look(ref hybridTransformationChance, "hybridTransformationChance", 0.1f);
            
            // Royalty integration
            Scribe_Values.Look(ref enableRoyaltyFeatures, "enableRoyaltyFeatures", true);
            Scribe_Values.Look(ref enableSymbiontPsycasts, "enableSymbiontPsycasts", true);
            Scribe_Values.Look(ref royalsHateSymbionts, "royalsHateSymbionts", true);
            Scribe_Values.Look(ref allowSymbiontMeditation, "allowSymbiontMeditation", true);
            Scribe_Values.Look(ref enableRoyalDecrees, "enableRoyalDecrees", true);
            Scribe_Values.Look(ref psycastPowerMultiplier, "psycastPowerMultiplier", 1.0f);
            
            // Horror/atmosphere
            Scribe_Values.Look(ref enableHorrorMessages, "enableHorrorMessages", true);
            Scribe_Values.Look(ref enableAtmosphericEffects, "enableAtmosphericEffects", true);
            Scribe_Values.Look(ref enableBodyHorrorDescriptions, "enableBodyHorrorDescriptions", true);
            Scribe_Values.Look(ref messageFrequency, "messageFrequency", 2);
            
            // Balancing
            Scribe_Values.Look(ref enableResearchRequirement, "enableResearchRequirement", false);
            Scribe_Values.Look(ref limitSymbiontsPerMap, "limitSymbiontsPerMap", true);
            Scribe_Values.Look(ref maxSymbiontsPerMap, "maxSymbiontsPerMap", 2);
            Scribe_Values.Look(ref enableSymbiontDecay, "enableSymbiontDecay", false);
            Scribe_Values.Look(ref symbiontDecayRate, "symbiontDecayRate", 0.1f);
        }
    }
    
    public class FleshSymbiontMod : Mod
    {
        private FleshSymbiontSettings settings;
        private Vector2 scrollPosition = Vector2.zero;
        
        public FleshSymbiontMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<FleshSymbiontSettings>();
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            var contentRect = new Rect(0, 0, inRect.width - 20f, 1200f);
            var viewRect = new Rect(0, 0, inRect.width, inRect.height);
            
            Widgets.BeginScrollView(viewRect, ref scrollPosition, contentRect);
            
            var listing = new Listing_Standard();
            listing.Begin(contentRect);
            
            // Header
            listing.Label("Signal Lost Settings", -1, "Configure the horror experience to your liking");
            listing.Gap(12f);
            
            // Event Frequency Section
            listing.Label("Event Frequency", -1, "How often flesh symbiont events occur");
            FleshSymbiontSettings.eventFrequencyMultiplier = listing.SliderLabeled(
                $"Event frequency: {FleshSymbiontSettings.eventFrequencyMultiplier:F1}x", 
                FleshSymbiontSettings.eventFrequencyMultiplier, 0.1f, 3.0f);
                
            FleshSymbiontSettings.minDaysBetweenEvents = (int)listing.SliderLabeled(
                $"Minimum days between events: {FleshSymbiontSettings.minDaysBetweenEvents}", 
                FleshSymbiontSettings.minDaysBetweenEvents, 10, 200);
                
            FleshSymbiontSettings.maxDaysBetweenEvents = (int)listing.SliderLabeled(
                $"Maximum days between events: {FleshSymbiontSettings.maxDaysBetweenEvents}", 
                FleshSymbiontSettings.maxDaysBetweenEvents, 
                FleshSymbiontSettings.minDaysBetweenEvents + 10, 400);
            listing.Gap(12f);
            
            // Symbiont Behavior Section
            listing.Label("Symbiont Behavior", -1, "How aggressive and active symbionts are");
            FleshSymbiontSettings.compulsionFrequencyMultiplier = listing.SliderLabeled(
                $"Compulsion frequency: {FleshSymbiontSettings.compulsionFrequencyMultiplier:F1}x", 
                FleshSymbiontSettings.compulsionFrequencyMultiplier, 0.1f, 3.0f);
                
            FleshSymbiontSettings.hungerGrowthMultiplier = listing.SliderLabeled(
                $"Hunger growth rate: {FleshSymbiontSettings.hungerGrowthMultiplier:F1}x", 
                FleshSymbiontSettings.hungerGrowthMultiplier, 0.1f, 3.0f);
                
            FleshSymbiontSettings.maxCompulsionRange = (int)listing.SliderLabeled(
                $"Compulsion range: {FleshSymbiontSettings.maxCompulsionRange} tiles", 
                FleshSymbiontSettings.maxCompulsionRange, 5, 50);
                
            listing.CheckboxLabeled("Allow compulsion resistance", ref FleshSymbiontSettings.allowCompulsionResistance,
                "Colonists can sometimes resist compulsion based on their traits");
                
            if (FleshSymbiontSettings.allowCompulsionResistance)
            {
                FleshSymbiontSettings.compulsionResistanceChance = listing.SliderLabeled(
                    $"Resistance chance: {FleshSymbiontSettings.compulsionResistanceChance:P0}", 
                    FleshSymbiontSettings.compulsionResistanceChance, 0.0f, 0.5f);
            }
            listing.Gap(12f);
            
            // Bonding Effects Section
            listing.Label("Bonding Effects", -1, "Benefits and penalties of symbiont bonding");
            listing.CheckboxLabeled("Allow voluntary bonding", ref FleshSymbiontSettings.allowVoluntaryBonding,
                "Colonists can choose to touch the symbiont voluntarily");
                
            FleshSymbiontSettings.bondingBenefitsMultiplier = listing.SliderLabeled(
                $"Bonding benefits: {FleshSymbiontSettings.bondingBenefitsMultiplier:F1}x", 
                FleshSymbiontSettings.bondingBenefitsMultiplier, 0.1f, 3.0f);
                
            FleshSymbiontSettings.bondingPenaltiesMultiplier = listing.SliderLabeled(
                $"Bonding penalties: {FleshSymbiontSettings.bondingPenaltiesMultiplier:F1}x", 
                FleshSymbiontSettings.bondingPenaltiesMultiplier, 0.1f, 3.0f);
                
            listing.CheckboxLabeled("Enable madness mental breaks", ref FleshSymbiontSettings.enableMadnessBreaks,
                "Bonded colonists can go into symbiont madness");
                
            if (FleshSymbiontSettings.enableMadnessBreaks)
            {
                FleshSymbiontSettings.madnessFrequencyMultiplier = listing.SliderLabeled(
                    $"Madness frequency: {FleshSymbiontSettings.madnessFrequencyMultiplier:F1}x", 
                    FleshSymbiontSettings.madnessFrequencyMultiplier, 0.1f, 3.0f);
            }
            listing.Gap(12f);
            
            // Symbiont Durability Section
            listing.Label("Symbiont Durability", -1, "How tough symbionts are and how they react to threats");
            FleshSymbiontSettings.symbiontHealthMultiplier = listing.SliderLabeled(
                $"Symbiont health: {FleshSymbiontSettings.symbiontHealthMultiplier:F1}x", 
                FleshSymbiontSettings.symbiontHealthMultiplier, 0.1f, 5.0f);
                
            listing.CheckboxLabeled("Allow symbiont destruction", ref FleshSymbiontSettings.allowSymbiontDestruction,
                "Symbionts can be destroyed by damage");
                
            listing.CheckboxLabeled("Bonded colonists defend symbiont", ref FleshSymbiontSettings.bondedDefendsSymbiont,
                "Bonded colonists go berserk when their symbiont is threatened");
                
            if (FleshSymbiontSettings.bondedDefendsSymbiont)
            {
                FleshSymbiontSettings.defenseAggressionLevel = listing.SliderLabeled(
                    $"Defense aggression: {FleshSymbiontSettings.defenseAggressionLevel:F1}x", 
                    FleshSymbiontSettings.defenseAggressionLevel, 0.1f, 3.0f);
            }
            listing.Gap(12f);
            
            // Ideology Integration Section (only if Ideology is loaded)
            if (ModsConfig.IdeologyActive)
            {
                listing.Label("Ideology Integration", -1, "Features requiring Ideology DLC");
                listing.CheckboxLabeled("Enable ideology features", ref FleshSymbiontSettings.enableIdeologyFeatures,
                    "Enable memes, precepts, and ideological reactions");
                    
                if (FleshSymbiontSettings.enableIdeologyFeatures)
                {
                    listing.CheckboxLabeled("Enable rituals", ref FleshSymbiontSettings.enableRituals,
                        "Enable symbiont communion rituals");
                        
                    if (FleshSymbiontSettings.enableRituals)
                    {
                        FleshSymbiontSettings.ritualBenefitsMultiplier = listing.SliderLabeled(
                            $"Ritual benefits: {FleshSymbiontSettings.ritualBenefitsMultiplier:F1}x", 
                            FleshSymbiontSettings.ritualBenefitsMultiplier, 0.1f, 3.0f);
                    }
                    
                    listing.CheckboxLabeled("Enable meme precepts", ref FleshSymbiontSettings.enableMemePrecepts,
                        "Enable flesh transcendence meme and related precepts");
                }
                listing.Gap(12f);
            }
            
            // Biotech Integration Section (only if Biotech is loaded)
            if (ModsConfig.BiotechActive)
            {
                listing.Label("Biotech Integration", -1, "Features requiring Biotech DLC");
                listing.CheckboxLabeled("Enable biotech features", ref FleshSymbiontSettings.enableBiotechFeatures,
                    "Enable xenogerm extraction and genetic modifications");
                    
                if (FleshSymbiontSettings.enableBiotechFeatures)
                {
                    listing.CheckboxLabeled("Allow xenogerm extraction", ref FleshSymbiontSettings.allowXenogermExtraction,
                        "Allow extracting genetic material from symbionts");
                        
                    if (FleshSymbiontSettings.allowXenogermExtraction)
                    {
                        FleshSymbiontSettings.xenogermExtractionDamage = listing.SliderLabeled(
                            $"Extraction damage: {FleshSymbiontSettings.xenogermExtractionDamage:P0}", 
                            FleshSymbiontSettings.xenogermExtractionDamage, 0.0f, 1.0f);
                            
                        listing.CheckboxLabeled("Require research for extraction", ref FleshSymbiontSettings.requireResearchForExtraction,
                            "Require symbiont genetics research before extraction");
                    }
                    
                    listing.CheckboxLabeled("Allow hybrid births", ref FleshSymbiontSettings.allowSymbiontHybridBirth,
                        "Bonded parents can give birth to symbiont hybrid children");
                        
                    if (FleshSymbiontSettings.allowSymbiontHybridBirth)
                    {
                        FleshSymbiontSettings.hybridTransformationChance = listing.SliderLabeled(
                            $"Hybrid birth chance: {FleshSymbiontSettings.hybridTransformationChance:P0}", 
                            FleshSymbiontSettings.hybridTransformationChance, 0.0f, 0.5f);
                    }
                }
                listing.Gap(12f);
            }
            
            // Royalty Integration Section (only if Royalty is loaded)
            if (ModsConfig.RoyaltyActive)
            {
                listing.Label("Royalty Integration", -1, "Features requiring Royalty DLC");
                listing.CheckboxLabeled("Enable royalty features", ref FleshSymbiontSettings.enableRoyaltyFeatures,
                    "Enable psycasts, royal reactions, and meditation focus");
                    
                if (FleshSymbiontSettings.enableRoyaltyFeatures)
                {
                    listing.CheckboxLabeled("Enable symbiont psycasts", ref FleshSymbiontSettings.enableSymbiontPsycasts,
                        "Bonded colonists gain unique psychic abilities");
                        
                    if (FleshSymbiontSettings.enableSymbiontPsycasts)
                    {
                        FleshSymbiontSettings.psycastPowerMultiplier = listing.SliderLabeled(
                            $"Psycast power: {FleshSymbiontSettings.psycastPowerMultiplier:F1}x", 
                            FleshSymbiontSettings.psycastPowerMultiplier, 0.5f, 2.0f);
                    }
                    
                    listing.CheckboxLabeled("Royals hate symbionts", ref FleshSymbiontSettings.royalsHateSymbionts,
                        "Royal colonists have negative reactions to symbionts and bonded colonists");
                        
                    listing.CheckboxLabeled("Allow symbiont meditation", ref FleshSymbiontSettings.allowSymbiontMeditation,
                        "Bonded colonists can meditate on symbionts for psyfocus");
                        
                    listing.CheckboxLabeled("Enable royal decrees", ref FleshSymbiontSettings.enableRoyalDecrees,
                        "Empire can issue decrees to purge symbionts");
                }
                listing.Gap(12f);
            }
            
            // Horror/Atmosphere Section
            listing.Label("Horror & Atmosphere", -1, "Control the intensity of horror elements");
            listing.CheckboxLabeled("Enable horror messages", ref FleshSymbiontSettings.enableHorrorMessages,
                "Show atmospheric horror messages during events");
                
            listing.CheckboxLabeled("Enable atmospheric effects", ref FleshSymbiontSettings.enableAtmosphericEffects,
                "Enable visual and audio atmospheric effects");
                
            listing.CheckboxLabeled("Enable body horror descriptions", ref FleshSymbiontSettings.enableBodyHorrorDescriptions,
                "Include detailed body horror in descriptions");
                
            var messageLabels = new[] { "None", "Minimal", "Normal", "Frequent" };
            FleshSymbiontSettings.messageFrequency = (int)listing.SliderLabeled(
                $"Message frequency: {messageLabels[FleshSymbiontSettings.messageFrequency]}", 
                FleshSymbiontSettings.messageFrequency, 0, 3);
            listing.Gap(12f);
            
            // Balancing Options Section
            listing.Label("Balancing Options", -1, "Fine-tune the gameplay balance");
            listing.CheckboxLabeled("Require research to interact", ref FleshSymbiontSettings.enableResearchRequirement,
                "Require xenobiology research before colonists can safely interact");
                
            listing.CheckboxLabeled("Limit symbionts per map", ref FleshSymbiontSettings.limitSymbiontsPerMap,
                "Prevent too many symbionts from spawning on one map");
                
            if (FleshSymbiontSettings.limitSymbiontsPerMap)
            {
                FleshSymbiontSettings.maxSymbiontsPerMap = (int)listing.SliderLabeled(
                    $"Max symbionts per map: {FleshSymbiontSettings.maxSymbiontsPerMap}", 
                    FleshSymbiontSettings.maxSymbiontsPerMap, 1, 10);
            }
            
            listing.CheckboxLabeled("Enable symbiont decay", ref FleshSymbiontSettings.enableSymbiontDecay,
                "Symbionts slowly lose health over time without feeding");
                
            if (FleshSymbiontSettings.enableSymbiontDecay)
            {
                FleshSymbiontSettings.symbiontDecayRate = listing.SliderLabeled(
                    $"Decay rate: {FleshSymbiontSettings.symbiontDecayRate:F2} HP/day", 
                    FleshSymbiontSettings.symbiontDecayRate, 0.01f, 1.0f);
            }
            listing.Gap(12f);
            
            // Reset buttons
            if (listing.ButtonText("Reset to Default Settings"))
            {
                ResetToDefaults();
            }
            
            if (listing.ButtonText("Hardcore Horror Settings"))
            {
                SetHardcoreSettings();
            }
            
            if (listing.ButtonText("Casual/Lite Settings"))
            {
                SetCasualSettings();
            }
            
            listing.End();
            Widgets.EndScrollView();
        }
        
        private void ResetToDefaults()
        {
            FleshSymbiontSettings.eventFrequencyMultiplier = 1.0f;
            FleshSymbiontSettings.minDaysBetweenEvents = 60;
            FleshSymbiontSettings.maxDaysBetweenEvents = 120;
            FleshSymbiontSettings.compulsionFrequencyMultiplier = 1.0f;
            FleshSymbiontSettings.hungerGrowthMultiplier = 1.0f;
            FleshSymbiontSettings.maxCompulsionRange = 25;
            FleshSymbiontSettings.allowCompulsionResistance = true;
            FleshSymbiontSettings.compulsionResistanceChance = 0.15f;
            FleshSymbiontSettings.allowVoluntaryBonding = true;
            FleshSymbiontSettings.bondingBenefitsMultiplier = 1.0f;
            FleshSymbiontSettings.bondingPenaltiesMultiplier = 1.0f;
            FleshSymbiontSettings.enableMadnessBreaks = true;
            FleshSymbiontSettings.madnessFrequencyMultiplier = 1.0f;
            FleshSymbiontSettings.symbiontHealthMultiplier = 1.0f;
            FleshSymbiontSettings.allowSymbiontDestruction = true;
            FleshSymbiontSettings.bondedDefendsSymbiont = true;
            FleshSymbiontSettings.defenseAggressionLevel = 1.0f;
            FleshSymbiontSettings.enableIdeologyFeatures = true;
            FleshSymbiontSettings.enableRituals = true;
            FleshSymbiontSettings.ritualBenefitsMultiplier = 1.0f;
            FleshSymbiontSettings.enableMemePrecepts = true;
            FleshSymbiontSettings.enableBiotechFeatures = true;
            FleshSymbiontSettings.allowXenogermExtraction = true;
            FleshSymbiontSettings.allowSymbiontHybridBirth = true;
            FleshSymbiontSettings.xenogermExtractionDamage = 0.3f;
            FleshSymbiontSettings.requireResearchForExtraction = true;
            FleshSymbiontSettings.hybridTransformationChance = 0.1f;
            FleshSymbiontSettings.enableRoyaltyFeatures = true;
            FleshSymbiontSettings.enableSymbiontPsycasts = true;
            FleshSymbiontSettings.royalsHateSymbionts = true;
            FleshSymbiontSettings.allowSymbiontMeditation = true;
            FleshSymbiontSettings.enableRoyalDecrees = true;
            FleshSymbiontSettings.psycastPowerMultiplier = 1.0f;
            FleshSymbiontSettings.enableHorrorMessages = true;
            FleshSymbiontSettings.enableAtmosphericEffects = true;
            FleshSymbiontSettings.enableBodyHorrorDescriptions = true;
            FleshSymbiontSettings.messageFrequency = 2;
            FleshSymbiontSettings.enableResearchRequirement = false;
            FleshSymbiontSettings.limitSymbiontsPerMap = true;
            FleshSymbiontSettings.maxSymbiontsPerMap = 2;
            FleshSymbiontSettings.enableSymbiontDecay = false;
            FleshSymbiontSettings.symbiontDecayRate = 0.1f;
        }
        
        private void SetHardcoreSettings()
        {
            FleshSymbiontSettings.eventFrequencyMultiplier = 2.0f;
            FleshSymbiontSettings.minDaysBetweenEvents = 30;
            FleshSymbiontSettings.maxDaysBetweenEvents = 60;
            FleshSymbiontSettings.compulsionFrequencyMultiplier = 2.0f;
            FleshSymbiontSettings.hungerGrowthMultiplier = 1.5f;
            FleshSymbiontSettings.maxCompulsionRange = 35;
            FleshSymbiontSettings.allowCompulsionResistance = true;
            FleshSymbiontSettings.compulsionResistanceChance = 0.05f;
            FleshSymbiontSettings.bondingBenefitsMultiplier = 1.2f;
            FleshSymbiontSettings.bondingPenaltiesMultiplier = 1.5f;
            FleshSymbiontSettings.madnessFrequencyMultiplier = 1.5f;
            FleshSymbiontSettings.symbiontHealthMultiplier = 1.5f;
            FleshSymbiontSettings.defenseAggressionLevel = 2.0f;
            FleshSymbiontSettings.limitSymbiontsPerMap = false;
            FleshSymbiontSettings.enableSymbiontDecay = false;
            FleshSymbiontSettings.messageFrequency = 3;
        }
        
        private void SetCasualSettings()
        {
            FleshSymbiontSettings.eventFrequencyMultiplier = 0.5f;
            FleshSymbiontSettings.minDaysBetweenEvents = 120;
            FleshSymbiontSettings.maxDaysBetweenEvents = 200;
            FleshSymbiontSettings.compulsionFrequencyMultiplier = 0.5f;
            FleshSymbiontSettings.hungerGrowthMultiplier = 0.7f;
            FleshSymbiontSettings.maxCompulsionRange = 15;
            FleshSymbiontSettings.allowCompulsionResistance = true;
            FleshSymbiontSettings.compulsionResistanceChance = 0.3f;
            FleshSymbiontSettings.bondingBenefitsMultiplier = 1.2f;
            FleshSymbiontSettings.bondingPenaltiesMultiplier = 0.7f;
            FleshSymbiontSettings.madnessFrequencyMultiplier = 0.5f;
            FleshSymbiontSettings.symbiontHealthMultiplier = 0.7f;
            FleshSymbiontSettings.defenseAggressionLevel = 0.5f;
            FleshSymbiontSettings.limitSymbiontsPerMap = true;
            FleshSymbiontSettings.maxSymbiontsPerMap = 1;
            FleshSymbiontSettings.enableSymbiontDecay = true;
            FleshSymbiontSettings.symbiontDecayRate = 0.2f;
            FleshSymbiontSettings.messageFrequency = 1;
        }
        
        public override string SettingsCategory()
        {
            return "Signal Lost";
        }
    }
}