using Network;
using Oxide.Ext.UiFramework.Pooling;

namespace Oxide.Ext.UiFramework.Threading;

internal class UiDestroyRequest : BaseUiRequest, IUiRequest
{
    public string DestroyLayerName;

    public static UiDestroyRequest Create(string destroyLayerName, SendInfo send)
    {
        UiDestroyRequest request = UiFrameworkPool.Get<UiDestroyRequest>();
        request.Init(destroyLayerName, send);
        return request;
    }
    
    private void Init(string destroyLayerName, SendInfo send)
    {
        base.Init(send);
        DestroyLayerName = destroyLayerName;
    }
    
    public void SendUi()
    {
        CommunityEntity.ServerInstance.ClientRPC(new RpcTarget
        {
            Function = UiConstants.RpcFunctions.DestroyUiFunc,
            Connections = Send
        }, DestroyLayerName);
    }
    
    protected override void EnterPool()
    {
        base.EnterPool();
        DestroyLayerName = null;
    }
}