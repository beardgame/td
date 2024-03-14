using System.Collections.Generic;

namespace Bearded.TD.UI.Animation;

interface IUpdatableAnimation
{
    void Update();
    bool HasEnded { get; }
}

sealed class AnimationUpdater
{
    private readonly List<IUpdatableAnimation> animations = [];

    public void Update()
    {
        foreach (var animation in animations)
        {
            animation.Update();
        }

        animations.RemoveAll(a => a.HasEnded);
    }

    public void Add(IUpdatableAnimation animation)
    {
        animations.Add(animation);
    }
}
