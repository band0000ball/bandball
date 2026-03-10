using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class ValuableItemDataStore : OwnedItemDataStore<ValuableItemData>
    {
        private new const string OriginalItemsKey = "OriginalValuableItemsData";
    }
}