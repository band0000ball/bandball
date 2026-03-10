using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "AmmoItemDataBase", menuName = "ScriptableObject/DB/AmmoItemDB", order = 0)]
    public class AmmoItemDataBase : OwnedItemDataBase<EnergyItemData>
    { }
}