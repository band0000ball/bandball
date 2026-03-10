using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// マップタブ。ミニマップ・エリアマップを表示するスタブ。
    ///
    /// マップシステムは未実装のためスタブとして機能する。
    /// 実装時は StageManager から現在ステージ情報を取得する。
    ///
    /// depends on: #32 StageManager（スタブ）
    /// </summary>
    public class MapTab : MonoBehaviour
    {
        [Header("ステージ情報")]
        [SerializeField] private TextMeshProUGUI _stageNameText;
        [SerializeField] private TextMeshProUGUI _checkpointText;

        [Header("マップ表示スタブ")]
        [SerializeField] private GameObject _mapStubPanel;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>タブ表示時に MenuCanvas から呼ばれる。</summary>
        public void Refresh()
        {
            RefreshStageInfo();
            _mapStubPanel?.SetActive(true);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void RefreshStageInfo()
        {
            var stageManager = Stage.StageManager.Instance;
            if (stageManager == null)
            {
                if (_stageNameText != null) _stageNameText.text = "ステージ: ---";
                if (_checkpointText != null) _checkpointText.text = "チェックポイント: ---";
                return;
            }

            var data = stageManager.CurrentStage;
            if (_stageNameText != null)
                _stageNameText.text = data != null ? $"ステージ: {data.stageName}" : "ステージ: ---";
            if (_checkpointText != null)
                _checkpointText.text = $"チェックポイント: {stageManager.CurrentCheckpointIndex}";
        }
    }
}
