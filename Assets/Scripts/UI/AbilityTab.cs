using Character;
using Stage;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// アビリティタブ。装備スキル一覧とアビリティツリーを表示する。
    ///
    /// depends on: #13 AbilityTreeManager, #14 AbilityTreePanel
    /// </summary>
    public class AbilityTab : MonoBehaviour
    {
        [Header("装備スキル欄（スロット0〜4）")]
        [SerializeField] private TextMeshProUGUI[] _skillSlotTexts;

        [Header("AP 表示")]
        [SerializeField] private TextMeshProUGUI _apText;

        [Header("アビリティツリーパネル（#14）")]
        [SerializeField] private AbilityTreePanel _treePanel;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>タブ表示時に MenuCanvas から呼ばれる。</summary>
        public void Refresh()
        {
            var character = GetCurrentOperator();

            RefreshSkillSlots(character);
            RefreshAP(character);
            RefreshTreePanel(character);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private static CharacterControl GetCurrentOperator()
        {
            var pcc = PlayerCharacterController.Instance;
            return pcc != null ? pcc.CurrentOperator : null;
        }

        private void RefreshSkillSlots(CharacterControl character)
        {
            if (_skillSlotTexts == null) return;

            for (int i = 0; i < _skillSlotTexts.Length; i++)
            {
                if (_skillSlotTexts[i] == null) continue;

                if (character != null)
                {
                    // UnlockSkill ノードで解放されたスキルGUIDをスロットに表示
                    // TODO: SkillComponent と連携してスロット名を取得する
                    _skillSlotTexts[i].text = $"スロット {i}: ---";
                }
                else
                {
                    _skillSlotTexts[i].text = $"スロット {i}: ---";
                }
            }
        }

        private void RefreshAP(CharacterControl character)
        {
            if (_apText == null) return;
            _apText.text = character != null ? $"AP: {character.GetAbilityPoints()}" : "AP: ---";
        }

        private void RefreshTreePanel(CharacterControl character)
        {
            if (_treePanel == null) return;
            _treePanel.SetCharacter(character);
        }
    }
}
