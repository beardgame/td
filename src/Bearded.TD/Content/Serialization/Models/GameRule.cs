using Bearded.TD.Content.Behaviors;

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class GameRule<TParameters> : IGameRule
    {
        public string Id { get; set; }
        public TParameters Parameters { get; set; }

        object IBehaviorTemplate.Parameters => Parameters;
    }
}
