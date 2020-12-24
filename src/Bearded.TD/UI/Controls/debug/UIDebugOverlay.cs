using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    class UIDebugOverlay : UpdateableNavigationNode<Void>
    {

        public override void Update(UpdateEventArgs args)
        {

        }

        public void Close(Button.ClickEventArgs t)
        {
            Navigation.Close(this);
        }
    }
}
