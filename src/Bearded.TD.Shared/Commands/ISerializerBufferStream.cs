namespace Bearded.TD.Shared.Commands;

public interface ISerializerBufferStream
{
    void Serialize(ref byte[] bytes);

    void Serialize(ref int i);
    void Serialize(ref byte b);
    void Serialize(ref string s);
    void Serialize(ref float f);
    void Serialize(ref double f);
}
