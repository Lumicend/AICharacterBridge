# AI Character Bridge - プロジェクト概要

**対象ゲーム**: Koikatsu (コイカツ)  
**最終更新**: 2026年6月

---

## プロジェクト概要

### 目的

AI Character Bridgeは、Unityベースのゲーム（特にIllusion社のコイカツ）において、**AIとゲームキャラクターを接続**し、文脈に応じた自然で高度な対話を実現するBepInExプラグインです。

### 主な機能

- **プロバイダーパターンによる拡張可能なAI通信**: 新しいAIクライアント（OpenAI、Claude等）を簡単に追加可能
- **CharacterCard & WorldSetting システム**: Character Card V2/V3規格に対応し、キャラクター人格データと世界設定をゲームセーブデータで一元管理
- **衣装プリセット対応**: ゲームの衣装プリセット（7種類）ごとの服装・容姿説明文をCharacterCardに付随して管理し、プロンプトへ自動反映
- **統合編集UI**: メインゲーム中にすべてのCharacterCardとWorldSettingを編集可能（衣装プリセットごとの説明文編集を含む）
- **拡張可能なログシステム**: 新しいログタイプの追加が既存コード変更なしで可能
- **統合データ管理**: HeroineごとのCharacterCardとログを一元管理
- **ターン単位の会話管理**: AIとの1回の通信を1ターンとして管理し、会話の流れを構造化
- **自動セッション管理**: TalkSceneの開始/終了と連動した会話ログの自動保存
- **ユーザー承認型アクション実行**: AIが提案した特殊アクションはユーザーの承認後に実行
- **汎用プロンプト構築システム**: 順序制御・通常置換・タグ付き置換を統合した拡張性の高いテンプレート変数置換機能
- **最適化されたログフォーマット**: 低性能LLMでも理解しやすい形式で過去ログを提供
- **モジュール独立セーブ機構**: 各モジュールが ExtensibleSaveFormat の独立したスロットに自己完結でデータを保存
- **モジュール設計**: 機能を独立したモジュールとして管理し、追加・削除が容易
- **AIレスポンスのロバスト処理**: Markdownコードブロックの自動除去・絵文字除去・必須フィールドバリデーションにより、AIの出力揺れを吸収

### 技術スタック

- **BepInEx**: MODローダー
- **KKAPI**: コイカツのAPI
- **ExtensibleSaveFormat**: 拡張セーブデータ
- **Newtonsoft.Json**: JSON処理
- **UnityEngine**: ゲームエンジン
- **.NET Framework 3.5**: ターゲットフレームワーク

---

## アーキテクチャ

### ディレクトリ構成
```
AICharacterBridge/
├── Core/                              # ゲーム非依存の汎用部分
│   ├── Data/                          # 汎用データモデル
│   │   ├── CharacterCard.cs          # CharacterCard基底クラス、V2/V3実装
│   │   └── WorldSetting.cs
│   ├── Communication/                 # AI通信層（プロバイダーパターン）
│   │   ├── Interfaces/               # 通信インターフェース
│   │   │   ├── IClientProvider.cs    # プロバイダーインターフェース（外部API）
│   │   │   ├── ICommunicationClient.cs # 通信クライアントインターフェース
│   │   │   └── IResponseExtractor.cs # レスポンス抽出インターフェース
│   │   ├── ClientRegistry.cs         # クライアントプロバイダー管理
│   │   └── Clients/                  # クライアント実装
│   │       ├── Ollama/               # Ollamaクライアント
│   │       │   ├── OllamaClient.cs
│   │       │   ├── OllamaResponseExtractor.cs
│   │       │   └── OllamaClientProvider.cs
│   │       └── LMStudio/             # LM Studioクライアント
│   │           ├── LMStudioClient.cs
│   │           ├── LMStudioResponseExtractor.cs
│   │           └── LMStudioClientProvider.cs
│   ├── Prompt/                        # プロンプト構築システム
│   │   ├── PromptReplacer.cs          # 変数置換エンジン
│   │   └── ReplaceEntry.cs           # 置換エントリーデータクラス
│   └── Utilities/
│       └── ConfigurationManagerAttributes.cs  # BepInEx ConfigurationManager用
│
├── Data/                              # データモデル・データアクセス層
│   ├── GameDataFormatter.cs          # データ変換（フォーマット）専用
│   ├── GameStateProvider.cs          # 現在のゲーム状態取得
│   ├── CharacterCardProvider.cs      # CharacterCard取得と初期化（生データ）
│   ├── CharacterCardResolver.cs      # CharacterCardのプレースホルダー解決
│   ├── CoordinateData.cs             # 衣装プリセットごとの服装・容姿説明文
│   ├── MainGameLog.cs                # メインゲームログ基底クラス
│   ├── MainGameLogCollection.cs      # ログコレクション管理
│   ├── HeroineGameData.cs            # Heroine統合データ（Card + Logs）
│   ├── ExpressionData.cs             # 表情データ
│   ├── CharaMotionData.cs            # モーションデータ
│   ├── ExpressionPresets.cs          # 表情プリセット
│   └── CharaMotionPresets.cs         # モーションプリセット
│
├── TalkSceneChat/                     # TalkSceneChatモジュール
│   ├── TalkSceneChatModule.cs        # モジュール本体（MonoBehaviour）
│   ├── TalkSceneChatGameController.cs # モジュール専用セーブコントローラー
│   ├── TalkSceneChatSaveData.cs      # モジュール専用セーブデータ
│   ├── TalkSceneSessionManager.cs    # 会話セッション管理
│   ├── TalkSceneLogFormatter.cs      # ログフォーマット処理
│   ├── TalkSceneActionFilter.cs      # 利用可能アクション判定
│   ├── TalkSceneActionExecutor.cs    # 特殊アクション実行
│   ├── TalkSceneEventExecutor.cs     # ADVイベント構築・実行
│   ├── TalkScenePromptBuilder.cs     # プロンプト構築
│   ├── Data/                          # TalkSceneChat専用データ
│   │   ├── TalkSceneLog.cs
│   │   ├── ConversationTurn.cs       # 会話ターン（1回の通信単位）
│   │   ├── ConversationEntry.cs
│   │   ├── ConversationEntryType.cs
│   │   ├── ChatEntry.cs
│   │   ├── ActionEntry.cs
│   │   └── HeroineChatSettings.cs    # ヒロイン固有チャット設定（context_note等）
│   ├── Response/                      # レスポンス処理
│   │   ├── TalkSceneResponse.cs
│   │   └── DialogueSegment.cs
│   ├── UI/                            # TalkSceneChat専用UI
│   │   └── TalkSceneUI.cs
│   └── TalkSceneDefaultTemplate.cs   # デフォルトプロンプト（タグ構造はBuilder側で付与）
│
├── UI/                                # 共通UIコンポーネント
│   └── CharacterCardEditorUI.cs
│
├── AICharacterBridgePlugin.cs
├── GameController.cs
└── AICharacterBridgeSaveData.cs
```

---

## モジュール設計

### 概要

プラグインは**モジュール単位**で機能を管理します。各モジュールは独立しており、プラグイン本体への影響を最小限に抑えて追加・削除が可能です。

### モジュール構造

各モジュールは以下の構成を持ちます：
```
ModuleName/
├── ModuleNameModule.cs          # モジュール本体（MonoBehaviour）
├── ModuleNameGameController.cs  # モジュール専用セーブコントローラー
├── ModuleNameSaveData.cs        # モジュール専用セーブデータ
├── Data/                         # モジュール専用データ
├── UI/                           # モジュール専用UI
└── その他必要なコンポーネント
```

モジュールのセーブ機構は `GameCustomFunctionController` を継承した専用コントローラーが担い、`ModuleGUID`（`AICharacterBridgePlugin.GUID + ".modulename"`）をキーとして ExtensibleSaveFormat の独立したスロットにデータを保存します。これにより、コアのセーブデータ（`AICharacterBridgeSaveData`）からモジュール固有データが完全に分離されます。

### TalkSceneChatモジュール

会話シーンでのAI対話機能を提供するモジュール。TalkSceneのライフサイクルと完全連動した自動セッション管理を実現。

#### アーキテクチャ
```
TalkSceneChatGameController（セーブ管理・GameCustomFunctionController）
├── ExtensibleSaveFormat 独立スロット（GUID: "...kk.talkscenechat"）
├── TalkSceneChatSaveData の保存・復元
└── TalkSceneChatSaveData.CurrentSaveData を公開

TalkSceneChatModule（制御層・MonoBehaviour）
├── TalkScene開始/終了の監視（Update()）
├── セッションの自動開始/終了
├── UI開閉制御（enabled プロパティ）
└── SessionManagerへの参照提供

TalkSceneSessionManager（セッション管理）
├── セッション状態管理（IsSessionActive）
├── ActiveSessionLogの保持と操作
├── ターンの追加
└── ログ保存処理

TalkSceneLogFormatter（ログフォーマット）
├── 過去ログと現在セッションログの統合
├── UIとプロンプトビルダー両方で使用
└── 静的メソッドによる単一責任

TalkSceneUI（表示層・ImguiWindow）
├── UI描画のみ
├── ユーザー入力受付
└── モジュール経由でセッションにアクセス

TalkScenePromptBuilder（プロンプト構築）
├── CharacterCardResolver でプレースホルダー解決済みのカードを取得
├── WorldSetting取得（コアセーブデータから）
├── TalkSceneLogFormatterを使用したログフォーマット
├── context_note 取得（TalkSceneChatGameController.CurrentSaveData から）
├── 利用可能アクションのフィルタリング
└── ReplaceEntry リストの構築と PromptReplacer.ReplaceAll による一括置換
    ├── Plain: テンプレート内でインラインで使用される場合（囲みタグが直書きされている場合を含む）
    ├── Tagged block: テンプレートは {{key}} のみ記述し、タグをBuilder側で付与する場合
    └── Tagged block + note: タグに note 属性が必要な場合

TalkSceneActionFilter（アクション判定）
├── ゲーム状態に基づく利用可能アクション判定
└── アクションボタンのテキスト生成

TalkSceneActionExecutor（アクション実行）
├── 特殊アクションの実行
└── ゲームステートの変更

TalkSceneEventExecutor（イベント実行）
├── ADVイベントの構築
└── イベントの実行
```

#### セッション管理フロー
```
1. TalkScene開始（targetHeroine設定済み）
   ↓
2. TalkSceneChatModule が検知
   → SessionManager.StartSession() 自動実行
   → ActiveSessionLog 作成
   ↓
3. ユーザーがチャット実行（UI開閉は任意）
   ↓
4. TalkSceneUI → ConversationTurn作成 → SessionManager.AddTurn()
   → ActiveSessionLog に蓄積
   ↓
5. TalkScene終了
   ↓
6. TalkSceneChatModule が検知
   → SessionManager.EndSession() 自動実行
   → ログをコアセーブデータに保存
   → UI自動クローズ（enabled = false）
```

#### 主要コンポーネント

**TalkSceneChatGameController.cs（GameCustomFunctionController）**
- ExtensibleSaveFormat の独立スロットで TalkSceneChat 固有データを保存・復元
- `CurrentSaveData`（`TalkSceneChatSaveData`）をグローバルアクセスポイントとして公開
- `TalkSceneChatModule.ModuleGUID`（`"...kk.talkscenechat"`）をスロットキーとして使用

**TalkSceneChatSaveData.cs**
- `CustomPromptTemplate`: カスタムプロンプトテンプレート文字列（未設定の場合は `TalkSceneDefaultTemplate.GetTemplate()` を使用）
- `HeroineSettingsList`: ヒロインごとのチャット設定リスト（シリアライズ用）
- `GetContextNote(heroine)` / `SetContextNote(heroine, note)`: ヒロインごとの `context_note` アクセス
- `HeroineGameData` / `AICharacterBridgeSaveData` と同じ `PrepareForSave` / `RestoreAfterLoad` パターンを踏襲

**TalkSceneChatModule.cs（MonoBehaviour）**
- `ModuleGUID` 定数（`AICharacterBridgePlugin.GUID + ".talkscenechat"`）
- `Initialize()` 内で `GameAPI.RegisterExtraBehaviour<TalkSceneChatGameController>(ModuleGUID)` を呼び出し
- Update() でTalkScene状態監視、セッションライフサイクル管理
- UI 開放時に `InitializePromptTemplate()` および `InitializeContextNote()` を呼び出し、各初期値を UI に反映
  - `InitializePromptTemplate()`: `TalkSceneChatSaveData.CustomPromptTemplate` が設定されていればそれを使用し、未設定なら `TalkSceneDefaultTemplate.GetTemplate()` を使用する

**TalkSceneSessionManager.cs**
- セッション状態管理（IsSessionActive）
- ActiveSessionLogの操作
- ターンの追加と管理
- ログの自動保存（コアセーブデータへ）

**TalkSceneLogFormatter.cs**
- 過去ログと現在セッションログの統合フォーマット
- UIとプロンプトビルダー両方で共通使用
- 静的メソッドによる単一責任の実現

**TalkSceneUI.cs（ImguiWindow）**
- 純粋な表示層
- `CustomPromptTemplate` の読み書きは `TalkSceneChatGameController.CurrentSaveData` 経由
  - Prompt タブの「Apply Changes」は、編集中のテキストが `TalkSceneDefaultTemplate.GetTemplate()` と一致する場合は `CustomPromptTemplate` を `null` にしてデフォルトへ戻し、異なる場合はそのテキストを `CustomPromptTemplate` として保存する
  - Prompt タブの「Reset」は、編集中のテキストを `TalkSceneDefaultTemplate.GetTemplate()` で上書きする（セーブデータへの反映は「Apply Changes」時）
- `context_note` の読み書きは `TalkSceneChatGameController.CurrentSaveData` 経由
- ログの読み書きは `GameController.CurrentSaveData` 経由（ログはコアデータ管轄）

**設定項目:**
- `Toggle UI Key`: UI表示切り替えキー（デフォルト: "L"キー）
- `Enable Favorability Update`: 会話内容をヒロインの好感度に反映するか（デフォルト: true）
- `Enable Arousal Update`: 会話内容をヒロインの性的興奮度に反映するか（デフォルト: true）

**プラグインへの導入:**
```csharp
// AICharacterBridgePlugin.cs
private void InitializeModules()
{
    TalkSceneChatModule.Initialize(gameObject, Config);
    // 将来的に他のモジュールもここに追加
}
```

### 新しいモジュールの追加方法

1. モジュールディレクトリを作成
2. `{ModuleName}Module.cs`（MonoBehaviour継承）を作成し、`Initialize()` 内で専用コントローラーを登録
3. `{ModuleName}GameController.cs`（GameCustomFunctionController継承）を作成
4. `{ModuleName}SaveData.cs` を作成
5. `AICharacterBridgePlugin.InitializeModules()` に1行追加

例：
```csharp
// DateSystemModule.cs
public class DateSystemModule : MonoBehaviour
{
    public const string ModuleGUID = AICharacterBridgePlugin.GUID + ".datesystem";

    public static void Initialize(GameObject gameObject, ConfigFile config)
    {
        GameAPI.RegisterExtraBehaviour<DateSystemGameController>(ModuleGUID);
        gameObject.AddComponent<DateSystemModule>();
    }
}

// AICharacterBridgePlugin.cs
private void InitializeModules()
{
    TalkSceneChatModule.Initialize(gameObject, Config);
    DateSystemModule.Initialize(gameObject, Config);  // ← 追加
}
---


## 通信層の設計（Core/Communication）

### プロバイダーパターンの採用

通信層は**プロバイダーパターン**を採用し、各AIクライアントが設定・初期化・通信を自己管理します。

### アーキテクチャ図
```
プラグイン本体
    ↓
ClientRegistry (レジストリ)
    ↓
IClientProvider (外部API)
    ↓ 内部で使用
ICommunicationClient + IResponseExtractor
```

### インターフェース設計

#### IClientProvider（外部API）

プラグインが使用する唯一のインターフェース。
```csharp
public interface IClientProvider
{
    string GetName();
    void RegisterConfiguration(ConfigFile config);
    IEnumerator SendPrompt(string prompt, Action<string> onSuccess, Action<Exception> onError);
}
```

**特徴:**
- 設定の登録から通信まで全て自己管理
- ClientOptionsなどの中間データ構造が不要
- プラグイン側は設定の詳細を意識しない

#### ICommunicationClient（内部API）

低レベル通信を担当。
```csharp
public interface ICommunicationClient
{
    string GetName();
    void Configure(string model, int timeoutSeconds, JObject llmOptions);
    IEnumerator Post(string prompt, Action<string> onSuccess, Action<Exception> onError);
}
```

#### IResponseExtractor（内部API）

AIレスポンスからメッセージを抽出。
```csharp
public interface IResponseExtractor
{
    string GetName();
    string ExtractMessage(string rawResponse);
}
```

### ClientRegistry

全てのクライアントプロバイダーを管理する静的クラス。
```csharp
public static class ClientRegistry
{
    static ClientRegistry()
    {
        RegisterProvider(new OllamaClientProvider());
        RegisterProvider(new LMStudioClientProvider());
        // 新しいクライアントを追加する場合はここに1行追加
    }
    
    public static void RegisterAllConfigurationsTo(ConfigFile config);
    public static IEnumerator SendPrompt(string clientName, string prompt, ...);
}
```

### 実装済みクライアント一覧

| クライアント名 | エンドポイント | プロンプトキー | レスポンス取り出しパス |
|---|---|---|---|
| Ollama | `/api/generate` | `"prompt"` | `response` |
| LM Studio | `/v1/responses` | `"input"` | `output[0].content[0].text` |

#### Ollama と LM Studio の主な実装上の差異

| 比較項目 | Ollama | LM Studio |
|---|---|---|
| LLMオプションの渡し方 | `"options": { ... }` にネスト | トップレベルフィールドとして展開 |
| LLMオプションの設定フォーマット | JSON形式（中括弧なし）。Ollama API の `options` オブジェクトの中身をそのまま記述 | JSON形式（中括弧なし）。`/v1/responses` のトップレベルフィールドをそのまま記述 |
| トークン上限パラメーター名 | `max_tokens` | `max_output_tokens` |
| モデル名 | 必須（空不可） | 空文字列許容（起動中のモデルを自動使用） |
| クライアントインスタンスの生成 | Provider フィールドとして保持 | `SendPrompt` 呼び出し毎に生成（BaseUrl が実行時変更される可能性のため） |
| think オプション | 設定項目あり。`"Default"` / `"True"` / `"False"` のドロップダウンで選択。トップレベルフィールドとして付与（`IClientProvider` 固有メソッド `SetThinkOption()` で設定） | なし |

---

## データモデル

### Core/Data

#### CharacterCard

キャラクターまたはユーザーの人格データ。Character Card V2/V3規格に対応。
```csharp
public abstract class CharacterCard
{
    [JsonProperty("raw_json")]
    public string RawJson { get; set; }  // 完全なJSON保持
    
    [JsonIgnore]
    protected JObject ParsedData { get; set; }  // パース済みキャッシュ
    
    // 共通フィールドアクセス
    public abstract string GetName();
    public abstract void SetName(string name);
    public abstract string GetDescription();
    public abstract void SetDescription(string description);
    public abstract string GetPersonality();
    public abstract void SetPersonality(string personality);
    public abstract string GetMessageExample();
    public abstract void SetMessageExample(string mesExample);
    public abstract string GetFirstMessage();
    public abstract void SetFirstMessage(string firstMes);
    public abstract string GetScenario();
    public abstract void SetScenario(string scenario);
    
    // extensions アクセス（名前空間キーで階層化して管理）
    public JToken GetExtensionValue(string namespaceKey, string key);
    public void SetExtensionValue(string namespaceKey, string key, JToken value);
    public void RemoveExtensionValue(string namespaceKey, string key);
    public bool HasExtensionValue(string namespaceKey, string key);
    
    // ファクトリメソッド
    public static CharacterCard FromJson(string json);
    public static CharacterCard CreateNew(string version = "v2");
}
```

**重要な設計方針:**
- RAW JSONを完全に保持し、データ欠落を防止
- V2/V3の両方をサポート
- 新規作成時はデフォルトでV2を使用
- 使用しないフィールドも保持し、入出力で情報が失われない
- `data.extensions` 内のデータは名前空間キーで階層化して管理。指定したキー以外には一切触れない

**extensions の JSON 構造:**
```json
{
  "extensions": {
    "ai_character_bridge_kk": {
      "coordinate_data": { ... }
    }
  }
}
```

**extensions 関連の定数（AICharacterBridgePlugin.cs）:**
```csharp
public const string ExtensionNamespace = "ai_character_bridge_kk";
public const string CoordinateDataKey  = "coordinate_data";
```

#### WorldSetting

ゲームの世界設定データ。Character Card V2 と同様の `spec / spec_version / data` 構造を採用しています。

**JSON 構造:**
```json
{
  "spec": "world_setting",
  "spec_version": "1.0",
  "data": {
    "description": "..."
  }
}
```

```csharp
public class WorldSetting
{
    // spec 識別子定数（ファイル種別の検証に使用）
    public const string SpecIdentifier = "world_setting";
    public const string CurrentSpecVersion = "1.0";
    
    [JsonProperty("spec")]
    public string Spec { get; set; }        // 常に "world_setting"
    
    [JsonProperty("spec_version")]
    public string SpecVersion { get; set; } // 現在は "1.0"
    
    [JsonProperty("data")]
    public WorldSettingData Data { get; set; }
    
    // データアクセス
    public string GetDescription();
    public void SetDescription(string description);
    
    // ユーティリティ
    public bool IsDefault();
    public WorldSetting Clone();
    public string FormatForPrompt();
    public string ToJson();
    
    // ファクトリ（FromJson は spec フィールドを検証し、
    // "world_setting" 以外であれば NotSupportedException をスローする）
    public static WorldSetting FromJson(string json);
    public static WorldSetting CreateNew();
}
```

**設計の特徴:**
- `spec` フィールドによってファイル種別を明示的に識別できる
- `FromJson()` がロード前に `spec` を検証するため、誤ったファイルの読み込みを防止できる
- データアクセスは `GetDescription()` / `SetDescription()` 経由で行い、内部構造の変更から呼び出し側を保護する
- `CharacterCard` と異なり、プラグインが完全に管理するフォーマットのため RAW JSON 保持は行わない

### Core/Prompt - プロンプト構築システム

#### ReplaceEntry

プロンプトテンプレートの置換エントリーを表すクラス。通常置換とタグ付き置換（インライン／ブロック）の両方をサポートします。

```csharp
public class ReplaceEntry
{
    public string Key      { get; }  // プレースホルダーのキー（例: "user_name"）
    public string Value    { get; }  // 置換する値
    public bool   IsTagged { get; }  // タグ付き置換かどうか
    public string TagName  { get; }  // タグ名（IsTagged == true のときのみ使用）
    public string Note     { get; }  // タグの note 属性（オプション）
    public bool   IsBlock  { get; }  // ブロック形式かどうか（タグ間に改行を挿入）
    
    // ファクトリメソッド
    public static ReplaceEntry Plain(string key, string value);
    public static ReplaceEntry Tagged(string key, string value, string tagName, bool block = true);
    public static ReplaceEntry Tagged(string key, string value, string tagName, string note, bool block = true);
    
    // ユーティリティ
    public static string FormatStringListAsJson(List<string> items);
}
```

**ファクトリメソッドの使い分け:**

| メソッド | 生成されるエントリー |
|---|---|
| `Plain("key", value)` | 通常置換。`{{key}}` → `value` |
| `Tagged("key", value, "tag")` | タグ付き置換（ブロック形式）。`{{key}}` → `<tag>\nvalue\n</tag>` |
| `Tagged("key", value, "tag", block: false)` | タグ付き置換（インライン形式）。`{{key}}` → `<tag>value</tag>` |
| `Tagged("key", value, "tag", "note")` | タグ付き置換（ブロック形式・note属性あり）。`{{key}}` → `<tag note="note">\nvalue\n</tag>` |

**block パラメータについて:**
`block = true`（デフォルト）のとき、開タグと閉タグの間に改行が挿入されます。複数行テキストを囲む場合に適しています。`block = false` はインライン置換で、短い値を既存タグ内に埋め込む場合に使用します。

**タグ付き置換の空値処理:**
`value` が `null` または空文字の場合、`{{key}}` を含む行が前後の改行ごとごっそり削除されます。`{{world_setting}}` のようにオプション情報で記述がない場合でも、テンプレートに余分な空行を残しません。

#### PromptReplacer

プロンプトテンプレートの変数置換を行う静的クラス。`CharacterCardResolver` と `TalkScenePromptBuilder` の両方で使用します。

```csharp
public static class PromptReplacer
{
    // 単一の通常置換: {{key}} → value
    public static string Replace(string template, string key, string value);
    
    // 単一のタグ付き置換（note なし）
    // block=true (デフォルト): {{key}} → <tagName>\nvalue\n</tagName>
    // block=false:             {{key}} → <tagName>value</tagName>
    // value が null または空文字の場合: プレースホルダーを含む行を削除
    public static string ReplaceWithTag(string template, string key, string value, string tagName, bool block = true);
    
    // 単一のタグ付き置換（note あり）
    // block=true (デフォルト): {{key}} → <tagName note="note">\nvalue\n</tagName>
    // block=false:             {{key}} → <tagName note="note">value</tagName>
    // value が null または空文字の場合: プレースホルダーを含む行を削除
    public static string ReplaceWithTag(string template, string key, string value, string tagName, string note, bool block = true);
    
    // 順序付き一括置換: ReplaceEntry リストを順番に処理
    public static string ReplaceAll(string template, List<ReplaceEntry> entries);
}
```

**使い方のガイドライン:**
- 複数の置換を行う場合は `ReplaceAll` を使用する
- `ReplaceEntry` の **リストの順序が置換の実行順序** になる。順序を意識する必要がある場合はリスト内での位置で制御する
- 単発の置換には `Replace` / `ReplaceWithTag` を使用する

### Data - データアクセス層

#### GameDataFormatter

ゲーム内データを人間/AI可読な文字列に変換する静的クラス。**変換（フォーマット）のみ**を担当。
```csharp
public static class GameDataFormatter
{
    public static string FormatTimePeriod(string timePeriod);
    public static string FormatLocation(string location);
    public static string FormatWeek(string week);
}
```

#### GameStateProvider

現在のゲーム状態を取得する静的クラス。
```csharp
public static class GameStateProvider
{
    public static string GetCurrentTimePeriod();
    public static string GetCurrentWeek();
    public static string GetCurrentLocation();
    public static string GetSchoolName();
}
```

#### CharacterCardProvider

CharacterCardデータの取得を担当する静的クラス。プレースホルダーは未解決の生データを返す。  
プロンプト構築時は `CharacterCardResolver` を経由すること。
```csharp
public static class CharacterCardProvider
{
    // PlayerのCharacterCard取得（プレースホルダー未解決）
    public static CharacterCard GetPlayerCharacterCard();
    
    // HeroineのCharacterCard取得（プレースホルダー未解決）
    public static CharacterCard GetHeroineCharacterCard(SaveData.Heroine heroine);
}
```

#### CharacterCardResolver

CharacterCard 内のプレースホルダーをゲームの現在状態に基づいて解決する静的クラス。  
**プロンプト構築時はこちらを使用する。**

```csharp
public static class CharacterCardResolver
{
    // プレイヤーカードを取得し、{{clothes}}/{{appearance}} を解決して返す
    // 衣装インデックスは Singleton<Game>.Instance.Player.changeClothesType から取得
    // -1（自動）は暫定的にインデックス 0 として扱う
    public static CharacterCard GetResolvedPlayerCard();
    
    // ヒロインカードを取得し、{{clothes}}/{{appearance}} を解決して返す
    // 衣装インデックスは heroine.NowCoordinate から取得
    public static CharacterCard GetResolvedHeroineCard(SaveData.Heroine heroine);
}
```

**設計の特徴:**
- 元の CharacterCard は変更しない（`RawJson` からクローンを生成して置換）
- `PromptReplacer.ReplaceAll` と `ReplaceEntry.Plain` を使用してフィールドごとに置換
- 置換対象フィールド: Name / Description / Personality / MessageExample / FirstMessage / Scenario
- `CoordinateData` が未設定の場合は空文字列に置換（既存動作を破壊しない）

**対応プレースホルダー:**

| プレースホルダー | 置換内容 |
|---|---|
| `{{clothes}}` | 現在の衣装プリセットに対応する服装説明文 |
| `{{appearance}}` | 現在の衣装プリセットに対応する容姿説明文 |

#### CoordinateData

衣装プリセット（Coordinate）ごとの服装・容姿説明文を保持するクラス。  
`CharacterCard` の `data.extensions` 内に JSON として格納される。

```csharp
public class CoordinateData
{
    public const int PresetCount = 7;
    
    public List<string> Appearance { get; set; }  // 要素数7、各プリセットの容姿説明文
    public List<string> Clothes { get; set; }     // 要素数7、各プリセットの服装説明文
    
    public string GetAppearance(int presetIndex);
    public string GetClothes(int presetIndex);
    public void SetAppearance(int presetIndex, string value);
    public void SetClothes(int presetIndex, string value);
    public void EnsurePresetCount();  // ロード後の要素数不足を補完
    public CoordinateData Clone();
}
```

**衣装プリセット番号:**

| インデックス | 衣装 |
|---|---|
| 0 | 学生服（校内） |
| 1 | 学生服（下校） |
| 2 | 体操着 |
| 3 | 水着 |
| 4 | 部活 |
| 5 | 私服 |
| 6 | お泊り |

### TalkSceneChat - セーブデータ

#### TalkSceneChatSaveData

TalkSceneChatモジュール専用のセーブデータ。`TalkSceneChatGameController` が管理します。

```csharp
public class TalkSceneChatSaveData
{
    [JsonProperty("custom_prompt_template")]
    public string CustomPromptTemplate { get; set; }

    [JsonProperty("heroine_settings_list")]
    public List<HeroineChatSettings> HeroineSettingsList { get; set; }

    public string GetContextNote(SaveData.Heroine heroine);
    public void SetContextNote(SaveData.Heroine heroine, string note);
    public void PrepareForSave();
    public void RestoreAfterLoad();
    public string ToJson();
    public static TalkSceneChatSaveData FromJson(string jsonString);
}
```

`CustomPromptTemplate` が `null` または空文字の場合、プロンプトテンプレートには `TalkSceneDefaultTemplate.GetTemplate()` が使用されます。

#### HeroineChatSettings

ヒロインごとの TalkSceneChat 設定データ。`TalkSceneChatSaveData` がリストで保持します。

```csharp
public class HeroineChatSettings
{
    [JsonProperty("heroine_index")]
    public int HeroineIndex { get; set; }

    [JsonProperty("context_note")]
    public string ContextNote { get; set; }  // {{context_note}} プレースホルダーに展開
    
    public bool IsEmpty();
}
```

### TalkSceneChat - ログフォーマット

#### TalkSceneLogFormatter

TalkSceneログのフォーマットを担当する静的クラス。UIとプロンプトビルダーの両方で使用。
```csharp
public static class TalkSceneLogFormatter
{
    // 過去ログと現在セッションログを統合してフォーマット
    // sessionManager は省略可能（過去ログのみ表示も可能）
    public static string FormatLogs(
        SaveData.Heroine heroine,
        TalkSceneSessionManager sessionManager = null);
}
```

### Data - ログシステム

#### MainGameLog（基底クラス）

メインゲーム内でのヒロインに関連するログの基底クラス。
```csharp
public abstract class MainGameLog
{
    [JsonProperty("elapsed_days")]
    public int ElapsedDays { get; set; }
    
    public virtual string GetLogTypeName();
    public abstract string FormatForPrompt(MainGameLogCollection collection, int index);
    public abstract bool IsValid();
    public abstract MainGameLog Clone();
    public virtual void IncrementElapsedDays();
}
```

**設計の特徴:**
- `LogType` enumは使用せず、型システムで管理
- 新しいログタイプは `MainGameLog` を継承するだけで動作
- `FormatForPrompt()` でコレクション全体を参照可能（文脈依存の省略ロジックに対応）

#### MainGameLogCollection

ログコレクションを管理するクラス。
```csharp
public class MainGameLogCollection
{
    public void AddLog(MainGameLog log);
    public bool RemoveLog(MainGameLog log);
    public void ClearLogs();
    public List<MainGameLog> GetAllLogs();
    
    public List<T> GetLogsByType<T>() where T : MainGameLog;
    public bool HasLogsOfType<T>() where T : MainGameLog;
    
    public void IncrementAllLogDays();
    public void EnforceLogLimit(int maxLogs);
    public string FormatForPrompt();
    public Dictionary<string, int> GetLogCountByType();
}
```

#### HeroineGameData

HeroineごとのCharacterCardとログを統合管理するクラス。コアセーブデータ（`AICharacterBridgeSaveData`）が管理します。
```csharp
public class HeroineGameData
{
    [JsonProperty("heroine_index")]
    public int HeroineIndex { get; set; }
    
    [JsonProperty("character_card_json")]
    public string CharacterCardJson { get; set; }
    
    [JsonProperty("logs", ItemTypeNameHandling = TypeNameHandling.Auto)]
    public List<MainGameLog> Logs { get; set; }
    
    public CharacterCard GetCharacterCard();
    public void SetCharacterCard(CharacterCard card);
    
    public MainGameLogCollection GetLogCollection();
    public void IncrementAllLogDays();
    public void EnforceLogLimit(int maxLogs);
}
```

### TalkSceneChat/Data

#### ConversationTurn

AIとの1回の通信における会話のやり取り（ターン）を表すクラス。
```csharp
public class ConversationTurn
{
    [JsonProperty("entries", ItemTypeNameHandling = TypeNameHandling.Auto)]
    public List<ConversationEntry> Entries { get; set; }
    
    public void AddEntry(ConversationEntry entry);
    public bool IsValid();
    public ConversationTurn Clone();
    public string FormatForDisplay();
}
```

#### TalkSceneLog

1回のTalkSceneでのやり取り全体を記録。
```csharp
public class TalkSceneLog : MainGameLog
{
    [JsonProperty("conversation_turns", ItemTypeNameHandling = TypeNameHandling.Auto)]
    public List<ConversationTurn> ConversationTurns { get; set; }
    
    [JsonProperty("week")]
    public string Week { get; set; }
    
    [JsonProperty("time_period")]
    public string TimePeriod { get; set; }
    
    [JsonProperty("location")]
    public string Location { get; set; }
    
    public void AddTurn(ConversationTurn turn);
    public int TurnCount { get; }
    public override string FormatForPrompt(MainGameLogCollection collection, int index);
    public string FormatAsCurrentConversation();
}
```

**データ構造:**
```
TalkSceneLog
└── ConversationTurns: List<ConversationTurn>
    ├── ConversationTurn #1（1回目の通信）
    │   └── Entries: List<ConversationEntry>
    │       ├── ChatEntry (user message)
    │       ├── ChatEntry (character dialogue)
    │       └── ChatEntry (character observation)
    ├── ConversationTurn #2（2回目の通信）
    │   └── Entries: List<ConversationEntry>
    │       ├── ChatEntry (user message)
    │       ├── ActionEntry
    │       └── ...
    └── ...
```

### TalkSceneChat/Response

#### DialogueSegment

AIの応答に含まれる会話の断片（セグメント）を表すクラス。

```csharp
public class DialogueSegment
{
    [JsonProperty("type")]
    public string Type { get; set; }         // "dialogue" または "observation"

    [JsonProperty("content")]
    public string Content { get; set; }      // セリフまたは描写のテキスト

    [JsonProperty("expression")]
    public string Expression { get; set; }  // 表情名（Available Expressions から選択）

    [JsonProperty("pose")]
    public string CharaMotion { get; set; } // ポーズ名（Available Poses から選択）
    
    public bool IsValid();
    public DialogueSegment Clone();
}
```

#### TalkSceneResponse

AIからの完全な応答を格納するクラス。`TalkSceneUI` が `FromJson()` でデシリアライズして使用します。

```csharp
public class TalkSceneResponse
{
    [JsonProperty("conversation_segments")]
    public List<DialogueSegment> ConversationSegments { get; set; }

    [JsonProperty("impression_on_user")]
    public string ImpressionOnUser { get; set; }

    [JsonProperty("is_aroused_by_conversation")]
    public string IsArousedByConversation { get; set; }  // "yes" または "no"

    [JsonProperty("post_conversation_action")]
    public string PostConversationAction { get; set; }
    
    public static TalkSceneResponse FromJson(string jsonString);
    public bool IsValid();
    public TalkSceneResponse Clone();
    public List<DialogueSegment> GetDialogues();
    public List<DialogueSegment> GetObservations();
}
```

**`FromJson()` の処理フロー:**

1. **JSONクリーニング（`CleanJsonResponse()`）**: AIの出力に Markdown コードブロック（`` ```json ``` `` または `` ``` ``` ``）が含まれる場合に除去し、`{` ～ `}` の範囲を抽出して純粋な JSON 文字列を得る。
2. **デシリアライズ**: `JsonConvert.DeserializeObject<TalkSceneResponse>()` でオブジェクトに変換。
3. **絵文字除去（`RemoveEmoji()`）**: 各セグメントの `content` から絵文字を除去する。対象範囲はBMP範囲の主要な絵文字・記号（`U+2300–U+23FF`、`U+2600–U+27BF`、`U+2B00–U+2BFF` 等）と補助面のサロゲートペア（`U+1F000` 以降）。
4. **バリデーション**: 以下の全項目を検証し、不足があれば例外をスローする。
   - `conversation_segments` が非空であること
   - `impression_on_user`、`is_aroused_by_conversation`、`post_conversation_action` が非空であること
   - 各セグメントの `type`、`content`、`expression`、`pose` が非空であること

---

## 新しいログタイプの追加方法

**既存コードの変更なしで**新しいログタイプを追加できます：
```csharp
// 1. MainGameLogを継承したクラスを作成（これだけ！）
public class DateLog : MainGameLog
{
    [JsonProperty("location")]
    public string Location { get; set; }
    
    [JsonProperty("date_result")]
    public string DateResult { get; set; }
    
    public override string FormatForPrompt(MainGameLogCollection collection, int index)
    {
        return $"[date at {Location}]\n{DateResult}";
    }
    
    public override bool IsValid() => !string.IsNullOrEmpty(Location);
    
    public override MainGameLog Clone()
    {
        return new DateLog { ElapsedDays = this.ElapsedDays, Location = this.Location, DateResult = this.DateResult };
    }
}

// 2. 使用（既存コードの変更不要）
saveData.AddLogForHeroine(heroine, new DateLog { Location = "Park", DateResult = "Success" });

// 3. 型別取得も可能
var dateLogs = logCollection.GetLogsByType<DateLog>();
```

---

## 新しいAIクライアントの追加方法

プロバイダーパターンにより、**プラグイン本体を変更せずに**新しいAIクライアントを追加できます。

### 手順

#### 1. ディレクトリ作成
```
Core/Communication/Clients/OpenAI/
├── OpenAIClient.cs
├── OpenAIResponseExtractor.cs
└── OpenAIClientProvider.cs
```

#### 2. インターフェースの実装

各ファイルで`ICommunicationClient`、`IResponseExtractor`、`IClientProvider`を実装します。

#### 3. ClientRegistryへの登録

`ClientRegistry.cs`の静的コンストラクタに1行追加するだけ：
```csharp
static ClientRegistry()
{
    RegisterProvider(new OllamaClientProvider());
    RegisterProvider(new LMStudioClientProvider());
    RegisterProvider(new OpenAIClientProvider());  // ← この1行を追加
}
```

**プラグイン本体（AICharacterBridgePlugin.cs）は0行変更**で新しいクライアントが使用可能になります。

---

## TalkSceneChatモジュールへの新しいアクション追加方法

責務分離により、アクション追加が明確な手順で行えます：
```csharp
// 1. TalkSceneActionFilter.cs の ALL_ACTIONS に追加
private static readonly List<string> ALL_ACTIONS = new List<string>
{
    // ...
    "new_action_name"  // ← 追加
};

// 2. TalkSceneActionFilter.cs の GetActionDisplayText に追加
public string GetActionDisplayText(string actionName)
{
    switch (actionName)
    {
        case "new_action_name": return "New Action Display Text";  // ← 追加
    }
}

// 3. TalkSceneActionExecutor.cs に実装追加
public bool ExecuteAction(string actionName, TalkScene talkScene)
{
    switch (actionName)
    {
        case "new_action_name": return ExecuteNewAction(talkScene);  // ← 追加
    }
}

private bool ExecuteNewAction(TalkScene talkScene) { /* 実装 */ }
```

---

## UI コンポーネント

### CharacterCardEditorUI ("K"キー)
WorldSetting、Player、全HeroineのCharacterCardを統合編集。

**機能:**
- 左パネル: World / Player / Heroine選択
- 右パネル: タブ切り替え編集
  - **Description / Name / Personality**: 基本フィールドの編集
  - **Coordinate**: 衣装プリセット（0〜6）ごとの Clothes / Appearance 説明文を編集
- JSON形式でのインポート/エクスポート（`spec` フィールドによる種別検証付き）
- TalkScene中は会話相手を優先表示

**Coordinate タブの動作:**
- プリセット番号（0〜6）とフィールド（Clothes / Appearance）を選択して編集
- 編集データは `CharacterCard` の `data.extensions` 内に `CoordinateData` として保存
- Apply ボタン押下時にセーブデータへ反映

### TalkSceneUI ("L"キー)
メインゲームのTalkScene中にAIとチャット。ImguiWindowベースの実装。

**機能:**
- **Chat タブ**: メッセージ入力、チャット実行、特殊アクション実行、Resend Last ボタン
- **Log タブ**: TalkScene履歴表示（ターン単位）、ログ削除機能
- **Context タブ**: ヒロインごとの context_note 編集（Apply Changes / Reset / Clear）
- **Prompt タブ**: プロンプトテンプレート編集、リセット機能

**好感度・親密度・興奮度への影響:**

`impression_on_user` の値に応じた好感度 (`favor`) 変化量（`Enable Favorability Update = true` のとき有効）：

| `impression_on_user` | `favor` 変化 | 備考 |
|---|---|---|
| `very_bad` | -4 | 最低値 0 |
| `bad` | -2 | 最低値 0 |
| `neutral` | 変化なし | |
| `good` | +4 | `favor >= 100 && isGirlfriend` の場合は代わりに `intimacy += 1`（最大値 100） |
| `very_good` | +6 | `favor >= 100 && isGirlfriend` の場合は代わりに `intimacy += 1`（最大値 100） |

`is_aroused_by_conversation` が `"yes"` のとき（`Enable Arousal Update = true` のとき有効）：
- `lewdness += 4`（最大値 100）

**アーキテクチャ:**
- ImguiWindowベースの実装（KKABMX_AdvancedGUIを参考）
- 純粋な表示層、制御ロジックはTalkSceneChatModuleに委譲
- `enabled`プロパティで表示制御

---

## データ管理

### データの保存場所

| データ | 保存場所 | 管理クラス |
|--------|----------|-----------|
| WorldSetting | ゲームセーブ (コア) | GameController |
| Player CharacterCard | ゲームセーブ (コア・RAW JSON) | GameController |
| Heroine CharacterCard | ゲームセーブ (コア・RAW JSON) | GameController |
| CoordinateData | CharacterCard の extensions 内 | CharacterCardEditorUI（書き込み）/ CharacterCardResolver（読み取りのみ） |
| MainGameLog | ゲームセーブ (コア) | GameController |
| CustomPromptTemplate | ゲームセーブ (TalkSceneChat スロット) | TalkSceneChatGameController |
| HeroineChatSettings (context_note等) | ゲームセーブ (TalkSceneChat スロット) | TalkSceneChatGameController |
| 一般設定 | BepInEx設定ファイル | AICharacterBridgePlugin |
| クライアント固有設定 | BepInEx設定ファイル | 各ClientProvider |
| モジュール設定 | BepInEx設定ファイル | 各モジュール |

### AICharacterBridgeSaveData（コアセーブデータ）

ゲームセーブに保存されるコアデータ。TalkSceneChat固有データは含まない。
```csharp
public class AICharacterBridgeSaveData
{
    [JsonProperty("version")]
    public string Version { get; set; }  // "2.0"
    
    [JsonProperty("player_character_card_json")]
    public string PlayerCharacterCardJson { get; set; }
    
    [JsonProperty("world_setting")]
    public WorldSetting WorldSetting { get; set; }
    
    [JsonProperty("heroine_data_list")]
    public List<HeroineGameData> HeroineDataList { get; set; }
    
    public CharacterCard GetPlayerCharacterCard();
    public void SetPlayerCharacterCard(CharacterCard card);
    public CharacterCard GetCharacterCardForHeroine(SaveData.Heroine heroine);
    public void SetCharacterCardForHeroine(SaveData.Heroine heroine, CharacterCard card);
    public MainGameLogCollection GetLogsForHeroine(SaveData.Heroine heroine);
    public void AddLogForHeroine(SaveData.Heroine heroine, MainGameLog log);
}
```

---

## 設定

### BepInEx Config
```ini
[General]
AI Client Type = Ollama
Language = English
Max Logs Per Heroine = 20

[Keyboard Shortcuts]
Toggle Character Card Editor UI = KeyCode.K

[Client - Ollama]
1. Model Name = 
2. Timeout (seconds) = 300
3. Think Option = Default
4. LLM Options = 

[Client - LM Studio]
1. Base URL = http://localhost:1234
2. Model Name =
3. Timeout (seconds) = 300
4. LLM Options = 

[TalkSceneChat]
Toggle UI Key = KeyCode.L
Enable Favorability Update = true
Enable Arousal Update = true
```

**LLM Options の記述形式:**
- JSON形式で記述します。オブジェクト全体を囲む `{}` は不要です。
- 例（Ollama）:
  ```
  "top_k": 40,
  "top_p": 0.9,
  "temperature": 0.8
  ```
- 例（LM Studio）:
  ```
  "temperature": 0.8,
  "top_p": 0.9,
  "max_output_tokens": 500
  ```

**Ollama の Think Option について:**
- `Default`: リクエストに `think` フィールドを含めません。
- `True`: リクエストのトップレベルに `"think": true` を追加します。
- `False`: リクエストのトップレベルに `"think": false` を追加します。
- QwQ や DeepSeek-R1 など、思考（Thinking）機能をサポートするモデルで使用します。
- `think` は `options` オブジェクト内ではなくリクエストのトップレベルフィールドとして付与されます。

**LM Studio の LLM Options に関する注意:**
- トークン上限は `max_tokens` ではなく `max_output_tokens` を使用してください。

---

## 使用方法

### 1. CharacterCard & World Info の設定
1. メインゲーム中に **"K"キー** を押す
2. 左側のリストから編集対象を選択 (World / Player / Heroine)
3. タブで情報を入力 (Description / Name / Personality)
4. **Apply** ボタンで保存、ゲームをセーブして永続化

### 2. 衣装プリセットごとの説明文設定
1. **"K"キー** → 対象の Player または Heroine を選択
2. **Coordinate** タブを開く
3. プリセット番号（0〜6）と項目（Clothes / Appearance）を選択して説明文を入力
4. **Apply** ボタンで保存
5. CharacterCard の各フィールドに `{{clothes}}` / `{{appearance}}` を記述しておくと、プロンプト生成時に自動置換される

### 3. AI とのチャット
1. TalkScene で **"L"キー** を押す（セッションは自動開始済み）
2. 必要に応じて **Context タブ** で context_note を入力し **Apply Changes** で確定する
3. **Chat タブ**でメッセージを入力し **Talk** ボタンをクリック
4. AIの応答が1ターンとして自動的に記録される
5. 特殊アクションが提案された場合、緑色のボタンをクリックして実行
6. TalkScene終了時、ログは自動保存され、UIは自動的に閉じる

### 4. ログの確認
1. TalkSceneUI の **Log タブ** を開く
2. 過去のTalkSceneログと現在のセッションログを確認
3. 必要に応じて **Delete All Logs** でログをクリア

### 5. JSON エクスポート/インポート
- **Save JSON**: 選択中の種別に応じた形式でエクスポート
  - World 選択中: `WorldSetting` 形式（`spec: "world_setting"`）でエクスポート
  - Player / Heroine 選択中: Character Card V2/V3形式でエクスポート
- **Load JSON**: `spec` フィールドで種別を検証してからインポート
  - World 選択中は `spec = "world_setting"` のファイルのみ受け付ける
  - Character 選択中は `spec = "chara_card_v2"` / `"chara_card_v3"` のファイルのみ受け付ける
  - 種別不一致の場合はロードを拒否し、編集中のデータは変更されない

---

## CharacterCard 規格対応

### サポートバージョン
- **Character Card V2**: 完全サポート
- **Character Card V3**: 完全サポート

### 新規作成時のデフォルト
- プラグインから新規作成する場合、**V2形式**で作成

### データの完全性保証
- 入力したJSONファイルの情報は、編集→保存→出力を経ても欠落しない
- プラグインで使用しないフィールド（`mes_example`、`first_mes`、`scenario` 等）も`RawJson`として完全保持
- V2とV3の相互変換は行わず、元のバージョンを維持

---

## 設計原則

1. **単一責任の原則**: 各クラスは明確な責任を持つ
2. **データの完全性**: CharacterCardのRAW JSON保持により情報欠落を防止
3. **拡張性**: 新しいログタイプやAIクライアントを既存コード変更なしで追加可能
4. **型安全性**: enumではなく型システムでログタイプを管理
5. **凝集性**: 関連データ（CharacterCardとログ）を統合管理
6. **再利用性**: データクラス自身がフォーマット機能を持つ
7. **保守性**: ドメインロジックと汎用処理の明確な分離
8. **最適化**: 低性能LLM向けの冗長性削減（情報の文脈依存省略）
9. **モジュール独立性**: 各機能をモジュールとして独立させ、追加・削除を容易に
10. **プロバイダーパターン**: AI通信層の完全な外部化と自己管理
11. **ゲーム非依存性**: Core部分はゲーム固有APIに依存せず、汎用性を維持
12. **責務分離**: 制御層（Module）と表示層（UI）の明確な分離
13. **データアクセスの階層化**: フォーマット・状態取得・データ取得・プレースホルダー解決を明確に分離
14. **自動ライフサイクル管理**: セッションをTalkSceneと完全連動させ、手動管理を排除
15. **Single Source of Truth**: データの真実の源泉を一箇所に集約
16. **構造化された会話管理**: ターン単位でデータを管理し、会話の流れを明確化
17. **ロジックの集約**: TalkSceneLogFormatterによるログフォーマットロジックの一元化
18. **非破壊的プレースホルダー解決**: CharacterCardResolverはクローン上で置換を行い、元データを保護
19. **順序制御を保証した置換**: ReplaceEntryリストのインデックス順で置換が実行されることを保証
20. **モジュール自己完結セーブ**: 各モジュールは専用の GameCustomFunctionController を持ち、コアセーブデータに依存しない
21. **テンプレートとBuilderの責務分離**: プロンプトテンプレートはプレースホルダーのみを記述し、タグ構造はBuilder側で付与する。テンプレートの記述をスッキリ保ちつつ、タグの変更をコード側で一元管理できる

---

**ドキュメントバージョン**: 34.0  
**対応プラグインバージョン**: AI Character Bridge v0.0.1  
**最終更新**: 2026年6月
