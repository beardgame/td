using System;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using OpenToolkit.Windowing.Common.Input;

namespace Bearded.TD.UI.Controls
{
    class AutoCompletingTextInput : TextInput
    {
        private readonly Func<string, string> autoCompletionProvider;

        public string AutoCompletionText { get; private set; } = "";

        public AutoCompletingTextInput(Func<string, string> autoCompletionProvider)
        {
            this.autoCompletionProvider = autoCompletionProvider;
            TextChanged += onTextChanged;
        }

        private void onTextChanged()
        {
            AutoCompletionText = autoCompletionProvider(Text);
        }

        public override void KeyHit(KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Tab && Text != AutoCompletionText)
            {
                Text = AutoCompletionText;
                MoveCursorToEnd();
                eventArgs.Handled = true;
                return;
            }

            base.KeyHit(eventArgs);
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
