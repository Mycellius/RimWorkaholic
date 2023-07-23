using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;


public static class ListingExtensions
{
    public static void EnumDropdown<TEnum>(this Listing_Standard listing, TEnum enumValue, Action<TEnum> setValue, Func<TEnum, string> labelFunc) where TEnum : Enum
    {
        Rect rect = listing.GetRect(30f);
        if (Widgets.ButtonText(rect, labelFunc(enumValue)))
        {
            List<FloatMenuOption> options = new();
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                TEnum localValue = value;
                options.Add(new FloatMenuOption(labelFunc(value), () => setValue(localValue)));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}

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
                WeightedAverage
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

        public class RimWorkaholicSettings : ModSettings
        {
            public Settings.CalculationMethod scoreCalculationMethod = Settings.CalculationMethod.SimpleSum;
            public Settings.DisplayFormat scoreDisplayFormat = Settings.DisplayFormat.RawNumber;
            public int scoreUpdateFrequency = 1; // In-game days
            public float scoreDecayRate = 0.1f; // 10% decay rate
            public Color highScoreColor = Color.green;
            public Color lowScoreColor = Color.red;

            public override void ExposeData()
            {
                base.ExposeData();
                Scribe_Values.Look(ref scoreCalculationMethod, "scoreCalculationMethod");
                Scribe_Values.Look(ref scoreDisplayFormat, "scoreDisplayFormat");
                Scribe_Values.Look(ref scoreUpdateFrequency, "scoreUpdateFrequency", 1);
                Scribe_Values.Look(ref scoreDecayRate, "scoreDecayRate", 0.1f);
            }

            public void DoWindowContents(Rect inRect)
            {
                Listing_Standard listing = new();
                listing.Begin(inRect);

                listing.Label("Score calculation method:");
                ListingExtensions.EnumDropdown(listing, scoreCalculationMethod, value => scoreCalculationMethod = value, (Settings.CalculationMethod method) => method.ToString());

                listing.Gap(24f); // Add a gap between the dropdowns

                listing.Label("Score display format:");
                ListingExtensions.EnumDropdown(listing, scoreDisplayFormat, value => scoreDisplayFormat = value, (Settings.DisplayFormat format) => format.ToString());

                listing.Label("Score update frequency (in-game days):");
                scoreUpdateFrequency = (int)listing.Slider(scoreUpdateFrequency, 1, 30);

                listing.Label("Score decay rate:");
                scoreDecayRate = listing.Slider(scoreDecayRate, 0, 1);

                listing.End();
            }
        }
    }
}