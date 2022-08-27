using Bearded.Audio;

namespace Bearded.TD.Audio;

sealed class Sound : ISound
{
    private readonly SoundBufferData bufferData;

    private Sound(SoundBufferData bufferData)
    {
        this.bufferData = bufferData;
    }

    public static Sound FromWav(string file)
    {
        return new Sound(SoundBufferData.FromWav(file));
    }
}
