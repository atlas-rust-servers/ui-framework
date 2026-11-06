using Oxide.Ext.UiFramework.Data;
using Oxide.Ext.UiFramework.Types;

namespace Oxide.Ext.UiFramework.Libraries;

internal class UiFileStorageDatabase : BaseUiFrameworkLibrary, ISingleton, IImageDatabase
{
    private UiFileStorageDatabase() { }

    public void OnImageRegistered(ImageId id)
    {
        // Do nothing for FP FileStorage
    }

    public bool Exists(ImageId id) => Get(id) != null;
    public ImageId Store(byte[] image) => new(FileStorage.server.StoreByCrc(CommunityEntity.ServerInstance.net.ID, FileStorage.Type.png, image));
    public byte[] Get(ImageId id) => FileStorage.server.Get(CommunityEntity.ServerInstance.net.ID, id.Id, id.Id);
    public SaveVersion GetSaveVersion(ICommunityEntity entity) => new(entity.Id);
}
