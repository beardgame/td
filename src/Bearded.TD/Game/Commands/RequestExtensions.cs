using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;

namespace Bearded.TD.Game.Commands
{
    static class RequestExtensions
    {
        public static void Request(this GameInstance game, IRequest<Player, GameInstance> request)
            => game.RequestDispatcher.Dispatch(game.Me, request);

        public static void Request(this GameInstance game, Func<IRequest<Player, GameInstance>> func)
            => game.Request(func());
        public static void Request<T>(this GameInstance game, Func<T, IRequest<Player, GameInstance>> func, T p)
            => game.Request(func(p));
        public static void Request<T1, T2>(this GameInstance game, Func<T1, T2, IRequest<Player, GameInstance>> func, T1 p1, T2 p2)
            => game.Request(func(p1, p2));
        public static void Request<T1, T2, T3>(this GameInstance game, Func<T1, T2, T3, IRequest<Player, GameInstance>> func, T1 p1, T2 p2, T3 p3)
            => game.Request(func(p1, p2, p3));

        public static void Request(this GameInstance game, Func<GameState.GameState, IRequest<Player, GameInstance>> func)
            => game.Request(func(game.State));
        public static void Request<T>(this GameInstance game, Func<GameState.GameState, T, IRequest<Player, GameInstance>> func, T p)
            => game.Request(func(game.State, p));
        public static void Request<T1, T2>(this GameInstance game, Func<GameState.GameState, T1, T2, IRequest<Player, GameInstance>> func, T1 p1, T2 p2)
            => game.Request(func(game.State, p1, p2));
        public static void Request<T1, T2, T3>(this GameInstance game, Func<GameState.GameState, T1, T2, T3, IRequest<Player, GameInstance>> func, T1 p1, T2 p2, T3 p3)
            => game.Request(func(game.State, p1, p2, p3));


        public static void Request(this GameInstance game, Func<GameInstance, IRequest<Player, GameInstance>> func)
            => game.Request(func(game));
        public static void Request<T>(this GameInstance game, Func<GameInstance, T, IRequest<Player, GameInstance>> func, T p)
            => game.Request(func(game, p));
        public static void Request<T1, T2>(this GameInstance game, Func<GameInstance, T1, T2, IRequest<Player, GameInstance>> func, T1 p1, T2 p2)
            => game.Request(func(game, p1, p2));
        public static void Request<T1, T2, T3>(this GameInstance game, Func<GameInstance, T1, T2, T3, IRequest<Player, GameInstance>> func, T1 p1, T2 p2, T3 p3)
            => game.Request(func(game, p1, p2, p3));
    }
}
