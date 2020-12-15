using amulware.Graphics;
using Bearded.TD.Game.Simulation.Resources;
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
                public static TimeSpan SevereNotificationDuration = 20.S();
                public static readonly TimeSpan NotificationFadeOutTime = 1.S();
                public const int MaxNotifications = 4;

                public static readonly Color ResourcesColor = new Color(255, 191, 0); // amber
                public static readonly Color TechPointsColor = Color.Turquoise;
            }

            public static class WaveGeneration
            {
                public static readonly TimeSpan FirstDownTimeDuration = 40.S();
                public static readonly TimeSpan DownTimeDuration = 20.S();

                public static readonly TimeSpan MaxSpawnTimeDuration = 12.S();
                public static readonly TimeSpan PreferredTimeBetweenSpawns = 1.S();
                public static readonly TimeSpan MinTimeBetweenSpawns = 0.1.S();

                public const double FirstWaveValue = 30;
                public const double WaveValueErrorFactor = 0.1;
                public const double WaveValueMultiplier = 1.3;

                public static readonly ResourceAmount InitialResources = 240.Resources();
                public static readonly ResourceAmount FirstWaveResources = 200.Resources();
                public static readonly double WaveResourcesMultiplier = 1.2;
            }

            public static class Worker
            {
                public static readonly ResourceRate WorkerSpeed = 15.ResourcesPerSecond();
                public static readonly ResourceAmount TotalMiningProgressRequired = WorkerSpeed * 8.S();
            }

            public static class Technology
            {
                public const double TechCostMultiplicationFactor = 1;
            }

            public static class Enemy
            {
                public static readonly Speed PathIndicatorSpeed = 10.UnitsPerSecond();
                public static readonly TimeSpan TimeBetweenIndicators = 1.25.S();
            }

            public static class Navigation
            {
                public const int StepsPerFrame = 100;

                public static readonly Unit MaxWalkableHeightDifference = 0.1.U();
            }

            public static class Elements
            {
                public static readonly EnergyConsumptionRate DefaultBurnSpeed = new EnergyConsumptionRate(1);
                public static readonly double DefaultDamagePerFuelBurned = 1;
                public static readonly Energy EnergyPerEnergyDamage = new Energy(0.2);
                public static readonly Energy EnergyPerFireDamage = new Energy(50);
                public static readonly EnergyConsumptionRate EnergyPerSecondInWater = new EnergyConsumptionRate(15);

                public const double MinWetness = 0;
                public static readonly TimeSpan WaterEvaporationHalfTime = .5.S();
                public static readonly Energy EnergyPerUnitWaterEvaporated = new Energy(0.05);

                public static readonly Energy MinHeat = new Energy(0);
                public static readonly Energy AmbientHeat = new Energy(100);
                public static readonly TimeSpan AmbientHeatApproachHalfTime = 1.S();
            }

            public static class Physics
            {
                public static readonly Acceleration Gravity = new Acceleration(-20f);
                public static readonly Acceleration3 Gravity3 = Acceleration2.Zero.WithZ(Gravity);
            }
        }
    }
}
