using Items.ItemData;
using UnityEngine;

namespace Items.DataBase
{
    [CreateAssetMenu(fileName = "FoodItemDataBase", menuName = "ScriptableObject/DB/FoodItemDB", order = 0)]
    public class FoodItemDataBase : OwnedItemDataBase<FoodItemData>
    { }
}