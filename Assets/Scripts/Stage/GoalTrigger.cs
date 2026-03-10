using UnityEngine;

namespace Stage
{
    /// <summary>
    /// シーンに配置するゴール地点。ClearCondition.ReachGoal ステージで使用する。
    /// プレイヤーが Trigger コライダーに入ると StageManager.NotifyGoalReached() を呼び出す。
    ///
    /// Inspector 設定:
    ///   - Collider を "Is Trigger = true" に設定すること
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GoalTrigger : MonoBehaviour
    {
        private bool _reached;

        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_reached) return;
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

            _reached = true;
            StageManager.Instance?.NotifyGoalReached();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _reached ? Color.green : Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 1f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.1f, "GOAL");
        }
#endif
    }
}
