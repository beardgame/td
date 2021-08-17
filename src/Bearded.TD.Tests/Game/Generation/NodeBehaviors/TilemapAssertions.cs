using System.Collections.Generic;
using Bearded.TD.Tiles;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public static class TilemapAssertions
    {
        public static TilemapAssertions<T> Should<T>(this Tilemap<T> tilemap) => new(tilemap);
    }

    public sealed class TilemapAssertions<T>
    {
        private readonly Tilemap<T> subject;
        private readonly EqualityComparer<T> comparer;

        public TilemapAssertions(Tilemap<T> subject)
        {
            this.subject = subject;
            comparer = EqualityComparer<T>.Default;
        }

        [CustomAssertion]
        public void HaveSameContentAs(Tilemap<T> expected)
        {
            subject.Radius.Should().Be(expected.Radius);
            foreach (var tile in subject)
            {
                var subjectTile = subject[tile];
                var expectedTile = expected[tile];

                Execute.Assertion
                    .ForCondition(comparer.Equals(subjectTile, expectedTile))
                    .FailWith("At coordinates {0}, expected\n{1}, but found\n{2}.",
                        tile, expectedTile, subjectTile);
            }
        }
    }
}
