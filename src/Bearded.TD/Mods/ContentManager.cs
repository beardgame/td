using System.Collections.ObjectModel;
using Bearded.Utilities.Threading;

namespace Bearded.TD.Mods
{
    sealed class ContentManager
    {
        public IActionQueue GlActions { get; }
        public ReadOnlyCollection<ModMetadata> Mods { get; }

        public ContentManager(IActionQueue glActions)
        {
            GlActions = glActions;
            Mods = new ModLister().GetAll().AsReadOnly();
        }
    }
}
