using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "WeaponItemDataBase", menuName = "ScriptableObject/DB/WeaponItemDB")]
    public class WeaponItemDataBase : OwnedItemDataBase<WeaponItemData>
    { }
}