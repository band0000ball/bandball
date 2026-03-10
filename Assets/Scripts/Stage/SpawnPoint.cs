using UnityEngine;

namespace Stage
{
    /// <summary>スポーンの発動タイミング</summary>
    public enum SpawnType
    {
        /// <summary>シーン開始時に即スポーン</summary>
        Fixed,
        /// <summary>プレイヤーがコライダーに入ったときスポーン</summary>
        Trigger,
        /// <summary>StageManager が任意タイミングで指示するスポーン（将来拡張用）</summary>
        Manual,
    }

    /// <summary>
    /// シーンに配置するスポーン地点コンポーネント。
    ///
    /// SpawnType.Fixed  : Start() 時に即スポーン。
    /// SpawnType.Trigger: プレイヤーがコライダー（Is Trigger = true）に入ったときスポーン。
    /// SpawnType.Manual : StageManager または外部から Spawn() を直接呼び出す。
    ///
    /// スポーン後、敵は StageManager に登録されクリア条件判定に参加する。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SpawnPoint : MonoBehaviour
    {
        [Header("スポーン設定")]
        [SerializeField] private SpawnType _spawnType = SpawnType.Fixed;
        [SerializeField] private EncounterData _encounter;

        [Tooltip("true の場合、この SpawnPoint が DefeatBoss 条件のボスをスポーンするとして扱われる")]
        [SerializeField] private bool _isBoss;

        private bool _spawned;

        // Collider の参照（Trigger 判定用。Fixed/Manual 時は Collider を無効化しても動作する）
        private Collider _col;

        private void Awake()
        {
            _col = GetComponent<Collider>();
        }

        private void Start()
        {
            if (_spawnType == SpawnType.Fixed)
                Spawn();
            else if (_spawnType == SpawnType.Trigger)
                _col.isTrigger = true; // Trigger 型は必ず isTrigger=true を保証
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_spawnType != SpawnType.Trigger) return;
            if (_spawned) return;
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

            Spawn();
        }

        /// <summary>エンカウンターデータに基づいて敵をスポーンする。</summary>
        public void Spawn()
        {
            if (_spawned || _encounter == null) return;
            _spawned = true;

            for (int i = 0; i < _encounter.enemyPrefabs.Length; i++)
            {
                if (_encounter.enemyPrefabs[i] == null) continue;

                Vector3 offset = (i < _encounter.spawnOffsets.Length)
                    ? _encounter.spawnOffsets[i]
                    : Vector3.zero;

                var go = Instantiate(_encounter.enemyPrefabs[i],
                                     transform.position + offset,
                                     Quaternion.identity);

                StageManager.Instance?.RegisterEnemy(go, _isBoss);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _isBoss ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f,
                $"[{_spawnType}]{(_isBoss ? " BOSS" : "")}");
        }
#endif
    }
}
