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
        }
    }
}
