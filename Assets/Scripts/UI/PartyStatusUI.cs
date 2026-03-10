using System.Collections.Generic;
using Character;
using Stage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// ステージ中に常時表示するパーティステータスUI。
    /// 操作キャラ + パートナー全員の名前・HPバー・死亡状態を表示する。
    /// 生存キャラのスロットをクリックすると PlayerCharacterController.SwitchOperator() を呼ぶ。
    ///
    /// セットアップ:
    ///   - UserUI Canvas の常駐 GameObject に AddComponent
    ///   - _slots に最大4スロット分の PartyMemberSlot を配列で設定
    ///
    /// depends on: #34b PlayerCharacterController, #34a CharacterControl
    /// </summary>
    public class PartyStatusUI : MonoBehaviour
    {
        [SerializeField] private PartyMemberSlot[] _slots;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Update()
        {
            var pcc = PlayerCharacterController.Instance;
            if (pcc == null)
            {
                HideAll();
                return;
            }

            int slotIndex = 0;

            // 操作キャラを先頭に表示
            if (pcc.CurrentOperator != null)
            {
                ShowSlot(slotIndex, pcc.CurrentOperator, isOperator: true);
                slotIndex++;
            }

            // パートナー
            IReadOnlyList<CharacterControl> partners = pcc.ActivePartners;
            for (int i = 0; i < partners.Count && slotIndex < _slots.Length; i++, slotIndex++)
                ShowSlot(slotIndex, partners[i], isOperator: false);

            // 残りスロットを非表示
            for (int i = slotIndex; i < _slots.Length; i++)
                _slots[i]?.Hide();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void ShowSlot(int index, CharacterControl cc, bool isOperator)
        {
            if (index < 0 || index >= _slots.Length || _slots[index] == null) return;

            bool isDead = cc.GetIsDead();
            float hp    = cc.GetHealth();
            float maxHp = cc.GetMaxHealth();

            _slots[index].Show(
                characterLabel: $"ID: {cc.CharacterId}",
                hp:             hp,
                maxHp:          maxHp,
                isDead:         isDead,
                isOperator:     isOperator,
                onClicked:      isDead ? null : () => PlayerCharacterController.Instance?.SwitchOperator(cc)
            );
        }

        private void HideAll()
        {
            if (_slots == null) return;
            foreach (var s in _slots)
                s?.Hide();
        }
    }
}
