using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class FoodItemDataStore : OwnedItemDataStore<FoodItemData>
    {
        private new const string OriginalItemsKey = "OriginalFoodItemsData";
    }
}