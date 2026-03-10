using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class BagItemDataStore : OwnedItemDataStore<BagItemData>
    {
        private new const string OriginalItemsKey = "OriginalBagItemsData";
    }
}