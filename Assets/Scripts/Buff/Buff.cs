namespace Buff
{
    /// <summary>
    /// 個別のバフ/デバフを表すクラス
    /// </summary>
    public class Buff
    {
        public readonly BuffType BuffType;
        public float BuffValue;
        private float _buffTime;

        public Buff(BuffType buffType, float buffValue, float buffTime)
        {
            BuffType = buffType;
            BuffValue = buffValue;
            _buffTime = buffTime;
        }

        /// <summary>
        /// バフの残り時間を減少させる
        /// </summary>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>残り時間</returns>
        public float Update(float deltaTime)
        {
            _buffTime -= deltaTime;
            if (_buffTime <= 0f) _buffTime = 0f;
            return _buffTime;
        }

        /// <summary>
        /// バフの残り時間を増加させる
        /// </summary>
        public void IncreaseBuffTime(float deltaTime)
        {
            if (_buffTime > 0) _buffTime += deltaTime;
        }

        /// <summary>
        /// バフの残り時間を減少させる
        /// </summary>
        public void DecreaseBuffTime(float deltaTime)
        {
            if (_buffTime > 0) _buffTime -= deltaTime;
        }
    }
}