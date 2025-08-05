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
            
            // DLC integration
            Scribe_Values.Look(ref enableIdeologyFeatures, "enableIdeologyFeatures", true);
            Scribe_Values.Look(ref enableRituals, "enableRituals", true);
            Scribe_Values.Look(ref ritualBenefitsMultiplier, "ritualBenefitsMultiplier", 1.0f);
            Scribe_Values.Look(ref enableMemePrecepts, "enableMemePrecepts", true);
            Scribe_Values.Look(ref enableBiotechFeatures, "enableBiotechFeatures", true);
            Scribe_Values.Look(ref allowXenogermExtraction, "allowXenogermExtraction", true);
            Scribe_Values.Look(ref allowSymbiontHybridBirth, "allowSymbiontHybridBirth", true);
            Scribe_Values.Look(ref xenogermExtractionDamage, "xenogermExtractionDamage", 0.3f);
            Scribe_Values.Look(ref requireResearchForExtraction, "requireResearchForExtraction", true);
            Scribe_Values.Look(ref hybridTransformationChance, "hybridTransformationChance", 0.1f);
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
        private string selectedTab = "general";
        
        public FleshSymbiontMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<FleshSymbiontSettings>();
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            var tabRect = new Rect(0, 0, inRect.width, 30f);
            var contentRect = new Rect(0, 35f, inRect.width - 20f, inRect.height - 35f);
            var scrollRect = new Rect(0, 0, contentRect.width - 20f, GetContentHeight());
            
            // Tab buttons
            DrawTabs(tabRect);
            
            // Scrollable content
            Widgets.BeginScrollView(contentRect, ref scrollPosition, scrollRect);
            
            var listing = new Listing_Standard();
            listing.Begin(scrollRect);
            
            switch (selectedTab)
            {
                case "general": DrawGeneralSettings(listing); break;
                case "behavior": DrawBehaviorSettings(listing); break;
                case "horror": DrawHorrorSettings(listing); break;
                case "dlc": DrawDLCSettings(listing); break;
                case "balance": DrawBalanceSettings(listing); break;
            }
            
            listing.End();
            Widgets.EndScrollView();
        }
        
        private void DrawTabs(Rect rect)
        {
            var tabWidth = rect.width / 5f;
            var tabs = new[]
            {
                ("general", "General"),
                ("behavior", "Behavior"),
                ("horror", "Horror"),
                ("dlc", "DLC Features"),
                ("balance", "Balance")
            };
            
            for (int i = 0; i < tabs.Length; i++)
            {
                var tabRect = new Rect(i * tabWidth, rect.y, tabWidth, rect.height);
                bool isSelected = selectedTab == tabs[i].Item1;
                
                if (Widgets.ButtonText(tabRect, tabs[i].Item2, true, true, isSelected))
                {
                    selectedTab = tabs[i].Item1;
                    scrollPosition = Vector2.zero;
                }
            }
        }
        
        private void DrawGeneralSettings(Listing_Standard listing)
        {
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
                
            // Validation
            FleshSymbiontSettings.maxDaysBetweenEvents = Mathf.Max(
                FleshSymbiontSettings.minDaysBetweenEvents + 10, 
                FleshSymbiontSettings.maxDaysBetweenEvents);
                
            listing.Gap(12f);
            
            listing.Label("Symbiont Durability");
            FleshSymbiontSettings.symbiontHealthMultiplier = listing.SliderLabeled(
                $"Symbiont health: {FleshSymbiontSettings.symbiontHealthMultiplier:F1}x", 
                FleshSymbiontSettings.symbiontHealthMultiplier, 0.1f, 5.0f);
                
            listing.CheckboxLabeled("Allow symbiont destruction", ref FleshSymbiontSettings.allowSymbiontDestruction,
                "Symbionts can be destroyed by damage");
        }
        
        private void DrawBehaviorSettings(Listing_Standard listing)
        {
            listing.Label("Compulsion Behavior", -1, "How aggressively symbionts compel colonists");
            
            FleshSymbiontSettings.compulsionFrequencyMultiplier = listing.SliderLabeled(
                $"Compulsion frequency: {FleshSymbiontSettings.compulsionFrequencyMultiplier:F1}x", 
                FleshSymbiontSettings.compulsionFrequencyMultiplier, 0.1f, 3.0f);
                
            FleshSymbiontSettings.hungerGrowthMultiplier = listing.SliderLabeled(
                $"Hunger growth rate: {FleshSymbiontSettings.hungerGrowthMultiplier:F1}x", 
                FleshSymbiontSettings.hungerGrowthMultiplier, 0.1f, 3.0f);
                
            FleshSymbiontSettings.maxCompulsionRange = (int)listing.SliderLabeled(
                $"Compulsion range: {FleshSymbiontSettings.maxCompulsionRange} tiles", 
                FleshSymbiontSettings.maxCompulsionRange, 5, 50);
                
            listing.Gap(12f);
            
            listing.Label("Resistance & Bonding");
            listing.CheckboxLabeled("Allow compulsion resistance", ref FleshSymbiontSettings.allowCompulsionResistance,
                "Colonists can sometimes resist compulsion based on their traits");
                
            if (FleshSymbiontSettings.allowCompulsionResistance)
            {
                FleshSymbiontSettings.compulsionResistanceChance = listing.SliderLabeled(
                    $"Base resistance chance: {FleshSymbiontSettings.compulsionResistanceChance:P0}", 
                    FleshSymbiontSettings.compulsionResistanceChance, 0.0f, 0.5f);
            }
            
            listing.CheckboxLabeled("Allow voluntary bonding", ref FleshSymbiontSettings.allowVoluntaryBonding,
                "Colonists can choose to touch the symbiont voluntarily");
                
            listing.Gap(12f);
            
            listing.Label("Bonding Effects");
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
            
            listing.Label("Defense Behavior");
            listing.CheckboxLabeled("Bonded colonists defend symbiont", ref FleshSymbiontSettings.bondedDefendsSymbiont,
                "Bonded colonists go berserk when their symbiont is threatened");
                
            if (FleshSymbiontSettings.bondedDefendsSymbiont)
            {
                FleshSymbiontSettings.defenseAggressionLevel = listing.SliderLabeled(
                    $"Defense aggression: {FleshSymbiontSettings.defenseAggressionLevel:F1}x", 
                    FleshSymbiontSettings.defenseAggressionLevel, 0.1f, 3.0f);
            }
        }
        
        private void DrawHorrorSettings(Listing_Standard listing)
        {
            listing.Label("Atmospheric Horror", -1, "Control the intensity and frequency of horror elements");
            
            listing.CheckboxLabeled("Enable horror messages", ref FleshSymbiontSettings.enableHorrorMessages,
                "Show atmospheric horror messages during events");
                
            listing.CheckboxLabeled("Enable atmospheric effects", ref FleshSymbiontSettings.enableAtmosphericEffects,
                "Enable visual and audio atmospheric effects");
                
            listing.CheckboxLabeled("Enable body horror descriptions", ref FleshSymbiontSettings.enableBodyHorrorDescriptions,
                "Include detailed body horror in descriptions (affects bonding messages)");
                
            listing.Gap(12f);
            
            listing.Label("Message Intensity");
            var messageLabels = new[] { "None", "Minimal", "Normal", "Frequent" };
            FleshSymbiontSettings.messageFrequency = (int)listing.SliderLabeled(
                $"Message frequency: {messageLabels[FleshSymbiontSettings.messageFrequency]}", 
                FleshSymbiontSettings.messageFrequency, 0, 3);
                
            if (FleshSymbiontSettings.messageFrequency > 0)
            {
                listing.Label("Message Preview:", -1);
                string preview = FleshSymbiontSettings.messageFrequency switch
                {
                    1 => "\"John has bonded with the symbiont.\"",
                    2 => "\"John screams as the flesh symbiont's tendrils burrow into their spine...\"",
                    3 => "\"John convulses as the symbiont's tendrils burrow deep into their spinal cord. Bone cracks, flesh tears...\"",
                    _ => ""
                };
                listing.Label(preview, -1);
            }
        }
        
        private void DrawDLCSettings(Listing_Standard listing)
        {
            // Ideology Integration
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
            
            // Biotech Integration
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
            
            // Royalty Integration
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
            }
        }
        
        private void DrawBalanceSettings(Listing_Standard listing)
        {
            listing.Label("Gameplay Balance", -1, "Fine-tune the gameplay balance and difficulty");
            
            listing.CheckboxLabeled("Require research to interact", ref FleshSymbiontSettings.enableResearchRequirement,
                "Require xenobiology research before symbiont events can occur");
                
            listing.CheckboxLabeled("Limit symbionts per map", ref FleshSymbiontSettings.limitSymbiontsPerMap,
                "Prevent too many symbionts from spawning on one map");
                
            if (FleshSymbiontSettings.limitSymbiontsPerMap)
            {
                FleshSymbiontSettings.maxSymbiontsPerMap = (int)listing.SliderLabeled(
                    $"Max symbionts per map: {FleshSymbiontSettings.maxSymbiontsPerMap}", 
                    FleshSymbiontSettings.maxSymbiontsPerMap, 1, 10);
            }
            
            listing.Gap(12f);
            
            listing.Label("Symbiont Maintenance");
            listing.CheckboxLabeled("Enable symbiont decay", ref FleshSymbiontSettings.enableSymbiontDecay,
                "Symbionts slowly lose health over time without feeding");
                
            if (FleshSymbiontSettings.enableSymbiontDecay)
            {
                FleshSymbiontSettings.symbiontDecayRate = listing.SliderLabeled(
                    $"Decay rate: {FleshSymbiontSettings.symbiontDecayRate:F2} HP/day", 
                    FleshSymbiontSettings.symbiontDecayRate, 0.01f, 1.0f);
            }
            
            listing.Gap(24f);
            
            // Preset buttons
            listing.Label("Quick Presets", -1, "Apply pre-configured setting combinations");
            
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
            
            if (listing.ButtonText("Cinematic Horror Settings"))
            {
                SetCinematicSettings();
            }
        }
        
        private float GetContentHeight()
        {
            return selectedTab switch
            {
                "general" => 400f,
                "behavior" => 600f,
                "horror" => 300f,
                "dlc" => 700f,
                "balance" => 500f,
                _ => 400f
            };
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
            FleshSymbiontSettings.enableHorrorMessages = true;
            FleshSymbiontSettings.enableAtmosphericEffects = true;
            FleshSymbiontSettings.enableBodyHorrorDescriptions = true;
            FleshSymbiontSettings.messageFrequency = 2;
            FleshSymbiontSettings.enableResearchRequirement = false;
            FleshSymbiontSettings.limitSymbiontsPerMap = true;
            FleshSymbiontSettings.maxSymbiontsPerMap = 2;
            FleshSymbiontSettings.enableSymbiontDecay = false;
            FleshSymbiontSettings.symbiontDecayRate = 0.1f;
            
            // DLC features remain at defaults
            FleshSymbiontSettings.enableIdeologyFeatures = true;
            FleshSymbiontSettings.enableBiotechFeatures = true;
            FleshSymbiontSettings.enableRoyaltyFeatures = true;
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
        
        private void SetCinematicSettings()
        {
            // Balanced gameplay with maximum atmospheric horror
            FleshSymbiontSettings.eventFrequencyMultiplier = 1.2f;
            FleshSymbiontSettings.compulsionFrequencyMultiplier = 1.0f;
            FleshSymbiontSettings.bondingBenefitsMultiplier = 1.0f;
            FleshSymbiontSettings.bondingPenaltiesMultiplier = 1.0f;
            FleshSymbiontSettings.enableHorrorMessages = true;
            FleshSymbiontSettings.enableAtmosphericEffects = true;
            FleshSymbiontSettings.enableBodyHorrorDescriptions = true;
            FleshSymbiontSettings.messageFrequency = 3;
            FleshSymbiontSettings.limitSymbiontsPerMap = true;
            FleshSymbiontSettings.maxSymbiontsPerMap = 2;
        }
        
        public override string SettingsCategory()
        {
            return "Signal Lost";
        }
    }
}
