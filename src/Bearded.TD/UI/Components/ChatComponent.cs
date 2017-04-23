﻿using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.UI;
using Bearded.TD.Rendering;

namespace Bearded.TD.UI.Components
{
    class ChatComponent : CompositeComponent
    {
        private readonly GameInstance game;
        private const float textInputHeight = 16;
        private static readonly Color textColor = Color.White;
        
        private readonly TextInput textInput;

        public ChatComponent(Bounds bounds, GameInstance game) : base(bounds)
        {
            this.game = game;
            AddComponent(new TextBox<ChatMessage>(
                Bounds.Within(bounds, 0, 0, textInputHeight, 0), () => game.ChatLog.Messages, msg => (msg.GetDisplayString(), textColor)));
            AddComponent(textInput = new TextInput(new Bounds(new ScalingDimension(bounds.X), new FixedSizeDimension(bounds.Y, textInputHeight, 1, 1))));

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