using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;

namespace Bearded.TD.Game.Commands
{
    static class DispatcherExtensions
    {
        private static IDispatcher<GameInstance> d(GameObject obj) => obj.Game.Meta.Dispatcher;

        // direct pass throughs

        public static void Sync<T>(this T obj, Action action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action);
        public static void Sync<T, T1>(this T obj, Action<T1> action, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1);
        public static void Sync<T, T1, T2>(this T obj, Action<T1, T2> action, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2);
        public static void Sync<T, T1, T2, T3>(this T obj, Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2, p3);
        public static void Sync<T, T1, T2, T3, T4>(this T obj, Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2, p3, p4);

        public static void Sync<T>(this T obj, Action<ICommandDispatcher<GameInstance>> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action);
        public static void Sync<T, T1>(this T obj, Action<T1, ICommandDispatcher<GameInstance>> action, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1);
        public static void Sync<T, T1, T2>(this T obj, Action<T1, T2, ICommandDispatcher<GameInstance>> action, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2);
        public static void Sync<T, T1, T2, T3>(this T obj, Action<T1, T2, T3, ICommandDispatcher<GameInstance>> action, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2, p3);
        public static void Sync<T, T1, T2, T3, T4>(this T obj, Action<T1, T2, T3, T4, ICommandDispatcher<GameInstance>> action, T1 p1, T2 p2, T3 p3, T4 p4)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2, p3, p4);

        public static void Sync<T>(this T obj, Func<ISerializableCommand<GameInstance>> func)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func);
        public static void Sync<T, T1>(this T obj, Func<T1, ISerializableCommand<GameInstance>> func, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1);
        public static void Sync<T, T1, T2>(this T obj, Func<T1, T2, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1, p2);
        public static void Sync<T, T1, T2, T3>(this T obj, Func<T1, T2, T3, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1, p2, p3);
        public static void Sync<T, T1, T2, T3, T4>(this T obj, Func<T1, T2, T3, T4, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2, T3 p3, T4 p4)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1, p2, p3, p4);

        // convenience methods

        public static void Sync<T>(this T obj, Action<T> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, obj);
        public static void Sync<T>(this T obj, Action<GameState> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, obj.Game);

        public static void Sync<T>(this T obj, Action<T, ICommandDispatcher<GameInstance>> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, obj);
        public static void Sync<T>(this T obj, Action<GameState, ICommandDispatcher<GameInstance>> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, obj.Game);

        public static void Sync<T>(this T obj, Func<T, ISerializableCommand<GameInstance>> func)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, obj);
        public static void Sync<T>(this T obj, Func<GameState, ISerializableCommand<GameInstance>> func)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, obj.Game);
        public static void Sync<T, T1>(this T obj, Func<GameState, T1, ISerializableCommand<GameInstance>> func, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, obj.Game, p1);
        public static void Sync<T, T1, T2>(this T obj, Func<GameState, T1, T2, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, obj.Game, p1, p2);
        public static void Sync<T, T1, T2, T3>(this T obj, Func<GameState, T1, T2, T3, ISerializableCommand<GameInstance>> func, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, obj.Game, p1, p2, p3);
    }
}
