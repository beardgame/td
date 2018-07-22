﻿using amulware.Graphics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units
{
    interface IUnitBlueprint : IBlueprint
    {
        string Name { get; }
        int Health { get; }
        int Damage { get; }
        TimeSpan TimeBetweenAttacks { get; }
        Speed Speed { get; }
        float Value { get; }
        Color Color { get; }
    }
}