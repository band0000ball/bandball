using System.Collections.Generic;
using System.Linq;
using Character.Interfaces;
using UnityEngine;

namespace StatusEffect
{
    /// <summary>
    /// キャラクターの状態異常を管理するクラス
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        #region Private Fields

        private readonly List<IStatusEffect> _activeEffects = new();
        private ICharacterStats _owner;

        #endregion

        #region Public Properties

        /// <summary>現在アクティブな状態異常のリスト（読み取り専用）</summary>
        public IReadOnlyList<IStatusEffect> ActiveEffects => _activeEffects;

        /// <summary>状態異常を持っているか</summary>
        public bool HasAnyEffect => _activeEffects.Count > 0;

        #endregion

        #region Initialization

        /// <summary>
        /// マネージャーを初期化する
        /// </summary>
        /// <param name="owner">所有者のステータス</param>
        public void Initialize(ICharacterStats owner)
        {
            _owner = owner;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (_owner == null) return;

            UpdateEffects(Time.deltaTime);
        }

        #endregion

        #region Effect Management

        /// <summary>
        /// 状態異常を追加する
        /// </summary>
        /// <param name="effect">追加する状態異常</param>
        public void AddEffect(IStatusEffect effect)
        {
            if (_owner == null)
            {
                Debug.LogWarning("StatusEffectManager: Owner not initialized");
                return;
            }

            // 同じ種類の効果が既に存在する場合の処理
            var existingEffect = _activeEffects.Find(e => e.Name == effect.Name);

            if (existingEffect != null)
            {
                if (effect.IsStackable)
                {
                    // スタック可能な場合はスタックを追加
                    existingEffect.AddStack();
                }
                else
                {
                    // スタック不可の場合は時間を延長
                    existingEffect.ExtendDuration(effect.RemainingTime);
                }
                return;
            }

            // 新規追加
            effect.Apply(_owner);
            _activeEffects.Add(effect);

            OnEffectAdded(effect);
        }

        /// <summary>
        /// 指定した名前の状態異常を削除する
        /// </summary>
        /// <param name="effectName">削除する状態異常の名前</param>
        public void RemoveEffect(string effectName)
        {
            var effect = _activeEffects.Find(e => e.Name == effectName);
            if (effect != null)
            {
                RemoveEffectInternal(effect);
            }
        }

        /// <summary>
        /// 指定した種類の状態異常を全て削除する
        /// </summary>
        /// <param name="type">削除する状態異常の種類</param>
        public void RemoveEffectsByType(StatusEffectType type)
        {
            var effectsToRemove = _activeEffects.Where(e => e.Type == type).ToList();
            foreach (var effect in effectsToRemove)
            {
                RemoveEffectInternal(effect);
            }
        }

        /// <summary>
        /// 全ての状態異常を削除する
        /// </summary>
        public void ClearAllEffects()
        {
            foreach (var effect in _activeEffects.ToList())
            {
                RemoveEffectInternal(effect);
            }
        }

        /// <summary>
        /// 指定した名前の状態異常を持っているか
        /// </summary>
        public bool HasEffect(string effectName)
        {
            return _activeEffects.Any(e => e.Name == effectName);
        }

        /// <summary>
        /// 指定した種類の状態異常を持っているか
        /// </summary>
        public bool HasEffectOfType(StatusEffectType type)
        {
            return _activeEffects.Any(e => e.Type == type);
        }

        /// <summary>
        /// 指定した名前の状態異常を取得する
        /// </summary>
        public IStatusEffect GetEffect(string effectName)
        {
            return _activeEffects.Find(e => e.Name == effectName);
        }

        /// <summary>
        /// 指定した型の状態異常を取得する
        /// </summary>
        public T GetEffect<T>() where T : class, IStatusEffect
        {
            return _activeEffects.Find(e => e is T) as T;
        }

        /// <summary>
        /// 不安状態によりスタミナ回復がブロックされているか
        /// </summary>
        public bool IsStaminaRegenBlocked
        {
            get
            {
                var anxiety = GetEffect("Anxiety");
                return anxiety != null && !anxiety.IsExpired;
            }
        }

        /// <summary>
        /// 憂鬱状態によるスキル遅延倍率を取得する（憂鬱でない場合は1.0）
        /// </summary>
        public float GetSkillDelayMultiplier()
        {
            var depression = GetEffect("Depression") as Effects.DepressionEffect;
            return depression != null && !depression.IsExpired
                ? depression.SkillDelayMultiplier
                : 1.0f;
        }

        #endregion

        #region Private Methods

        private void UpdateEffects(float deltaTime)
        {
            // 逆順でループして削除に対応
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.Update(deltaTime);

                if (effect.IsExpired)
                {
                    RemoveEffectInternal(effect);
                }
            }
        }

        private void RemoveEffectInternal(IStatusEffect effect)
        {
            effect.Remove(_owner);
            _activeEffects.Remove(effect);

            OnEffectRemoved(effect);
        }

        #endregion

        #region Events (for UI/Audio feedback)

        /// <summary>状態異常が追加された時のコールバック</summary>
        protected virtual void OnEffectAdded(IStatusEffect effect)
        {
            // 派生クラスでUI更新やエフェクト再生を実装可能
        }

        /// <summary>状態異常が削除された時のコールバック</summary>
        protected virtual void OnEffectRemoved(IStatusEffect effect)
        {
            // 派生クラスでUI更新やエフェクト終了を実装可能
        }

        #endregion
    }
}
