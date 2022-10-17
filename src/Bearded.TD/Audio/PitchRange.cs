namespace Bearded.TD.Audio;

readonly record struct PitchRange(float LowerPitch, float UpperPitch)
{
    public static readonly PitchRange One = new(1, 1);
}
