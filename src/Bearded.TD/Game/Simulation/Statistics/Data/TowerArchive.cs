using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics.Data;

sealed class TowerArchive
{
    private readonly Dictionary<GameObject, TowerMetadata> metadataByObj = new();
    private readonly Dictionary<Id<GameObject>, TowerMetadata> metadataById = new();

    public TowerMetadata Find(GameObject obj) => metadataByObj[obj];
    public TowerMetadata Find(Id<GameObject> id) => metadataById[id];

    public bool EnsureTowerExists(GameObject obj)
    {
        if (metadataByObj.ContainsKey(obj)) return false;

        var metadata = TowerMetadata.FromObject(obj);
        metadataByObj.Add(obj, metadata);
        metadataById.Add(obj.FindId(), metadata);

        return true;
    }
}
