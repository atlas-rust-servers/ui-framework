using System.Reflection;
using HarmonyLib;
using Oxide.Ext.UiFramework.Libraries;
using Oxide.Ext.UiFramework.Types;

namespace Oxide.Ext.UiFramework.Harmony;

internal static class FileStorage_Get_Prefix_Patch
{
    private static NetworkableId _communityEntityId;
    
    internal static void Patch(ICommunityEntity entity)
    {
        _communityEntityId = new NetworkableId(entity.Id);
        MethodInfo target = AccessTools.Method(typeof(FileStorage), nameof(FileStorage.Get), [typeof(NetworkableId), typeof(ulong), typeof(uint)]);
        MethodInfo patch = AccessTools.Method(typeof(FileStorage_Get_Prefix_Patch), nameof(FileStorage_Get_Prefix));
        UiHarmony.Harmony.Patch(target, prefix: patch);
    }
    
    public static bool FileStorage_Get_Prefix(NetworkableId entityId, ulong part, uint crc, ref byte[] __result)
    {
        if (_communityEntityId != entityId || part != crc)
        {
            return true;
        }
        
        byte[] image = Singleton<UiImageDatabase>.Instance.Get(new ImageId(crc));
        if (image != null)
        {
            __result = image;
            return false;
        }

        return true;
    }
}
