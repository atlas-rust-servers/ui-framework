using Oxide.Ext.UiFramework.Enums;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.Types;

namespace Rust.UiFramework.UnitTests.Positions;

public class UiPaddingTests
{
    [Fact]
    public void Constructor_PreservesLeftBottomRightTopOrder()
    {
        UiPadding padding = new(1, 2, 3, 4);
        (float left, float bottom, float right, float top) = padding;

        Assert.Equal(1, left);
        Assert.Equal(2, bottom);
        Assert.Equal(3, right);
        Assert.Equal(4, top);
        Assert.Equal(2, padding.ToOffset().Min.y);
        Assert.Equal(-4, padding.ToOffset().Max.y);
        Assert.False(padding.IsSingleValue);
        Assert.True(new UiPadding(5).IsSingleValue);
    }

    [Fact]
    public void Parse_RoundTripsAsymmetricPadding()
    {
        UiPadding padding = new(1, 2, 3, 4);

        Assert.Equal(padding, UiPadding.Parse(padding.ToString()));
    }

    [Theory]
    [InlineData(0f, 2f, 4f)]
    [InlineData(0.5f, 12f, 24f)]
    [InlineData(1f, 22f, 44f)]
    public void Lerp_PreservesBottomAndTop(float progress, float bottom, float top)
    {
        UiPadding start = new(1, 2, 3, 4);
        UiPadding end = new(11, 22, 33, 44);

        UiPadding result = UiPadding.Lerp(start, end, progress);

        Assert.Equal(bottom, result.Bottom);
        Assert.Equal(top, result.Top);
    }

    [Fact]
    public void JsonWriter_PreservesLegacyFormatAndSupportsExplicitOrder()
    {
        UiPadding padding = new(1, 2, 3, 4);
        using JsonFrameworkWriter writer = JsonFrameworkWriter.Create(UnitTestHelpers.Plugin);
        writer.WriteStartArray();
        writer.WriteValue(padding);
        writer.WriteComma();
        writer.WriteValue(padding, UiPaddingFormat.LBRT);
        writer.WriteEndArray();

        Assert.Equal("[\"1 4 3 2\",\"1 2 3 4\"]", writer.ToString());
    }
}
