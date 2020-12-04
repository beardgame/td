using Bearded.TD.Game.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Directors
{
    sealed class WaveScriptSerializer
    {
        private Id<int> id;
        private Id<Faction> targetFaction;
        private double spawnStart;
        private double spawnEnd;
        private double resourcesAwardedOverTime;

        public WaveScriptSerializer() {}

        public WaveScriptSerializer(WaveScript waveScript)
        {
            id = waveScript.Id;
            targetFaction = waveScript.TargetFaction.Id;
        }

        public WaveScript ToWaveScript(GameState game)
        {
            return new(
                id,
                game.FactionFor(targetFaction),
                new Instant(spawnStart),
                new Instant(spawnEnd),
                resourcesAwardedOverTime);
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref id);
            stream.Serialize(ref targetFaction);
            stream.Serialize(ref spawnStart);
            stream.Serialize(ref spawnEnd);
            stream.Serialize(ref resourcesAwardedOverTime);
        }
    }
}
