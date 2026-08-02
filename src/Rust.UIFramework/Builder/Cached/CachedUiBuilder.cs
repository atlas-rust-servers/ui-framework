using System;
using Network;
using Oxide.Ext.UiFramework.Builder.UI;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.Plugins;

namespace Oxide.Ext.UiFramework.Builder.Cached;

public class CachedUiBuilder : BaseBuilder
{
    private readonly byte[] _cachedJson;

    private CachedUiBuilder(UiBuilder builder)
    {
        _cachedJson = builder.GetBytes();
        RootName = builder.GetRootName();
        Plugin = builder.Plugin;
    }

    private CachedUiBuilder(byte[] json, IUiFrameworkPlugin plugin)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        if (json.Length < 2 || json[0] != (byte)'[' || json[json.Length - 1] != (byte)']')
        {
            throw new ArgumentException("Cached UI json must be a JSON array.", nameof(json));
        }

        _cachedJson = json;
        Plugin = plugin;
    }

    public static CachedUiBuilder Create(byte[] json) => new(json, null);

    public static CachedUiBuilder Create(byte[] json, IUiFrameworkPlugin plugin) => new(json, plugin);

    internal static CachedUiBuilder CreateCachedBuilder(UiBuilder builder) => new(builder);

    public override byte[] GetBytes() => _cachedJson;

    public ReadOnlySpan<byte> JsonBody => _cachedJson.AsSpan(1, _cachedJson.Length - 2);

    public override void Combine(SendInfo send, JsonFrameworkWriter writer)
    {
        writer.WriteRaw(JsonBody);
    }

    internal override void SendUi(SendInfo send, in UiDebugOptions? options)
    {
        AddUi(send, GetBytes(), options);
    }

    internal override void SendAnimations(SendInfo send) { }
}