using System.Collections.Generic;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Synchronization
{
    sealed class CompositeStateToSync : IStateToSync
    {
        private readonly IEnumerable<IStateToSync> statesToSync;

        public CompositeStateToSync(IEnumerable<IStateToSync> statesToSync)
        {
            this.statesToSync = statesToSync;
        }

        public void Serialize(INetBufferStream stream)
        {
            foreach (var state in statesToSync)
            {
                state.Serialize(stream);
            }
        }

        public void Apply()
        {
            foreach (var state in statesToSync)
            {
                state.Apply();
            }
        }
    }
}
