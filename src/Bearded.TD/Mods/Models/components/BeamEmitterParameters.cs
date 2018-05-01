using amulware.Graphics;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models.components
{
    sealed class BeamEmitterParameters
    {
        public int DamagePerSecond { get; } = 10;
        public Unit Range { get; } = 5.U();
        public Color Color { get; } = Color.Orange;

        [JsonConstructor]
        public BeamEmitterParameters(int? damagePerSecond, Unit? range, Color? color)
        {
            DamagePerSecond = damagePerSecond ?? DamagePerSecond;
            Range = range ?? Range;
            Color = color ?? Color;
        }
    }
}
