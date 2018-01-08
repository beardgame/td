using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class TileVisibilityParameters
    {
        public Unit Range { get; }

        [JsonConstructor]
        public TileVisibilityParameters(Unit range)
        {
            Range = range;
        }
    }
}
