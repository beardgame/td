using System;
using System.Linq;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Navigation;

static class NextDirectionFinder
{
    public static Direction FindNextDirection(
        MultipleSinkNavigationSystem navigator, Tile from, Direction currentDirection, Bias bias)
    {
        var possibleDirections = navigator.GetAllDirectionsToSink(from);
        // Prefer going in the same direction as we are going. This biases towards straight paths, but also ensures that
        // we don't make changes to our current velocity vector if it5 isn't necessary.
        if (currentDirection != Direction.Unknown && possibleDirections.Includes(currentDirection))
        {
            return currentDirection;
        }

        var allPossibleDirections = bias == Bias.Left
            ? possibleDirections.EnumerateCounterClockwise(currentDirection)
            : possibleDirections.EnumerateClockwise(currentDirection);
        var firstPossibleDirection = allPossibleDirections.FirstOrDefault(Direction.Unknown);

        return firstPossibleDirection;
    }

    public enum Bias
    {
        Left,
        Right
    }
}
