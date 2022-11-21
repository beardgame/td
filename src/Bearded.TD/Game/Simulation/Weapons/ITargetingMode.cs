using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Weapons;

interface ITargetingMode
{
    string Name { get; }
    GameObject? SelectTarget(IEnumerable<GameObject> candidates, TargetingContext context);
}
