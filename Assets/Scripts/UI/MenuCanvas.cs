using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    /// <summary>
    /// メインメニューキャンバスコントローラー。
    /// Esc キー押下でトグル表示。リアルタイム動作（Time.timeScale 変更なし）。
    ///
    /// タブ構成（ShowTab インデックス対応）:
    ///   0: ステータス, 1: インベントリ, 2: アビリティ, 3: マップ, 4: 設定
    ///
    /// セットアップ:
    ///   - 常駐 GameObject に AddComponent し、_menuPanel と各タブルートを Inspector で設定
    ///   - 各タブルートの GameObject は MenuCanvas が開閉と表示切替を管理する
    ///
    /// depends on: #14（アビリティタブはスタブ）
    /// </summary>
    public class MenuCanvas : MonoBehaviour
    {
        public static MenuCanvas Instance { get; private set; }

        [Header("メニュールートパネル")]
        [SerializeField] private GameObject _menuPanel;

        [Header("各タブのルート GameObject")]
        [SerializeField] private GameObject _statusTabRoot;
        [SerializeField] private GameObject _inventoryTabRoot;
        [SerializeField] private GameObject _abilityTabRoot;
        [SerializeField] private GameObject _mapTabRoot;
        [SerializeField] private GameObject _settingsTabRoot;

        [Header("各タブコンポーネント")]
        [SerializeField] private StatusTab _statusTab;
        [SerializeField] private InventoryTab _inventoryTab;
        [SerializeField] private AbilityTab _abilityTab;
        [SerializeField] private MapTab _mapTab;
        [SerializeField] private SettingsTab _settingsTab;

        private bool _isOpen;
        private int _activeTabIndex;
        private GameObject[] _tabRoots;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _tabRoots = new[]
            {
                _statusTabRoot,
                _inventoryTabRoot,
                _abilityTabRoot,
                _mapTabRoot,
                _settingsTabRoot,
            };

            Close();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Toggle();
        }

        // ── Public API ────────────────────────────────────────────────────────

        public bool IsOpen => _isOpen;

        public void Toggle() { if (_isOpen) Close(); else Open(); }

        public void Open(int tabIndex = -1)
        {
            _isOpen = true;
            _menuPanel?.SetActive(true);
            ShowTab(tabIndex >= 0 ? tabIndex : _activeTabIndex);
        }

        public void Close()
        {
            _isOpen = false;
            _menuPanel?.SetActive(false);
        }

        /// <summary>指定タブを表示する（0=ステータス 1=インベントリ 2=アビリティ 3=マップ 4=設定）</summary>
        public void ShowTab(int index)
        {
            if (index < 0 || index >= _tabRoots.Length) return;

            _activeTabIndex = index;

            for (int i = 0; i < _tabRoots.Length; i++)
                _tabRoots[i]?.SetActive(i == index);

            RefreshActiveTab(index);
        }

        // ── ボタンから呼ぶラッパー ─────────────────────────────────────────────

        public void ShowStatusTab()    => ShowTab(0);
        public void ShowInventoryTab() => ShowTab(1);
        public void ShowAbilityTab()   => ShowTab(2);
        public void ShowMapTab()       => ShowTab(3);
        public void ShowSettingsTab()  => ShowTab(4);

        // ── Private ───────────────────────────────────────────────────────────

        private void RefreshActiveTab(int index)
        {
            switch (index)
            {
                case 0: _statusTab?.Refresh();    break;
                case 1: _inventoryTab?.Refresh(); break;
                case 2: _abilityTab?.Refresh();   break;
                case 3: _mapTab?.Refresh();       break;
                case 4: _settingsTab?.Refresh();  break;
            }
        }
    }
}
