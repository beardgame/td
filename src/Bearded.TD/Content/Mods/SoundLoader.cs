using System.IO;
using System.Linq;
using Bearded.TD.Audio;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Content.Mods;

sealed class SoundLoader
{
    private readonly ModLoadingContext context;
    private readonly ModMetadata meta;

    public SoundLoader(ModLoadingContext context, ModMetadata meta)
    {
        this.context = context;
        this.meta = meta;
    }

    public SoundEffect Load(FileInfo file, Serialization.Models.SoundEffect jsonModel)
    {
        var wavFile = file.Directory!.GetFiles(file.Name.Replace(file.Extension, ".wav")).SingleOrDefault() ??
            throw new InvalidDataException("Could not find wav file with same name as json file");
        return new SoundEffect(
            ModAwareId.FromNameInMod(jsonModel.Id ?? throw new InvalidDataException("Missing ID"), meta),
            Sound.FromWav(wavFile.FullName));
    }
}
