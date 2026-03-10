using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class WeaponItemDataStore : OwnedItemDataStore<WeaponItemData>
    {
        private new const string OriginalItemsKey = "OriginalWeaponItemsData";
    }
}