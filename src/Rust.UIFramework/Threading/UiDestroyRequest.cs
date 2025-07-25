using Network;
using Oxide.Ext.UiFramework.Pooling;

namespace Oxide.Ext.UiFramework.Threading;

internal class UiDestroyRequest : BaseUiRequest, IUiRequest
{
    private string _layer;

    public static UiDestroyRequest Create(SendInfo send, string layer)
    {
        var request = UiFrameworkPool.Get<UiDestroyRequest>();
        request.Init(send, layer);
        
        return request;
    }

    private void Init(SendInfo send, string layer)
    {
        base.Init(send);
        _layer = layer;
    }

    void IUiRequest.SendUi()
    {
        CommunityEntity.ServerInstance.ClientRPC(new RpcTarget
        {
            Function = UiConstants.RpcFunctions.DestroyUiFunc,
            Connections = Send
        }, _layer);
    }

    protected override void EnterPool()
    {
        base.EnterPool();
        _layer = null;
    }
}