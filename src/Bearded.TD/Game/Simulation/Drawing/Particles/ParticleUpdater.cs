using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

abstract class ParticleUpdater : Component
{
    protected Particles Particles { get; private set; } = null!;

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        ComponentDependencies.Depend<Particles>(Owner, Events, p => Particles = p);
    }
}

abstract class ParticleUpdater<TParameters> : Component<TParameters>
    where TParameters : IParametersTemplate<TParameters>
{
    protected Particles Particles { get; private set; } = null!;

    protected ParticleUpdater(TParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        ComponentDependencies.Depend<Particles>(Owner, Events, p => Particles = p);
    }
}
