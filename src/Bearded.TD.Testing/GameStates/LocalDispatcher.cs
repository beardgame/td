using Bearded.TD.Commands;
using Bearded.TD.Game;

namespace Bearded.TD.Testing.GameStates;

sealed class LocalDispatcher : BaseServerDispatcher<GameInstance>
{
    public LocalDispatcher() : base(new CommandDispatcher()) { }

    private sealed class CommandDispatcher : ICommandDispatcher<GameInstance>
    {
        private readonly ICommandExecutor executor = new DefaultCommandExecutor();

        public void Dispatch(ISerializableCommand<GameInstance>? command)
        {
            if (command != null)
            {
                executor.Execute(command);
            }
        }
    }
}
