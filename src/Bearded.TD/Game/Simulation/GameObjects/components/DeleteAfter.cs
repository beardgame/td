using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.GameObjects
{
    [Component("deleteAfter")]
    sealed class DeleteAfter<T> : Component<T, IDeleteAfterParameters>
        where T : GameObject
    {
        private TimeSpan timeAlive = TimeSpan.Zero;

        public DeleteAfter(IDeleteAfterParameters parameters) : base(parameters)
        {
        }

        protected override void OnAdded()
        {
        }

        public override void Update(TimeSpan elapsedTime)
        {
            timeAlive += elapsedTime;
            if (timeAlive >= Parameters.TimeSpan)
            {
                Owner.Delete();
            }
        }
    }
}
