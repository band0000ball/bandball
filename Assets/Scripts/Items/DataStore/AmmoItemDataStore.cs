using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class AmmoItemDataStore : OwnedItemDataStore<EnergyItemData>
    {
        private new const string OriginalItemsKey = "OriginalAmmoItemsData";
    }
}