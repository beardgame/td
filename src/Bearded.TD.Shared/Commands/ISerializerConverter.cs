using System;

namespace Bearded.TD.Shared.Commands;

public interface ISerializerConverter<TDeserialized, TSerialized, in TContext>
{
    TSerialized Serialize(TDeserialized value);
    TDeserialized Deserialize(TSerialized value, TContext context);
}

public sealed class SerializerConverter<TDeserialized, TSerialized, TContext>(
    Func<TDeserialized, TSerialized> serialize,
    Func<TSerialized, TContext, TDeserialized> deserialize
    )
    : ISerializerConverter<TDeserialized, TSerialized, TContext>
{
    public TSerialized Serialize(TDeserialized value) => serialize(value);

    public TDeserialized Deserialize(TSerialized value, TContext context) => deserialize(value, context);
}
