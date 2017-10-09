using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Model;

namespace Bearded.TD.UI.Components
{
    class ChatComponent : CompositeComponent
    {
        private readonly GameInstance game;
        private static readonly Color textColor = Color.White;
        
        private readonly TextInput textInput;

        public ChatComponent(Bounds bounds, GameInstance game) : base(bounds)
        {
            this.game = game;

            textInput = new TextInput(
                new Bounds(new ScalingDimension(bounds.X), new FixedSizeDimension(bounds.Y, Constants.UI.FontSize, 1, 1)));
            AddComponent(new InjectedTextBox<ChatMessage>(
                Bounds.Within(bounds, 0, 0, Constants.UI.FontSize, 0),
                () => game.ChatLog.Messages, msg => (msg.GetDisplayString(), textColor)));
            AddComponent(textInput);

            textInput.Submitted += sendChatMessage;
        }

        private void sendChatMessage(string chatMessage)
        {
            game.Request(SendChatMessage.Request, game.Me, textInput.Text);
            textInput.Text = "";
        }

        public override void Draw(GeometryManager geometries)
        {
            geometries.ConsoleBackground.Color = Color.Black;
            geometries.ConsoleBackground.DrawRectangle(Bounds.XStart, Bounds.YStart, Bounds.Width, Bounds.Height);
            base.Draw(geometries);
        }
    }
}