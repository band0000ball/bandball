using System;
using System.Collections.Generic;
using System.Linq;

namespace Buff
{
    /// <summary>
    /// キャラクターのバフ/デバフを管理するクラス
    /// </summary>
    public class BuffManager : IBuffManager
    {
        private readonly List<Buff> _buffs = new();
        private readonly Dictionary<BuffType, float> _buffValues = new();

        public BuffManager()
        {
            Init();
        }

        /// <summary>
        /// バフ値のディクショナリを初期化する
        /// </summary>
        private void Init()
        {
            foreach (BuffType buff in Enum.GetValues(typeof(BuffType)))
            {
                _buffValues.Add(buff, 0f);
            }
        }

        /// <inheritdoc/>
        public float GetBuffValue(BuffType buffType)
        {
            return _buffValues.TryGetValue(buffType, out float value) ? value : 0f;
        }

        /// <inheritdoc/>
        public void Add(BuffType buffType, float buffValue, float buffTime)
        {
            _buffs.Add(new Buff(buffType, buffValue, buffTime));
            _buffValues[buffType] = _buffs.Where(x => x.BuffType == buffType).Select(x => x.BuffValue).Sum();
        }

        /// <inheritdoc/>
        public void RemoveBuffByType(BuffType buffType)
        {
            _buffs.RemoveAll(x => x.BuffType == buffType && x.BuffValue > 0);
            _buffValues[buffType] = _buffs.Where(x => x.BuffType == buffType).Select(x => x.BuffValue).Sum();
        }

        /// <inheritdoc/>
        public void RemoveDeBuffByType(BuffType buffType)
        {
            _buffs.RemoveAll(x => x.BuffType == buffType && x.BuffValue < 0);
            _buffValues[buffType] = _buffs.Where(x => x.BuffType == buffType).Select(x => x.BuffValue).Sum();
        }

        /// <inheritdoc/>
        public void RemoveByType(BuffType buffType)
        {
            _buffs.RemoveAll(b => b.BuffType == buffType);
            _buffValues[buffType] = 0f;
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
            _buffs.Clear();
            foreach (BuffType buff in Enum.GetValues(typeof(BuffType)))
            {
                _buffValues[buff] = 0f;
            }
        }

        /// <inheritdoc/>
        public void DecTimeBuffByType(BuffType buffType, float time)
        {
            foreach (var buff in _buffs)
            {
                if (buff.BuffType != buffType) continue;
                buff.DecreaseBuffTime(time);
            }
        }

        /// <inheritdoc/>
        public void IncTimeBuffByType(BuffType buffType, float time)
        {
            foreach (var buff in _buffs)
            {
                if (buff.BuffType != buffType) continue;
                buff.IncreaseBuffTime(time);
            }
        }

        /// <inheritdoc/>
        public void DecValueBuffByType(BuffType buffType, float value)
        {
            foreach (Buff buff in _buffs)
            {
                if (buff.BuffType != buffType) continue;
                if (buff.BuffValue >= value)
                {
                    buff.BuffValue -= value;
                    value = 0;
                }
                else
                {
                    value -= buff.BuffValue;
                    buff.BuffValue = 0;
                }
                if (value <= 0) break;
            }
            _buffs.RemoveAll(x => x.BuffType == buffType && x.BuffValue <= 0);
            _buffValues[buffType] = _buffs.Where(x => x.BuffType == buffType).Select(x => x.BuffValue).Sum();
        }

        /// <inheritdoc/>
        public void Update(float deltaTime)
        {
            if (_buffs.Count <= 0) return;

            foreach (var buff in _buffs.ToList())
            {
                float remainingTime = buff.Update(deltaTime);
                if (remainingTime <= 0f)
                {
                    _buffs.Remove(buff);
                }
            }

            foreach (var buffGroup in _buffs.GroupBy(x => x.BuffType))
            {
                _buffValues[buffGroup.Key] = buffGroup.Select(x => x.BuffValue).Sum();
            }

            // 削除されたバフタイプの値をリセット
            var activeTypes = _buffs.Select(b => b.BuffType).Distinct().ToHashSet();
            foreach (BuffType buffType in Enum.GetValues(typeof(BuffType)))
            {
                if (!activeTypes.Contains(buffType))
                {
                    _buffValues[buffType] = 0f;
                }
            }
        }
    }
}
