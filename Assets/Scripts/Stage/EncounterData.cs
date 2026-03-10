using UnityEngine;

namespace Stage
{
    /// <summary>
    /// SpawnPoint 1か所分のエンカウンター定義。
    /// スポーンする敵Prefabと各敵のスポーン位置オフセットを指定する。
    /// </summary>
    [System.Serializable]
    public class EncounterData
    {
        [Tooltip("スポーンする敵PrefabのリスT")]
        public GameObject[] enemyPrefabs;

        [Tooltip("SpawnPoint中心からの各敵のスポーンオフセット（enemyPrefabs と同じ順序）")]
        public Vector3[] spawnOffsets;
    }
}
