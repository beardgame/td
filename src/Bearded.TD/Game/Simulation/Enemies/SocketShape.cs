using System.ComponentModel;
using Bearded.TD.Game.Simulation.Enemies.Serialization;

namespace Bearded.TD.Game.Simulation.Enemies;

[TypeConverter(typeof(SocketShapeTypeConverter))]
readonly record struct SocketShape(string Id)
{
    public static SocketShape FromLiteral(string s) => new(s);
}
