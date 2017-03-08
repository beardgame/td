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

                public const float HexagonWidth = 1;
                public const float HexagonDistanceX = HexagonWidth;

                public const float HexagonSide = HexagonWidth / sqrtOfThree;
                public const float HexagonDiameter = HexagonSide * 2;

                public const float HexagonDistanceY = HexagonSide * 1.5f;

                public static readonly Difference2 HexagonGridUnitX = new Difference2(HexagonDistanceX, 0);
                public static readonly Difference2 HexagonGridUnitY = new Difference2(HexagonDistanceX * 0.5f, HexagonDistanceY);

                public const float HexagonInnerRadiusSquared = (HexagonWidth * 0.5f) * (HexagonWidth * 0.5f);
                public const float HexagonOuterRadiusSquared = HexagonSide * HexagonSide;
            }
        }
    }
}
