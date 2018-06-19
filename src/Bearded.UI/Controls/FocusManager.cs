namespace Bearded.UI.Controls
{
    internal class FocusManager
    {
        private Control currentFocus;
        
        public void Focus(Control control)
        {
            ensureNoFocus();

            currentFocus = control;
        }

        private void ensureNoFocus()
        {
            if (currentFocus == null)
                return;

            if (currentFocus.IsFocused)
                currentFocus.Unfocus();
            
            currentFocus = null;
        }
    }
}
