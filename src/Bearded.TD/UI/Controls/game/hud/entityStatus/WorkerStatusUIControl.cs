using System.Collections.Generic;
using amulware.Graphics;
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

        public WorkerStatusUIControl(WorkerStatusUI workerStatus)
        {
            this.workerStatus = workerStatus;

            Add(new BackgroundBox());

            Add(new Label("Workers") {FontSize = 24}
                .Anchor(a => a.Top(margin: 4, height: 24).Left(margin: 4).Right(margin: 4)));
            Add(new Label($"Owned by {workerStatus.Faction.Name}") {FontSize = 16}
                .Anchor(a => a.Top(margin: 32, height: 16).Left(margin: 4).Right(margin: 4)));

            Add(new DynamicLabel(() => $"Idle workers: {workerStatus.NumIdleWorkers} / {workerStatus.NumWorkers}")
                {FontSize = 16}
                .Anchor(a => a.Top(margin: 52, height: 16).Left(margin: 4).Right(margin: 4)));

            Add(taskList.Anchor(a => a.Top(margin: 72).Bottom(margin: 40).Left(margin: 4).Right(margin: 4)));

            Add(Default.Button("Close")
                .Anchor(a => a.Bottom(margin: 4, height: 32).Left(margin: 4).Right(relativePercentage: .5, margin: 2))
                .Subscribe(btn => btn.Clicked += workerStatus.OnCloseClicked));

            updateDisplayValues();
            workerStatus.WorkerValuesUpdated += updateDisplayValues;
        }

        private void updateDisplayValues()
        {
            workerTaskItemSource = new WorkerTaskItemSource(workerStatus, workerStatus.QueuedTasks);
            taskList.ItemSource = workerTaskItemSource;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private class WorkerTaskItemSource : IListItemSource
        {
            private readonly WorkerStatusUI workerStatus;
            private readonly IList<IWorkerTask> workerTasks;
            public int ItemCount { get; }

            public WorkerTaskItemSource(WorkerStatusUI workerStatus, IList<IWorkerTask> workerTasks)
            {
                this.workerStatus = workerStatus;
                this.workerTasks = workerTasks;
                ItemCount = workerTasks.Count;
            }

            public double HeightOfItemAt(int index) => 24;

            public Control CreateItemControlFor(int index) => new WorkerTaskControl(workerStatus, workerTasks[index]);

            public void DestroyItemControlAt(int index, Control control) { }
        }

        private class WorkerTaskControl : CompositeControl
        {
            private readonly IWorkerTask task;
            private readonly BackgroundBox progressBar;
            private readonly Button cancelButton;

            public WorkerTaskControl(WorkerStatusUI workerStatus, IWorkerTask task)
            {
                this.task = task;

                progressBar = new BackgroundBox { Color = Color.White * 0.25f };
                Add(progressBar);

                Add(new Label {FontSize = 16, Text = task.Name});

                cancelButton = Default.Button("x");
                cancelButton.Clicked += () => workerStatus.OnTaskCancelClicked(task);
                Add(cancelButton.Anchor(a => a.Right(margin: 24, width: 24)));

                var bumpButton = Default.Button("+");
                bumpButton.Clicked += () => workerStatus.OnTaskBumpClicked(task);
                Add(bumpButton.Anchor(a => a.Right(width: 24)));
            }

            public override void Render(IRendererRouter r)
            {
                var percentage = task.PercentCompleted;
                progressBar.Anchor(a => a.Right(relativePercentage: percentage));

                cancelButton.IsVisible = percentage == 0;

                base.Render(r);
            }
        }
    }
}
