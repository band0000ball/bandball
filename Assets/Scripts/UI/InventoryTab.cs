using Items.ItemData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// インベントリタブ。所持アイテム一覧とアイテム詳細を表示する。
    ///
    /// アイテムグリッドの生成は Inspector で設定した _gridRoot 配下に
    /// _itemCellPrefab をインスタンス化して行う（アイテムシステム整備後に実装）。
    ///
    /// depends on: Items システム（現時点でスタブ）
    /// </summary>
    public class InventoryTab : MonoBehaviour
    {
        [Header("グリッド")]
        [SerializeField] private Transform _gridRoot;
        [SerializeField] private GameObject _itemCellPrefab;

        [Header("アイテム詳細パネル")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _itemTypeText;
        [SerializeField] private TextMeshProUGUI _itemDescText;
        [SerializeField] private Image _itemIconImage;

        private OwnedItemData _selectedItem;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>タブ表示時に MenuCanvas から呼ばれる。</summary>
        public void Refresh()
        {
            // TODO: インベントリシステム実装後にグリッド生成を追加
            HideDetail();
        }

        /// <summary>アイテムセルがクリックされたときに呼ぶ。</summary>
        public void OnItemCellClicked(OwnedItemData item)
        {
            _selectedItem = item;
            ShowDetail(item);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void ShowDetail(OwnedItemData item)
        {
            if (item == null) { HideDetail(); return; }

            _detailPanel?.SetActive(true);
            if (_itemNameText != null) _itemNameText.text = item.ItemName;
            if (_itemTypeText != null) _itemTypeText.text = item.Type.ToString();
            if (_itemDescText != null) _itemDescText.text = string.Empty; // TODO: 説明文取得
            if (_itemIconImage != null) _itemIconImage.sprite = null;     // TODO: アイコン取得
        }

        private void HideDetail()
        {
            _detailPanel?.SetActive(false);
            _selectedItem = null;
        }
    }
}
