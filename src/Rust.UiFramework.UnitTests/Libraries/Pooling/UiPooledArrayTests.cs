using Oxide.Ext.UiFramework.Pooling;

namespace Rust.UiFramework.UnitTests.Libraries.Pooling;

public class UiPooledArrayTests
{
    [Fact]
    public void SetArray_PreservesExistingStorageAndClearsValuesOnReturn()
    {
        int[] storage = [1, 2, 3];
        UiPooledArray<int> array = new();
        array.SetArray(storage);
        array.SetArray([4, 5]);
        array.WithLength(1);

        UnitTestHelpers.EnterPool(array);

        Assert.Same(storage, array.Array);
        Assert.Equal(3, array.Count);
        Assert.Equal(new[] { 0, 0, 0 }, storage);
    }
}
