﻿using System;

namespace Bearded.TD.Commands
{
    interface IDispatcher<out TContext>
    {
        void RunOnlyOnServer(Action action);
        void RunOnlyOnServer<T>(Action<T> action, T p);
        void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2);
        void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3);
        void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4);

        void RunOnlyOnServer(Action<ICommandDispatcher<TContext>> action);
        void RunOnlyOnServer<T>(Action<T, ICommandDispatcher<TContext>> action, T p);
        void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher<TContext>> action, T1 p1, T2 p2);
        void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher<TContext>> action, T1 p1, T2 p2, T3 p3);
        void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4, ICommandDispatcher<TContext>> action, T1 p1, T2 p2, T3 p3, T4 p4);

        void RunOnlyOnServer(Func<ICommand<TContext>> func);
        void RunOnlyOnServer<T>(Func<T, ICommand<TContext>> func, T p);
        void RunOnlyOnServer<T1, T2>(Func<T1, T2, ICommand<TContext>> func, T1 p1, T2 p2);
        void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ICommand<TContext>> func, T1 p1, T2 p2, T3 p3);
        void RunOnlyOnServer<T1, T2, T3, T4>(Func<T1, T2, T3, T4, ICommand<TContext>> func, T1 p1, T2 p2, T3 p3, T4 p4);
    }

    abstract class BaseClientDispatcher<TContext> : IDispatcher<TContext>
    {
        public void RunOnlyOnServer(Action action) {}
        public void RunOnlyOnServer<T>(Action<T> action, T p) {}
        public void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2) {}
        public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3) { }
        public void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4) { }

        public void RunOnlyOnServer(Action<ICommandDispatcher<TContext>> action) {}
        public void RunOnlyOnServer<T>(Action<T, ICommandDispatcher<TContext>> action, T p) {}
        public void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher<TContext>> action, T1 p1, T2 p2) { }
        public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher<TContext>> action, T1 p1, T2 p2, T3 p3) { }
        public void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4, ICommandDispatcher<TContext>> action, T1 p1, T2 p2, T3 p3, T4 p4) {}

        public void RunOnlyOnServer(Func<ICommand<TContext>> func) {}
        public void RunOnlyOnServer<T>(Func<T, ICommand<TContext>> func, T p) {}
        public void RunOnlyOnServer<T1, T2>(Func<T1, T2, ICommand<TContext>> func, T1 p1, T2 p2) {}
        public void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ICommand<TContext>> func, T1 p1, T2 p2, T3 p3) { }
        public void RunOnlyOnServer<T1, T2, T3, T4>(Func<T1, T2, T3, T4, ICommand<TContext>> func, T1 p1, T2 p2, T3 p3, T4 p4) { }
    }

    abstract class BaseServerDispatcher<TContext> : IDispatcher<TContext>
    {
        private readonly ICommandDispatcher<TContext> commandDispatcher;

        protected BaseServerDispatcher(ICommandDispatcher<TContext> commandDispatcher)
        {
            this.commandDispatcher = commandDispatcher;
        }

        public void RunOnlyOnServer(Action action) => action();
        public void RunOnlyOnServer<T>(Action<T> action, T p) => action(p);
        public void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2) => action(p1, p2);
        public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3) => action(p1, p2, p3);
        public void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4) => action(p1, p2, p3, p4);

        public void RunOnlyOnServer(Action<ICommandDispatcher<TContext>> action) => action(commandDispatcher);
        public void RunOnlyOnServer<T>(Action<T, ICommandDispatcher<TContext>> action, T p) => action(p, commandDispatcher);
        public void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher<TContext>> action, T1 p1, T2 p2)
            => action(p1, p2, commandDispatcher);
        public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher<TContext>> action, T1 p1, T2 p2, T3 p3)
            => action(p1, p2, p3, commandDispatcher);
        public void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4, ICommandDispatcher<TContext>> action, T1 p1, T2 p2, T3 p3, T4 p4)
            => action(p1, p2, p3, p4, commandDispatcher);

        public void RunOnlyOnServer(Func<ICommand<TContext>> func) => dispatch(func());
        public void RunOnlyOnServer<T>(Func<T, ICommand<TContext>> func, T p) => dispatch(func(p));
        public void RunOnlyOnServer<T1, T2>(Func<T1, T2, ICommand<TContext>> func, T1 p1, T2 p2)
            => dispatch(func(p1, p2));
        public void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ICommand<TContext>> func, T1 p1, T2 p2, T3 p3)
            => dispatch(func(p1, p2, p3));
        public void RunOnlyOnServer<T1, T2, T3, T4>(Func<T1, T2, T3, T4, ICommand<TContext>> func, T1 p1, T2 p2, T3 p3, T4 p4)
            => dispatch(func(p1, p2, p3, p4));

        private void dispatch(ICommand<TContext> command)
        {
            if (command == null)
                return;
            commandDispatcher.Dispatch(command);
        }
    }
}