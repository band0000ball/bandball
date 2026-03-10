using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "ValuableItemDataBase", menuName = "ScriptableObject/DB/ValuableItemDB", order = 0)]
    public class ValuableItemDataBase : OwnedItemDataBase<ValuableItemData>
    { }
}