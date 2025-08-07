using System;
using System.Buffers.Binary;
using System.Threading;
using Oxide.Ext.UiFramework.Builder.Cached;
using Oxide.Ext.UiFramework.Cache;
using Oxide.Ext.UiFramework.Exceptions;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.UiElements;

namespace Oxide.Ext.UiFramework.Builder.UI;

public partial class UiBuilder : BaseUiBuilder
{
    private static long s_uniqueId = DateTime.UtcNow.Ticks;
    
    public BaseUiComponent Root;

    private bool _randomNames;
    private bool _needsMouse;
    private bool _needsKeyboard;
    private bool _autoDestroy = true;

    #region Decontructor
    ~UiBuilder()
    {
        Dispose();
        //Need this because there is a global GC class that causes issues
        //ReSharper disable once RedundantNameQualifier
        System.GC.SuppressFinalize(this);
    }
    #endregion
        
    #region Setup
    public void SetRoot(BaseUiComponent component, string name, string parent)
    {
        Root = component;
        component.Reference = new UiReference(parent, name);
        Components.Add(component);
        RootName = name;
    }

    public void OverrideRoot(BaseUiComponent component)
    {
        Root = component;
    }

    public void NeedsMouse(bool enabled = true)
    {
        _needsMouse = enabled;
    }

    public void NeedsKeyboard(bool enabled = true)
    {
        _needsKeyboard = enabled;
    }

    public void UseRandomNames(bool enable = true)
    {
        _randomNames = enable;
    }
    
    public void EnableAutoDestroy(bool enabled = true)
    {
        _autoDestroy = enabled;
    }
    #endregion

    #region JSON
    public int WriteBuffer(byte[] buffer)
    {
        JsonFrameworkWriter writer = CreateWriter();
        int bytes = writer.WriteTo(buffer);
        writer.Dispose();
        return bytes;
    }

    public CachedUiBuilder ToCachedBuilder(bool dispose = true)
    {
        CachedUiBuilder cached = CachedUiBuilder.CreateCachedBuilder(this);
        if (dispose && !Disposed)
        {
            Dispose();
        }
        return cached;
    }
    #endregion

#region Methods

    private static string GenerateUniqueString()
    {
        long id = Interlocked.Increment(ref s_uniqueId);
        byte[] bytes = BitConverter.GetBytes(id);
        char[] chars = new char[bytes.Length * 2];

        const string hexChars = "0123456789ABCDEF";

        for (int i = 0; i < bytes.Length; i++)
        {
            chars[i * 2] = hexChars[bytes[i] >> 4];
            chars[i * 2 + 1] = hexChars[bytes[i] & 0xF];
        }

        return new string(chars);
    }

#endregion
        
#region Add Components
    public override void AddComponent(BaseUiComponent component, in UiReference parent)
    {
        UiReferenceException.ThrowIfInvalidParent(parent);
        component.Reference = parent.WithChild(_randomNames ? GenerateUniqueString() : UiNameCache.GetAnchorName(RootName, Anchors.Count));
        Components.Add(component);
    }
        
    protected override void AddAnchor(BaseUiComponent component, in UiReference parent)
    {
        UiReferenceException.ThrowIfInvalidParent(parent);
        component.Reference = parent.WithChild(_randomNames ? GenerateUniqueString() : UiNameCache.GetAnchorName(RootName, Anchors.Count));
        Anchors.Add(component);
    }
    #endregion

    protected override void WriteComponentsInternal(JsonFrameworkWriter writer)
    {
        Components[0].WriteRootComponent(writer, _needsMouse, _needsKeyboard, _autoDestroy);

        int count = Components.Count;
        for (int index = 1; index < count; index++)
        {
            Components[index].WriteComponent(writer);
        }

        count = Anchors.Count;
        for (int index = 0; index < count; index++)
        {
            Anchors[index].WriteComponent(writer);
        }
    }
        
    protected override void EnterPool()
    {
        base.EnterPool();
        Root = null;
        _needsKeyboard = false;
        _needsMouse = false;
        _autoDestroy = true;
    }
}