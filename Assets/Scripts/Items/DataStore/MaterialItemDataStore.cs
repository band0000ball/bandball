using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class MaterialItemDataStore : OwnedItemDataStore<MaterialItemData>
    {
        private new const string OriginalItemsKey = "OriginalMaterialItemsData";
    }
}