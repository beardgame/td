using System.Diagnostics;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.UI.Animation;

enum AnimationState
{
    Continue,
    Ended,
}

interface IAnimationController
{
    void End();
    void Cancel();
}

sealed class Animation<TState>(ITimeSource time, AnimationFunction<TState> function, TState state)
    : IUpdatableAnimation, IAnimationController
{
    private Instant startTime;
    private bool cancelled;

    public bool HasEnded { get; private set; }

    public void Update()
    {
        Debug.Assert(!HasEnded, "Trying to update an ended animation.");
        if (cancelled)
        {
            HasEnded = true;
            return;
        }

        var timeSinceStart = time.Time - startTime;

        var result = function.Update(state, timeSinceStart);

        if (result == AnimationState.Ended)
        {
            End();
        }
    }

    public void Start()
    {
        startTime = time.Time;
        function.Start?.Invoke(state);
    }

    public void End()
    {
        function.End?.Invoke(state);
        HasEnded = true;
    }

    public void Cancel()
    {
        cancelled = true;
    }
}
