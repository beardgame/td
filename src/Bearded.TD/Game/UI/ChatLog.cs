using System.Collections.Generic;

namespace Bearded.TD.Game.UI
{
    sealed class ChatLog
    {
        private readonly List<ChatMessage> messages = new List<ChatMessage>();
        public IReadOnlyCollection<ChatMessage> Messages { get; }

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