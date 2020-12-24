using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Controls
{
    sealed class GameStatusUI : IListener<WaveScheduled>, IListener<WaveEnded>
    {
        public event VoidEventHandler? StatusChanged;

        private GameInstance game = null!;

        private Id<WaveScript> currentWave;
        private Instant? waveSpawnStart;

        public string? WaveName { get; private set; }
        public ResourceAmount? WaveResources { get; private set; }

        public string FactionName => game.Me.Faction.Name;
        public Color FactionColor => game.Me.Faction.Color;
        public ResourceAmount FactionResources => game.Me.Faction.Resources.CurrentResources;
        public ResourceAmount FactionResourcesAfterReservation => game.Me.Faction.Resources.ResourcesAfterQueue;
        public long FactionTechPoints => game.Me.Faction.Technology?.TechPoints ?? 0;
        public TimeSpan? TimeUntilWaveSpawn =>
            waveSpawnStart == null || game.State.Time >= waveSpawnStart
                ? null
                : waveSpawnStart - game.State.Time;

        public void Initialize(GameInstance game)
        {
            this.game = game;
            game.Meta.Events.Subscribe<WaveScheduled>(this);
            game.Meta.Events.Subscribe<WaveEnded>(this);
        }

        public void Update()
        {
            StatusChanged?.Invoke();
        }

        public void HandleEvent(WaveScheduled @event)
        {
            currentWave = @event.WaveId;
            WaveName = @event.WaveName;
            waveSpawnStart = @event.SpawnStart;
            WaveResources = @event.ResourceAmount;
        }

        public void HandleEvent(WaveEnded @event)
        {
            if (@event.WaveId != currentWave)
            {
                return;
            }

            currentWave = Id<WaveScript>.Invalid;
            WaveName = null;
            waveSpawnStart = null;
            WaveResources = null;
        }
    }
}
