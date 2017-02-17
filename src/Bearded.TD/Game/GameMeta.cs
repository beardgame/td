using Bearded.Utilities;

namespace Bearded.TD.Game
{
    sealed class GameMeta
    {
        public Logger Logger { get; }

        public GameMeta(Logger logger) {
            Logger = logger;
        }
    }
}
