using System.Linq;
using Character;
using Commons;
using Skill.DataStore;
using Skill.SkillData;
using UnityEngine;

namespace Skill.Component
{
    public class SkillActivator : MonoBehaviour
    {
        [Header("Fire rate")]
        private int _prefab;
        [Range(0.0f, 10.0f)]
        public float invokeTime = 1f;

        public float skillSize = 5f;

        private bool[] _activeSkills;

        public GameObject firePoint;

        //How far you can point raycast for projectiles
        public float maxLifeTime = 3f;
        public GameObject[] prefabs;

        private CharacterControl _ctl;
        private AttackSkillDataStore asds;
        [SerializeField] private AttackSkillData defaultSkillData;

        void Start()
        {
            _ctl = gameObject.GetComponent<CharacterControl>();
            asds = FindFirstObjectByType<AttackSkillDataStore>();
            defaultSkillData = asds.FindWithID(0);
        }

        void Update()
        {
            bool isAttack = _ctl.GetAttack();
            bool isOnUI = _ctl.GetIsOnUI();

            // 入力処理はUpdate()で行い、フレームレートに依存しない応答性を確保
            if (!isOnUI && isAttack && _ctl.GetStamina() > 0)
            {
                bool hasMotion = false;
                _activeSkills = _ctl.GetActiveAttackSkills();
                if (_activeSkills.Select(x => x ? 1 : 0).Sum() > 0)
                {
                    for (int i = 0; i < _activeSkills.Length; ++i)
                    {
                        if (!_activeSkills[i]) continue;
                        if (!_ctl.IncreaseFireCooldown(i, _ctl.GetCacheSkill(i).cooldownTime)) continue;
                        if (!hasMotion)
                        {
                            EnableMotion();
                            hasMotion = true;
                        }

                        var num = i;
                        float invokeSpeed = GameBalance.INVOKE_BASE_SPEED + GameBalance.INVOKE_CONTROL_MULTIPLIER * _ctl.GetControl();
                        float delay = invokeTime / invokeSpeed + i * GameBalance.INVOKE_SKILL_DELAY_INTERVAL;
                        // 憂鬱状態の場合、スキル発動遅延を増加
                        delay *= _ctl.StatusEffectManager?.GetSkillDelayMultiplier() ?? 1.0f;
                        Tools.Invoke(this, () => Fire(num), delay);
                    }
                }
                else
                {
                    bool canIncreaseCooldown = true;
                    for (int i = 0; i < _activeSkills.Length; ++i) canIncreaseCooldown &= _ctl.GetFireCooldown(i) <= 0;
                    if (!canIncreaseCooldown) return;
                    
                    for (int i = 0; i < _activeSkills.Length; ++i)
                    {
                        if (!_ctl.isAttackItems[i] && !_ctl.isNullItems[i]) continue;
                        _ctl.IncreaseFireCooldown(i, defaultSkillData.cooldownTime);
                        if (hasMotion) continue;
                        
                        EnableMotion();
                        hasMotion = true;
                    }

                    float invokeSpeed = GameBalance.INVOKE_BASE_SPEED + GameBalance.INVOKE_CONTROL_MULTIPLIER * _ctl.GetControl();
                    float defaultDelay = invokeTime / invokeSpeed;
                    // 憂鬱状態の場合、スキル発動遅延を増加
                    defaultDelay *= _ctl.StatusEffectManager?.GetSkillDelayMultiplier() ?? 1.0f;
                    Tools.Invoke(this, () => ActivateSkill(defaultSkillData), defaultDelay);
                }
            }
            else
            {
                _ctl.PrevRotate = float.NaN;
            }
        }

        private void EnableMotion()
        {
            _ctl.PrevRotate = float.IsNaN(_ctl.PrevRotate) ? _ctl.RotateY : _ctl.PrevRotate;
            _ctl.AttackMotion();
        }
        
        private void Fire(int idx)
        {
            // todo ランダムにスキルの向きを変える
            // float diffusionRandom = Random.Range(-1f, 1f);
            // Vector3 fpRotation = firePoint.transform.rotation.eulerAngles;
            // Quaternion rotation = Quaternion.Euler(fpRotation.x, fpRotation.y, fpRotation.z + diffusionRandom);
            AttackSkillData skillData = (AttackSkillData)_ctl.GetCacheSkill(idx);
            ActivateSkill(skillData);
        }
        
        private void ActivateSkill(AttackSkillData skill)
        {
            if (!_ctl.UseSkill(skill)) return;
            SkillComponent skillComponent = skill.prefab;
            for (int i = 0; i < skill.popNum; i++)
            {
                var ef = Instantiate(skillComponent, firePoint.transform.position, firePoint.transform.rotation, transform);
                float size = skill.size * skillSize;
                ef.transform.localScale = new Vector3(size, size, size);
                ef.Initialize(_ctl, skill);
            }
        }
    }
}