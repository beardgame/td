using System.Collections.ObjectModel;

namespace Bearded.TD.Mods
{
    sealed class ContentManager
    {
        public ReadOnlyCollection<ModMetadata> Mods { get; set; }

        public ContentManager()
        {
            Mods = new ModLister().GetAll().AsReadOnly();
        }
    }
}
