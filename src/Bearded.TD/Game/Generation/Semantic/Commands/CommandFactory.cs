using Bearded.TD.Commands;

namespace Bearded.TD.Game.Generation.Semantic.Commands
{
    delegate ISerializableCommand<GameInstance> CommandFactory(GameInstance game);
}
