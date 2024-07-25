using System.Collections.Generic;
using System.IO;
using Bearded.Graphics.ShaderManagement;
using OpenTK.Graphics.OpenGL;
using Shader = Bearded.TD.Content.Models.Shader;

namespace Bearded.TD.Content.Mods;

sealed class ShaderLoader
{
    private readonly ModLoadingContext context;
    private readonly ModMetadata meta;

    public ShaderLoader(ModLoadingContext context, ModMetadata meta)
    {
        this.context = context;
        this.meta = meta;
    }

    public Shader TryLoad(FileInfo file, Serialization.Models.Shader jsonModel)
    {
        // don't load same key/friendly name twice, just assume it's already there (keep local hashset?)
        // consider implementing IShaderReloader with a more lazy version that can load source on one thread and compile the shader on a different one
        //   - or actually only fetch shader file the first time it's used? no that's no good, we want shaders compiled during loading!
        var shaderProgram = new ShaderCompiler(meta, file.Directory!, jsonModel.Id!)
            .TryAdd(ShaderType.VertexShader, jsonModel.VertexShader)
            .TryAdd(ShaderType.FragmentShader, jsonModel.FragmentShader)
            .TryAdd(ShaderType.TessControlShader, jsonModel.TessControlShader)
            .TryAdd(ShaderType.TessEvaluationShader, jsonModel.TessEvaluationShader)
            .TryAdd(ShaderType.GeometryShader, jsonModel.GeometryShader)
            .TryAdd(ShaderType.ComputeShader, jsonModel.ComputeShader)
            .Compile(context);

        return new Shader(ModAwareId.FromNameInMod(jsonModel.Id, meta), shaderProgram);
    }

    private sealed class ShaderCompiler
    {
        private readonly ModMetadata meta;
        private readonly DirectoryInfo directory;
        private readonly string shaderId;
        private readonly List<ModShaderFile> shaders = new();

        public ShaderCompiler(ModMetadata meta, DirectoryInfo baseDirectory, string shaderId)
        {
            this.meta = meta;
            directory = baseDirectory;
            this.shaderId = shaderId;
        }

        public ShaderCompiler TryAdd(ShaderType type, string? path)
        {
            if (path != null)
                shaders.Add(new ModShaderFile(type, filepath(path), friendlyName(path)));

            return this;
        }

        private string filepath(string path)
        {
            return Path.Combine(directory.ToString(), path);
        }

        private string friendlyName(string? path)
        {
            return $"{meta.Id}.{shaderId}.{path}";
        }

        public IRendererShader Compile(ModLoadingContext context)
        {
            return context.GraphicsLoader.CreateRendererShader(
                shaders, $"{meta.Id}.{shaderId}"
            );
        }
    }
}
