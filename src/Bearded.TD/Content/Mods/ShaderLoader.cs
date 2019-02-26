using System;
using System.Collections.Generic;
using System.IO;
using amulware.Graphics;
using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Content.Mods
{
    class ShaderLoader
    {
        private readonly ModLoadingContext context;
        private readonly ModMetadata meta;
        private readonly JsonSerializer serializer;

        public ShaderLoader(ModLoadingContext context, ModMetadata meta, JsonSerializer serializer)
        {
            this.context = context;
            this.meta = meta;
            this.serializer = serializer;
        }

        public Shader TryLoad(FileInfo file)
        {
            try
            {
                var text = file.OpenText();
                var reader = new JsonTextReader(text);
                var jsonModel = serializer.Deserialize<Serialization.Models.Shader>(reader);

                // don't load same key/friendly name twice, just assume it's already there (keep local hashset?)
                // consider implementing IShaderReloader with a more lazy version that can load source on one thread and compile the shader on a different one
                //   - or actually only fetch shader file the first time it's used? no that's no good, we want fast loading!
                var shaderProgram = new ShaderCompiler(meta, file.Directory, jsonModel.Id)
                    .Add(ShaderType.VertexShader, jsonModel.VertexShader)
                    .Add(ShaderType.FragmentShader, jsonModel.FragmentShader)
                    .Compile(context);

                return new Shader(jsonModel.Id, shaderProgram);
            }
            catch (Exception e)
            {
                context.Logger.Error?.Log($"Error loading '{meta.Id}/gfx/shaders/../{file.Name}': {e.Message}");

                return null;
            }
        }
        
        private class ShaderCompiler
        {
            private readonly ModMetadata meta;
            private readonly DirectoryInfo directory;
            private readonly string shaderId;
            private readonly List<(ShaderType Type, string Filepath, string FriendlyName)> shaders
                = new List<(ShaderType, string, string)>();

            public ShaderCompiler(ModMetadata meta, DirectoryInfo baseDirectory, string shaderId)
            {
                this.meta = meta;
                directory = baseDirectory;
                this.shaderId = shaderId;
                
            }

            public ShaderCompiler Add(ShaderType type, string path)
            {
                shaders.Add(
                    (Type: type, Filepath: path, FriendlyName: friendlyName(path))
                );

                return this;
            }

            private string friendlyName(string path)
            {
                return $"{meta.Id}.{shaderId}.{path}";
            }

            public ISurfaceShader Compile(ModLoadingContext context)
            {
                return context.GraphicsLoader.CreateShaderProgram(
                    shaders, $"{meta.Id}.{shaderId}"
                );
            }
        }
    }
}
