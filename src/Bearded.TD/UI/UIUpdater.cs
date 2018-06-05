using amulware.Graphics;
using Bearded.Utilities.Collections;

namespace Bearded.TD.UI
{
    sealed class UIUpdater
    {
        private readonly DeletableObjectList<IUpdatable> updatableNavigationNodes = new DeletableObjectList<IUpdatable>();

        public void Update(UpdateEventArgs args)
        {
            foreach (var node in updatableNavigationNodes)
            {
                node.Update(args);
            }
        }

        public void Add(IUpdatable node)
        {
            updatableNavigationNodes.Add(node);
        }

        public void Remove(IUpdatable node)
        {
            updatableNavigationNodes.Remove(node);
        }

        public interface IUpdatable : IDeletable
        {
            void Update(UpdateEventArgs args);
        }
    }
}
