using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Workers;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class WorkerStatusUIControl : CompositeControl
    {
        private readonly WorkerStatusUI workerStatus;

        private readonly ListControl taskList = new ListControl(new ViewportClippingLayerControl());

        private WorkerTaskItemSource workerTaskItemSource;
        private int totalNumWorkers;

        public WorkerStatusUIControl(WorkerStatusUI workerStatus)
        {
            this.workerStatus = workerStatus;

            Add(new BackgroundBox());

            Add(new Label("Workers") {FontSize = 24}
                .Anchor(a => a.Top(margin: 4, height: 24).Left(margin: 4).Right(margin: 4)));
            Add(new Label($"Owned by {workerStatus.Faction.Name}") {FontSize = 16}
                .Anchor(a => a.Top(margin: 32, height: 16).Left(margin: 4).Right(margin: 4)));

            Add(new DynamicLabel(
                        () => $"Idle workers: {workerStatus.Faction.Workers.NumIdleWorkers} / {totalNumWorkers}")
                    {FontSize = 16}
                .Anchor(a => a.Top(margin: 52, height: 16).Left(margin: 4).Right(margin: 4)));

            Add(taskList.Anchor(a => a.Top(margin: 72).Bottom(margin: 40).Left(margin: 4).Right(margin: 4)));

            Add(Default.Button("Close")
                .Anchor(a => a.Bottom(margin: 4, height: 32).Left(margin: 4).Right(relativePercentage: .5, margin: 2))
                .Subscribe(btn => btn.Clicked += workerStatus.OnCloseClicked));

            updateDisplayValues();
        }

        private void updateDisplayValues()
        {
            totalNumWorkers = workerStatus.Game.State.Enumerate<Worker>().Count();
            workerTaskItemSource = new WorkerTaskItemSource(workerStatus.Faction.Workers.QueuedTasks);
            taskList.ItemSource = workerTaskItemSource;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private class WorkerTaskItemSource : IListItemSource
        {
            private readonly IList<WorkerTask> workerTasks;
            public int ItemCount { get; }

            public WorkerTaskItemSource(IList<WorkerTask> workerTasks)
            {
                this.workerTasks = workerTasks;
                ItemCount = workerTasks.Count;
            }

            public double HeightOfItemAt(int index) => 16;

            public Control CreateItemControlFor(int index)
            {
                return new Label { FontSize = 16, Text = workerTasks[index].Name };
            }

            public void DestroyItemControlAt(int index, Control control) {}
        }
    }
}
