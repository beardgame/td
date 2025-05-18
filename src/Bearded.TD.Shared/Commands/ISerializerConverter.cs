namespace Bearded.TD.Shared.Commands;

public interface ISerializerConverter<TDeserialized, TSerialized, in TContext>
{
    TSerialized Serialize(TDeserialized value);
    TDeserialized Deserialize(TSerialized value, TContext context);
}
