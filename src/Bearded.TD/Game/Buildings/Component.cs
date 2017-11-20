using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    abstract class Component<TOwner, TParameters> : IComponent<TOwner>
    {
        protected TParameters Parameters { get; }

        protected Component(TParameters parameters)
        {
            Parameters = parameters;
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

    abstract class Component : Component<Building, Bearded.Utilities.Void>
    {
        protected Component() : base(default(Bearded.Utilities.Void)) { }

        public Building Building => Owner;
    }
}