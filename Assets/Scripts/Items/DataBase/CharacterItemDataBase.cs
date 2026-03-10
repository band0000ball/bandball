using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "CharacterItemDataBase", menuName = "ScriptableObject/DB/CharacterItemDB", order = 0)]
    public class CharacterItemDataBase : OwnedItemDataBase<CharacterItemData>
    { }
}