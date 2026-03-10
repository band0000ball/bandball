using System;
using System.Collections;
using HutongGames.PlayMaker.Actions;
using UnityEditor;
using UnityEngine;

namespace Items.ItemData
{
    [Serializable]
    [CreateAssetMenu(fileName = "AmmoItemData", menuName = "ScriptableObject/Data/Item/AmmoItemData")]
    public class EnergyItemData : OwnedItemData
    {
        // 弾薬系アイテムの基本情報
        public enum EnergyType
        {
            None, // このTypeのアイテムはないのでこのTypeがsettableに指定されている場合はEnergyをセットできないということ
            Orb, // 近接武器にセットできる
            Ammo, // 射撃武器にセットできる
            Battery, // ツールにセットできる
            Core, // 盾にセットできる
        }
        
        public EnergyType energyType;
        public float reloadTime = 0.5f;
        // 射撃武器は弾速として、その他は発動速度として使用する
        public float speed = 1f;
        // 武器は攻撃力、盾は防御力、ツールは効果量として使用する
        public float power = 1f;
        // 武器や盾はいいとして、ツールは未定
        public float attributeValue = 0f;
        // Ammoは射程として使い、その他は味方や敵への影響範囲として使う
        public float minRange = 0f;
        public float maxRange = 0f;
    }
}