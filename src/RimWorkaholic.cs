using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorkaholic
{
    public class WorkaholicScoreComp : ThingComp
    {
        private float workOutput = 0f;
        private float score = 0f;
        private Queue<float> workOutputHistory = new();
        private const int DaysInQuadrum = 15;

        public Settings ModSettings { get; set; }

        public WorkaholicScoreComp()
        {
            // Initialize the ModSettings property with default values or load them from a configuration file
            ModSettings = new Settings
            {
                ScoreCalculationMethod = Settings.CalculationMethod.SimpleSum,
                ScoreDisplayFormat = Settings.DisplayFormat.RawNumber
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref workOutput, "workOutput", 0f);
            Scribe_Values.Look(ref score, "score", 0f);
            Scribe_Deep.Look(ref workOutputHistory, "workOutputHistory");
        }

        public float GetScore()
        {
            return score;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (parent is Pawn)
            {
                score = CalculateWorkOutputScore(ModSettings);
            }
        }

        public class Settings
        {
            public enum CalculationMethod
            {
                SimpleSum,
                WeightedAverage,
                CustomFormula
            }

            public enum DisplayFormat
            {
                RawNumber,
                Percentage,
                LetterGrade
            }

            public CalculationMethod ScoreCalculationMethod { get; set; }
            public DisplayFormat ScoreDisplayFormat { get; set; }
        }

        public float CalculateWorkOutputScore(Settings settings)
        {
            float calculatedScore = 0f;

            switch (settings.ScoreCalculationMethod)
            {
                case Settings.CalculationMethod.SimpleSum:
                    // Calculate score using simple sum method
                    calculatedScore = workOutput;
                    break;
                case Settings.CalculationMethod.WeightedAverage:
                    // Calculate score using weighted average method
                    // You can adjust the weight of the work efficiency in the final score
                    float workEfficiencyWeight = 0.8f;
                    calculatedScore = (workOutput / DaysInQuadrum) * workEfficiencyWeight;
                    break;
                case Settings.CalculationMethod.CustomFormula:
                    // Calculate score using custom formula method
                    // Replace this with your own custom formula
                    calculatedScore = workOutput * 2;
                    break;
            }

            return calculatedScore;
        }

        public class RimWorkaholicMod : Mod
        {
            public static RimWorkaholicSettings settings;

            public RimWorkaholicMod(ModContentPack content) : base(content)
            {
                settings = GetSettings<RimWorkaholicSettings>();
            }

            public override string SettingsCategory() => "RimWorkaholic";

            public override void DoSettingsWindowContents(Rect inRect)
            {
                settings.DoWindowContents(inRect);
            }
        }

        // Add the new RimWorkaholicSettings class
        public class RimWorkaholicSettings : ModSettings
        {
            public float exampleSetting = 1f;

            public override void ExposeData()
            {
                base.ExposeData();
                Scribe_Values.Look(ref exampleSetting, "exampleSetting", 1f);
            }

            public void DoWindowContents(Rect inRect)
            {
                Listing_Standard listing = new();
                listing.Begin(inRect);
                listing.Label("Example setting:");
                exampleSetting = listing.Slider(exampleSetting, 0f, 10f);
                listing.End();
            }
        }
    }
}