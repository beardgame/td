using System;
using System.Collections.Generic;
using Bearded.TD.Game.Resources;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Directors
{
    sealed class WaveDirector
    {
        private readonly GameState game;
        private readonly List<SingleWaveDirector> waves = new();

        public WaveDirector(GameState game)
        {
            this.game = game;
        }

        public void ExecuteScript(WaveScript script)
        {
            waves.Add(new SingleWaveDirector(game, script));
        }

        public void Update()
        {
            foreach (var wave in waves)
            {
                wave.Update();
            }
            waves.RemoveAll(w => w.IsDone);
        }

        private sealed class SingleWaveDirector
        {
            private enum Phase
            {
                Downtime,
                Spawning,
                FinishOff,
                Completed,
            }

            private readonly GameState game;
            private readonly WaveScript script;

            private Phase phase;
            private ResourceAmount resourcesGiven;

            public bool IsDone => phase == Phase.Completed;

            public SingleWaveDirector(GameState game, WaveScript script)
            {
                this.game = game;
                this.script = script;
                phase = Phase.Downtime;
            }

            public void Update()
            {
                switch (phase)
                {
                    case Phase.Downtime:
                        if (game.Time >= script.SpawnStart)
                        {
                            phase = Phase.Spawning;
                            updateResources();
                        }
                        break;
                    case Phase.Spawning:
                        updateResources();
                        if (game.Time >= script.SpawnEnd)
                        {
                            phase = Phase.FinishOff;
                        }
                        break;
                    case Phase.FinishOff:
                        // TODO: detect if all enemies are killed. If so, move to completed.
                        break;
                    case Phase.Completed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void updateResources()
            {
                var spawnTimeElapsed = game.Time - script.SpawnStart;
                var percentageTimeElapsed = Math.Clamp(spawnTimeElapsed / script.SpawnDuration, 0, 1);
                var expectedResourcesGiven = script.ResourcesAwardedBySpawnPhase.DiscretizedPercentage(percentageTimeElapsed);
                State.Satisfies(expectedResourcesGiven >= resourcesGiven);
                script.TargetFaction.Resources.ProvideOneTimeResource(expectedResourcesGiven - resourcesGiven);
                resourcesGiven = expectedResourcesGiven;
            }
        }
    }
}
