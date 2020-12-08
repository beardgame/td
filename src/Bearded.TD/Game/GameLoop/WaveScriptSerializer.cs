using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.GameState.GameLoop;
using Bearded.TD.Game.GameState.Resources;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Directors
{
    sealed class WaveScriptSerializer
    {
        private Id<WaveScript> id;
        private Id<Faction> targetFaction;
        private double spawnStart;
        private double spawnDuration;
        private double resourcesAwardedBySpawnPhase;

        public WaveScriptSerializer() {}

        public WaveScriptSerializer(WaveScript waveScript)
        {
            id = waveScript.Id;
            targetFaction = waveScript.TargetFaction.Id;
            spawnStart = waveScript.SpawnStart.NumericValue;
            spawnDuration = waveScript.SpawnDuration.NumericValue;
            resourcesAwardedBySpawnPhase = waveScript.ResourcesAwardedBySpawnPhase.NumericValue;
        }

        public WaveScript ToWaveScript(GameState.GameState game)
        {
            return new(
                id,
                game.FactionFor(targetFaction),
                new Instant(spawnStart),
                new TimeSpan(spawnDuration),
                new ResourceAmount(resourcesAwardedBySpawnPhase));
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref id);
            stream.Serialize(ref targetFaction);
            stream.Serialize(ref spawnStart);
            stream.Serialize(ref spawnDuration);
            stream.Serialize(ref resourcesAwardedBySpawnPhase);
        }
    }
}
