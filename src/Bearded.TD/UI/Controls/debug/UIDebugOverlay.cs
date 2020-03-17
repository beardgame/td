using amulware.Graphics;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    class UIDebugOverlay : UpdateableNavigationNode<Void>
    {

        public override void Update(UpdateEventArgs args)
        {

        }

        public void Close()
        {
            Navigation.Close(this);
        }
    }
}
