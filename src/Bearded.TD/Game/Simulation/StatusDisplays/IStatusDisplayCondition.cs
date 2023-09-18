using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

interface IStatusDisplayCondition
{
    bool ShouldDraw { get; }

    void Activate(GameObject owner);
}
