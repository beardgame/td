using Bearded.TD.Rendering;
using Bearded.TD.Shared.TechEffects;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components
{
    abstract class Component<TOwner, TParameters> : IComponent<TOwner>
        where TParameters : IParametersTemplate<TParameters>
    {
        protected TParameters Parameters { get; }

        protected Component(TParameters parameters)
        {
            Parameters = parameters.CreateModifiableInstance();
        }

        public TOwner Owner { get; private set; }

        public void OnAdded(TOwner owner)
        {
            Owner = owner;
            Initialise();
        }

        protected abstract void Initialise();

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Draw(GeometryManager geometries);
    }

    abstract class Component<T> : Component<T, VoidParameters>
    {
        protected Component() : base(VoidParameters.Instance) { }
    }

    sealed class VoidParameters : IParametersTemplate<VoidParameters>
    {
        public static VoidParameters Instance => new VoidParameters();

        private VoidParameters() { }

        public VoidParameters CreateModifiableInstance() => this;

        public bool ModifyAttribute(AttributeType type, Modification modification) => false;
    }
}