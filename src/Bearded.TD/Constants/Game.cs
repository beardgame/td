﻿using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD;

static partial class Constants
{
    public static class Game
    {
        public static readonly TimeSpan MaxFrameTime = 0.1.S();

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

            public static readonly Unit HexagonInnerRadius = (HexagonWidth * 0.5f).U();
            public static readonly Squared<Unit> HexagonInnerRadiusSquared = HexagonInnerRadius.Squared;
            public static readonly Squared<Unit> HexagonOuterRadiusSquared = HexagonSide.U().Squared;
        }

        public static class GameUI
        {
            public const int ActionBarSize = 10;
            public static readonly TimeSpan NotificationDuration = 5.S();

            public static Color ActionBackgroundColor = new Color(27, 81, 156) * 0.75f;
            public static Color UrgentBackgroundColor = new Color(246, 190, 0) * 0.75f;

            public static readonly Color ResourcesColor = new Color(255, 191, 0); // amber
            public static readonly Color TechPointsColor = Color.Turquoise;
            public static readonly Color VeterancyColor = new Color(3, 252, 215);

            public static readonly Color DynamicsColor = Color.LightGray;
            public static readonly Color CombustionColor = Color.OrangeRed;
            public static readonly Color ConductivityColor = Color.Plum;
            public static readonly Color AlchemyColor = Color.LimeGreen;
            public static readonly Color HydrologyColor = Color.LightBlue;
            public static readonly Color PhotonicsColor = Color.Yellow;

            public static readonly Color EnemyIndicatorColor = Color.Orange;
        }

        public static class WaveGeneration
        {
            public static readonly TimeSpan? FirstDownTimeDuration = null;
            public static readonly TimeSpan DownTimeDuration = 30.S();

            public static readonly TimeSpan EnemyTrainLength = 20.S();
            public static readonly TimeSpan MaxTimeBetweenSpawns = 5.S(); // Overrides enemy train length.
            public static readonly TimeSpan MinTimeBetweenSpawns = 0.3.S();

            public const double FirstWaveValue = 2000;
            public const double WaveValueErrorFactor = 0.1;
            public const double WaveValueLinearGrowth = 2500;
            public const double WaveValueExponentialGrowth = 1.03;

            public static readonly ResourceAmount FirstWaveResources = 150.Resources();
            public static readonly double WaveResourcesMultiplier = 1;
        }

        public static class Building
        {
            public const double RuinedPercentage = 0.5;

            public static readonly ImmutableArray<Experience> VeterancyThresholds =
                ImmutableArray.Create(1200.Xp(), 4800.Xp(), 15000.Xp(), 34000.Xp(), 60000.Xp(), 80000.Xp());
        }

        public static class Resources
        {
            public static readonly ResourceRate ConstructionSpeed = 15.ResourcesPerSecond();
            public static readonly ResourceRate UpgradeSpeed = 25.ResourcesPerSecond();
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
            public static readonly TimeSpan TickDuration = 0.1.S();
        }

        public static class Physics
        {
            public static readonly Acceleration Gravity = new Acceleration(-20f);
            public static readonly Acceleration3 Gravity3 = Acceleration2.Zero.WithZ(Gravity);
        }

        public static class Technology
        {
            public static readonly int[] TierCompletionThresholds = { 0, 2, 2, 2 };
        }

        public static class Drones
        {
            public static readonly Speed Speed = 2.UnitsPerSecond();
            public static readonly Unit FlyingHeight = 0.5.U();
        }
    }
}
