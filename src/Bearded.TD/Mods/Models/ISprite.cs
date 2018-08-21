using amulware.Graphics;
using OpenTK;

namespace Bearded.TD.Mods.Models
{
    interface ISprite
    {
        void Draw(Vector3 position, Color color, float size);
    }
}
