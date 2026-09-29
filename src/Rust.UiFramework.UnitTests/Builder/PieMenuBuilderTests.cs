using Oxide.Ext.UiFramework.Builder;

namespace Rust.UiFramework.UnitTests.Builder;

public class PieMenuBuilderTests
{
    [Fact]
    public void Create_AcceptsRegularPluginAndReturnsItemsWithBuilder()
    {
        PieMenuBuilder builder = PieMenuBuilder.Create(UnitTestHelpers.Plugin);
        PieMenuItemBuilder item = builder.AddItem().SetName("Home").SetCommand("menu.home");

        try
        {
            Assert.Same(UnitTestHelpers.Plugin, builder.Plugin);
            Assert.False(builder.IsPooled);
            Assert.False(item.IsPooled);
        }
        finally
        {
            builder.TryDispose();
        }

        Assert.True(builder.IsPooled);
        Assert.True(item.IsPooled);
    }
}
