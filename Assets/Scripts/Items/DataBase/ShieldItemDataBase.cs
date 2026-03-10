using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "ShieldItemDataBase", menuName = "ScriptableObject/DB/ShieldItemDB", order = 0)]
    public class ShieldItemDataBase : OwnedItemDataBase<ShieldItemData>
    { }
}