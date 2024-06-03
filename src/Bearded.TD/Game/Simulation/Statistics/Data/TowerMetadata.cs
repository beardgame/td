using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Statistics.Data;

sealed class TowerMetadata
{
    public Id<GameObject> Id { get; }
    public IObjectAttributes Attributes { get; }
    public GameObject? LiveObject { get; private set; }

    private TowerMetadata(Id<GameObject> id, IObjectAttributes attributes, GameObject? liveObject)
    {
        Id = id;
        Attributes = attributes;
        LiveObject = liveObject;
        if (liveObject != null)
        {
            liveObject.Deleting += () => LiveObject = null;
        }
    }

    public static TowerMetadata FromObject(GameObject gameObject)
    {
        var attributes = gameObject.GetComponents<IObjectAttributes>().SingleOrDefault(ObjectAttributes.Default);
        return new TowerMetadata(gameObject.FindId(), attributes, gameObject);
    }

    public static TowerMetadata CreateDefault() => new(Id<GameObject>.Invalid, ObjectAttributes.Default, null);
}
