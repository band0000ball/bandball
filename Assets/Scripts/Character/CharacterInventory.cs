using System.Linq;
using Databrain;
using Databrain.Inventory;
using Items.ItemData;
using Monster;
using Skill.SkillData;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// キャラクターのインベントリ管理を担当するコンポーネント
    /// アイテム、スキルスロットの管理
    /// </summary>
    [RequireComponent(typeof(CharacterControl))]
    public class CharacterInventory : MonoBehaviour
    {
        #region Private Fields

        private CharacterControl _control;
        private CharacterStatusUI _statusUI;
        private DataLibrary _dataLibrary;

        private CharacterSlotItemData _characterItems;
        private SkillData[] _cacheSkills;
        private bool[] _isActiveSkills;
        private float[] _fireCooldowns;

        #endregion

        #region Public Properties

        public CharacterSlotItemData CharacterItems => _characterItems;

        /// <summary>
        /// 攻撃アイテムかどうかの配列
        /// </summary>
        public bool[] IsAttackItems => _characterItems.inventorySlots
            .Select(x => (x.item as ExpendableItemData)?.Skill is AttackSkillData)
            .ToArray();

        /// <summary>
        /// スキルがnullかどうかの配列
        /// </summary>
        public bool[] IsNullItems => _characterItems.inventorySlots
            .Select(x => (x?.item as ExpendableItemData)?.Skill is null)
            .ToArray();

        #endregion

        #region Initialization

        public void Initialize(CharacterControl control, CharacterStatusUI statusUI,
            DataLibrary dataLibrary, int characterId, int level)
        {
            _control = control;
            _statusUI = statusUI;
            _dataLibrary = dataLibrary;

            _cacheSkills = new SkillData[10];
            _isActiveSkills = new bool[10];
            _fireCooldowns = new float[10];

            // キャラクタースロット初期化
            _characterItems = new CharacterSlotItemData();
            _characterItems.InitByCharacterSlot(dataLibrary, characterId, level);

            // スキルキャッシュの構築
            BuildSkillCache();
        }

        private void BuildSkillCache()
        {
            foreach (var slotItem in _characterItems.inventorySlots.Select((x, i) => new { x, i }))
            {
                if (slotItem.x?.item is ExpendableItemData skillItem)
                {
                    SkillData skill = skillItem.Skill;
                    if (skill != null)
                    {
                        _cacheSkills[slotItem.i] = skill;
                    }
                }
                else
                {
                    _characterItems.inventorySlots[slotItem.i].item = ScriptableObject.CreateInstance<ExpendableItemData>();
                }
            }
        }

        /// <summary>
        /// アイテムアイコンの初期表示設定
        /// </summary>
        public void SetupItemIcons()
        {
            if (_statusUI == null) return;

            foreach (var slotItem in _characterItems.inventorySlots.Select((x, i) => new { x, i }))
            {
                _statusUI.SetItemIcon(slotItem.i, slotItem.x?.item?.icon);
            }
        }

        #endregion

        #region Public Methods - Cooldowns

        /// <summary>
        /// 発射クールダウンの減少
        /// </summary>
        public void DecreaseFireCooldowns(float deltaTime)
        {
            if (_fireCooldowns == null || _fireCooldowns.Sum() <= 0) return;

            for (int i = 0; i < _fireCooldowns.Length; i++)
            {
                if (_fireCooldowns[i] > 0f)
                {
                    _fireCooldowns[i] -= deltaTime;
                }
                else
                {
                    _fireCooldowns[i] = 0f;
                }
            }
        }

        /// <summary>
        /// 発射クールダウンの取得
        /// </summary>
        public float GetFireCooldown(int index)
        {
            if (_fireCooldowns == null || index < 0 || index >= _fireCooldowns.Length) return 0f;
            return _fireCooldowns[index];
        }

        /// <summary>
        /// 発射クールダウンの設定
        /// </summary>
        public bool SetFireCooldown(int index, float addTime)
        {
            if (_fireCooldowns == null || index < 0 || index >= _fireCooldowns.Length) return false;
            if (_fireCooldowns[index] > 0) return false;

            _fireCooldowns[index] = addTime;
            _statusUI?.ActivateItemCooldown(index, addTime);
            return true;
        }

        #endregion

        #region Public Methods - Skills

        /// <summary>
        /// スキル使用
        /// </summary>
        public bool UseSkill(SkillData skill, float thickness, float strength, ref float stamina)
        {
            float consumeValue = Commons.Tools.RoundValue(thickness + skill.staminaConsume / strength, 2);
            if (stamina < consumeValue) return false;

            stamina -= consumeValue;
            return true;
        }

        /// <summary>
        /// スキルのアクティブ状態切り替え
        /// </summary>
        public bool SwapSkillActive(int index)
        {
            if (_fireCooldowns[index] <= 0 && (_isActiveSkills[index] || _characterItems.inventorySlots[index] == null))
            {
                _isActiveSkills[index] = false;
                _statusUI?.SetSkillDeactive(index);
                return false;
            }

            _isActiveSkills[index] = true;
            ExpendableItemData skillData = _characterItems.inventorySlots[index].item as ExpendableItemData;
            if (skillData?.Skill != null)
            {
                _statusUI?.SetSkillActive(index, skillData.Skill);
            }
            return true;
        }

        /// <summary>
        /// スキルがアクティブかどうか
        /// </summary>
        public bool GetIsSkillActivate(int index)
        {
            if (_isActiveSkills == null || index < 0 || index >= _isActiveSkills.Length) return false;
            return _isActiveSkills[index];
        }

        /// <summary>
        /// アクティブな攻撃スキルの配列取得
        /// </summary>
        public bool[] GetActiveAttackSkills()
        {
            return CharacterSlotItemData.GetChooseAttackItems(_isActiveSkills, IsAttackItems);
        }

        /// <summary>
        /// スロットのアイテム取得
        /// </summary>
        public ExpendableItemData GetSlotItem(int slot)
        {
            if (_characterItems?.inventorySlots == null || slot < 0 || slot >= _characterItems.inventorySlots.Count)
                return null;
            return _characterItems.inventorySlots[slot].item as ExpendableItemData;
        }

        /// <summary>
        /// キャッシュされたスキル取得
        /// </summary>
        public SkillData GetCacheSkill(int slot)
        {
            if (_cacheSkills == null || slot < 0 || slot >= _cacheSkills.Length) return null;
            return _cacheSkills[slot];
        }

        /// <summary>
        /// GUIDからスキル取得
        /// </summary>
        public SkillData GetSkillByGuid(string guid)
        {
            var skill = _dataLibrary.GetInitialDataObjectByGuid(guid);
            return skill as SkillData;
        }

        /// <summary>
        /// GUIDからアイテム取得
        /// </summary>
        public OwnedItemData GetItemByGuid(string guid)
        {
            return _dataLibrary.GetInitialDataObjectByGuid<OwnedItemData>(guid);
        }

        #endregion

        #region Public Methods - Items

        /// <summary>
        /// アイテム追加
        /// </summary>
        public (bool result, int remain) AddItem(OwnedItemData itemData, int amount)
        {
            var addIndex = _characterItems.AddItem(itemData, amount);
            bool result = addIndex.remainingNum <= 0;

            if (!addIndex.inBag)
            {
                for (int i = 0; i < addIndex.index.Length; i++)
                {
                    if (!addIndex.index[i]) continue;

                    _statusUI?.SetItemIcon(i, itemData.ItemSprite);

                    if (itemData is ExpendableItemData item)
                    {
                        _cacheSkills[i] = item.Skill;
                    }
                }
            }

            // インベントリUI更新
            InventoryUIController.Instance?.UpdateInventory(null);

            return (result, addIndex.remainingNum);
        }

        /// <summary>
        /// アイテム保存
        /// </summary>
        public void SaveItem()
        {
            _characterItems?.Save();
        }

        /// <summary>
        /// シールド値の取得
        /// </summary>
        public float GetShieldValues()
        {
            return _characterItems?.GetShieldValues() ?? 0f;
        }

        /// <summary>
        /// シールドの減少
        /// </summary>
        public void DecreaseShield(float value, float buffShield)
        {
            value -= buffShield;
            _characterItems?.DecreaseShield(value);
        }

        #endregion
    }
}
