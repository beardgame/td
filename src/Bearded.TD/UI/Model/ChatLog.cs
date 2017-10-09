using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bearded.TD.UI.Model
{
    sealed class ChatLog
    {
        private readonly List<ChatMessage> messages = new List<ChatMessage>();
        public ReadOnlyCollection<ChatMessage> Messages { get; }

        public ChatLog()
        {
            Messages = messages.AsReadOnly();
        }

        public void Add(ChatMessage message)
        {
            messages.Add(message);
        }
    }
}