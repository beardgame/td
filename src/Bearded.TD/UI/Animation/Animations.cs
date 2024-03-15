using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.UI.Animation;

sealed class Animations(ITimeSource time, AnimationUpdater updater)
{
    public IAnimationController Start<TState>(AnimationFunction<TState> function, TState state)
    {
        var animation = new Animation<TState>(time, function, state);
        animation.Start();
        updater.Add(animation);
        return animation;
    }

    public IAnimationController Start(AnimationFunction<Void> function)
    {
        var animation = new Animation<Void>(time, function, default);
        animation.Start();
        updater.Add(animation);
        return animation;
    }
}
