using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class ShieldItemDataStore : OwnedItemDataStore<ShieldItemData>
    {
        private new const string OriginalItemsKey = "OriginalShieldItemsData";
    }
}