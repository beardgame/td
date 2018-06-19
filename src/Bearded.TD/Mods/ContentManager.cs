using System.Collections.ObjectModel;

namespace Bearded.TD.Mods
{
    sealed class ContentManager
    {
        public ReadOnlyCollection<ModMetadata> Mods { get; }

        public ContentManager()
        {
            Mods = new ModLister().GetAll().AsReadOnly();
        }
    }
}
