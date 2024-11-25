using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Modules;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Drawing.Particles.InheritParticleEffects;

namespace Bearded.TD.Game.Simulation.Drawing.Particles;

[Component("inheritParticleEffects")]
sealed class InheritParticleEffects(IParameters parameters) : Component<IParameters>(parameters)
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        string SourceEffectChild { get; }
        IGameObjectBlueprint NewEffect { get; }
        bool DeleteSourceChild { get; }
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        if (Owner.Parent is not { } parent)
            return;

        foreach (var child in parent.GetComponents<Child>())
        {
            if (child.Name != Parameters.SourceEffectChild)
                continue;

            inherit(child);
        }

        Owner.RemoveComponent(this);
    }

    private void inherit(Child child)
    {
        if (child.ChildObject is not { } source)
            return;

        if (!source.TryGetSingleComponent<Particles>(out var sourceParticles))
            return;

        var obj = GameObjectFactory
            .CreateFromBlueprintWithDefaultRenderer(Parameters.NewEffect, Owner, Owner.Position, Owner.Direction);

        if (!obj.TryGetSingleComponent<Particles>(out var particles))
            return;

        obj.AddComponent(new AttachToParent());
        Owner.Game.Add(obj);

        particles.AddParticles(sourceParticles.ImmutableParticles);

        if (Parameters.DeleteSourceChild)
        {
            source.Delete();
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

