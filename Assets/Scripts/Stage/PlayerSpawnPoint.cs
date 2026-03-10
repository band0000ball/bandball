using UnityEngine;

namespace Stage
{
    /// <summary>
    /// プレイヤーキャラクターのステージ開始地点マーカー。
    /// PlayerCharacterController.SetupForStage() でこの位置へ操作キャラが配置される。
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        public Vector3 Position => transform.position;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, "[Player Spawn]");
        }
#endif
    }
}
