using System;
using Bearded.TD.Commands;

namespace Bearded.TD.Game.Commands
{
    static class RequestExtensions
    {
        public static void Request(this GameInstance game, IRequest request)
            => game.RequestDispatcher.Dispatch(request);

        public static void Request(this GameInstance game, Func<IRequest> func)
            => game.Request(func());
        public static void Request<T>(this GameInstance game, Func<T, IRequest> func, T p)
            => game.Request(func(p));
        public static void Request<T1, T2>(this GameInstance game, Func<T1, T2, IRequest> func, T1 p1, T2 p2)
            => game.Request(func(p1, p2));
        public static void Request<T1, T2, T3>(this GameInstance game, Func<T1, T2, T3, IRequest> func, T1 p1, T2 p2, T3 p3)
            => game.Request(func(p1, p2, p3));

        public static void Request(this GameInstance game, Func<GameState, IRequest> func)
            => game.Request(func(game.State));
        public static void Request<T>(this GameInstance game, Func<GameState, T, IRequest> func, T p)
            => game.Request(func(game.State, p));
        public static void Request<T1, T2>(this GameInstance game, Func<GameState, T1, T2, IRequest> func, T1 p1, T2 p2)
            => game.Request(func(game.State, p1, p2));
        public static void Request<T1, T2, T3>(this GameInstance game, Func<GameState, T1, T2, T3, IRequest> func, T1 p1, T2 p2, T3 p3)
            => game.Request(func(game.State, p1, p2, p3));
    }
}