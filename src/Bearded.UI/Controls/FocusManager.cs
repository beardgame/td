namespace Bearded.UI.Controls
{
    public class FocusManager
    {
        private Control currentFocus;

        public Control FocusedControl
        {
            get
            {
                if (currentFocus == null) return null;
                return !currentFocus.IsFocused ? null : currentFocus;
            }
        }

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
