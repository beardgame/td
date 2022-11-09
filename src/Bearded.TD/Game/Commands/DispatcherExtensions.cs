using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Commands;

static class DispatcherExtensions
{
    private static IDispatcher<GameInstance> d(IGameObject obj) => obj.Game.Meta.Dispatcher;

    public static void Sync(this GameObject obj, Action<ICommandDispatcher<GameInstance>> action)
        => d(obj).RunOnlyOnServer(action);
    public static void Sync<T1>(this GameObject obj, Action<T1, ICommandDispatcher<GameInstance>> action, T1 p1)
        => d(obj).RunOnlyOnServer(action, p1);
    public static void Sync<T1, T2>(this GameObject obj, Action<T1, T2, ICommandDispatcher<GameInstance>> action, T1 p1, T2 p2)
        => d(obj).RunOnlyOnServer(action, p1, p2);
    public static void Sync<T1, T2, T3>(this GameObject obj, Action<T1, T2, T3, ICommandDispatcher<GameInstance>> action, T1 p1, T2 p2, T3 p3)
        => d(obj).RunOnlyOnServer(action, p1, p2, p3);
    public static void Sync<T1, T2, T3, T4>(this GameObject obj, Action<T1, T2, T3, T4, ICommandDispatcher<GameInstance>> action, T1 p1, T2 p2, T3 p3, T4 p4)
        => d(obj).RunOnlyOnServer(action, p1, p2, p3, p4);

    public static void Sync(this GameObject obj, Func<ISerializableCommand<GameInstance>> func)
        => d(obj).RunOnlyOnServer(func);
    public static void Sync<T1>(this GameObject obj, Func<T1, ISerializableCommand<GameInstance>> func, T1 p1)
        => d(obj).RunOnlyOnServer(func, p1);
    public static void Sync<T1, T2>(this GameObject obj, Func<T1, T2, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2)
        => d(obj).RunOnlyOnServer(func, p1, p2);
    public static void Sync<T1, T2, T3>(this GameObject obj, Func<T1, T2, T3, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2, T3 p3)
        => d(obj).RunOnlyOnServer(func, p1, p2, p3);
    public static void Sync<T1, T2, T3, T4>(this GameObject obj, Func<T1, T2, T3, T4, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2, T3 p3, T4 p4)
        => d(obj).RunOnlyOnServer(func, p1, p2, p3, p4);

    public static void Sync(this GameObject obj, Action<GameObject, ICommandDispatcher<GameInstance>> action)
        => d(obj).RunOnlyOnServer(action, obj);
    public static void Sync(this  GameObject obj, Action<GameState, ICommandDispatcher<GameInstance>> action)
        => d(obj).RunOnlyOnServer(action, obj.Game);

    public static void Sync(this GameObject obj, Func<GameObject, ISerializableCommand<GameInstance>> func)
        => d(obj).RunOnlyOnServer(func, obj);
    public static void Sync(this GameObject obj, Func<GameState, ISerializableCommand<GameInstance>> func)
        => d(obj).RunOnlyOnServer(func, obj.Game);
    public static void Sync<T1>(this GameObject obj, Func<GameState, T1, ISerializableCommand<GameInstance>> func, T1 p1)
        => d(obj).RunOnlyOnServer(func, obj.Game, p1);
    public static void Sync<T1, T2>(this GameObject obj, Func<GameState, T1, T2, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2)
        => d(obj).RunOnlyOnServer(func, obj.Game, p1, p2);
    public static void Sync<T1, T2, T3>(this GameObject obj, Func<GameState, T1, T2, T3, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2, T3 p3)
        => d(obj).RunOnlyOnServer(func, obj.Game, p1, p2, p3);
}
