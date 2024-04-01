using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation;

namespace Bearded.TD.Game.Commands;

sealed class GameRequestDispatcher(GameInstance game)
{
    public void Request(IRequest<Player, GameInstance> request) => game.RequestDispatcher.Dispatch(game.Me, request);

    public void Request(Func<IRequest<Player, GameInstance>> func)
        => game.Request(func());
    public void Request<T>(Func<T, IRequest<Player, GameInstance>> func, T p)
        => game.Request(func(p));
    public void Request<T1, T2>(Func<T1, T2, IRequest<Player, GameInstance>> func, T1 p1, T2 p2)
        => game.Request(func(p1, p2));
    public void Request<T1, T2, T3>(Func<T1, T2, T3, IRequest<Player, GameInstance>> func, T1 p1, T2 p2, T3 p3)
        => game.Request(func(p1, p2, p3));

    public void Request(Func<GameState, IRequest<Player, GameInstance>> func)
        => game.Request(func(game.State));
    public void Request<T>(Func<GameState, T, IRequest<Player, GameInstance>> func, T p)
        => game.Request(func(game.State, p));
    public void Request<T1, T2>(Func<GameState, T1, T2, IRequest<Player, GameInstance>> func, T1 p1, T2 p2)
        => game.Request(func(game.State, p1, p2));
    public void Request<T1, T2, T3>(Func<GameState, T1, T2, T3, IRequest<Player, GameInstance>> func, T1 p1, T2 p2, T3 p3)
        => game.Request(func(game.State, p1, p2, p3));

    public void Request(Func<GameInstance, IRequest<Player, GameInstance>> func)
        => game.Request(func(game));
    public void Request<T>(Func<GameInstance, T, IRequest<Player, GameInstance>> func, T p)
        => game.Request(func(game, p));
    public void Request<T1, T2>(Func<GameInstance, T1, T2, IRequest<Player, GameInstance>> func, T1 p1, T2 p2)
        => game.Request(func(game, p1, p2));
    public void Request<T1, T2, T3>(Func<GameInstance, T1, T2, T3, IRequest<Player, GameInstance>> func, T1 p1, T2 p2, T3 p3)
        => game.Request(func(game, p1, p2, p3));
}
