using System.Collections.ObjectModel;

namespace Bearded.TD.Mods
{
    sealed class ContentManager
    {
        public IGraphicsLoader GraphicsLoader { get; }
        public ReadOnlyCollection<ModMetadata> Mods { get; }

        public ContentManager(IGraphicsLoader graphicsLoader)
        {
            GraphicsLoader = graphicsLoader;
            Mods = new ModLister().GetAll().AsReadOnly();
        }
    }
}
