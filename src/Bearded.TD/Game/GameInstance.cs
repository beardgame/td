using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public Player Me { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        
        public ChatLog ChatLog { get; } = new ChatLog();
        public IdManager Ids { get; }

        public GameState State { get; private set; }
        public GameCamera Camera { get; private set; }
        public CursorState Cursor { get; private set; }

        private readonly List<Player> players = new List<Player>();
        private readonly Dictionary<Player> playersById = new Dictionary<Player>();
        public ReadOnlyCollection<Player> Players { get; }

        public GameInstance(Player me, IRequestDispatcher requestDispatcher, IdManager ids)
        {
            Me = me;
            RequestDispatcher = requestDispatcher;
            Ids = ids;

            AddPlayer(me);
            Players = players.AsReadOnly();
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
            playersById.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player);
            playersById.Remove(player);
        }

        public Player PlayerFor(Id<Player> id) => playersById[id];

        public void Start(GameState state, GameCamera camera)
        {
            if (State != null)
                throw new Exception();

            State = state;
            Camera = camera;
            Cursor = new CursorState(this); // bad.
        }
    }
}
