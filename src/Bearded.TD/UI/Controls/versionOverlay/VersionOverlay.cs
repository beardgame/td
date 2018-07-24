using Bearded.UI.Navigation;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class VersionOverlay : NavigationNode<Void>
    {
        public string VersionCodeString => Config.VersionString;

        protected override void Initialize(DependencyResolver dependencies, Void parameters) { }
    }
}
