using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands.Directors;
using Bearded.TD.Game.GameState.GameLoop;
using Bearded.Utilities;

namespace Bearded.TD.Game.Directors
{
    sealed class WaveScheduler
    {
        private readonly GameInstance game;
        private readonly ICommandDispatcher<GameInstance> commandDispatcher;
        public event VoidEventHandler? WaveEnded;

        public WaveScheduler(GameInstance game, ICommandDispatcher<GameInstance> commandDispatcher)
        {
            this.game = game;
            this.commandDispatcher = commandDispatcher;
        }

        public void StartWave(WaveRequirements requirements)
        {
            commandDispatcher.Dispatch(ExecuteWaveScript.Command(game, createWaveScript(requirements)));
        }

        private WaveScript createWaveScript(WaveRequirements requirements)
        {
            throw new NotImplementedException();
        }

        public readonly struct WaveRequirements
        {
        }
    }
}
