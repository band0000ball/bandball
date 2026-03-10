using Stage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// ドロップした回収物を画面上にマークするUI要素。
    /// DeathPenaltyManager のイベントを購読し、DroppedPickup の位置をワールド→スクリーン変換して表示する。
    ///
    /// セットアップ:
    ///   - Canvas（Screen Space – Camera 推奨）の子に配置
    ///   - ItemDropMarkerContainer（空オブジェクト）に本スクリプトをアタッチ
    ///   - _markerPrefab: RectTransform + Image + TextMeshProUGUI (タイマー表示)
    ///   - _camera: メインカメラ（省略時 Camera.main）
    ///
    /// depends on: #31 DeathPenaltyManager, #16 DifficultyManager
    /// </summary>
    public class ItemDropMarker : MonoBehaviour
    {
        [Header("マーカープレハブ")]
        [SerializeField] private GameObject _markerPrefab;

        [Header("親キャンバス")]
        [SerializeField] private RectTransform _canvasRect;

        [SerializeField] private Camera _camera;

        // アクティブなマーカー群
        private System.Collections.Generic.List<MarkerEntry> _entries = new();

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }

        private void OnEnable()
        {
            DeathPenaltyManager.OnPickupSpawned   += HandleSpawned;
            DeathPenaltyManager.OnPickupRecovered += HandleRemoved;
            DeathPenaltyManager.OnPickupLost      += HandleRemoved;
        }

        private void OnDisable()
        {
            DeathPenaltyManager.OnPickupSpawned   -= HandleSpawned;
            DeathPenaltyManager.OnPickupRecovered -= HandleRemoved;
            DeathPenaltyManager.OnPickupLost      -= HandleRemoved;
        }

        private void Update()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry.Pickup == null)
                {
                    RemoveMarker(i);
                    continue;
                }

                UpdatePosition(entry);
                UpdateTimer(entry);
            }
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void HandleSpawned(DroppedPickup pickup)
        {
            if (_markerPrefab == null || pickup == null) return;

            var go     = Instantiate(_markerPrefab, transform);
            var marker = go.GetComponent<RectTransform>();

            var entry = new MarkerEntry
            {
                Pickup    = pickup,
                Marker    = marker,
                TimerText = go.GetComponentInChildren<TextMeshProUGUI>(),
                Icon      = go.GetComponentInChildren<Image>(),
            };

            _entries.Add(entry);
            UpdatePosition(entry);
            UpdateTimer(entry);
        }

        private void HandleRemoved(DroppedPickup pickup)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].Pickup == pickup)
                {
                    RemoveMarker(i);
                    return;
                }
            }
        }

        private void RemoveMarker(int index)
        {
            var entry = _entries[index];
            if (entry.Marker != null)
                Destroy(entry.Marker.gameObject);
            _entries.RemoveAt(index);
        }

        private void UpdatePosition(MarkerEntry entry)
        {
            if (entry.Marker == null || entry.Pickup == null || _camera == null) return;

            var screenPos = _camera.WorldToScreenPoint(entry.Pickup.transform.position);

            // カメラ背後は画面端にクランプ
            if (screenPos.z < 0f) screenPos = -screenPos;

            if (_canvasRect != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect, screenPos, null, out var localPoint);
                entry.Marker.localPosition = localPoint;
            }
            else
            {
                entry.Marker.position = screenPos;
            }
        }

        private void UpdateTimer(MarkerEntry entry)
        {
            if (entry.TimerText == null || entry.Pickup == null) return;

            float remaining = entry.Pickup.RemainingSeconds;
            int   minutes   = Mathf.FloorToInt(remaining / 60f);
            int   seconds   = Mathf.FloorToInt(remaining % 60f);
            entry.TimerText.text = $"{minutes:D2}:{seconds:D2}";

            // 残り30秒以下で赤く点滅
            if (entry.TimerText != null)
                entry.TimerText.color = remaining <= 30f ? Color.red : Color.white;
        }

        // ── Inner Types ───────────────────────────────────────────────────────

        private class MarkerEntry
        {
            public DroppedPickup    Pickup;
            public RectTransform    Marker;
            public TextMeshProUGUI  TimerText;
            public Image            Icon;
        }
    }
}
