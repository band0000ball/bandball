using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class ToolItemDataStore : OwnedItemDataStore<ToolItemData>
    {
        private new const string OriginalItemsKey = "OriginalToolItemsData";
    }
}