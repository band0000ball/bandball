using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "ToolItemDataBase", menuName = "ScriptableObject/DB/ToolItemDB", order = 0)]
    public class ToolItemDataBase : OwnedItemDataBase<ToolItemData>
    { }
}