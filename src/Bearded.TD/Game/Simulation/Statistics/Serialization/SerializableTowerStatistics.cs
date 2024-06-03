// using System.Collections.Immutable;
// using Bearded.TD.Game.Simulation.GameObjects;
// using Bearded.TD.Game.Simulation.Statistics.Data;
// using Bearded.TD.Networking.Serialization;
// using Bearded.Utilities;
// using JetBrains.Annotations;
//
// namespace Bearded.TD.Game.Simulation.Statistics.Serialization;
//
// sealed class SerializableTowerStatistics
// {
//     private Id<GameObject> gameObject;
//
//     [UsedImplicitly]
//     public SerializableTowerStatistics() {}
//
//     public SerializableTowerStatistics(TowerStatistics instance)
//     {
//         gameObject = instance.GameObject.FindId();
//     }
//
//     public void Serialize(INetBufferStream stream)
//     {
//         stream.Serialize(ref gameObject);
//     }
//
//     public TowerStatistics ToInstance(GameInstance game)
//     {
//         var obj = game.State.Find(gameObject);
//         return new TowerStatistics(obj, ImmutableArray<TypedAccumulatedDamage>.Empty);
//     }
// }
