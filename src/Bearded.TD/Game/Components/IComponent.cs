﻿using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    interface IComponent<TOwner>
    {
        TOwner Owner { get; }

        void Draw(GeometryManager geometries);
        void OnAdded(TOwner owner);
        void Update(TimeSpan elapsedTime);
    }
}