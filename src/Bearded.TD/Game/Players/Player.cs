using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Players;

/// <summary>
/// In the context of TD, a Player is a connected client.
/// </summary>
sealed class Player : IIdable<Player>
{
    public Id<Player> Id { get; }
    public string Name { get; }
    public Faction Faction { get; private set; }
    public PlayerConnectionState ConnectionState { get; set; }
    public int LastKnownPing { get; set; } = -1;

    public Color Color => Faction.Color;

    public Player(Id<Player> id, string name)
    {
        Id = id;
        Name = name;
        ConnectionState = PlayerConnectionState.Unknown;
    }

    public void SetFaction(Faction faction)
    {
        Faction = faction;
    }

    public PlayerState GetCurrentState()
    {
        return new PlayerState((byte) ConnectionState, LastKnownPing);
    }

    public void SyncFrom(PlayerState state)
    {
        ConnectionState = (PlayerConnectionState) state.ConnectionState;
        LastKnownPing = state.LastKnownPing;
    }
}