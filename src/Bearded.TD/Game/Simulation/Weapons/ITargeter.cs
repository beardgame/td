
namespace Bearded.TD.Game.Simulation.Weapons;

interface ITargeter<out T>
{
    T? Target { get; }
}