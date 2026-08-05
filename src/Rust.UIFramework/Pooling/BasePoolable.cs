using System;
using Oxide.Ext.UiFramework.Libraries;

namespace Oxide.Ext.UiFramework.Pooling;

/// <summary>
/// Represents a poolable object
/// </summary>
public abstract class BasePoolable : IPoolable
{
    public bool IsPooled { get; private set; }

    internal bool CanPool => _pool != null && !IsPooled;
    private IObjectPool<BasePoolable> _pool;
    internal UiPluginPool PluginPool;
    UiPluginPool IPoolable.PluginPool => PluginPool;

    internal void OnInitInternal(IObjectPool<BasePoolable> pool)
    {
        _pool = pool;
        PluginPool = pool.PluginPool;
        OnInit();
    }
    
    internal virtual void OnInit() {}
    
    internal virtual void OverridePluginPool(UiPluginPool pluginPool)
    {
        PluginPool = pluginPool;
    }

    internal void EnterPoolInternal()
    {
        EnterPool();
        IsPooled = true;
    }

    internal void LeavePoolInternal()
    {
        IsPooled = false;
        LeavePool();
    }

    /// <summary>
    /// Called when the object is returned to the pool.
    /// Can be overriden in child classes to clean up used data
    /// </summary>
    protected virtual void EnterPool() { }
        
    /// <summary>
    /// Called when the object leaves the pool.
    /// Can be overriden in child classes to set the initial object state
    /// </summary>
    protected virtual void LeavePool() { }
    
#if UNIT_TESTS
    internal void TestEnterPool() => EnterPool();
    internal void TestLeavePool() => LeavePool();
#endif

    /// <summary>
    /// False while another owner is responsible for returning this object to the pool.
    /// Freeing it from the caller in that window would hand the object out again while the owner is still reading it.
    /// </summary>
    internal virtual bool IsOwnedByCaller => true;

    public void TryDispose()
    {
        if (CanPool && IsOwnedByCaller)
        {
            Dispose();
        }
    }

    /// <summary>
    /// Throws if the object was already returned to the pool.
    /// Reading a pooled object races with whoever owns it now, so this turns a use after free into an error that names the owning plugin.
    /// </summary>
    internal void ThrowIfPooled(string usage)
    {
        if (IsPooled)
        {
            throw new ObjectDisposedException(GetType().Name, $"{usage}. The object was already returned to the pool. Plugin: {PluginPool?.PluginName ?? "Unknown"}");
        }
    }
    
    public void Dispose()
    {
        if (_pool == null || !IsOwnedByCaller)
        {
            return;
        }

        if (IsPooled)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
        
        _pool.Free(this);
    }
}