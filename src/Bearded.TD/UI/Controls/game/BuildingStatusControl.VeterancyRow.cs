using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using Veterancy = Bearded.TD.Constants.UI.BuildingStatus.Veterancy;

namespace Bearded.TD.UI.Controls;

sealed partial class BuildingStatusControl
{
    private sealed class VeterancyRow : CompositeControl
    {
        private readonly Animations animations;
        private readonly Label levelLabel;

        private IAnimationController? experienceAnimation;
        private float currentAnimationExperience;
        private float animationStartExperience;
        private float animationTargetExperience;
        private readonly GradientStop[] experienceBarGradient =
        [
            new GradientStop(0, Veterancy.ExperienceColor),
            new GradientStop(0, Veterancy.ExperienceColor),
            new GradientStop(0, Veterancy.NewExperienceColor),
            new GradientStop(0, Veterancy.NewExperienceColor),
            new GradientStop(0, Color.Transparent),
            new GradientStop(1, Color.Transparent),
        ];

        public VeterancyRow(IReadonlyBinding<VeterancyStatus> veterancy, Animations animations)
        {
            this.animations = animations;
            var iconMargin = 0.5 * (Veterancy.RowHeight - Veterancy.LevelIconSize);
            var barTopMargin = 0.5 * (Veterancy.RowHeight - Veterancy.ExperienceBarHeight);
            var barLeftMargin = Veterancy.LevelIconSize + iconMargin;

            // TODO: replace by icon
            levelLabel = new Label
            {
                Color = Veterancy.ExperienceColor,
                FontSize = Veterancy.LevelIconSize,
            };

            var experienceBar = new ComplexBox
            {
                CornerRadius = Veterancy.ExperienceBarCornerRadius,
                Components = [
                    ..Veterancy.ExperienceBarStaticComponents,
                    Fill.With(ShapeColor.FromMutable(experienceBarGradient, GradientDefinition.Linear(
                        AnchorPoint.Relative((0, 0)),
                        AnchorPoint.Relative((1, 0))
                    ))),
                ],
            };

            this.Add([
                levelLabel.Anchor(a => a
                    .Left(width: Veterancy.LevelIconSize)
                    .Top(height: Veterancy.LevelIconSize, margin: iconMargin)),
                experienceBar.Anchor(a => a
                    .Left(margin: barLeftMargin)
                    .Top(margin: barTopMargin, height: Veterancy.ExperienceBarHeight)
                    .Right(relativePercentage: 0.75)),
            ]);

            update(veterancy.Value);
            experienceAnimation?.End();
            veterancy.SourceUpdated += update;
        }

        private void update(VeterancyStatus t)
        {
            levelLabel.Text = t.Level.ToString();
            startAnimationExperience((float)t.PercentageToNextLevel);
        }

        private void startAnimationExperience(float targetExperience)
        {
            experienceBarGradient[3] = (targetExperience, Veterancy.NewExperienceColor);
            experienceBarGradient[4] = (targetExperience, Color.Transparent);

            experienceAnimation?.Cancel();

            animationStartExperience = currentAnimationExperience;
            animationTargetExperience = targetExperience;

            experienceAnimation =
                animations.Start(AnimationFunction.ZeroToOne(0.5.S(), updateExperienceBarFromAnimation));
        }

        private void updateExperienceBarFromAnimation(float t)
        {
            t = Interpolate.Hermite(0, 0, 1, 0.58f, t);
            var xp = animationStartExperience + (animationTargetExperience - animationStartExperience) * t;
            currentAnimationExperience = xp;
            experienceBarGradient[1] = (xp, Veterancy.ExperienceColor);
            experienceBarGradient[2] = (xp, Veterancy.NewExperienceColor);
        }
    }
}
