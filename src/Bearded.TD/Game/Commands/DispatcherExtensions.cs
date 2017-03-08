using System;
using Bearded.TD.Commands;

namespace Bearded.TD.Game.Commands
{
    static class DispatcherExtensions
    {
        private static IDispatcher d(GameObject obj) => obj.Game.Meta.Dispatcher;

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

        public static void Sync<T>(this T obj, Action<ICommandDispatcher> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action);
        public static void Sync<T, T1>(this T obj, Action<T1, ICommandDispatcher> action, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1);
        public static void Sync<T, T1, T2>(this T obj, Action<T1, T2, ICommandDispatcher> action, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2);
        public static void Sync<T, T1, T2, T3>(this T obj, Action<T1, T2, T3, ICommandDispatcher> action, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2, p3);

        public static void Sync<T>(this T obj, Func<ICommand> func)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func);
        public static void Sync<T, T1>(this T obj, Func<T1, ICommand> func, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1);
        public static void Sync<T, T1, T2>(this T obj, Func<T1, T2, ICommand> func, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1, p2);
        public static void Sync<T, T1, T2, T3>(this T obj, Func<T1, T2, T3, ICommand> func, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1, p2, p3);

        // convenience methods

        public static void Sync<T>(this T obj, Action<T> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, obj);
        public static void Sync<T>(this T obj, Action<GameState> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, obj.Game);

        public static void Sync<T>(this T obj, Action<T, ICommandDispatcher> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, obj);
        public static void Sync<T>(this T obj, Action<GameState, ICommandDispatcher> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, obj.Game);

        public static void Sync<T>(this T obj, Func<T, ICommand> func)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, obj);
        public static void Sync<T>(this T obj, Func<GameState, ICommand> func)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, obj.Game);
    }
}