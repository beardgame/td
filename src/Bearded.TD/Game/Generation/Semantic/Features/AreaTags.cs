using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed class AreaTags
    {
        private readonly IArea area;
        private readonly Dictionary<string, AreaTagValues> layers = new();

        public AreaTags(IArea area)
        {
            this.area = area;
        }

        public AreaTagValues this[string name]
        {
            get
            {
                if (!layers.TryGetValue(name, out var layer))
                {
                    layer = new AreaTagValues(area);
                    layers.Add(name, layer);
                }

                return layer;
            }
        }
    }
}
