using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components
{
    abstract class Component<TOwner, TParameters> : IComponent<TOwner>
        where TParameters : IParametersTemplate<TParameters>
    {
        protected TParameters Parameters { get; }

        protected Component(TParameters parameters)
        {
            Parameters = parameters.CreateModifiableInstance();
        }

        protected TOwner Owner { get; private set; }

        protected ComponentEvents Events { get; private set; }

        public void OnAdded(TOwner owner, ComponentEvents events)
        {
            Owner = owner;
            Events = events;
            OnAdded();
        }

        protected abstract void OnAdded();

        public virtual void OnRemoved() {}

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Draw(CoreDrawers drawers);

        public virtual bool CanApplyUpgradeEffect(IUpgradeEffect effect) => effect.CanApplyTo(Parameters);

        public virtual void ApplyUpgradeEffect(IUpgradeEffect effect) => effect.ApplyTo(Parameters);

        public virtual bool RemoveUpgradeEffect(IUpgradeEffect effect) => effect.RemoveFrom(Parameters);
    }

    abstract class Component<TOwner> : IComponent<TOwner>
    {
        protected TOwner Owner { get; private set; }

        protected ComponentEvents Events { get; private set; } = null!;

        public void OnAdded(TOwner owner, ComponentEvents events)
        {
            Owner = owner;
            Events = events;
            OnAdded();
        }

        protected abstract void OnAdded();

        public virtual void OnRemoved() {}

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Draw(CoreDrawers drawers);

        public virtual bool CanApplyUpgradeEffect(IUpgradeEffect effect) => false;

        public virtual void ApplyUpgradeEffect(IUpgradeEffect effect) { }

        public virtual bool RemoveUpgradeEffect(IUpgradeEffect effect) => false;
    }

    sealed class VoidParameters : IParametersTemplate<VoidParameters>
    {
        public static VoidParameters Instance => new VoidParameters();

        private VoidParameters() { }

        public VoidParameters CreateModifiableInstance() => this;

        public bool HasAttributeOfType(AttributeType type) => false;

        public bool AddModification(AttributeType type, Modification modification) => false;
        public bool AddModificationWithId(AttributeType type, ModificationWithId modification) => false;
        public bool UpdateModification(AttributeType type, Id<Modification> id, Modification modification) => false;
        public bool RemoveModification(AttributeType type, Id<Modification> id) => false;
    }
}
