# コード設計レビュー＆改善ガイド

## プロジェクト: PictureStory (HD-2D 横スクロールアクションRPG)

**作成日**: 2026-01-19
**対象**: C#初学者・ゲーム開発未経験者向け

---

## 目次

1. [総合評価](#1-総合評価)
2. [最重要課題: CharacterControlの肥大化](#2-最重要課題-charactercontrolの肥大化)
3. [問題: マジックナンバーの多用](#3-問題-マジックナンバーの多用)
4. [問題: publicフィールドの多用](#4-問題-publicフィールドの多用カプセル化不足)
5. [問題: BuffType enumの肥大化](#5-問題-bufftype-enumの肥大化)
6. [問題: 属性相性表のハードコード](#6-問題-属性相性表のハードコード)
7. [問題: Update/FixedUpdateの混在](#7-問題-updatefixedupdateの混在)
8. [クラス別評価](#8-クラス別評価)
9. [改善優先度ロードマップ](#9-改善優先度ロードマップ)
10. [学習リソース](#10-学習リソース)

---

## 1. 総合評価

**現在のコード品質: 2.8 / 5.0**

| 評価項目 | スコア | コメント |
|---------|--------|---------|
| 機能実装 | 4/5 | 基本機能は動作している |
| アーキテクチャ | 2.5/5 | 責務分離が不十分 |
| 可読性 | 2.5/5 | 命名とコメント不足 |
| テスト性 | 2/5 | 強い結合でテスト困難 |
| 拡張性 | 2/5 | ハードコード多数 |
| 保守性 | 2/5 | クラスサイズ過大 |

### 良い点

- フォルダ構造が機能別に整理されている
- 基本的なゲームシステムが動作している
- Databrain/ScriptableObjectの活用を試みている

### 改善が必要な点

- CharacterControl.cs が1,325行と肥大化
- マジックナンバーが多数存在
- publicフィールドが多く、カプセル化が弱い
- 新機能追加時の影響範囲が広い

---

## 2. 最重要課題: CharacterControlの肥大化

### 現状

**ファイル**: `Scripts/Character/CharacterControl.cs`
**行数**: 1,325行（推奨: 400〜500行）
**メソッド数**: 約80個（推奨: 20〜30個）
**publicフィールド**: 22個

### 担当している責務（多すぎる）

1. 移動・ジャンプの物理演算
2. アニメーション管理
3. ダメージ計算と受け取り
4. バフ/デバフシステム
5. UI更新（HPバー、属性表示）
6. アイテム/スキル管理
7. ガード処理
8. イベント発火

### 問題点

```
今後追加したい機能:
├─ 状態異常システム → CharacterControlに追加? → さらに肥大化
├─ 回避システム → CharacterControlに追加? → さらに肥大化
├─ パリィシステム → CharacterControlに追加? → さらに肥大化
└─ コンボシステム → CharacterControlに追加? → さらに肥大化

結果: 2,000行超の「神クラス」が誕生 → バグの温床
```

### 改善案: 責務ごとにクラスを分割

```
現在                          改善後
CharacterControl (1,325行)    CharacterController (300行) ← 統括のみ
                              ├─ CharacterMovement (200行)  ← 移動・ジャンプ
                              ├─ CharacterStats (150行)     ← ステータス管理
                              ├─ CharacterBuffManager (150行) ← バフ/デバフ
                              ├─ CharacterCombat (200行)    ← ダメージ・ガード
                              ├─ CharacterAnimator (100行)  ← アニメーション
                              └─ CharacterUIController (100行) ← UI更新
```

### 分割後のコード例

```csharp
// CharacterController.cs - 統括クラス（300行以内）
public class CharacterController : MonoBehaviour
{
    // 各コンポーネントへの参照
    private ICharacterMovement _movement;
    private ICharacterStats _stats;
    private ICharacterAnimator _animator;
    private ICharacterCombat _combat;
    private ICharacterBuffManager _buffManager;

    private void Awake()
    {
        _movement = GetComponent<CharacterMovement>();
        _stats = GetComponent<CharacterStats>();
        _animator = GetComponent<CharacterAnimator>();
        _combat = GetComponent<CharacterCombat>();
        _buffManager = GetComponent<CharacterBuffManager>();
    }

    private void Update()
    {
        _animator.UpdateAnimation();
    }

    private void FixedUpdate()
    {
        _movement.UpdatePhysics(Time.fixedDeltaTime);
        _combat.UpdateCombat(Time.fixedDeltaTime);
        _buffManager.UpdateBuffs(Time.fixedDeltaTime);
    }
}

// CharacterMovement.cs - 移動担当（200行以内）
public interface ICharacterMovement
{
    void UpdatePhysics(float deltaTime);
    void Move(Vector2 direction);
    void Jump();
}

public class CharacterMovement : MonoBehaviour, ICharacterMovement
{
    [SerializeField] private float _moveSpeed = 13f;
    [SerializeField] private float _jumpForce = 2.4f;

    private Rigidbody _rigidbody;
    private bool _isGrounded;

    public void UpdatePhysics(float deltaTime)
    {
        CheckGrounded();
        ApplyGravity();
    }

    public void Move(Vector2 direction)
    {
        // 移動処理
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    private void CheckGrounded() { /* ... */ }
    private void ApplyGravity() { /* ... */ }
}

// CharacterStats.cs - ステータス管理（150行以内）
public interface ICharacterStats
{
    float Health { get; }
    float MaxHealth { get; }
    float Stamina { get; }
    float AttackPower { get; }
    float DefensePower { get; }

    void TakeDamage(float damage);
    void Heal(float amount);
    void ConsumeStamina(float amount);
}

public class CharacterStats : MonoBehaviour, ICharacterStats
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _maxStamina = 50f;

    private float _currentHealth;
    private float _currentStamina;

    public float Health => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float Stamina => _currentStamina;
    public float AttackPower { get; private set; }
    public float DefensePower { get; private set; }

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _currentStamina = _maxStamina;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - damage);
    }

    public void Heal(float amount)
    {
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
    }

    public void ConsumeStamina(float amount)
    {
        _currentStamina = Mathf.Max(0, _currentStamina - amount);
    }
}
```

### メリット

- 新機能追加時に影響範囲が限定される
- バグ発生時に原因特定が容易
- チーム開発時にコンフリクトが減少
- 単体テストが書きやすくなる

---

## 3. 問題: マジックナンバーの多用

### 問題のあるコード例

```csharp
// DamageManager.cs
damage = skillPower * 0.5f;              // なぜ0.5?
baseDamage -= defender.GetDefencePower() * 0.25f;  // なぜ0.25?

// CharacterControl.cs
public float hitStopTime = 0.23f;        // なぜ0.23?
public float fallMultiplier = 1.7f;      // 重力の何倍?

// SkillComponent.cs
durationDamage = character.GetAttackPower() * 0.02f + ...  // なぜ0.02?
```

### 問題点

- 数値の意味が分からない
- バランス調整時に全ファイルを検索する必要がある
- 同じ数値を複数箇所で使っていると不整合が起きる

### 改善案: 定数クラスに集約

```csharp
// Scripts/Common/GameBalance.cs（新規作成）
public static class GameBalance
{
    #region ダメージ計算係数

    /// <summary>基本ダメージは攻撃力の50%</summary>
    public const float DAMAGE_BASE_MULTIPLIER = 0.5f;

    /// <summary>防御力による減算は25%</summary>
    public const float DEFENSE_REDUCTION = 0.25f;

    /// <summary>シールドダメージは25%</summary>
    public const float SHIELD_DAMAGE_REDUCTION = 0.25f;

    #endregion

    #region 継続ダメージ

    /// <summary>継続ダメージは攻撃力の2%/秒</summary>
    public const float DOT_BASE_MULTIPLIER = 0.02f;

    #endregion

    #region 移動・物理

    /// <summary>落下時の重力倍率</summary>
    public const float FALL_GRAVITY_MULTIPLIER = 1.7f;

    /// <summary>ヒットストップ時間（秒）</summary>
    public const float HIT_STOP_DURATION = 0.23f;

    /// <summary>基本ジャンプ力</summary>
    public const float JUMP_VELOCITY_BASE = 2.4f;

    /// <summary>移動加速度</summary>
    public const float MOVEMENT_ACCELERATION = 0.2f;

    /// <summary>移動減速度</summary>
    public const float MOVEMENT_DECELERATION = 0.1f;

    #endregion

    #region クリティカル

    /// <summary>クリティカルダメージ倍率係数</summary>
    public const float CRITICAL_DAMAGE_MULTIPLIER = 0.1f;

    #endregion

    #region 状態異常

    /// <summary>昏睡の基本持続時間（秒）</summary>
    public const float STUN_BASE_DURATION = 2.0f;

    /// <summary>出血の基本ダメージ/秒</summary>
    public const float BLEEDING_DAMAGE_PER_SECOND = 5.0f;

    #endregion
}

// 使用例
damage = skillPower * GameBalance.DAMAGE_BASE_MULTIPLIER;
baseDamage -= defender.GetDefencePower() * GameBalance.DEFENSE_REDUCTION;
```

### メリット

- バランス調整が1ファイルで完結
- 数値の意味がコメントで明確
- 変更の影響範囲が把握しやすい

---

## 4. 問題: publicフィールドの多用（カプセル化不足）

### 問題のあるコード例

```csharp
// CharacterControl.cs
public float movementSpeed = 13f;              // 外部から自由に変更可能
public bool isFacingRight;                     // 外部から自由に変更可能
[SerializeField] public int characterId;       // SerializeFieldなのにpublic（矛盾）
public CharacterSlotItemData characterItems;   // 重要データが丸見え
```

### publicフィールド統計

| クラス | publicフィールド数 | 問題度 |
|-------|-----------------|------|
| CharacterControl | 22 | 危険 |
| StatusData | 21 (3クラス) | 高 |
| SkillComponent | 11 | 高 |
| DamageManager | 0 | 良好 |

### 問題点

- どこからでも値を変更できてしまう
- 「誰がいつ変更したか」が追跡困難
- 意図しない値の設定でバグが発生

### 改善案: プロパティでアクセス制御

```csharp
// 改善前
public float movementSpeed = 13f;
public bool isFacingRight;
[SerializeField] public int characterId;

// 改善後
[SerializeField] private float _movementSpeed = 13f;
public float MovementSpeed => _movementSpeed;  // 読み取り専用

[SerializeField] private bool _isFacingRight;
public bool IsFacingRight => _isFacingRight;

[SerializeField] private int _characterId;
public int CharacterId => _characterId;

// 変更が必要な場合はメソッド経由（バリデーション可能）
public void SetMovementSpeed(float speed)
{
    if (speed < 0)
    {
        Debug.LogWarning("Speed must be positive");
        return;
    }
    _movementSpeed = speed;
}

public void SetFacingDirection(bool facingRight)
{
    _isFacingRight = facingRight;
    // 必要に応じてスプライト反転などの処理
}
```

### 命名規則の統一

```csharp
// 推奨命名規則
private int _privateField;           // プライベートフィールド: _camelCase
public int PublicProperty { get; }   // パブリックプロパティ: PascalCase
private const int CONSTANT_VALUE;    // 定数: UPPER_SNAKE_CASE
```

---

## 5. 問題: BuffType enumの肥大化

### 現状（62個の値）

```csharp
public enum BuffType
{
    // Behaviour (10個)
    JumpTime, MoveSpeed, JumpForce, JumpSpeed, CorePower,
    Strength, Control, Thickness, Endurance, AdditionalGravity,

    // Attribute (10個)
    Frame, Aqua, Plant, Electric, Ground, Ice, Oil, Toxin, Wind, Spirit,

    // Battle (16個)
    Shield, Health, Stamina, GuardHealth, GuardNum, Rate, ...

    // Other (4個以上)
    AutoGuardNum, CooldownAccelerate, HealHitPoint, HealStamina, HealGuardHealth,
}
```

### 問題点

- 新しいバフを追加するたびにenumを変更
- 関連するswitch文も全て修正が必要
- カテゴリー管理が曖昧

### 仕様書の状態異常を追加する場合（現在の方法）

```csharp
// 毎回enumに追加が必要
public enum BuffType
{
    // ... 既存62個 ...
    Stun,        // 昏睡（新規）
    Depression,  // 憂鬱（新規）
    Bleeding,    // 出血（新規）
    Anxiety,     // 不安（新規）
}

// さらに処理を追加（巨大なswitch文）
switch (buffType)
{
    case BuffType.Stun:
        // 行動不能処理
        break;
    case BuffType.Depression:
        // スキル遅延処理
        break;
    // ... 全部書く必要がある
}
```

### 改善案: インターフェースで拡張可能に

```csharp
// Scripts/StatusEffect/IStatusEffect.cs
/// <summary>
/// 状態異常の基本インターフェース
/// 新しい状態異常を追加する場合はこのインターフェースを実装する
/// </summary>
public interface IStatusEffect
{
    /// <summary>状態異常の名前</summary>
    string Name { get; }

    /// <summary>残り時間</summary>
    float RemainingTime { get; }

    /// <summary>効果が終了したか</summary>
    bool IsExpired { get; }

    /// <summary>効果を適用</summary>
    void Apply(ICharacterStats character);

    /// <summary>毎フレーム更新</summary>
    void Update(float deltaTime);

    /// <summary>効果を解除</summary>
    void Remove(ICharacterStats character);
}

// Scripts/StatusEffect/StunEffect.cs - 昏睡
public class StunEffect : IStatusEffect
{
    public string Name => "Stun";
    public float RemainingTime { get; private set; }
    public bool IsExpired => RemainingTime <= 0;

    private ICharacterStats _target;

    public StunEffect(float duration)
    {
        RemainingTime = duration;
    }

    public void Apply(ICharacterStats character)
    {
        _target = character;
        character.CanMove = false;
        character.CanGuard = false;
        character.CanAttack = false;
    }

    public void Update(float deltaTime)
    {
        RemainingTime -= deltaTime;
    }

    public void Remove(ICharacterStats character)
    {
        character.CanMove = true;
        character.CanGuard = true;
        character.CanAttack = true;
    }
}

// Scripts/StatusEffect/BleedingEffect.cs - 出血
public class BleedingEffect : IStatusEffect
{
    public string Name => "Bleeding";
    public float RemainingTime { get; private set; }
    public bool IsExpired => RemainingTime <= 0;

    private readonly float _damagePerSecond;
    private ICharacterStats _target;

    public BleedingEffect(float duration, float damagePerSecond)
    {
        RemainingTime = duration;
        _damagePerSecond = damagePerSecond;
    }

    public void Apply(ICharacterStats character)
    {
        _target = character;
    }

    public void Update(float deltaTime)
    {
        RemainingTime -= deltaTime;
        _target?.TakeDamage(_damagePerSecond * deltaTime);
    }

    public void Remove(ICharacterStats character)
    {
        // 出血解除時の処理（必要に応じて）
    }
}

// Scripts/StatusEffect/StatusEffectManager.cs - 状態異常管理
public class StatusEffectManager
{
    private List<IStatusEffect> _activeEffects = new();
    private ICharacterStats _owner;

    public StatusEffectManager(ICharacterStats owner)
    {
        _owner = owner;
    }

    public void AddEffect(IStatusEffect effect)
    {
        effect.Apply(_owner);
        _activeEffects.Add(effect);
    }

    public void Update(float deltaTime)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            effect.Update(deltaTime);

            if (effect.IsExpired)
            {
                effect.Remove(_owner);
                _activeEffects.RemoveAt(i);
            }
        }
    }

    public bool HasEffect(string effectName)
    {
        return _activeEffects.Any(e => e.Name == effectName);
    }

    public void RemoveEffect(string effectName)
    {
        var effect = _activeEffects.Find(e => e.Name == effectName);
        if (effect != null)
        {
            effect.Remove(_owner);
            _activeEffects.Remove(effect);
        }
    }
}
```

### メリット

- 新しい状態異常を追加する場合 → 新しいクラスを作るだけ
- 既存コードの変更が不要
- 各状態異常の処理が独立していてテストしやすい

---

## 6. 問題: 属性相性表のハードコード

### 現状

```csharp
// AttributeMagnification.cs
private static readonly float[][] Magnification = {
    new[] {1.0f, 0.8f, 1.2f, 0.8f, 1.4f, 0.6f, 0.6f, 1.0f, 0.8f, 0.4f, 1.0f},
    new[] {1.2f, 1.0f, 0.6f, 0.4f, 1.0f, 1.4f, 0.0f, 0.8f, 0.8f, 0.8f, 1.0f},
    // ... 10行の配列（意味が分からない）
};
```

### 問題点

- 新属性追加時に全行を修正
- 相性値の意味が分からない
- バランス調整が困難

### 改善案: ScriptableObjectでデータ駆動設計

```csharp
// Scripts/Data/AttributeAffinityData.cs
[CreateAssetMenu(fileName = "AttributeAffinity", menuName = "Game/Attribute Affinity")]
public class AttributeAffinityData : ScriptableObject
{
    [System.Serializable]
    public class AffinityEntry
    {
        public Attribute attacker;
        public Attribute defender;
        [Range(0f, 2f)] public float multiplier = 1f;
        [TextArea] public string reason;  // 例: "火は水に弱い"
    }

    [SerializeField]
    private List<AffinityEntry> _affinities = new();

    // キャッシュ用
    private Dictionary<(Attribute, Attribute), float> _cache;

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        _cache = new Dictionary<(Attribute, Attribute), float>();
        foreach (var entry in _affinities)
        {
            _cache[(entry.attacker, entry.defender)] = entry.multiplier;
        }
    }

    public float GetMultiplier(Attribute attacker, Attribute defender)
    {
        if (_cache == null) BuildCache();

        if (_cache.TryGetValue((attacker, defender), out float multiplier))
        {
            return multiplier;
        }
        return 1f;  // デフォルト: 等倍
    }
}

// 使用例
public class DamageCalculator
{
    [SerializeField] private AttributeAffinityData _affinityData;

    public float CalculateAttributeDamage(Attribute attackAttr, Attribute defenseAttr, float baseDamage)
    {
        float multiplier = _affinityData.GetMultiplier(attackAttr, defenseAttr);
        return baseDamage * multiplier;
    }
}
```

### メリット

- Unityエディタ上でバランス調整可能
- 相性の理由をコメントで残せる
- 新属性追加が容易
- プログラマー以外でも調整可能

---

## 7. 問題: Update/FixedUpdateの混在

### 現状

```csharp
private void FixedUpdate()
{
    // ... 物理処理 ...

    SetUI();  // ← UIはUpdate()で更新すべき
    DecreaseCountdown();  // ← 時間管理はUpdate()が適切
    _havingBuffs.Update(Time.deltaTime);  // ← Time.fixedDeltaTimeを使うべき
}
```

### 正しい使い分け

| メソッド | 用途 | 呼び出し間隔 | 使用するdeltaTime |
|---------|------|-------------|------------------|
| Update() | 入力処理、UI更新、アニメーション | 毎フレーム（可変） | Time.deltaTime |
| FixedUpdate() | 物理演算、Rigidbody操作 | 固定間隔（0.02秒） | Time.fixedDeltaTime |
| LateUpdate() | カメラ追従、他オブジェクト追従 | Update後 | Time.deltaTime |

### 改善案

```csharp
private void Update()
{
    // 入力処理
    _axisInput = input.move;
    _attack = input.attack && !_guard;

    // UI更新（毎フレーム）
    SetUI();
    UpdateAttribute();

    // クールダウン減少（Time.deltaTimeを使用）
    DecreaseCountdown();

    // バフ更新
    _havingBuffs.Update(Time.deltaTime);
}

private void FixedUpdate()
{
    if (_isDead) return;

    // 物理チェックのみ
    CheckGrounded();
    FixZAxis();

    // 物理演算のみ
    MoveWalk();
    MoveJump();
    ApplyGravity();
}

private void LateUpdate()
{
    // カメラやターゲット追従
    FocusTarget();
}
```

---

## 8. クラス別評価

| クラス | 行数 | 複雑度 | カプセル化 | テスト性 | 拡張性 | 総合評価 |
|--------|------|--------|---------|---------|--------|----------|
| CharacterControl | 1,325 | 危険 | 低 | 低 | 低 | 1/5 |
| DamageManager | 344 | 中 | 中 | 中 | 中 | 2.5/5 |
| StatusData | 140 | 中 | 中 | 高 | 低 | 2.5/5 |
| SkillComponent | 147 | 中 | 低 | 中 | 中 | 2.5/5 |
| AttributeMagnification | 68 | 低 | 高 | 高 | 低 | 3/5 |
| GuardComponent | 51 | 低 | 中 | 高 | 高 | 4/5 |

### 改善後の目標評価

| クラス | 現在 | 目標 | 改善内容 |
|--------|------|------|---------|
| CharacterControl | 1/5 | 4/5 | 6クラスに分割 |
| DamageManager | 2.5/5 | 4/5 | Singletonパターン適用 |
| StatusData | 2.5/5 | 4/5 | プロパティ化 |
| SkillComponent | 2.5/5 | 4/5 | Factoryパターン適用 |

---

## 9. 改善優先度ロードマップ

### Phase 1: 今すぐ（1〜2週間）

| 優先度 | 作業 | 効果 | 難易度 |
|-------|------|------|--------|
| 最高 | マジックナンバーを定数化 | バランス調整が容易に | 低 |
| 最高 | publicフィールドをプロパティに | バグ防止 | 低 |
| 高 | Update/FixedUpdateの整理 | パフォーマンス向上 | 低 |

#### Phase 1 チェックリスト

- [ ] GameBalance.cs を作成
- [ ] DamageManager.cs のマジックナンバーを置換
- [ ] CharacterControl.cs のマジックナンバーを置換
- [ ] SkillComponent.cs のマジックナンバーを置換
- [ ] CharacterControl.cs のpublicフィールドをプロパティ化
- [ ] FixedUpdate()からUI更新を移動

### Phase 2: 短期（2〜4週間）

| 優先度 | 作業 | 効果 | 難易度 |
|-------|------|------|--------|
| 最高 | CharacterControlの分割開始 | 保守性向上 | 中 |
| 高 | IStatusEffectインターフェース導入 | 状態異常実装の準備 | 中 |
| 中 | 属性相性をScriptableObject化 | バランス調整が容易に | 中 |

#### Phase 2 チェックリスト

- [ ] ICharacterStats インターフェース作成
- [ ] CharacterStats クラス抽出
- [ ] ICharacterMovement インターフェース作成
- [ ] CharacterMovement クラス抽出
- [ ] IStatusEffect インターフェース作成
- [ ] StunEffect, BleedingEffect 実装
- [ ] AttributeAffinityData ScriptableObject作成

### Phase 3: 中期（1〜2ヶ月）

| 優先度 | 作業 | 効果 | 難易度 |
|-------|------|------|--------|
| 高 | スキルシステムのファクトリ化 | 新スキル追加が容易に | 中 |
| 中 | DamageManagerのリファクタ | テスト可能に | 中 |
| 中 | CharacterBuffManager抽出 | バフ管理の独立 | 中 |

### Phase 4: 長期（2〜3ヶ月）

| 優先度 | 作業 | 効果 | 難易度 |
|-------|------|------|--------|
| 中 | 単体テストの導入 | 品質保証 | 中 |
| 低 | DIコンテナの検討 | 依存関係管理 | 高 |
| 低 | コードドキュメント整備 | チーム開発対応 | 低 |

---

## 10. 学習リソース

### 10.1 推奨する設計パターン

#### 単一責任原則（SRP: Single Responsibility Principle）

**概要**: 1クラス1責務

```csharp
// 悪い例: 複数の責務
public class Player
{
    public void Move() { }      // 移動
    public void Attack() { }    // 攻撃
    public void UpdateUI() { }  // UI更新
    public void SaveData() { }  // セーブ
}

// 良い例: 責務を分離
public class PlayerMovement { public void Move() { } }
public class PlayerCombat { public void Attack() { } }
public class PlayerUI { public void UpdateUI() { } }
public class PlayerDataManager { public void SaveData() { } }
```

**参考リンク**:
- [SOLID原則 - Unity公式](https://unity.com/how-to/how-use-solid-principles-create-better-code)
- [単一責任の原則 - Wikipedia](https://ja.wikipedia.org/wiki/%E5%8D%98%E4%B8%80%E8%B2%AC%E4%BB%BB%E3%81%AE%E5%8E%9F%E5%89%87)

#### Strategyパターン

**概要**: アルゴリズムを切り替え可能にする

```csharp
// 状態異常システムで活用
public interface IStatusEffect
{
    void Apply(Character character);
    void Update(float deltaTime);
    void Remove(Character character);
}

public class PoisonEffect : IStatusEffect { /* 毒の実装 */ }
public class BurnEffect : IStatusEffect { /* 火傷の実装 */ }
public class FreezeEffect : IStatusEffect { /* 凍結の実装 */ }
```

**参考リンク**:
- [Strategy パターン - Refactoring Guru](https://refactoring.guru/ja/design-patterns/strategy)

#### Factoryパターン

**概要**: オブジェクト生成を専用クラスに委譲

```csharp
// スキル生成で活用
public class SkillFactory
{
    public ISkill CreateSkill(SkillType type)
    {
        return type switch
        {
            SkillType.Attack => new AttackSkill(),
            SkillType.Buff => new BuffSkill(),
            SkillType.Heal => new HealSkill(),
            _ => throw new ArgumentException($"Unknown skill type: {type}")
        };
    }
}
```

**参考リンク**:
- [Factory Method パターン - Refactoring Guru](https://refactoring.guru/ja/design-patterns/factory-method)

#### State パターン

**概要**: 状態ごとに振る舞いを変える

```csharp
// キャラクター状態管理で活用
public interface ICharacterState
{
    void Enter(CharacterController character);
    void Update(CharacterController character, float deltaTime);
    void Exit(CharacterController character);
}

public class IdleState : ICharacterState { /* 待機状態 */ }
public class WalkState : ICharacterState { /* 移動状態 */ }
public class AttackState : ICharacterState { /* 攻撃状態 */ }
public class StunnedState : ICharacterState { /* 昏睡状態 */ }
```

**参考リンク**:
- [State パターン - Refactoring Guru](https://refactoring.guru/ja/design-patterns/state)

---

### 10.2 Unity特有の知識

#### MonoBehaviourのライフサイクル

```
シーンロード
    ↓
Awake()         ← 最初に1回。他コンポーネント参照取得
    ↓
OnEnable()      ← アクティブになるたび
    ↓
Start()         ← 最初のフレーム前に1回
    ↓
┌─────────────────────────┐
│ FixedUpdate()  ← 物理更新（0.02秒間隔） │
│      ↓                   │
│ Update()       ← 毎フレーム           │
│      ↓                   │
│ LateUpdate()   ← Update後            │
└─────────────────────────┘
    ↓（繰り返し）
OnDisable()     ← 非アクティブになるたび
    ↓
OnDestroy()     ← 破棄時
```

**参考リンク**:
- [Unity - イベント関数の実行順序](https://docs.unity3d.com/ja/current/Manual/ExecutionOrder.html)

#### ScriptableObject

**概要**: アセットとしてデータを保存できるクラス

```csharp
[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public int maxHealth;
    public float moveSpeed;
    public Sprite icon;
}
```

**メリット**:
- エディタ上でデータを編集可能
- プレイモード中の変更が保存される
- プレハブ間でデータを共有できる

**参考リンク**:
- [Unity - ScriptableObject](https://docs.unity3d.com/ja/current/Manual/class-ScriptableObject.html)
- [ScriptableObject活用術 - Unity Learning](https://learn.unity.com/tutorial/introduction-to-scriptable-objects)

#### オブジェクトプーリング

**概要**: 頻繁な生成/破棄を避けて再利用する

```csharp
public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private int _poolSize = 20;

    private Queue<GameObject> _pool = new();

    private void Start()
    {
        // 事前に生成
        for (int i = 0; i < _poolSize; i++)
        {
            var bullet = Instantiate(_bulletPrefab);
            bullet.SetActive(false);
            _pool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        if (_pool.Count > 0)
        {
            var bullet = _pool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        // プールが空なら新規生成
        return Instantiate(_bulletPrefab);
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        _pool.Enqueue(bullet);
    }
}
```

**参考リンク**:
- [Unity - Object Pooling](https://docs.unity3d.com/ja/current/Manual/object-pooling.html)

---

### 10.3 C#の重要概念

#### インターフェース

**概要**: クラスが実装すべきメソッドを定義

```csharp
// 定義
public interface IDamageable
{
    void TakeDamage(float damage);
    bool IsDead { get; }
}

// 実装
public class Player : MonoBehaviour, IDamageable
{
    private float _health;

    public bool IsDead => _health <= 0;

    public void TakeDamage(float damage)
    {
        _health -= damage;
    }
}

public class Enemy : MonoBehaviour, IDamageable
{
    private float _health;

    public bool IsDead => _health <= 0;

    public void TakeDamage(float damage)
    {
        _health -= damage;
        // 敵固有の処理
        PlayHitAnimation();
    }
}

// 使用（どちらの型でも同じように扱える）
public void DealDamage(IDamageable target, float damage)
{
    target.TakeDamage(damage);
}
```

**参考リンク**:
- [Microsoft - インターフェース](https://learn.microsoft.com/ja-jp/dotnet/csharp/programming-guide/interfaces/)

#### ジェネリクス

**概要**: 型をパラメータとして受け取る

```csharp
// 汎用的なオブジェクトプール
public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> _pool = new();
    private T _prefab;

    public ObjectPool(T prefab, int initialSize)
    {
        _prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Object.Instantiate(_prefab);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (_pool.Count > 0)
        {
            var obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        return Object.Instantiate(_prefab);
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}

// 使用
var bulletPool = new ObjectPool<Bullet>(bulletPrefab, 20);
var effectPool = new ObjectPool<ParticleEffect>(effectPrefab, 10);
```

**参考リンク**:
- [Microsoft - ジェネリック](https://learn.microsoft.com/ja-jp/dotnet/csharp/programming-guide/generics/)

#### LINQ

**概要**: コレクション操作を簡潔に書ける

```csharp
using System.Linq;

// 配列から条件に合う要素を取得
var activeEnemies = enemies.Where(e => e.IsAlive);

// 最大値を持つ要素を取得
var strongestEnemy = enemies.OrderByDescending(e => e.Attack).First();

// 条件に合う要素があるか確認
bool hasLowHealthEnemy = enemies.Any(e => e.Health < 10);

// 合計値を計算
float totalDamage = attacks.Sum(a => a.Damage);
```

**注意**: LINQは便利だが、毎フレーム呼ぶとパフォーマンスに影響するため、結果をキャッシュすること

**参考リンク**:
- [Microsoft - LINQ](https://learn.microsoft.com/ja-jp/dotnet/csharp/linq/)

---

### 10.4 推奨書籍・サイト

#### 書籍

| タイトル | 対象 | 内容 |
|---------|------|------|
| Unity 2023入門 | 初心者 | Unity基礎 |
| Unityの教科書 | 初心者 | C#とUnityの基礎 |
| Game Programming Patterns | 中級者 | ゲーム設計パターン |
| Clean Code | 中級者 | コード品質向上 |

#### Webサイト

| サイト | URL | 内容 |
|-------|-----|------|
| Unity Learn | https://learn.unity.com/ | 公式チュートリアル |
| Unity Documentation | https://docs.unity3d.com/ja/ | 公式ドキュメント |
| Refactoring Guru | https://refactoring.guru/ja | 設計パターン解説 |
| Qiita (Unity) | https://qiita.com/tags/unity | 日本語記事 |
| Zenn (Unity) | https://zenn.dev/topics/unity | 日本語記事 |

#### YouTube

| チャンネル | 内容 |
|-----------|------|
| Unity Japan | 公式日本語チャンネル |
| Brackeys | Unity入門（英語） |
| Code Monkey | Unity中級（英語） |

---

## まとめ

現在のコードは**動作する**という点で合格ですが、仕様書の機能を全て実装するには**設計の改善が必要**です。

### 最優先で取り組むべき3点

1. **CharacterControlの分割** → 新機能追加のボトルネック解消
2. **マジックナンバーの定数化** → バランス調整の効率化
3. **状態異常用インターフェース導入** → 仕様書の機能実装準備

### 改善による効果

| 項目 | 現在 | 改善後 |
|------|------|--------|
| コード品質 | 2.8/5 | 4.0/5 |
| 新機能追加の工数 | 高 | 低 |
| バグ発生率 | 高 | 低 |
| バランス調整 | 困難 | 容易 |

これらを段階的に改善することで、仕様書に記載された全機能を**無理なく実装できる基盤**が整います。

---

**ドキュメント作成日**: 2026-01-19
**バージョン**: 1.0
**ステータス**: Active
