using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.UI;

namespace Bearded.TD.UI.Components
{
    class ChatComponent : CompositeComponent
    {
        private const float textInputHeight = 16;
        private static readonly Color textColor = Color.White;
        
        private readonly TextInput textInput;

        public ChatComponent(Bounds bounds, ChatLog chatLog) : base(bounds)
        {
            AddComponent(new TextBox<ChatMessage>(
                Bounds.Within(bounds, 0, 0, textInputHeight, 0), () => chatLog.Messages.ToList(), msg => (msg.Text, textColor)));
            AddComponent(textInput = new TextInput(Bounds.Within(bounds, bounds.Height - textInputHeight, 0, 0, 0)));

            textInput.Submitted += sendChatMessage;
        }

        private void sendChatMessage(string chatMessage)
        {
            // Send chat message request.
            textInput.Text = "";
        }
    }
}