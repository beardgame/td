using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD
{
    static partial class Constants
    {
        public static class Game
        {
            public static class World
            {
                public const int Radius = 20;

                private const float sqrtOfThree = 1.73205080757f;

                public const float HexagonWidth = 1; // distance between hexagon centers == min diamater
                public const float HexagonDistanceX = HexagonWidth; // horizontal distance between hexagons

                public const float HexagonSide = HexagonWidth / sqrtOfThree; // side length
                public const float HexagonDiameter = HexagonSide * 2; // corner to corner (max) diameter

                public const float HexagonDistanceY = HexagonSide * 1.5f; // vertical distance between hexagons

                public static readonly Difference2 HexagonGridUnitX = new Difference2(HexagonDistanceX, 0); // step in (1, 0) direction
                public static readonly Difference2 HexagonGridUnitY = new Difference2(HexagonDistanceX * 0.5f, HexagonDistanceY); // step in (0, 1) direction

                public static readonly Squared<Unit> HexagonInnerRadiusSquared = (HexagonWidth * 0.5f).U().Squared;
                public static readonly Squared<Unit> HexagonOuterRadiusSquared = HexagonSide.U().Squared;
            }

            public static class UI
            {
                public const int ActionBarSize = 10;
            }

            public static class EnemyGeneration
            {
                public static readonly TimeSpan TimeBeforeFirstWave = 20.S();
                public static readonly TimeSpan WarningTime = 20.S();
                public static readonly TimeSpan MinTimeBetweenEnemies = .1.S();
                public static readonly TimeSpan MaxTimeBetweenEnemies = 2.S();
                public static readonly TimeSpan MinWaveDuration = 10.S();
                public static readonly TimeSpan MaxWaveDuration = 30.S();

                public const double InitialMinWaveCost = 12;
                public const double InitialMaxWaveCost = 16;
                public const double WaveCostGrowth = 1.007;

                public const double InitialDebitPayoffRate = .8;
                public const double DebitPayoffGrowth = 1.009;
            }

            public static class Worker
            {
                public static readonly Acceleration Acceleration = 10.UnitsPerSecondSquared();
                public const float Friction = .1f;

                public static readonly Squared<Unit> WorkerWorkRadiusSquared = World.HexagonOuterRadiusSquared;
                public const double WorkerSpeed = 15;
                
                public const double TotalMiningProgressRequired = 4 * WorkerSpeed;
            }
        }
    }
}
