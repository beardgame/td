using System;
using Bearded.TD.Commands;

namespace Bearded.TD.Game.Commands
{
    static class RequestExtensions
    {
        public static void Request(this GameInstance game, IRequest<GameInstance> request)
            => game.RequestDispatcher.Dispatch(request);

        public static void Request(this GameInstance game, Func<IRequest<GameInstance>> func)
            => game.Request(func());
        public static void Request<T>(this GameInstance game, Func<T, IRequest<GameInstance>> func, T p)
            => game.Request(func(p));
        public static void Request<T1, T2>(this GameInstance game, Func<T1, T2, IRequest<GameInstance>> func, T1 p1, T2 p2)
            => game.Request(func(p1, p2));
        public static void Request<T1, T2, T3>(this GameInstance game, Func<T1, T2, T3, IRequest<GameInstance>> func, T1 p1, T2 p2, T3 p3)
            => game.Request(func(p1, p2, p3));

        public static void Request(this GameInstance game, Func<GameState, IRequest<GameInstance>> func)
            => game.Request(func(game.State));
        public static void Request<T>(this GameInstance game, Func<GameState, T, IRequest<GameInstance>> func, T p)
            => game.Request(func(game.State, p));
        public static void Request<T1, T2>(this GameInstance game, Func<GameState, T1, T2, IRequest<GameInstance>> func, T1 p1, T2 p2)
            => game.Request(func(game.State, p1, p2));
        public static void Request<T1, T2, T3>(this GameInstance game, Func<GameState, T1, T2, T3, IRequest<GameInstance>> func, T1 p1, T2 p2, T3 p3)
            => game.Request(func(game.State, p1, p2, p3));


        public static void Request(this GameInstance game, Func<GameInstance, IRequest<GameInstance>> func)
            => game.Request(func(game));
        public static void Request<T>(this GameInstance game, Func<GameInstance, T, IRequest<GameInstance>> func, T p)
            => game.Request(func(game, p));
        public static void Request<T1, T2>(this GameInstance game, Func<GameInstance, T1, T2, IRequest<GameInstance>> func, T1 p1, T2 p2)
            => game.Request(func(game, p1, p2));
        public static void Request<T1, T2, T3>(this GameInstance game, Func<GameInstance, T1, T2, T3, IRequest<GameInstance>> func, T1 p1, T2 p2, T3 p3)
            => game.Request(func(game, p1, p2, p3));
    }
}