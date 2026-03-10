using UnityEngine;

namespace Manager
{
    /// <summary>
    /// プレイヤーの所持通貨（ゴールド）を管理するシングルトン。
    /// DontDestroyOnLoad でシーン間を跨いで維持される。
    ///
    /// UI やステージ報酬処理から Add() を呼び、
    /// レベルアップ処理から TrySpend() を呼ぶ。
    /// </summary>
    public class PlayerWallet : MonoBehaviour
    {
        public static PlayerWallet Instance { get; private set; }

        private int _gold;

        /// <summary>現在の所持ゴールド</summary>
        public int Gold => _gold;

        /// <summary>ゴールドが変化したときに発火。引数は変化後の残高。</summary>
        public event System.Action<int> OnGoldChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 指定額を消費する。残高が不足する場合は false を返し消費しない。
        /// </summary>
        public bool TrySpend(int amount)
        {
            if (amount <= 0 || _gold < amount) return false;
            _gold -= amount;
            OnGoldChanged?.Invoke(_gold);
            return true;
        }

        /// <summary>
        /// 指定額を加算する。
        /// </summary>
        public void Add(int amount)
        {
            if (amount <= 0) return;
            _gold += amount;
            OnGoldChanged?.Invoke(_gold);
        }

#if UNITY_EDITOR
        /// <summary>エディタデバッグ用: 残高を直接設定する。</summary>
        public void DebugSetGold(int amount) => _gold = Mathf.Max(0, amount);
#endif
    }
}
