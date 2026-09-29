using Network;
using Oxide.Ext.UiFramework.Builder;
using Oxide.Ext.UiFramework.Builder.UI;
using Oxide.Ext.UiFramework.Colors;
using Oxide.Ext.UiFramework.Components;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.Libraries;
using Oxide.Ext.UiFramework.UiElements;

namespace Rust.UiFramework.UnitTests.Builder;

public class BuilderCompatibilityTests
{
    [Fact]
    public void ColorBlock_PreservesLegacyPositionalArguments()
    {
        using UiButton button = UnitTestHelpers.UnitTestPool.Get<UiButton>();
        using UiBuilder builder = UiBuilder.Create(UnitTestHelpers.Plugin);

        ColorBlockComponent colors = button.AddColorBlock(UiColors.Blue, UiColors.Red, UiColors.Green, 2f, 0.25f);
        Assert.Equal(2f, colors.ColorMultiplier);
        Assert.Equal(0.25f, colors.FadeDuration);
        Assert.Equal(new UiColor(128, 128, 128, 255), colors.DisabledColor);

        colors = builder.ColorBlock(button, null, null, null, 3f);
        Assert.Equal(3f, colors.ColorMultiplier);
        Assert.Equal(0.25f, colors.FadeDuration);

        colors = button.AddColorBlock(null, null, null, UiColors.Magenta, null, null);
        Assert.Equal(UiColors.Magenta, colors.DisabledColor);
    }

    [Fact]
    public void Scrollbars_SupportLegacyAndNewOverloads()
    {
        using UiScrollView view = UnitTestHelpers.UnitTestPool.Get<UiScrollView>();
        using UiBuilder builder = UiBuilder.Create(UnitTestHelpers.Plugin);

        ScrollbarComponent vertical = view.AddVerticalScrollBar(false, true, null, null, 5f, null, null, null, null);
        Assert.True(vertical.AutoHide);
        Assert.Equal(5f, vertical.Size);
        Assert.Equal(JsonDefaults.ScrollBar.FadeDuration, vertical.FadeDuration);

        ScrollbarComponent horizontal = builder.AddHorizontalScrollBar(view, true, false, null, null, 7f, null, null, null, null);
        Assert.True(horizontal.Invert);
        Assert.Equal(7f, horizontal.Size);

        view.AddScrollBars(fadeDuration: 0.75f);
        Assert.Equal(0.75f, view.HorizontalScrollbar.FadeDuration);
        Assert.Equal(0.75f, view.VerticalScrollbar.FadeDuration);
    }

    [Fact]
    public void SendInfo_FiltersDisconnectedConnectionsAndKeepsAnimationOrdering()
    {
        Connection connected = new() { connected = true };
        Connection disconnected = new() { connected = false };
        List<Connection> source = [connected, disconnected];
        SendInfo send = SendInfoBuilder.Get(source);
        SendInfo animation = default;

        try
        {
            animation = SendInfoBuilder.GetForAnimations(send);
            Assert.Single(send.connections);
            Assert.Same(connected, send.connections[0]);
            Assert.NotSame(source, send.connections);
            Assert.NotSame(send.connections, animation.connections);
            Assert.Equal(send.channel, animation.channel);
            Assert.Equal(2, source.Count);
        }
        finally
        {
            if (animation.connections != null)
            {
                UiPool.Connections.FreeList(animation.connections);
            }
            UiPool.Connections.FreeList(send.connections);
        }
    }
}
