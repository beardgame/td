using System;
using System.Collections.Generic;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Game
{
    public class GameCamera
    {
        private const float scrollSpeed = 8;

        private static readonly Dictionary<IAction, Vector2> scrollActions = new Dictionary<IAction, Vector2>
        {
            {KeyboardAction.FromKey(Key.Left), -Vector2.UnitX},
            {KeyboardAction.FromKey(Key.Right), Vector2.UnitX},
            {KeyboardAction.FromKey(Key.Up), Vector2.UnitY},
            {KeyboardAction.FromKey(Key.Down), -Vector2.UnitY},
        };

        private readonly float tilemapRadius;

        public Vector2 CameraPosition { get; private set; }
        public float CameraDistance { get; private set; }

        public GameCamera(float tilemapRadius)
        {
            this.tilemapRadius = tilemapRadius;

            CameraPosition = Vector2.Zero;
            CameraDistance = 10;
        }

        public void Update(float elapsedTime)
        {
            foreach (KeyValuePair<IAction, Vector2> scrollAction in scrollActions)
            {
                if (scrollAction.Key.Active)
                {
                    CameraPosition += elapsedTime * scrollSpeed * scrollAction.Value;
                }
            }
        }
    }
}
