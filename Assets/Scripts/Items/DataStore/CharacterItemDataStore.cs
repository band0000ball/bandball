using Items.DataBase;
using Items.ItemData;

namespace Items.DataStore
{
    public class CharacterItemDataStore : OwnedItemDataStore<CharacterItemData>
    {
        private new const string OriginalItemsKey = "OriginalCharacterItemsData";
    }
}