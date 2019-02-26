using System.Collections.Generic;
using System.Drawing;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Content
{
    interface IGraphicsLoader
    {
        PackedSpriteSet CreateSpriteSet(IEnumerable<(Bitmap Image, string Name)> sprites);
        
        ISurfaceShader CreateShaderProgram(
            IList<(ShaderType Type, string Filepath, string FriendlyName)> shaders, string shaderProgramName);
    }
}
