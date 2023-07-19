using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class PlayerStatusUI
{
    private GameInstance game;

    private readonly List<PlayerModel> players = new List<PlayerModel>();

    public ReadOnlyCollection<PlayerModel> Players { get; }

    public event VoidEventHandler? PlayersChanged;

    public PlayerStatusUI()
    {
        Players = players.AsReadOnly();
    }

    public void Initialize(GameInstance game)
    {
        this.game = game;
    }

    public void Update()
    {
        if (players.Count != game.SortedPlayers.Count)
        {
            resetPlayerList();
            return;
        }

        for (var i = 0; i < players.Count; i++)
        {
            if (players[i].Id != game.SortedPlayers[i].Id)
            {
                resetPlayerList();
                break;
            }
            players[i].Ping = game.SortedPlayers[i].LastKnownPing;
        }
    }

    private void resetPlayerList()
    {
        players.Clear();
        foreach (var player in game.SortedPlayers)
        {
            players.Add(new PlayerModel(player.Id, player.Name, player.Color, player.LastKnownPing));
        }
        PlayersChanged?.Invoke();
    }

    public sealed class PlayerModel
    {
        public Id<Player> Id { get; }
        public string Name { get; }
        public Color Color { get; }
        public int Ping { get; set; }

        public PlayerModel(Id<Player> id, string name, Color color, int ping)
        {
            Name = name;
            Color = color;
            Id = id;
            Ping = ping;
        }
    }
}