using System.Runtime.CompilerServices;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;

namespace Bearded.TD.Game.Simulation.World;

static class LevelRayCasterExtensions
{
    // TODO: make these extensions on Ray? or something else?
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LevelRayCaster Cast(this Level level, Ray ray)
    {
        var rayCaster = new LevelRayCaster();
        rayCaster.StartEnumeratingTiles(ray);
        return rayCaster;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Cast(this Level level, Ray ray, out LevelRayCaster rayCaster)
    {
        rayCaster = new LevelRayCaster();
        rayCaster.StartEnumeratingTiles(ray);
    }
}