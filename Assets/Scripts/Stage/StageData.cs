using UnityEngine;

namespace Stage
{
    /// <summary>
    /// ステージ1つ分の定義データ。ScriptableObjectとして各ステージごとに作成する。
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "PictureStory/Stage/StageData")]
    public class StageData : ScriptableObject
    {
        [Header("基本情報")]
        public int stageId;
        public string stageName;

        [Tooltip("このステージを構成するシーン名のリスト（複数シーンに跨がる場合）")]
        public string[] sceneNames;

        [Header("BGM")]
        public AudioClip bgmClip;

        [Header("クリア条件")]
        public ClearCondition clearCondition;

        [Tooltip("Survival の場合の生存目標時間（秒）")]
        public float survivalTime;

        [Header("報酬")]
        public StageReward[] rewards;
    }

    /// <summary>ステージクリア条件の種別</summary>
    public enum ClearCondition
    {
        /// <summary>シーン内の登録済み敵を全滅させる</summary>
        KillAllEnemies,
        /// <summary>GoalTrigger に到達する</summary>
        ReachGoal,
        /// <summary>ボス（SpawnPoint.isBoss=true）を倒す</summary>
        DefeatBoss,
        /// <summary>指定時間を生き残る</summary>
        Survival,
    }

    /// <summary>ステージクリア報酬</summary>
    [System.Serializable]
    public class StageReward
    {
        public RewardType rewardType;
        public int amount;

        [Tooltip("アイテム報酬の場合のアイテムID。RewardType.Item のみ有効。")]
        public int itemId;
    }

    public enum RewardType
    {
        Experience,
        Gold,
        Item,
        AbilityPoint,
    }
}
