using System;
using System.ComponentModel;
using System.Globalization;

namespace Bearded.TD.Game.Simulation.Enemies.Serialization;

sealed class SocketShapeTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string s ? SocketShape.FromLiteral(s) : base.ConvertFrom(context, culture, value);
}
