# PictureStory 状態遷移図

## 目次
1. [キャラクター状態遷移](#1-キャラクター状態遷移)
2. [戦闘状態遷移](#2-戦闘状態遷移)
3. [アイテムスキル発動フロー](#3-アイテムスキル発動フロー)
4. [ガードシステム状態](#4-ガードシステム状態)
5. [バフ/デバフ状態](#5-バフデバフ状態)
6. [ゲーム全体の状態遷移（設計案）](#6-ゲーム全体の状態遷移設計案)
7. [敵AI状態遷移](#7-敵ai状態遷移)
8. [クエスト状態遷移](#8-クエスト状態遷移)
9. [インベントリ状態遷移](#9-インベントリ状態遷移)

---

## 1. キャラクター状態遷移

### 1.1 メイン状態遷移図

```mermaid
stateDiagram-v2
    [*] --> Idle : 初期化完了

    Idle --> Walk : 移動入力あり
    Walk --> Idle : 移動入力なし

    Idle --> Jump : ジャンプ入力 & ジャンプ可能
    Walk --> Jump : ジャンプ入力 & ジャンプ可能
    Jump --> Idle : 着地 & 移動入力なし
    Jump --> Walk : 着地 & 移動入力あり

    Idle --> Attack : 攻撃入力 & UI外
    Walk --> Attack : 攻撃入力 & UI外
    Jump --> Attack : 攻撃入力 & UI外
    Attack --> Idle : 攻撃アニメーション終了

    Idle --> Guard : ガード入力
    Walk --> Guard : ガード入力
    Guard --> Idle : ガード入力解除

    Idle --> Damaged : ダメージ受ける
    Walk --> Damaged : ダメージ受ける
    Jump --> Damaged : ダメージ受ける
    Attack --> Damaged : ダメージ受ける
    Guard --> Damaged : ガード貫通ダメージ
    Damaged --> Idle : ダメージモーション終了

    Idle --> Dead : HP <= 0
    Walk --> Dead : HP <= 0
    Jump --> Dead : HP <= 0
    Attack --> Dead : HP <= 0
    Guard --> Dead : HP <= 0
    Damaged --> Dead : HP <= 0

    Dead --> [*] : Destroy(3秒後)
```

### 1.2 アニメーター状態（推定）

```mermaid
stateDiagram-v2
    [*] --> Idle

    state "Locomotion" as Loco {
        Idle --> Walk : Walk = true
        Walk --> Idle : Walk = false
    }

    Loco --> Jump : Jump trigger
    Jump --> Loco : normalizedTime >= 1.0

    Loco --> Attack1 : Attack1 trigger
    Attack1 --> Loco : normalizedTime >= 1.0

    Loco --> Guard : Guard trigger & 入力継続
    Guard --> Loco : Speed = 1.0 (入力解除)

    state Guard {
        [*] --> GuardStart
        GuardStart --> GuardHold : normalizedTime > 0.5
        GuardHold --> GuardEnd : 入力解除
    }

    Loco --> Damaged : CrossFade呼び出し
    Damaged --> Loco : アニメーション終了

    Loco --> KnockOut : KnockOut trigger
    KnockOut --> [*]
```

### 1.3 物理状態フラグ

```mermaid
stateDiagram-v2
    state "接地判定" as Ground {
        Grounded : _isGrounded = true
        Airborne : _isGrounded = false

        [*] --> Grounded
        Grounded --> Airborne : 地面から離れる
        Airborne --> Grounded : 地面に接触
    }

    state "壁/斜面判定" as Surface {
        Normal : 通常地面
        Slope : _isTouchingSlope = true
        Step : _isTouchingStep = true
        Wall : _isTouchingWall = true

        [*] --> Normal
        Normal --> Slope : 斜面検出
        Normal --> Step : 段差検出
        Normal --> Wall : 壁検出
        Slope --> Normal : 斜面離脱
        Step --> Normal : 段差離脱
        Wall --> Normal : 壁離脱
    }
```

---

## 2. 戦闘状態遷移

### 2.1 ダメージ処理フロー

```mermaid
stateDiagram-v2
    [*] --> SkillHit : スキル衝突検出

    SkillHit --> CheckTarget : ターゲット判定

    state CheckTarget {
        [*] --> IsPlayer : Player Layer
        [*] --> IsEnemy : Enemy Layer
        [*] --> IsShield : Shield Layer
    }

    IsPlayer --> CalcDamage
    IsEnemy --> CalcDamage
    IsShield --> GuardDamage

    state CalcDamage {
        [*] --> CalcAttack : 基礎ダメージ計算
        CalcAttack --> CalcDefence : 防御計算
        CalcDefence --> CalcShield : シールド計算
        CalcShield --> ApplyDamage : 最終ダメージ
    }

    state GuardDamage {
        [*] --> GuardCalcAttack
        GuardCalcAttack --> GuardCalcDefence
        GuardCalcDefence --> GuardInflicted
        GuardInflicted --> CheckGuardBreak
    }

    CalcDamage --> DamageInflicted
    GuardDamage --> DamageInflicted : ガード貫通時

    state DamageInflicted {
        [*] --> CheckExtraHP : 追加HP確認
        CheckExtraHP --> ReduceExtraHP : 追加HPあり
        CheckExtraHP --> ReduceMainHP : 追加HPなし
        ReduceExtraHP --> ReduceMainHP : 追加HP消費完了
        ReduceMainHP --> ShowDamageNumber
        ShowDamageNumber --> DamageMotion
    }

    DamageInflicted --> CheckDead
    CheckDead --> [*] : HP > 0
    CheckDead --> Dead : HP <= 0
```

### 2.2 ダメージ計算詳細

```mermaid
flowchart TD
    A[スキルヒット] --> B{power > 0?}

    B -->|Yes| C[CalcAttack]
    B -->|No| H[Heal処理]

    C --> D[基礎ダメージ = power × 0.5]
    D --> E[ランダム変動適用]
    E --> F{クリティカル判定}
    F -->|Yes| G[クリティカル倍率適用]
    F -->|No| I[CalcDefence]
    G --> I

    I --> J[防御力による減算]
    J --> K[属性ダメージ計算]
    K --> L[CalcShield]

    L --> M{シールドあり?}
    M -->|Yes| N[シールドでダメージ吸収]
    M -->|No| O[最終ダメージ適用]
    N --> P[シールド耐久減少]
    P --> O

    H --> Q[回復量計算]
    Q --> R{overHeal?}
    R -->|Yes| S[追加HPバー生成]
    R -->|No| T[通常回復]
```

---

## 3. アイテムスキル発動フロー

### 3.1 アイテムスキル発動状態遷移

```mermaid
stateDiagram-v2
    [*] --> Idle : 初期状態

    Idle --> CheckInput : 攻撃入力検出

    state CheckInput {
        [*] --> CheckOnUI
        CheckOnUI --> CheckStamina : UI外
        CheckOnUI --> Idle : UI上
        CheckStamina --> CheckActiveSkills : スタミナあり
        CheckStamina --> Idle : スタミナなし
    }

    CheckActiveSkills --> HasActiveSkill : アクティブスキルあり
    CheckActiveSkills --> UseDefaultSkill : アクティブスキルなし

    state HasActiveSkill {
        [*] --> CheckCooldown
        CheckCooldown --> IncreaseCooldown : クールダウン完了
        CheckCooldown --> Skip : クールダウン中
        IncreaseCooldown --> EnableMotion
        EnableMotion --> InvokeFire : 遅延後
    }

    state UseDefaultSkill {
        [*] --> CheckAllCooldowns
        CheckAllCooldowns --> DefaultMotion : 全クールダウン完了
        DefaultMotion --> DefaultFire
    }

    InvokeFire --> ActivateSkill
    DefaultFire --> ActivateSkill

    state ActivateSkill {
        [*] --> UseSkillStamina : スタミナ消費
        UseSkillStamina --> InstantiateSkill : スキル生成
        InstantiateSkill --> InitializeSkill : パラメータ設定
    }

    ActivateSkill --> Idle
```

### 3.2 アイテムスキルアイコン状態

```mermaid
stateDiagram-v2
    [*] --> Inactive : 初期状態

    Inactive --> Active : SwapSkillActive(true)
    Active --> Inactive : SwapSkillActive(false)

    state Active {
        [*] --> Ready : クールダウン完了
        Ready --> Cooldown : スキル使用
        Cooldown --> Ready : クールダウン経過
    }

    state "アイコン表示" as IconDisplay {
        Normal : 通常表示
        CooldownOverlay : クールダウン表示
        ActivateEffect : 発動エフェクト

        Normal --> CooldownOverlay : スキル使用
        CooldownOverlay --> Normal : クールダウン完了
        Normal --> ActivateEffect : アクティブ化
        ActivateEffect --> Normal : 非アクティブ化
    }
```

#### 3.2.1 クールダウン表示仕様

- **表示形式**: 円形ゲージ（Radial Fill）
- **動作**: 時計回りに減少（12時位置から開始）
- **完了演出**: ゲージ消滅時に軽いパルスエフェクト
- **色**: クールダウン中はグレーオーバーレイ、完了時は元の色に復帰

---

## 4. ガードシステム状態

### 4.1 ガード状態遷移

```mermaid
stateDiagram-v2
    [*] --> Available : 初期状態

    state Available {
        [*] --> Ready
        Ready --> Guarding : ガード入力
        Guarding --> Ready : ガード入力解除
    }

    state Guarding {
        [*] --> GuardActive
        GuardActive --> DamageBlocked : ダメージ受ける
        DamageBlocked --> CheckGuardHealth
    }

    CheckGuardHealth --> GuardActive : guardHealth > 0
    CheckGuardHealth --> GuardBreak : guardHealth <= 0

    state GuardBreak {
        [*] --> Cooldown : クールダウン開始
        Cooldown --> IconHide : アイコン非表示
        IconHide --> ShakeAnimation : シェイクアニメーション
        ShakeAnimation --> WaitCooldown
    }

    WaitCooldown --> GuardRecover : クールダウン完了

    state GuardRecover {
        [*] --> RestoreHealth : guardHealth = maxGuardHealth
        RestoreHealth --> IconShow : アイコン表示
        IconShow --> PunchAnimation : パンチアニメーション
    }

    GuardRecover --> Available
```

### 4.2 複数ガードの管理

```mermaid
stateDiagram-v2
    state "ガードスロット管理" as Guards {
        state "Guard[0]" as G0 {
            G0_Active : アクティブ
            G0_Cooldown : クールダウン中
        }

        state "Guard[1]" as G1 {
            G1_Active : アクティブ
            G1_Cooldown : クールダウン中
        }

        state "Guard[n]" as GN {
            GN_Active : アクティブ
            GN_Cooldown : クールダウン中
        }
    }

    note right of Guards
        guardNum分のスロットが存在
        各スロットは独立してクールダウン
        ダメージは先頭のアクティブガードから消費
    end note
```

---

## 5. バフ/デバフ状態

### 5.1 バフライフサイクル

```mermaid
stateDiagram-v2
    [*] --> Created : AddBuff呼び出し

    state Created {
        [*] --> Initialize
        Initialize : BuffType設定
        Initialize : BuffValue設定
        Initialize : BuffTime設定
    }

    Created --> Active : リストに追加

    state Active {
        [*] --> Ticking
        Ticking --> Ticking : Update(deltaTime)
        Ticking : BuffValues[type] に加算
    }

    Active --> CheckTime : 毎フレーム

    CheckTime --> Active : remainingTime > 0
    CheckTime --> Expired : remainingTime <= 0

    state "手動削除" as ManualRemove {
        RemoveBuffByType : プラスのみ削除
        RemoveDeBuffByType : マイナスのみ削除
        RemoveByType : 全て削除
        RemoveAll : 全バフ削除
    }

    Active --> ManualRemove : 削除メソッド呼び出し

    Expired --> Cleanup : リストから削除
    ManualRemove --> Cleanup

    Cleanup --> [*]
```

### 5.2 バフ効果の適用

```mermaid
flowchart TD
    subgraph "バフ値の計算"
        A[Get〇〇メソッド呼び出し] --> B[baseValue取得]
        B --> C[BuffValues取得]
        C --> D[baseValue + BuffValue]
        D --> E[最終値を返す]
    end

    subgraph "バフ種類"
        F[Behaviour系]
        G[Attribute系]
        H[Battle系]
        I[Meta系]
        J[その他]
    end

    F --> |MoveSpeed,JumpForce等| D
    G --> |Frame,Aqua等| D
    H --> |Health,AttackPower等| D
    I --> |Luck| D
    J --> |HealHitPoint等| D
```

---

## 6. ゲーム全体の状態遷移（設計案）

### 6.1 シーン遷移（未実装部分含む）

```mermaid
stateDiagram-v2
    [*] --> Title : ゲーム起動

    state Title {
        [*] --> TitleScreen
        TitleScreen --> NewGame : 新規ゲーム選択
        TitleScreen --> Continue : 続きから選択
        TitleScreen --> Settings : 設定選択
    }

    NewGame --> CharacterSelect : キャラクター選択
    Continue --> LoadSave : セーブデータロード

    CharacterSelect --> Base : キャラクター決定
    LoadSave --> Base : ロード完了

    state Base {
        [*] --> Hub : 拠点画面
        Hub --> StageSelect : ステージ選択
        Hub --> Shop : ショップ
        Hub --> Inventory : インベントリ
        Hub --> AbilityTree : アビリティツリー
    }

    StageSelect --> Gameplay : ステージ開始

    state Gameplay {
        [*] --> Playing
        Playing --> Paused : ポーズ
        Paused --> Playing : 再開
        Playing --> StageClear : クリア条件達成
        Playing --> GameOver : プレイヤー死亡
    }

    StageClear --> Result : リザルト画面
    GameOver --> Result

    Result --> Base : 拠点へ戻る
    Result --> Gameplay : リトライ

    Paused --> Base : 拠点へ戻る
    Paused --> Title : タイトルへ
```

### 6.2 ゲームプレイ内の状態

```mermaid
stateDiagram-v2
    state Gameplay {
        [*] --> StageInit : ステージ初期化

        state StageInit {
            [*] --> LoadStage
            LoadStage --> SpawnPlayer
            SpawnPlayer --> SpawnEnemies
            SpawnEnemies --> StartGameplay
        }

        StartGameplay --> MainLoop

        state MainLoop {
            [*] --> InputProcess : 入力処理
            InputProcess --> CharacterUpdate : キャラクター更新
            CharacterUpdate --> SkillProcess : アイテムスキル処理
            SkillProcess --> DamageCalc : ダメージ計算
            DamageCalc --> AIProcess : AI処理
            AIProcess --> CollisionCheck : 判定処理
            CollisionCheck --> UIUpdate : UI更新
            UIUpdate --> InputProcess : 次フレーム
        }

        MainLoop --> CheckClear : 毎フレーム

        state CheckClear {
            [*] --> EnemyCheck
            EnemyCheck --> BossCheck : 雑魚全滅
            BossCheck --> ClearFlag : ボス撃破
        }

        CheckClear --> MainLoop : 条件未達成
        CheckClear --> StageClear : 条件達成

        MainLoop --> CheckGameOver : 毎フレーム

        state CheckGameOver {
            [*] --> PlayerHealthCheck
            PlayerHealthCheck --> GameOverFlag : HP <= 0
        }

        CheckGameOver --> MainLoop : 生存
        CheckGameOver --> GameOver : 死亡
    }
```

### 6.3 セーブ/ロードタイミング（設計案）

```mermaid
stateDiagram-v2
    state "セーブトリガー" as SaveTrigger {
        Checkpoint : チェックポイント通過
        ReturnBase : 拠点帰還
        ManualSave : 手動セーブ
        StageClear : ステージクリア
    }

    SaveTrigger --> SaveProcess

    state SaveProcess {
        [*] --> CollectData
        CollectData --> SerializeJSON
        SerializeJSON --> WriteFile
        WriteFile --> [*]
    }

    state "ロードトリガー" as LoadTrigger {
        Continue : 続きから
        Retry : リトライ
    }

    LoadTrigger --> LoadProcess

    state LoadProcess {
        [*] --> ReadFile
        ReadFile --> DeserializeJSON
        DeserializeJSON --> RestoreData
        RestoreData --> [*]
    }
```

---

## 7. 敵AI状態遷移

### 7.1 基本AI状態遷移

```mermaid
stateDiagram-v2
    [*] --> Idle : 初期化完了

    state Idle {
        [*] --> Waiting
        Waiting : 周囲を監視
        Waiting --> Patrol : 巡回モード有効
    }

    Idle --> Chase : プレイヤー検知\n(距離 < detectionRange)

    state Chase {
        [*] --> Moving
        Moving : ターゲットに接近
        Moving --> DistanceCheck : 毎フレーム
    }

    Chase --> Idle : ターゲット見失う\n(距離 > loseTargetRange)
    Chase --> Attack : 攻撃範囲内\n(距離 <= attackRange)

    state Attack {
        [*] --> SelectPattern
        SelectPattern --> CastTime : パターン選択完了
        CastTime --> Execute : 詠唱完了
        Execute --> Cooldown : 攻撃実行
    }

    Attack --> Chase : 距離 > attackRange
    Attack --> Flee : HP <= fleeThreshold

    state Flee {
        [*] --> Retreating
        Retreating : ターゲットから逃走
        Retreating --> SafetyCheck : 毎フレーム
    }

    Flee --> Recovery : 安全距離確保\n または HP > reengageThreshold

    state Recovery {
        [*] --> Healing
        Healing : 回復行動（あれば）
        Healing --> [*] : 回復完了
    }

    Recovery --> Chase : 回復完了

    state Dead {
        [*] --> DeathAnimation
        DeathAnimation --> DropItems
        DropItems --> Destroy
    }

    Idle --> Dead : HP <= 0
    Chase --> Dead : HP <= 0
    Attack --> Dead : HP <= 0
    Flee --> Dead : HP <= 0
```

### 7.2 AI種類別の行動パターン

```mermaid
stateDiagram-v2
    state "近接型AI" as MeleeAI {
        [*] --> M_Idle
        M_Idle --> M_Chase : 検知
        M_Chase --> M_Attack : 距離 <= 2.0
        M_Attack --> M_ComboCheck
        M_ComboCheck --> M_Attack : 30%でコンボ継続
        M_ComboCheck --> M_Cooldown : コンボ終了
        M_Cooldown --> M_Chase
    }

    state "遠隔型AI" as RangedAI {
        [*] --> R_Idle
        R_Idle --> R_Position : 検知
        R_Position --> R_Aim : 距離 = preferredRange
        R_Aim --> R_Fire : 照準完了
        R_Fire --> R_Reposition : 射撃後
        R_Reposition --> R_Position
        R_Position --> R_Retreat : 距離 < retreatRange
        R_Retreat --> R_Position
    }

    state "魔法型AI" as MagicAI {
        [*] --> Mg_Idle
        Mg_Idle --> Mg_Evaluate : 検知
        Mg_Evaluate --> Mg_Cast : スキル選択
        Mg_Cast --> Mg_Execute : 詠唱完了
        Mg_Execute --> Mg_Cooldown
        Mg_Cooldown --> Mg_Evaluate
        Mg_Evaluate --> Mg_Heal : HP < 30%
        Mg_Heal --> Mg_Evaluate
    }
```

### 7.3 ボスAIフェーズ遷移

```mermaid
stateDiagram-v2
    [*] --> Phase1 : ボス戦開始

    state Phase1 {
        [*] --> P1_Attack
        P1_Attack : 基本攻撃パターン
        P1_Attack : 攻撃頻度: 低
        P1_Attack --> P1_HPCheck : 毎フレーム
        P1_HPCheck --> P1_Attack : HP > 70%
    }

    Phase1 --> Transition1_2 : HP <= 70%

    state Transition1_2 {
        [*] --> T1_Invincible
        T1_Invincible : 2-3秒無敵
        T1_Invincible --> T1_Effect
        T1_Effect : 演出再生
    }

    Transition1_2 --> Phase2

    state Phase2 {
        [*] --> P2_Attack
        P2_Attack : 追加攻撃パターン解放
        P2_Attack : 攻撃頻度: 中
        P2_Attack : 特殊攻撃解放
        P2_Attack --> P2_Special : 15-30秒ごと
        P2_Special --> P2_Attack
        P2_Attack --> P2_HPCheck
        P2_HPCheck --> P2_Attack : HP > 40%
    }

    Phase2 --> Transition2_3 : HP <= 40%

    state Transition2_3 {
        [*] --> T2_Invincible
        T2_Invincible --> T2_Effect
    }

    Transition2_3 --> Phase3

    state Phase3 {
        [*] --> P3_Attack
        P3_Attack : 全攻撃パターン使用
        P3_Attack : 攻撃頻度: 高
        P3_Attack --> P3_HPCheck
        P3_HPCheck --> P3_Attack : HP > 10%
    }

    Phase3 --> Enrage : HP <= 10%

    state Enrage {
        [*] --> E_Buff
        E_Buff : 攻撃力 +50%
        E_Buff : 攻撃速度 +30%
        E_Buff --> E_Attack
        E_Attack : 連続攻撃増加
    }

    Phase1 --> Dead : HP <= 0
    Phase2 --> Dead : HP <= 0
    Phase3 --> Dead : HP <= 0
    Enrage --> Dead : HP <= 0
```

### 7.4 グループAI状態

```mermaid
stateDiagram-v2
    [*] --> Scattered : グループ生成

    state Scattered {
        [*] --> Individual
        Individual : 各メンバーが独立行動
    }

    Scattered --> Forming : リーダー指令

    state Forming {
        [*] --> GatherPoint
        GatherPoint : 集合地点へ移動
        GatherPoint --> Formation : 全員集合
    }

    Forming --> Coordinated : 陣形完成

    state Coordinated {
        [*] --> Holding
        Holding --> Attack : 攻撃指令
        Attack --> Holding : 攻撃完了
        Holding --> Flank : 挟撃指令
        Flank --> Holding
    }

    Coordinated --> Retreat : グループHP < 30%

    state Retreat {
        [*] --> Withdrawing
        Withdrawing : 安全地帯へ撤退
    }

    Retreat --> Scattered : 撤退完了

    Coordinated --> Scattered : リーダー撃破
```

---

## 8. クエスト状態遷移

### 8.1 クエストライフサイクル

```mermaid
stateDiagram-v2
    [*] --> NotStarted : クエスト定義

    state NotStarted {
        [*] --> Hidden
        Hidden --> Available : 前提条件達成
    }

    Available --> InProgress : クエスト受注

    state InProgress {
        [*] --> Active
        Active : 目標追跡中
        Active --> ObjectiveCheck : 進捗更新
        ObjectiveCheck --> Active : 未完了
        ObjectiveCheck --> AllComplete : 全目標達成
    }

    InProgress --> Abandoned : プレイヤー放棄\n(サブクエストのみ)
    Abandoned --> Available : 再受注可能

    AllComplete --> ReadyToComplete : 報告先あり
    AllComplete --> Completed : 報告先なし（自動完了）

    state ReadyToComplete {
        [*] --> WaitingReport
        WaitingReport : NPC/掲示板へ報告待ち
    }

    ReadyToComplete --> RewardSelection : 報告完了

    state RewardSelection {
        [*] --> ShowOptions
        ShowOptions : 選択式報酬表示
        ShowOptions --> PlayerChoice : プレイヤー選択
    }

    RewardSelection --> Completed : 報酬選択完了

    state Completed {
        [*] --> GrantRewards
        GrantRewards : 報酬付与
        GrantRewards --> UnlockNext
        UnlockNext : 次クエスト解放
    }

    Completed --> [*]
```

### 8.2 クエスト目標の状態

```mermaid
stateDiagram-v2
    state "討伐目標" as Defeat {
        [*] --> D_Tracking
        D_Tracking : currentAmount / requiredAmount
        D_Tracking --> D_Update : 対象撃破
        D_Update --> D_Tracking : currentAmount < requiredAmount
        D_Update --> D_Complete : currentAmount >= requiredAmount
    }

    state "収集目標" as Collect {
        [*] --> C_Tracking
        C_Tracking --> C_Update : アイテム取得
        C_Update --> C_Tracking : currentAmount < requiredAmount
        C_Update --> C_Complete : currentAmount >= requiredAmount
    }

    state "到達目標" as Reach {
        [*] --> R_NotReached
        R_NotReached --> R_Check : エリア進入
        R_Check --> R_Complete : 対象地点
        R_Check --> R_NotReached : 異なる地点
    }

    state "護衛目標" as Escort {
        [*] --> E_NotStarted
        E_NotStarted --> E_InProgress : NPCに接触
        E_InProgress --> E_Check : 毎フレーム
        E_Check --> E_InProgress : NPC生存 & 未到着
        E_Check --> E_Complete : 目的地到着
        E_Check --> E_Failed : NPC死亡
        E_Failed --> E_NotStarted : リトライ
    }

    state "タイムアタック目標" as TimeAttack {
        [*] --> T_NotStarted
        T_NotStarted --> T_Running : 開始トリガー
        T_Running --> T_Check : 毎フレーム
        T_Check --> T_Running : 制限時間内 & 未達成
        T_Check --> T_Complete : 条件達成
        T_Check --> T_Failed : 時間切れ
        T_Failed --> T_NotStarted : リトライ
    }
```

### 8.3 クエストUI状態

```mermaid
stateDiagram-v2
    [*] --> QuestLogClosed

    QuestLogClosed --> QuestLogOpen : メニュー開く

    state QuestLogOpen {
        [*] --> TabSelection
        TabSelection --> ActiveQuests : 進行中タブ
        TabSelection --> AvailableQuests : 受注可能タブ
        TabSelection --> CompletedQuests : 完了タブ

        state ActiveQuests {
            [*] --> QuestList
            QuestList --> QuestDetail : クエスト選択
            QuestDetail --> QuestList : 戻る
            QuestDetail --> AbandonConfirm : 放棄ボタン
            AbandonConfirm --> QuestList : 放棄確定
        }

        state AvailableQuests {
            [*] --> AvailableList
            AvailableList --> AcceptConfirm : クエスト選択
            AcceptConfirm --> AvailableList : 受注確定
        }
    }

    QuestLogOpen --> QuestLogClosed : メニュー閉じる
```

---

## 9. インベントリ状態遷移

### 9.1 インベントリシステム状態

```mermaid
stateDiagram-v2
    [*] --> Closed : 初期状態

    Closed --> Open : インベントリ開く

    state Open {
        [*] --> Browse
        Browse : スロット閲覧

        Browse --> ItemSelected : アイテム選択

        state ItemSelected {
            [*] --> ShowDetail
            ShowDetail : アイテム詳細表示
            ShowDetail --> ActionMenu : アクション表示
        }

        state ActionMenu {
            Use : 使用
            Equip : 装備
            Drop : 捨てる
            Split : 分割
        }

        ActionMenu --> UseItem : 使用選択
        ActionMenu --> EquipItem : 装備選択
        ActionMenu --> DropItem : 捨てる選択
        ActionMenu --> SplitItem : 分割選択

        UseItem --> Browse : 使用完了
        EquipItem --> Browse : 装備完了
        DropItem --> DropConfirm : 確認
        DropConfirm --> Browse : 確定
        SplitItem --> SplitUI : 分割UI
        SplitUI --> Browse : 分割完了

        ItemSelected --> Browse : キャンセル
    }

    Open --> Closed : インベントリ閉じる
```

### 9.2 アイテム追加フロー

```mermaid
stateDiagram-v2
    [*] --> ItemAcquired : アイテム取得

    ItemAcquired --> CheckStack : スタック確認

    state CheckStack {
        [*] --> FindExisting
        FindExisting --> HasExisting : 同アイテムあり
        FindExisting --> NoExisting : 同アイテムなし
    }

    HasExisting --> CheckStackLimit

    state CheckStackLimit {
        [*] --> CanStack : スタック可能
        CanStack --> AddToStack : スタックに追加
        [*] --> StackFull : スタック上限
        StackFull --> FindEmptySlot
    }

    NoExisting --> FindEmptySlot

    state FindEmptySlot {
        [*] --> MainInventory
        MainInventory --> HasEmpty : 空きあり
        MainInventory --> NoEmpty : 空きなし
        NoEmpty --> CheckBag : バッグ確認
        CheckBag --> BagHasEmpty : バッグに空きあり
        CheckBag --> InventoryFull : 完全に満杯
    }

    HasEmpty --> AddToEmpty : 空きスロットに追加
    BagHasEmpty --> AddToBag : バッグに追加

    AddToStack --> UpdateUI
    AddToEmpty --> UpdateUI
    AddToBag --> UpdateUI

    state UpdateUI {
        [*] --> RefreshSlots
        RefreshSlots --> RefreshIcons
        RefreshIcons --> ShowNotification
    }

    UpdateUI --> Success : 追加成功

    InventoryFull --> Overflow : 追加失敗

    state Overflow {
        [*] --> DropToGround : 地面に落とす
        DropToGround --> ShowWarning : 警告表示
    }

    Success --> [*]
    Overflow --> [*]
```

### 9.3 装備変更フロー

```mermaid
stateDiagram-v2
    [*] --> EquipRequest : 装備リクエスト

    EquipRequest --> CheckRequirements

    state CheckRequirements {
        [*] --> CheckLevel : レベル確認
        CheckLevel --> LevelOK : 条件クリア
        CheckLevel --> LevelNG : レベル不足
        LevelOK --> CheckSlot : スロット確認
    }

    LevelNG --> EquipFailed : 装備失敗

    state CheckSlot {
        [*] --> SlotEmpty : スロット空き
        [*] --> SlotOccupied : 装備中アイテムあり
    }

    SlotEmpty --> DirectEquip

    SlotOccupied --> SwapEquipment

    state SwapEquipment {
        [*] --> UnequipCurrent
        UnequipCurrent --> MoveToInventory
        MoveToInventory --> EquipNew
    }

    DirectEquip --> ApplyStats
    SwapEquipment --> ApplyStats

    state ApplyStats {
        [*] --> UpdateCharacterStats
        UpdateCharacterStats --> UpdateSkillCache
        UpdateSkillCache --> RefreshUI
    }

    ApplyStats --> EquipSuccess

    EquipSuccess --> [*]
    EquipFailed --> [*]
```

### 9.4 アイテムスキルスロット状態

```mermaid
stateDiagram-v2
    state "アイテムスキルスロット[0-9]" as SkillSlots {
        state "各スロット状態" as SlotState {
            [*] --> Empty : 初期状態

            Empty --> Equipped : アイテムスキルアイテム装備

            state Equipped {
                [*] --> Ready : クールダウンなし
                Ready --> Active : アクティブ化
                Active --> Casting : アイテムスキル発動
                Casting --> Cooldown : 発動完了
                Cooldown --> Ready : クールダウン完了
                Active --> Ready : 非アクティブ化
            }

            Equipped --> Empty : アイテム外す

            state "耐久度管理" as Durability {
                [*] --> Full
                Full --> Damaged : 使用で消耗
                Damaged --> Damaged : 継続使用
                Damaged --> Broken : 耐久度 = 0
                Broken --> Repaired : 修理
                Repaired --> Full
            }
        }
    }

    note right of SkillSlots
        10個のスロット（inventorySlots[10]）
        各スロットは独立して状態管理
        UI表示は4スロットまで
    end note
```

---

## 補足: CharacterControlの内部状態フラグ一覧

| フラグ名 | 型 | 説明 | 初期値 |
|---------|-----|------|--------|
| _isGrounded | bool | 接地判定 | false |
| _isTouchingSlope | bool | 斜面接触 | false |
| _isTouchingStep | bool | 段差接触 | false |
| _isTouchingWall | bool | 壁接触 | false |
| _isJumping | bool | ジャンプ中 | false |
| _isCrouch | bool | しゃがみ/ガード中 | false |
| _isDead | bool | 死亡状態 | false |
| _isOnUI | bool | UI操作中 | false |
| _jump | bool | ジャンプ入力 | false |
| _jumpHold | bool | ジャンプ長押し | false |
| _attack | bool | 攻撃入力 | false |
| _guard | bool | ガード入力 | false |
| _sprint | bool | ダッシュ入力 | false |
| _modeChange | bool | モード変更入力 | false |

---

**作成日**: 2026-01-14
**バージョン**: 1.0
