using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class WorkerStatusUIControl : CompositeControl
    {
        private readonly WorkerStatusUI workerStatus;

        public WorkerStatusUIControl(WorkerStatusUI workerStatus)
        {
            this.workerStatus = workerStatus;

            Add(new BackgroundBox());

            Add(new Label("Workers") {FontSize = 24}
                .Anchor(a => a.Top(margin: 4, height: 24).Left(margin: 4).Right(margin: 4)));
            Add(new Label($"Owned by {workerStatus.Faction.Name}") {FontSize = 16}
                .Anchor(a => a.Top(margin: 32, height: 16).Left(margin: 4).Right(margin: 4)));

            Add(Default.Button("Close")
                .Anchor(a => a.Bottom(margin: 4, height: 32).Left(margin: 4).Right(relativePercentage: .5, margin: 2))
                .Subscribe(btn => btn.Clicked += workerStatus.OnCloseClicked));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
