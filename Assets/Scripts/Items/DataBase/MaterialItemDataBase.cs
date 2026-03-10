using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "MaterialItemDataBase", menuName = "ScriptableObject/DB/MaterialItemDB", order = 0)]
    public class MaterialItemDataBase : OwnedItemDataBase<MaterialItemData>
    { }
}