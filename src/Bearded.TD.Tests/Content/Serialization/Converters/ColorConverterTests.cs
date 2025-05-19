using Bearded.Graphics;
using Bearded.TD.Content.Serialization.Converters;
using FluentAssertions;
using FsCheck.Xunit;
using Newtonsoft.Json;
using Xunit;
using static Bearded.TD.Tests.Content.Serialization.Converters.Common;

namespace Bearded.TD.Tests.Content.Serialization.Converters;

public sealed class ColorConverterTests
{
    private readonly JsonSerializer serializer;

    public ColorConverterTests()
    {
        serializer = new JsonSerializer();
        serializer.Converters.Add(new ColorConverter());
    }

    [Fact]
    public void DeserializesNamedColors()
    {
        var c = serializer.Deserialize<Color>(StaticStringReader("\"hotPink\""));

        c.Should().Be(Color.HotPink);
    }

    [Property]
    public void DeserializesArraysWithoutAlpha(byte r, byte g, byte b)
    {
        var c = serializer.Deserialize<Color>(StaticStringReader($"[{r}, {g}, {b}]"));

        c.Should().Be(new Color(r, g, b));
    }

    [Property]
    public void DeserializesArraysWithAlpha(byte r, byte g, byte b, byte a)
    {
        var c = serializer.Deserialize<Color>(StaticStringReader($"[{r}, {g}, {b}, {a}]"));

        c.Should().Be(new Color(r, g, b, a));
    }

    [Fact]
    public void DeserializesHexStrings()
    {
        var c = serializer.Deserialize<Color>(StaticStringReader("\"00abcdef\""));

        c.Should().Be(new Color(0x00abcdef));
    }
}
