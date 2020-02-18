using amulware.Graphics;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD
{
    static partial class Constants
    {
        public static class Game
        {
            public static class World
            {
                public const int Radius = 32;

                private const float sqrtOfThree = 1.73205080757f;

                public const float HexagonWidth = 1; // distance between hexagon centers == min diameter
                public const float HexagonDistanceX = HexagonWidth; // horizontal distance between hexagons

                public const float HexagonSide = HexagonWidth / sqrtOfThree; // side length
                public const float HexagonDiameter = HexagonSide * 2; // corner to corner (max) diameter

                public const float HexagonDistanceY = HexagonSide * 1.5f; // vertical distance between hexagons

                public static readonly Difference2 HexagonGridUnitX = new Difference2(HexagonDistanceX, 0); // step in (1, 0) direction
                public static readonly Difference2 HexagonGridUnitY = new Difference2(HexagonDistanceX * 0.5f, HexagonDistanceY); // step in (0, 1) direction

                public static readonly Squared<Unit> HexagonInnerRadiusSquared = (HexagonWidth * 0.5f).U().Squared;
                public static readonly Squared<Unit> HexagonOuterRadiusSquared = HexagonSide.U().Squared;
            }

            public static class GameUI
            {
                public const int ActionBarSize = 10;
                public static readonly TimeSpan NotificationDuration = 5.S();
                public static readonly TimeSpan NotificationFadeOutTime = 1.S();
                public const int MaxNotifications = 4;

                public static readonly Color ResourcesColor = new Color(255, 191, 0); // amber
                public static readonly Color TechPointsColor = Color.Turquoise;
            }

            public static class EnemyGeneration
            {
                public static readonly TimeSpan TimeBeforeFirstWave = 40.S();
                public static readonly TimeSpan WarningTime = 20.S();
                public static readonly TimeSpan MinTimeBetweenEnemies = .1.S();
                public static readonly TimeSpan MaxTimeBetweenEnemies = 2.S();
                public static readonly TimeSpan MinWaveDuration = 3.S();
                public static readonly TimeSpan MaxWaveDuration = 10.S();

                public const double InitialMinWaveCost = 12;
                public const double InitialMaxWaveCost = 16;
                public const double WaveCostGrowth = 1.011;

                public const double InitialDebitPayoffRate = .6;
                public const double DebitPayoffGrowth = 1.008;
            }

            public static class Worker
            {
                public const double WorkerSpeed = 15;

                public const double TotalMiningProgressRequired = 8 * WorkerSpeed;
            }

            public static class Resources
            {
                public const long InitialResources = 0;
                public const double ResourcesOnKillFactor = 0;
            }

            public static class Enemy
            {
                public static readonly Speed PathIndicatorSpeed = 10.UnitsPerSecond();
                public static readonly TimeSpan TimeBetweenIndicators = 1.25.S();
            }

            public static class Navigation
            {
                public const int StepsPerFrame = 100;

                public static readonly Unit MaxWalkableHeightDifference = 0.05.U();
            }

            public static class Elements
            {
                public static readonly EnergyConsumptionRate DefaultBurnSpeed = new EnergyConsumptionRate(1);
                public static readonly double DefaultDamagePerFuelBurned = 1;
                public static readonly Energy EnergyPerEnergyDamage = new Energy(0.2);
                public static readonly Energy EnergyPerFireDamage = new Energy(50);
                public static readonly EnergyConsumptionRate EnergyPerSecondInWater = new EnergyConsumptionRate(15);
            }

            public static class Physics
            {
                public static readonly Acceleration Gravity = new Acceleration(-20f);
                public static readonly Acceleration3 Gravity3 = Acceleration2.Zero.WithZ(Gravity);
            }
        }
    }
}
