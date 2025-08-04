using Network;
using Oxide.Ext.UiFramework.Builder.UI;

namespace Oxide.Ext.UiFramework.Builder.Cached;

public class CachedUiBuilder : BaseBuilder
{
    private readonly bool _disposeBuilderOnGeneration;
    private UiBuilder _builder;

    private byte[] _cachedJson;

    internal static CachedUiBuilder CreateCachedBuilder(UiBuilder builder, bool disposeBuilder = true)
    {
        return new CachedUiBuilder(builder, disposeBuilder);
    }

    private CachedUiBuilder(UiBuilder builder, bool disposeBuilder = true)
    {
        _disposeBuilderOnGeneration = disposeBuilder;
        _builder = builder;

        RootName = builder.GetRootName();
    }

    public override byte[] GetBytes()
    {
        if (_cachedJson == null)
            CacheJson();

        return _cachedJson;
    }

    internal override void SendUi(SendInfo send)
    {
        byte[] data = GetBytes();
        AddUi(send, data);
    }

    private void CacheJson()
    {
        _cachedJson = _builder.GetBytes();

        if (_disposeBuilderOnGeneration && !_builder.Disposed)
            _builder.Dispose();

        _builder = null;
    }
}