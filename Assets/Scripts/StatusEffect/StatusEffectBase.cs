using Character.Interfaces;

namespace StatusEffect
{
    /// <summary>
    /// 状態異常の基底クラス
    /// 共通の処理を実装し、派生クラスで具体的な効果を定義する
    /// </summary>
    public abstract class StatusEffectBase : IStatusEffect
    {
        #region Properties

        public abstract string Name { get; }
        public abstract StatusEffectType Type { get; }

        public float RemainingTime { get; protected set; }
        public bool IsExpired => RemainingTime <= 0;

        public virtual bool IsStackable => false;
        public int StackCount { get; protected set; } = 1;

        protected ICharacterStats Target { get; private set; }

        #endregion

        #region Constructor

        protected StatusEffectBase(float duration)
        {
            RemainingTime = duration;
        }

        #endregion

        #region IStatusEffect Implementation

        public virtual void Apply(ICharacterStats target)
        {
            Target = target;
            OnApply();
        }

        public virtual void Update(float deltaTime)
        {
            RemainingTime -= deltaTime;
            OnUpdate(deltaTime);
        }

        public virtual void Remove(ICharacterStats target)
        {
            OnRemove();
            Target = null;
        }

        public virtual void AddStack(int count = 1)
        {
            if (IsStackable)
            {
                StackCount += count;
                OnStackAdded(count);
            }
        }

        public void ExtendDuration(float duration)
        {
            RemainingTime += duration;
        }

        #endregion

        #region Virtual Methods for Override

        /// <summary>効果適用時の処理</summary>
        protected virtual void OnApply() { }

        /// <summary>毎フレームの処理</summary>
        protected virtual void OnUpdate(float deltaTime) { }

        /// <summary>効果解除時の処理</summary>
        protected virtual void OnRemove() { }

        /// <summary>スタック追加時の処理</summary>
        protected virtual void OnStackAdded(int count) { }

        #endregion
    }
}
