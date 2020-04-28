using Bearded.TD.Game.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Rules
{
    interface IGameRule<in TOwner>
    {
        void OnAdded(TOwner owner, GlobalGameEvents events);

        void Update(TimeSpan elapsedTime);

        void Draw(GeometryManager geometries);
    }
}
