using System.Collections.ObjectModel;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content
{
    sealed class ContentManager
    {
        public IGraphicsLoader GraphicsLoader { get; }
        public ReadOnlyCollection<ModMetadata> Mods { get; }

        public ContentManager(IGraphicsLoader graphicsLoader)
        {
            GraphicsLoader = graphicsLoader;
            Mods = new ModSorter().SortByDependency(new ModLister().GetAll()).AsReadOnly();
        }
    }
}
