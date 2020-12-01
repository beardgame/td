using System;
using System.Collections.Generic;
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

        public void StartWave(WaveScript script)
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

        private enum Phase
        {
            Downtime,
            Spawning,
            FinishOff,
            Completed,
        }

        private sealed class SingleWaveDirector
        {
            private readonly GameState game;
            private readonly WaveScript script;

            private Phase phase;
            private double resourcesGiven;

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
                var resourcesPerSecond = script.ResourcesAwardedOverTime / script.SpawnDuration;
                var spawnTimeElapsed = game.Time - script.SpawnStart;
                var expectedResourcesGiven = Math.Clamp(
                    spawnTimeElapsed * resourcesPerSecond, 0, script.ResourcesAwardedOverTime);
                State.Satisfies(expectedResourcesGiven >= resourcesGiven);
                script.TargetFaction.Resources.ProvideOneTimeResource(expectedResourcesGiven - resourcesGiven);
                resourcesGiven = expectedResourcesGiven;
            }
        }
    }
}
