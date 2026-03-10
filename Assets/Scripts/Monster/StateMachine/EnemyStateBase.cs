using Character;
using UnityEngine;

namespace Monster.StateMachine
{
    /// <summary>
    /// 敵 AI 状態の基底クラス。
    /// 各状態クラスはこのクラスを継承して OnEnter/OnUpdate/OnFixedUpdate/OnExit をオーバーライドする。
    /// </summary>
    public abstract class EnemyStateBase : IEnemyState
    {
        // ── IEnemyState ───────────────────────────────────────────────────────

        public virtual string Name => GetType().Name.Replace("Enemy", "").Replace("State", "");

        void IEnemyState.Enter(EnemyStateMachine machine)
        {
            Machine = machine;
            OnEnter();
        }

        void IEnemyState.Update(EnemyStateMachine machine) => OnUpdate();
        void IEnemyState.FixedUpdate(EnemyStateMachine machine) => OnFixedUpdate();
        void IEnemyState.Exit(EnemyStateMachine machine) => OnExit();

        public virtual bool CanBeInterruptedBy(IEnemyState newState) => true;

        // ── Protected Properties ──────────────────────────────────────────────

        protected EnemyStateMachine Machine { get; private set; }
        protected CharacterControl Control => Machine?.Control;
        protected EnemyAIData AIData => Machine?.AIData;
        protected CharacterControl Target => Machine?.Target;

        // ── Protected Virtual Methods ─────────────────────────────────────────

        protected virtual void OnEnter() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnExit() { }

        // ── State Transition Helpers ──────────────────────────────────────────

        protected bool ChangeState<T>() where T : class, IEnemyState, new()
            => Machine?.ChangeState<T>() ?? false;

        protected void ForceChangeState<T>() where T : class, IEnemyState, new()
            => Machine?.ForceChangeState<T>();

        // ── Input Helpers ─────────────────────────────────────────────────────

        /// <summary>AI 移動入力を設定する</summary>
        protected void SetMove(float x)
            => Control.Input.move = new Vector2(x, 0f);

        protected void StopMove()
            => Control.Input.move = Vector2.zero;

        protected void SetAttack(bool v)
            => Control.Input.attack = v;

        protected void SetGuard(bool v)
            => Control.Input.guard = v;

        // ── Spatial Helpers ───────────────────────────────────────────────────

        /// <summary>ターゲットとの X 軸距離（絶対値）</summary>
        protected float DistanceToTarget()
        {
            if (Target == null) return float.MaxValue;
            return Mathf.Abs(Control.GetPosition().x - Target.GetPosition().x);
        }

        /// <summary>ターゲット方向（+1 = 右, -1 = 左）</summary>
        protected float DirectionToTarget()
        {
            if (Target == null) return 0f;
            return Mathf.Sign(Target.GetPosition().x - Control.GetPosition().x);
        }

        /// <summary>ターゲットが検知範囲内か</summary>
        protected bool IsTargetInDetectionRange()
            => Target != null && !Target.GetIsDead() && DistanceToTarget() <= AIData.detectionRange;

        /// <summary>ターゲットが攻撃範囲内か</summary>
        protected bool IsTargetInAttackRange()
            => Target != null && !Target.GetIsDead() && DistanceToTarget() <= AIData.attackRange;

        /// <summary>HP が逃走閾値以下か</summary>
        protected bool ShouldFlee()
            => AIData.fleeThreshold > 0 && Machine.HpRatio <= AIData.fleeThreshold;
    }
}
