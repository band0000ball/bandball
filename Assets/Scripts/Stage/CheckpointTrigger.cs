using UnityEngine;

namespace Stage
{
    /// <summary>
    /// シーンに配置するチェックポイント。
    /// プレイヤーが Trigger コライダーに入ると StageManager にチェックポイント到達を通知する。
    /// 一度通過すると再発火しない（_activated フラグ）。
    ///
    /// Inspector 設定:
    ///   - Collider を "Is Trigger = true" に設定すること
    ///   - _checkpointIndex はステージ内で連番かつユニークに設定すること
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private int _checkpointIndex;

        [Tooltip("通過済みを視覚的に示す SpriteRenderer（任意）")]
        [SerializeField] private SpriteRenderer _indicator;

        private bool _activated;

        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_activated) return;
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

            _activated = true;
            StageManager.Instance?.RegisterCheckpoint(_checkpointIndex, transform.position);

            if (_indicator != null)
                _indicator.color = Color.green;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _activated ? Color.green : Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                $"CP[{_checkpointIndex}]");
        }
#endif
    }
}
