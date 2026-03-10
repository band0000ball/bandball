using UnityEngine;

namespace Monster
{
    /// <summary>AI タイプ</summary>
    public enum AIType
    {
        /// <summary>近接攻撃型</summary>
        Melee,
        /// <summary>遠隔攻撃型</summary>
        Ranged,
        /// <summary>魔法攻撃型</summary>
        Magic,
        /// <summary>防御重視型</summary>
        Defensive,
        /// <summary>召喚型</summary>
        Summoner,
        /// <summary>ボス型（フェーズ管理あり）</summary>
        Boss,
    }

    /// <summary>
    /// 敵 AI のパラメーター定義。ScriptableObject として各敵種ごとに作成する。
    ///
    /// メニュー: Assets > Create > PictureStory > Monster > EnemyAIData
    /// </summary>
    [CreateAssetMenu(menuName = "PictureStory/Monster/EnemyAIData", fileName = "EnemyAIData_New")]
    public class EnemyAIData : ScriptableObject
    {
        [Header("AI タイプ")]
        public AIType aiType = AIType.Melee;

        [Header("探知・追跡")]
        [Tooltip("この距離以内にプレイヤーが入ると Chase へ移行する")]
        public float detectionRange = 10f;

        [Tooltip("この距離以内になると Attack へ移行する")]
        public float attackRange = 2f;

        [Tooltip("この距離以上離れると Idle へ戻る")]
        public float lostRange = 15f;

        [Header("逃走・回復")]
        [Tooltip("HP 割合がこれ以下で Flee 状態へ移行する（0 = 逃走しない）")]
        [Range(0f, 1f)] public float fleeThreshold = 0.2f;

        [Tooltip("HP 割合がこれ以上で再度 Chase へ戻る")]
        [Range(0f, 1f)] public float reengageThreshold = 0.4f;

        [Header("移動")]
        public float moveSpeed = 3f;
        public float patrolRange = 5f;

        [Header("クールダウン")]
        [Tooltip("攻撃後の待機時間（秒）")]
        public float globalCooldown = 1.5f;

        [Tooltip("ダメージ後の硬直時間（秒）")]
        public float recoveryTime = 0.5f;

        [Header("攻撃パターン")]
        public AttackPattern[] attackPatterns = new AttackPattern[0];

        [Header("ボス設定")]
        public bool isBoss = false;

        [Tooltip("hpThreshold 降順で記述すること（Phase1 → Phase2 → ...）")]
        public BossPhase[] bossPhases = new BossPhase[0];

        [Tooltip("強制エンレイジまでの時間（秒）")]
        public float enrageTimer = 300f;

        [Tooltip("エンレイジ時の攻撃力倍率")]
        public float enrageAttackMultiplier = 1.5f;

        [Tooltip("エンレイジ時の速度倍率")]
        public float enrageSpeedMultiplier = 1.3f;
    }
}
