using System;
using Bearded.TD.Game;

namespace Bearded.TD.Commands
{
    interface IDispatcher
    {
        void RunOnlyOnServer(Action action);
        void RunOnlyOnServer<T>(T self, Action<T> action);
        void RunOnlyOnServer<T>(Action<T> action, T p);
        void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2);
        void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3);

        void RunOnlyOnServer(Action<ICommandDispatcher> action);
        void RunOnlyOnServer<T>(T self, Action<T, ICommandDispatcher> action);
        void RunOnlyOnServer<T>(Action<T, ICommandDispatcher> action, T p);
        void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher> action, T1 p1, T2 p2);
        void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher> action, T1 p1, T2 p2, T3 p3);

        void RunOnlyOnServer(Func<ICommand> func);
        void RunOnlyOnServer<T>(T self, Func<T, ICommand> func);
        void RunOnlyOnServer<T>(Func<T, ICommand> func, T p);
        void RunOnlyOnServer<T1, T2>(Func<T1, T2, ICommand> func, T1 p1, T2 p2);
        void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ICommand> func, T1 p1, T2 p2, T3 p3);
    }

    static class DispatcherExtensions
    {
        private static IDispatcher d(GameObject obj) => obj.Game.Meta.Dispatcher;

        public static void OnServer<T>(this T obj, Action action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action);
        public static void OnServer<T>(this T obj, Action<T> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(obj, action);
        public static void OnServer<T, T1>(this T obj, Action<T1> action, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1);
        public static void OnServer<T, T1, T2>(this T obj, Action<T1, T2> action, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2);
        public static void OnServer<T, T1, T2, T3>(this T obj, Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2, p3);

        public static void OnServer<T>(this T obj, Action<ICommandDispatcher> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action);
        public static void OnServer<T>(this T obj, Action<T, ICommandDispatcher> action)
            where T : GameObject
            => d(obj).RunOnlyOnServer(obj, action);
        public static void OnServer<T, T1>(this T obj, Action<T1, ICommandDispatcher> action, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1);
        public static void OnServer<T, T1, T2>(this T obj, Action<T1, T2, ICommandDispatcher> action, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2);
        public static void OnServer<T, T1, T2, T3>(this T obj, Action<T1, T2, T3, ICommandDispatcher> action, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(action, p1, p2, p3);

        public static void OnServer<T>(this T obj, Func<ICommand> func)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func);
        public static void OnServer<T>(this T obj, Func<T, ICommand> func)
            where T : GameObject
            => d(obj).RunOnlyOnServer(obj, func);
        public static void OnServer<T, T1>(this T obj, Func<T1, ICommand> func, T1 p1)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1);
        public static void OnServer<T, T1, T2>(this T obj, Func<T1, T2, ICommand> func, T1 p1, T2 p2)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1, p2);
        public static void OnServer<T, T1, T2, T3>(this T obj, Func<T1, T2, T3, ICommand> func, T1 p1, T2 p2, T3 p3)
            where T : GameObject
            => d(obj).RunOnlyOnServer(func, p1, p2, p3);
    }

    abstract class BaseClientDispatcher : IDispatcher
    {
        public void RunOnlyOnServer(Action action) {}
        public void RunOnlyOnServer<T>(T self, Action<T> action) {}
        public void RunOnlyOnServer<T>(Action<T> action, T p) {}
        public void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2) {}
        public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3) {}

        public void RunOnlyOnServer(Action<ICommandDispatcher> action) {}
        public void RunOnlyOnServer<T>(T self, Action<T, ICommandDispatcher> action) {}
        public void RunOnlyOnServer<T>(Action<T, ICommandDispatcher> action, T p) {}
        public void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher> action, T1 p1, T2 p2) {}
        public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher> action, T1 p1, T2 p2, T3 p3) {}

        public void RunOnlyOnServer(Func<ICommand> func) {}
        public void RunOnlyOnServer<T>(T self, Func<T, ICommand> func) {}
        public void RunOnlyOnServer<T>(Func<T, ICommand> func, T p) {}
        public void RunOnlyOnServer<T1, T2>(Func<T1, T2, ICommand> func, T1 p1, T2 p2) {}

        public void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ICommand> func, T1 p1, T2 p2, T3 p3) {}
    }

    abstract class BaseServerDispatcher : IDispatcher
    {
        private readonly ICommandDispatcher commandDispatcher;

        protected BaseServerDispatcher(ICommandDispatcher commandDispatcher)
        {
            this.commandDispatcher = commandDispatcher;
        }

        public void RunOnlyOnServer(Action action) => action();
        public void RunOnlyOnServer<T>(T self, Action<T> action) => action(self);
        public void RunOnlyOnServer<T>(Action<T> action, T p) => action(p);
        public void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2) => action(p1, p2);
        public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3) => action(p1, p2, p3);

        public void RunOnlyOnServer(Action<ICommandDispatcher> action) => action(commandDispatcher);
        public void RunOnlyOnServer<T>(T self, Action<T, ICommandDispatcher> action) => action(self, commandDispatcher);
        public void RunOnlyOnServer<T>(Action<T, ICommandDispatcher> action, T p) => action(p, commandDispatcher);
        public void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher> action, T1 p1, T2 p2)
            => action(p1, p2, commandDispatcher);
        public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher> action, T1 p1, T2 p2, T3 p3)
            => action(p1, p2, p3, commandDispatcher);

        public void RunOnlyOnServer(Func<ICommand> func) => dispatch(func());
        public void RunOnlyOnServer<T>(T self, Func<T, ICommand> func) => dispatch(func(self));
        public void RunOnlyOnServer<T>(Func<T, ICommand> func, T p) => dispatch(func(p));
        public void RunOnlyOnServer<T1, T2>(Func<T1, T2, ICommand> func, T1 p1, T2 p2)
            => dispatch(func(p1, p2));
        public void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ICommand> func, T1 p1, T2 p2, T3 p3)
            => dispatch(func(p1, p2, p3));

        private void dispatch(ICommand command)
        {
            if (command == null)
                return;
            commandDispatcher.Dispatch(command);
        }
    }
}