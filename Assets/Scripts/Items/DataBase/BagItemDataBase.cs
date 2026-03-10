using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "BagItemDataBase", menuName = "ScriptableObject/DB/BagItemDB", order = 0)]
    public class BagItemDataBase : OwnedItemDataBase<BagItemData>
    { }
}