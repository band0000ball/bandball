using Stage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 難易度選択UI。ステージ選択画面に配置し、出撃前に難易度を設定する。
    /// ステージ攻略中（DifficultyManager.IsLocked）はボタンを無効化する。
    ///
    /// セットアップ:
    ///   - 各難易度ボタンの OnClick に対応するメソッドを設定
    ///   - _penaltyDescText に選択難易度のペナルティ説明を表示する
    ///
    /// depends on: #16 DifficultyManager
    /// </summary>
    public class DifficultySelectUI : MonoBehaviour
    {
        [Header("難易度ボタン")]
        [SerializeField] private Button _normalButton;
        [SerializeField] private Button _hardButton;
        [SerializeField] private Button _ultraButton;

        [Header("選択中表示")]
        [SerializeField] private GameObject _normalSelected;
        [SerializeField] private GameObject _hardSelected;
        [SerializeField] private GameObject _ultraSelected;

        [Header("情報テキスト")]
        [SerializeField] private TextMeshProUGUI _penaltyDescText;
        [SerializeField] private TextMeshProUGUI _rewardDescText;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            DifficultyManager.OnDifficultyChanged += OnDifficultyChanged;
            Refresh();
        }

        private void OnDisable()
        {
            DifficultyManager.OnDifficultyChanged -= OnDifficultyChanged;
        }

        // ── ボタンコールバック ─────────────────────────────────────────────────

        public void SelectNormal() => DifficultyManager.Instance?.SetDifficulty(StageDifficulty.Normal);
        public void SelectHard()   => DifficultyManager.Instance?.SetDifficulty(StageDifficulty.Hard);
        public void SelectUltra()  => DifficultyManager.Instance?.SetDifficulty(StageDifficulty.Ultra);

        // ── Private ───────────────────────────────────────────────────────────

        private void OnDifficultyChanged(DifficultySettings settings) => Refresh();

        private void Refresh()
        {
            var dm = DifficultyManager.Instance;
            if (dm == null) return;

            bool locked = dm.IsLocked;
            if (_normalButton != null) _normalButton.interactable = !locked;
            if (_hardButton   != null) _hardButton.interactable   = !locked;
            if (_ultraButton  != null) _ultraButton.interactable  = !locked;

            var current = dm.Current.difficulty;
            _normalSelected?.SetActive(current == StageDifficulty.Normal);
            _hardSelected?.SetActive(current   == StageDifficulty.Hard);
            _ultraSelected?.SetActive(current  == StageDifficulty.Ultra);

            UpdateDescriptions(dm.Current);
        }

        private void UpdateDescriptions(DifficultySettings s)
        {
            if (_penaltyDescText != null)
            {
                _penaltyDescText.text = s.difficulty switch
                {
                    StageDifficulty.Normal =>
                        "所持金 10% ロスト\n装備・キャラクターへのペナルティなし",
                    StageDifficulty.Hard =>
                        "所持金 50% ロスト\n装備の一部をドロップ（10分以内に回収可）",
                    StageDifficulty.Ultra =>
                        "所持金 100% ロスト（回収可）\n装備を全てドロップ（10分）\nキャラクタードロップ（30分・失敗で永久ロスト）",
                    _ => string.Empty,
                };
            }

            if (_rewardDescText != null)
            {
                _rewardDescText.text =
                    $"通貨 x{s.currencyDropMultiplier:F1}  " +
                    $"アイテム x{s.itemDropRateMultiplier:F1}  " +
                    $"レア x{s.rareItemRateMultiplier:F1}\n" +
                    $"敵レベル +{s.enemyLevelBonus}";
            }
        }
    }
}
