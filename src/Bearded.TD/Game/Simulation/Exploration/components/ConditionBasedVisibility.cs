using System;
using Bearded.TD.Game.Simulation.GameObjects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Exploration;

sealed class ConditionBasedVisibility : Component, IVisibility
{
    private readonly Func<GameObject, bool> condition;

    public ObjectVisibility Visibility => condition(Owner) ? ObjectVisibility.Visible : ObjectVisibility.Invisible;

    public ConditionBasedVisibility(Func<GameObject, bool> condition)
    {
        this.condition = condition;
    }

    protected override void OnAdded() {}

    public override void Update(TimeSpan elapsedTime) {}
}
