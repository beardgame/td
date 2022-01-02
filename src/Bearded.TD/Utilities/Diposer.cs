using System;
using System.Collections.Generic;

namespace Bearded.TD.Utilities;

sealed class Disposer : IDisposable
{
    private bool isDisposed;
    private readonly List<IDisposable> disposables = new();

    public void AddDisposable(IDisposable disposable)
    {
        if (isDisposed)
        {
            throw new InvalidOperationException("Cannot register new disposables to a disposer after disposing.");
        }
        disposables.Add(disposable);
    }

    public void AddOnDispose(Action action)
    {
        disposables.Add(new ActionDisposable(action));
    }

    private sealed class ActionDisposable : IDisposable
    {
        private readonly Action onDispose;

        public ActionDisposable(Action onDispose)
        {
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            onDispose();
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            throw new InvalidOperationException("Can only dispose a disposer once.");
        }

        foreach (var d in disposables)
        {
            d.Dispose();
        }
        isDisposed = true;
    }

    public void DisposeAndReset()
    {
        Dispose();
        disposables.Clear();
        isDisposed = false;
    }
}