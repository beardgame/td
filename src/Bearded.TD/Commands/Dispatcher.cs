using System;

namespace Bearded.TD.Commands;

interface IDispatcher<TObject>
{
    void RunOnlyOnServer(Action action);
    void RunOnlyOnServer<T>(Action<T> action, T p);
    void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2);
    void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3);
    void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4);

    void RunOnlyOnServer(Action<ICommandDispatcher<TObject>> action);
    void RunOnlyOnServer<T>(Action<T, ICommandDispatcher<TObject>> action, T p);
    void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher<TObject>> action, T1 p1, T2 p2);
    void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher<TObject>> action, T1 p1, T2 p2, T3 p3);
    void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4, ICommandDispatcher<TObject>> action, T1 p1, T2 p2, T3 p3, T4 p4);

    void RunOnlyOnServer(Func<ISerializableCommand<TObject>> func);
    void RunOnlyOnServer<T>(Func<T, ISerializableCommand<TObject>> func, T p);
    void RunOnlyOnServer<T1, T2>(Func<T1, T2, ISerializableCommand<TObject>> func, T1 p1, T2 p2);
    void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3);
    void RunOnlyOnServer<T1, T2, T3, T4>(Func<T1, T2, T3, T4, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3, T4 p4);
    void RunOnlyOnServer<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
}

abstract class BaseClientDispatcher<TObject> : IDispatcher<TObject>
{
    public void RunOnlyOnServer(Action action) {}
    public void RunOnlyOnServer<T>(Action<T> action, T p) {}
    public void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2) {}
    public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3) { }
    public void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4) { }

    public void RunOnlyOnServer(Action<ICommandDispatcher<TObject>> action) {}
    public void RunOnlyOnServer<T>(Action<T, ICommandDispatcher<TObject>> action, T p) {}
    public void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher<TObject>> action, T1 p1, T2 p2) { }
    public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher<TObject>> action, T1 p1, T2 p2, T3 p3) { }
    public void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4, ICommandDispatcher<TObject>> action, T1 p1, T2 p2, T3 p3, T4 p4) {}

    public void RunOnlyOnServer(Func<ISerializableCommand<TObject>> func) {}
    public void RunOnlyOnServer<T>(Func<T, ISerializableCommand<TObject>> func, T p) {}
    public void RunOnlyOnServer<T1, T2>(Func<T1, T2, ISerializableCommand<TObject>> func, T1 p1, T2 p2) {}
    public void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3) { }
    public void RunOnlyOnServer<T1, T2, T3, T4>(Func<T1, T2, T3, T4, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3, T4 p4) { }
    public void RunOnlyOnServer<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) { }
}

abstract class BaseServerDispatcher<TObject> : IDispatcher<TObject>
{
    private readonly ICommandDispatcher<TObject> commandDispatcher;

    protected BaseServerDispatcher(ICommandDispatcher<TObject> commandDispatcher)
    {
        this.commandDispatcher = commandDispatcher;
    }

    public void RunOnlyOnServer(Action action) => action();
    public void RunOnlyOnServer<T>(Action<T> action, T p) => action(p);
    public void RunOnlyOnServer<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2) => action(p1, p2);
    public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3) => action(p1, p2, p3);
    public void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4) => action(p1, p2, p3, p4);

    public void RunOnlyOnServer(Action<ICommandDispatcher<TObject>> action)
        => action(commandDispatcher);
    public void RunOnlyOnServer<T>(Action<T, ICommandDispatcher<TObject>> action, T p)
        => action(p, commandDispatcher);
    public void RunOnlyOnServer<T1, T2>(Action<T1, T2, ICommandDispatcher<TObject>> action, T1 p1, T2 p2)
        => action(p1, p2, commandDispatcher);
    public void RunOnlyOnServer<T1, T2, T3>(Action<T1, T2, T3, ICommandDispatcher<TObject>> action, T1 p1, T2 p2, T3 p3)
        => action(p1, p2, p3, commandDispatcher);
    public void RunOnlyOnServer<T1, T2, T3, T4>(Action<T1, T2, T3, T4, ICommandDispatcher<TObject>> action, T1 p1, T2 p2, T3 p3, T4 p4)
        => action(p1, p2, p3, p4, commandDispatcher);

    public void RunOnlyOnServer(Func<ISerializableCommand<TObject>> func) => dispatch(func());
    public void RunOnlyOnServer<T>(Func<T, ISerializableCommand<TObject>> func, T p) => dispatch(func(p));
    public void RunOnlyOnServer<T1, T2>(Func<T1, T2, ISerializableCommand<TObject>> func, T1 p1, T2 p2)
        => dispatch(func(p1, p2));
    public void RunOnlyOnServer<T1, T2, T3>(Func<T1, T2, T3, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3)
        => dispatch(func(p1, p2, p3));
    public void RunOnlyOnServer<T1, T2, T3, T4>(Func<T1, T2, T3, T4, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3, T4 p4)
        => dispatch(func(p1, p2, p3, p4));
    public void RunOnlyOnServer<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, ISerializableCommand<TObject>> func, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        => dispatch(func(p1, p2, p3, p4, p5));

    private void dispatch(ISerializableCommand<TObject> command)
    {
        if (command == null)
            return;
        commandDispatcher.Dispatch(command);
    }
}
