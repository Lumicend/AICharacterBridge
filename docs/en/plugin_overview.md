# AI Character Bridge - Project Overview

**Target Game**: Koikatsu (コイカツ)  
**Last Updated**: June 2026

---

## Project Overview

### Purpose

AI Character Bridge is a BepInEx plugin for Unity-based games (particularly Illusion's Koikatsu) that **connects AI to game characters**, enabling natural and context-aware conversations.

### Key Features

- **Extensible AI communication via Provider Pattern**: New AI clients (OpenAI, Claude, etc.) can be added with ease.
- **CharacterCard & WorldSetting System**: Supports Character Card V2/V3 spec; character personality data and world settings are managed centrally in the game save data.
- **Outfit Preset Support**: Clothing and appearance descriptions for each of the game's 7 outfit presets are stored alongside the CharacterCard and automatically reflected in prompts.
- **Integrated Editor UI**: All CharacterCards and WorldSettings can be edited during the main game, including per-outfit-preset description editing.
- **Extensible Log System**: New log types can be added without modifying existing code.
- **Unified Data Management**: CharacterCards and logs are managed together per heroine.
- **Turn-Based Conversation Management**: Each AI communication round is treated as a single "turn", structuring the flow of conversation.
- **Automatic Session Management**: Conversation logs are automatically saved in sync with TalkScene start/end.
- **User-Approved Action Execution**: Special actions suggested by the AI are only executed after user confirmation.
- **General-Purpose Prompt Construction System**: A highly extensible template variable replacement system integrating order control, plain replacements, and tagged replacements.
- **Optimized Log Formatting**: Past logs are presented in a format that is easy for lower-performance LLMs to understand.
- **Modular Independent Save Mechanism**: Each module saves its data self-containedly to an independent ExtensibleSaveFormat slot.
- **Modular Design**: Features are managed as independent modules, making them easy to add or remove.
- **Robust AI Response Handling**: Handles AI output variations by automatically stripping Markdown code blocks and emoji, and validating required fields.

### Technology Stack

- **BepInEx**: MOD loader
- **KKAPI**: Koikatsu API
- **ExtensibleSaveFormat**: Extended save data
- **Newtonsoft.Json**: JSON processing
- **UnityEngine**: Game engine
- **.NET Framework 3.5**: Target framework

---

## Architecture

### Directory Structure
```
AICharacterBridge/
├── Core/                              # Game-agnostic general-purpose components
│   ├── Data/                          # General data models
│   │   ├── CharacterCard.cs          # CharacterCard base class, V2/V3 implementations
│   │   └── WorldSetting.cs
│   ├── Communication/                 # AI communication layer (Provider Pattern)
│   │   ├── Interfaces/               # Communication interfaces
│   │   │   ├── IClientProvider.cs    # Provider interface (external API)
│   │   │   ├── ICommunicationClient.cs # Communication client interface
│   │   │   └── IResponseExtractor.cs # Response extraction interface
│   │   ├── ClientRegistry.cs         # Client provider registry
│   │   └── Clients/                  # Client implementations
│   │       ├── Ollama/               # Ollama client
│   │       │   ├── OllamaClient.cs
│   │       │   ├── OllamaResponseExtractor.cs
│   │       │   └── OllamaClientProvider.cs
│   │       └── LMStudio/             # LM Studio client
│   │           ├── LMStudioClient.cs
│   │           ├── LMStudioResponseExtractor.cs
│   │           └── LMStudioClientProvider.cs
│   ├── Prompt/                        # Prompt construction system
│   │   ├── PromptReplacer.cs          # Variable replacement engine
│   │   └── ReplaceEntry.cs           # Replacement entry data class
│   └── Utilities/
│       └── ConfigurationManagerAttributes.cs  # For BepInEx ConfigurationManager
│
├── Data/                              # Data models and data access layer
│   ├── GameDataFormatter.cs          # Data conversion (formatting) only
│   ├── GameStateProvider.cs          # Current game state retrieval
│   ├── CharacterCardProvider.cs      # CharacterCard retrieval and initialization (raw data)
│   ├── CharacterCardResolver.cs      # CharacterCard placeholder resolution
│   ├── CoordinateData.cs             # Per-outfit-preset clothing/appearance descriptions
│   ├── MainGameLog.cs                # Main game log base class
│   ├── MainGameLogCollection.cs      # Log collection management
│   ├── HeroineGameData.cs            # Integrated heroine data (Card + Logs)
│   ├── ExpressionData.cs             # Expression data
│   ├── CharaMotionData.cs            # Motion data
│   ├── ExpressionPresets.cs          # Expression presets
│   └── CharaMotionPresets.cs         # Motion presets
│
├── TalkSceneChat/                     # TalkSceneChat module
│   ├── TalkSceneChatModule.cs        # Module entry point (MonoBehaviour)
│   ├── TalkSceneChatGameController.cs # Module-dedicated save controller
│   ├── TalkSceneChatSaveData.cs      # Module-dedicated save data
│   ├── TalkSceneSessionManager.cs    # Conversation session management
│   ├── TalkSceneLogFormatter.cs      # Log formatting
│   ├── TalkSceneActionFilter.cs      # Available action determination
│   ├── TalkSceneActionExecutor.cs    # Special action execution
│   ├── TalkSceneEventExecutor.cs     # ADV event construction and execution
│   ├── TalkScenePromptBuilder.cs     # Prompt construction
│   ├── Data/                          # TalkSceneChat-specific data
│   │   ├── TalkSceneLog.cs
│   │   ├── ConversationTurn.cs       # Conversation turn (one communication unit)
│   │   ├── ConversationEntry.cs
│   │   ├── ConversationEntryType.cs
│   │   ├── ChatEntry.cs
│   │   ├── ActionEntry.cs
│   │   └── HeroineChatSettings.cs    # Per-heroine chat settings (context_note, etc.)
│   ├── Response/                      # Response processing
│   │   ├── TalkSceneResponse.cs
│   │   └── DialogueSegment.cs
│   ├── UI/                            # TalkSceneChat-specific UI
│   │   └── TalkSceneUI.cs
│   └── TalkSceneDefaultTemplate.cs   # Default prompt (tags are added by the Builder)
│
├── UI/                                # Common UI components
│   └── CharacterCardEditorUI.cs
│
├── AICharacterBridgePlugin.cs
├── GameController.cs
└── AICharacterBridgeSaveData.cs
```

---

## Modular Design

### Overview

The plugin manages features in **module units**. Each module is independent, and can be added or removed with minimal impact on the plugin core.

### Module Structure

Each module has the following structure:
```
ModuleName/
├── ModuleNameModule.cs          # Module entry point (MonoBehaviour)
├── ModuleNameGameController.cs  # Module-dedicated save controller
├── ModuleNameSaveData.cs        # Module-dedicated save data
├── Data/                         # Module-specific data
├── UI/                           # Module-specific UI
└── Other required components
```

Each module's save mechanism is handled by a dedicated controller inheriting from `GameCustomFunctionController`. Data is saved to an independent ExtensibleSaveFormat slot, identified by `ModuleGUID` (`AICharacterBridgePlugin.GUID + ".modulename"`). This ensures complete separation of module-specific data from the core save data (`AICharacterBridgeSaveData`).

### TalkSceneChat Module

A module that provides AI conversation functionality during talk scenes. Achieves fully automatic session management synchronized with the TalkScene lifecycle.

#### Architecture
```
TalkSceneChatGameController (save management · GameCustomFunctionController)
├── ExtensibleSaveFormat independent slot (GUID: "...kk.talkscenechat")
├── TalkSceneChatSaveData save/restore
└── Exposes TalkSceneChatSaveData.CurrentSaveData

TalkSceneChatModule (control layer · MonoBehaviour)
├── Monitors TalkScene start/end (Update())
├── Automatic session start/end
├── UI open/close control (enabled property)
└── Provides reference to SessionManager

TalkSceneSessionManager (session management)
├── Session state management (IsSessionActive)
├── ActiveSessionLog management
├── Turn addition
└── Log save processing

TalkSceneLogFormatter (log formatting)
├── Integrates past logs and current session log
├── Used by both UI and prompt builder
└── Single responsibility via static methods

TalkSceneUI (presentation layer · ImguiWindow)
├── UI rendering only
├── User input handling
└── Accesses session via module

TalkScenePromptBuilder (prompt construction)
├── Retrieves placeholder-resolved cards via CharacterCardResolver
├── WorldSetting retrieval (from core save data)
├── Log formatting using TalkSceneLogFormatter
├── context_note retrieval (from TalkSceneChatGameController.CurrentSaveData)
├── Available action filtering
└── Builds ReplaceEntry list and performs batch replacement via PromptReplacer.ReplaceAll
    ├── Plain: used inline within the template (including cases where enclosing tags are written directly)
    ├── Tagged block: template has only {{key}}, tags are added by the Builder
    └── Tagged block + note: when the tag requires a note attribute

TalkSceneActionFilter (action determination)
├── Determines available actions based on game state
└── Generates action button text

TalkSceneActionExecutor (action execution)
├── Executes special actions
└── Modifies game state

TalkSceneEventExecutor (event execution)
├── Constructs ADV events
└── Executes events
```

#### Session Management Flow
```
1. TalkScene starts (targetHeroine set)
   ↓
2. TalkSceneChatModule detects it
   → SessionManager.StartSession() runs automatically
   → ActiveSessionLog created
   ↓
3. User runs chat (UI open/close is optional)
   ↓
4. TalkSceneUI → creates ConversationTurn → SessionManager.AddTurn()
   → Accumulated in ActiveSessionLog
   ↓
5. TalkScene ends
   ↓
6. TalkSceneChatModule detects it
   → SessionManager.EndSession() runs automatically
   → Log saved to core save data
   → UI auto-closes (enabled = false)
```

#### Key Components

**TalkSceneChatGameController.cs (GameCustomFunctionController)**
- Saves/restores TalkSceneChat-specific data to an independent ExtensibleSaveFormat slot
- Exposes `CurrentSaveData` (`TalkSceneChatSaveData`) as a global access point
- Uses `TalkSceneChatModule.ModuleGUID` (`"...kk.talkscenechat"`) as the slot key

**TalkSceneChatSaveData.cs**
- `CustomPromptTemplate`: Custom prompt template string (uses `TalkSceneDefaultTemplate.GetTemplate()` when null or empty)
- `HeroineSettingsList`: List of per-heroine chat settings (for serialization)
- `GetContextNote(heroine)` / `SetContextNote(heroine, note)`: Per-heroine `context_note` access
- Follows the same `PrepareForSave` / `RestoreAfterLoad` pattern as `HeroineGameData` / `AICharacterBridgeSaveData`

**TalkSceneChatModule.cs (MonoBehaviour)**
- `ModuleGUID` constant (`AICharacterBridgePlugin.GUID + ".talkscenechat"`)
- Calls `GameAPI.RegisterExtraBehaviour<TalkSceneChatGameController>(ModuleGUID)` inside `Initialize()`
- Monitors TalkScene state in `Update()`, manages session lifecycle
- When UI is opened, calls `InitializePromptTemplate()` and `InitializeContextNote()` to set initial values in UI
  - `InitializePromptTemplate()`: Uses `TalkSceneChatSaveData.CustomPromptTemplate` if set; otherwise uses `TalkSceneDefaultTemplate.GetTemplate()`

**TalkSceneSessionManager.cs**
- Session state management (`IsSessionActive`)
- ActiveSessionLog manipulation
- Turn addition and management
- Auto-save of logs (to core save data)

**TalkSceneLogFormatter.cs**
- Integrated formatting of past logs and current session log
- Shared use by both UI and prompt builder
- Single responsibility via static methods

**TalkSceneUI.cs (ImguiWindow)**
- Pure presentation layer
- `CustomPromptTemplate` read/write via `TalkSceneChatGameController.CurrentSaveData`
  - Prompt tab "Apply Changes": sets `CustomPromptTemplate` to `null` (reverts to default) if the edited text matches `TalkSceneDefaultTemplate.GetTemplate()`; otherwise saves as `CustomPromptTemplate`
  - Prompt tab "Reset": overwrites the editing text with `TalkSceneDefaultTemplate.GetTemplate()` (save data is updated only on "Apply Changes")
- `context_note` read/write via `TalkSceneChatGameController.CurrentSaveData`
- Log read/write via `GameController.CurrentSaveData` (logs are under core data management)

**Configuration:**
- `Toggle UI Key`: Key to toggle UI visibility (default: "L")
- `Enable Favorability Update`: Whether to reflect conversation content in heroine's favorability (default: true)
- `Enable Arousal Update`: Whether to reflect conversation content in heroine's arousal level (default: true)

**Adding to the Plugin:**
```csharp
// AICharacterBridgePlugin.cs
private void InitializeModules()
{
    TalkSceneChatModule.Initialize(gameObject, Config);
    // Add future modules here
}
```

### How to Add a New Module

1. Create a module directory
2. Create `{ModuleName}Module.cs` (inheriting MonoBehaviour) and register the dedicated controller inside `Initialize()`
3. Create `{ModuleName}GameController.cs` (inheriting GameCustomFunctionController)
4. Create `{ModuleName}SaveData.cs`
5. Add one line to `AICharacterBridgePlugin.InitializeModules()`

Example:
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
    DateSystemModule.Initialize(gameObject, Config);  // ← Add this line
}
```

---

## Communication Layer (Core/Communication)

### Adopting the Provider Pattern

The communication layer uses the **Provider Pattern**, where each AI client self-manages its own configuration, initialization, and communication.

### Architecture Diagram
```
Plugin Core
    ↓
ClientRegistry (registry)
    ↓
IClientProvider (external API)
    ↓ used internally
ICommunicationClient + IResponseExtractor
```

### Interface Design

#### IClientProvider (External API)

The only interface used by the plugin.
```csharp
public interface IClientProvider
{
    string GetName();
    void RegisterConfiguration(ConfigFile config);
    IEnumerator SendPrompt(string prompt, Action<string> onSuccess, Action<Exception> onError);
}
```

**Characteristics:**
- Self-manages everything from configuration registration to communication
- No intermediate data structures like ClientOptions needed
- The plugin side does not need to know the details of each configuration

#### ICommunicationClient (Internal API)

Handles low-level communication.
```csharp
public interface ICommunicationClient
{
    string GetName();
    void Configure(string model, int timeoutSeconds, JObject llmOptions);
    IEnumerator Post(string prompt, Action<string> onSuccess, Action<Exception> onError);
}
```

#### IResponseExtractor (Internal API)

Extracts the message from an AI response.
```csharp
public interface IResponseExtractor
{
    string GetName();
    string ExtractMessage(string rawResponse);
}
```

### ClientRegistry

A static class that manages all client providers.
```csharp
public static class ClientRegistry
{
    static ClientRegistry()
    {
        RegisterProvider(new OllamaClientProvider());
        RegisterProvider(new LMStudioClientProvider());
        // Add one line here to register new clients
    }
    
    public static void RegisterAllConfigurationsTo(ConfigFile config);
    public static IEnumerator SendPrompt(string clientName, string prompt, ...);
}
```

### Implemented Clients

| Client Name | Endpoint | Prompt Key | Response Extraction Path |
|---|---|---|---|
| Ollama | `/api/generate` | `"prompt"` | `response` |
| LM Studio | `/v1/responses` | `"input"` | `output[0].content[0].text` |

#### Key Implementation Differences Between Ollama and LM Studio

| Comparison | Ollama | LM Studio |
|---|---|---|
| LLM option passing | Nested under `"options": { ... }` | Flattened as top-level fields |
| LLM options format | JSON without enclosing braces; written as the body of Ollama API's `options` object | JSON without enclosing braces; written as top-level fields of `/v1/responses` |
| Token limit parameter name | `max_tokens` | `max_output_tokens` |
| Model name | Required (cannot be empty) | Empty string allowed (auto-uses currently loaded model) |
| Client instance creation | Held as a Provider field | Created on each `SendPrompt` call (because BaseUrl may change at runtime) |
| Think option | Available. Dropdown with `"Default"` / `"True"` / `"False"`. Added as a top-level field via provider-specific method `SetThinkOption()` | Not available |

---

## Data Models

### Core/Data

#### CharacterCard

Personality data for a character or user. Supports Character Card V2/V3 spec.
```csharp
public abstract class CharacterCard
{
    [JsonProperty("raw_json")]
    public string RawJson { get; set; }  // Full JSON retained

    [JsonIgnore]
    protected JObject ParsedData { get; set; }  // Parsed cache

    // Common field accessors
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

    // extensions access (managed hierarchically by namespace key)
    public JToken GetExtensionValue(string namespaceKey, string key);
    public void SetExtensionValue(string namespaceKey, string key, JToken value);
    public void RemoveExtensionValue(string namespaceKey, string key);
    public bool HasExtensionValue(string namespaceKey, string key);

    // Factory methods
    public static CharacterCard FromJson(string json);
    public static CharacterCard CreateNew(string version = "v2");
}
```

**Key Design Policies:**
- Retains the full RAW JSON to prevent data loss
- Supports both V2 and V3
- Creates V2 by default when creating new cards
- Retains even unused fields so no information is lost during import/export
- Data inside `data.extensions` is managed hierarchically by namespace key; only the specified key is touched

**extensions JSON Structure:**
```json
{
  "extensions": {
    "ai_character_bridge_kk": {
      "coordinate_data": { ... }
    }
  }
}
```

**extensions-Related Constants (AICharacterBridgePlugin.cs):**
```csharp
public const string ExtensionNamespace = "ai_character_bridge_kk";
public const string CoordinateDataKey  = "coordinate_data";
```

#### WorldSetting

Game world setting data. Uses the same `spec / spec_version / data` structure as Character Card V2.

**JSON Structure:**
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
    // spec identifier constant (used for file type validation)
    public const string SpecIdentifier = "world_setting";
    public const string CurrentSpecVersion = "1.0";

    [JsonProperty("spec")]
    public string Spec { get; set; }        // Always "world_setting"

    [JsonProperty("spec_version")]
    public string SpecVersion { get; set; } // Currently "1.0"

    [JsonProperty("data")]
    public WorldSettingData Data { get; set; }

    // Data access
    public string GetDescription();
    public void SetDescription(string description);

    // Utilities
    public bool IsDefault();
    public WorldSetting Clone();
    public string FormatForPrompt();
    public string ToJson();

    // Factory (FromJson validates the spec field;
    // throws NotSupportedException if spec is not "world_setting")
    public static WorldSetting FromJson(string json);
    public static WorldSetting CreateNew();
}
```

**Design Characteristics:**
- The `spec` field explicitly identifies the file type
- `FromJson()` validates `spec` before loading, preventing loading of the wrong file type
- Data access is done through `GetDescription()` / `SetDescription()`, shielding callers from internal structural changes
- Unlike `CharacterCard`, the plugin fully controls this format, so RAW JSON retention is not performed

### Core/Prompt - Prompt Construction System

#### ReplaceEntry

A class representing a replacement entry in a prompt template. Supports both plain replacements and tagged replacements (inline/block).

```csharp
public class ReplaceEntry
{
    public string Key      { get; }  // Placeholder key (e.g., "user_name")
    public string Value    { get; }  // Replacement value
    public bool   IsTagged { get; }  // Whether this is a tagged replacement
    public string TagName  { get; }  // Tag name (used only when IsTagged is true)
    public string Note     { get; }  // Tag note attribute (optional)
    public bool   IsBlock  { get; }  // Whether to use block format (inserts newlines between tags)

    // Factory methods
    public static ReplaceEntry Plain(string key, string value);
    public static ReplaceEntry Tagged(string key, string value, string tagName, bool block = true);
    public static ReplaceEntry Tagged(string key, string value, string tagName, string note, bool block = true);

    // Utilities
    public static string FormatStringListAsJson(List<string> items);
}
```

**Factory Method Usage:**

| Method | Generated Entry |
|---|---|
| `Plain("key", value)` | Plain replacement. `{{key}}` → `value` |
| `Tagged("key", value, "tag")` | Tagged replacement (block format). `{{key}}` → `<tag>\nvalue\n</tag>` |
| `Tagged("key", value, "tag", block: false)` | Tagged replacement (inline format). `{{key}}` → `<tag>value</tag>` |
| `Tagged("key", value, "tag", "note")` | Tagged replacement (block format with note). `{{key}}` → `<tag note="note">\nvalue\n</tag>` |

**About the `block` Parameter:**
When `block = true` (default), newlines are inserted between the opening and closing tags. Suitable for enclosing multi-line text. `block = false` is for inline replacement, used when embedding a short value inside an existing tag.

**Empty Value Handling in Tagged Replacements:**
If `value` is `null` or empty, the entire line containing `{{key}}` is removed along with surrounding newlines. This ensures no extra blank lines remain in the template even when optional content like `{{world_setting}}` is not set.

#### PromptReplacer

A static class that performs variable replacement in prompt templates. Used by both `CharacterCardResolver` and `TalkScenePromptBuilder`.

```csharp
public static class PromptReplacer
{
    // Single plain replacement: {{key}} → value
    public static string Replace(string template, string key, string value);

    // Single tagged replacement (without note)
    // block=true (default): {{key}} → <tagName>\nvalue\n</tagName>
    // block=false:          {{key}} → <tagName>value</tagName>
    // If value is null or empty: removes the placeholder line
    public static string ReplaceWithTag(string template, string key, string value, string tagName, bool block = true);

    // Single tagged replacement (with note)
    // block=true (default): {{key}} → <tagName note="note">\nvalue\n</tagName>
    // block=false:          {{key}} → <tagName note="note">value</tagName>
    // If value is null or empty: removes the placeholder line
    public static string ReplaceWithTag(string template, string key, string value, string tagName, string note, bool block = true);

    // Ordered batch replacement: processes the ReplaceEntry list in order
    public static string ReplaceAll(string template, List<ReplaceEntry> entries);
}
```

**Usage Guidelines:**
- Use `ReplaceAll` when making multiple replacements
- The **order of the `ReplaceEntry` list is the replacement execution order**; use list position to control order when needed
- Use `Replace` / `ReplaceWithTag` for single replacements

### Data - Data Access Layer

#### GameDataFormatter

A static class that converts in-game data to human/AI-readable strings. Responsible **only for conversion (formatting)**.
```csharp
public static class GameDataFormatter
{
    public static string FormatTimePeriod(string timePeriod);
    public static string FormatLocation(string location);
    public static string FormatWeek(string week);
}
```

#### GameStateProvider

A static class for retrieving the current game state.
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

A static class responsible for retrieving CharacterCard data. Returns raw data with placeholders unresolved.  
Use `CharacterCardResolver` when building prompts.
```csharp
public static class CharacterCardProvider
{
    // Gets the Player's CharacterCard (placeholders unresolved)
    public static CharacterCard GetPlayerCharacterCard();

    // Gets the Heroine's CharacterCard (placeholders unresolved)
    public static CharacterCard GetHeroineCharacterCard(SaveData.Heroine heroine);
}
```

#### CharacterCardResolver

A static class that resolves placeholders in CharacterCards based on the current game state.  
**Use this when building prompts.**

```csharp
public static class CharacterCardResolver
{
    // Gets the player card and returns it with {{clothes}}/{{appearance}} resolved
    // Outfit index is obtained from Singleton<Game>.Instance.Player.changeClothesType
    // -1 (auto) is treated as index 0 for now
    public static CharacterCard GetResolvedPlayerCard();

    // Gets the heroine card and returns it with {{clothes}}/{{appearance}} resolved
    // Outfit index is obtained from heroine.NowCoordinate
    public static CharacterCard GetResolvedHeroineCard(SaveData.Heroine heroine);
}
```

**Design Characteristics:**
- Does not modify the original CharacterCard (generates a clone from `RawJson` and applies replacements)
- Uses `PromptReplacer.ReplaceAll` and `ReplaceEntry.Plain` to replace per field
- Replacement target fields: Name / Description / Personality / MessageExample / FirstMessage / Scenario
- If `CoordinateData` is not set, placeholders are replaced with empty strings (does not break existing behavior)

**Supported Placeholders:**

| Placeholder | Replacement Content |
|---|---|
| `{{clothes}}` | Clothing description for the current outfit preset |
| `{{appearance}}` | Appearance description for the current outfit preset |

#### CoordinateData

A class that holds clothing and appearance descriptions for each outfit preset (Coordinate).  
Stored as JSON inside `CharacterCard`'s `data.extensions`.

```csharp
public class CoordinateData
{
    public const int PresetCount = 7;

    public List<string> Appearance { get; set; }  // 7 elements, appearance description per preset
    public List<string> Clothes { get; set; }     // 7 elements, clothing description per preset

    public string GetAppearance(int presetIndex);
    public string GetClothes(int presetIndex);
    public void SetAppearance(int presetIndex, string value);
    public void SetClothes(int presetIndex, string value);
    public void EnsurePresetCount();  // Fills in missing elements after load
    public CoordinateData Clone();
}
```

**Outfit Preset Numbers:**

| Index | Outfit |
|---|---|
| 0 | School Uniform (on campus) |
| 1 | School Uniform (after school) |
| 2 | Gym Clothes |
| 3 | Swimsuit |
| 4 | Club Activities |
| 5 | Casual Wear |
| 6 | Sleepwear |

### TalkSceneChat - Save Data

#### TalkSceneChatSaveData

Module-dedicated save data for the TalkSceneChat module. Managed by `TalkSceneChatGameController`.

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

When `CustomPromptTemplate` is `null` or empty, `TalkSceneDefaultTemplate.GetTemplate()` is used as the prompt template.

#### HeroineChatSettings

Per-heroine TalkSceneChat settings data. Held as a list by `TalkSceneChatSaveData`.

```csharp
public class HeroineChatSettings
{
    [JsonProperty("heroine_index")]
    public int HeroineIndex { get; set; }

    [JsonProperty("context_note")]
    public string ContextNote { get; set; }  // Expanded into the {{context_note}} placeholder

    public bool IsEmpty();
}
```

### TalkSceneChat - Log Formatting

#### TalkSceneLogFormatter

A static class responsible for formatting TalkScene logs. Used by both UI and prompt builder.
```csharp
public static class TalkSceneLogFormatter
{
    // Integrates and formats past logs and current session log
    // sessionManager is optional (can display past logs only)
    public static string FormatLogs(
        SaveData.Heroine heroine,
        TalkSceneSessionManager sessionManager = null);
}
```

### Data - Log System

#### MainGameLog (Base Class)

Base class for logs related to a heroine in the main game.
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

**Design Characteristics:**
- Log types are managed by the type system rather than a `LogType` enum
- New log types work simply by inheriting from `MainGameLog`
- `FormatForPrompt()` can reference the entire collection (supports context-dependent omission logic)

#### MainGameLogCollection

A class that manages a collection of logs.
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

A class that manages CharacterCards and logs for each heroine in an integrated manner. Managed by the core save data (`AICharacterBridgeSaveData`).
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

A class representing a single conversation turn (one round of communication with AI).
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

Records the entire interaction of a single TalkScene.
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

**Data Structure:**
```
TalkSceneLog
└── ConversationTurns: List<ConversationTurn>
    ├── ConversationTurn #1 (1st communication round)
    │   └── Entries: List<ConversationEntry>
    │       ├── ChatEntry (user message)
    │       ├── ChatEntry (character dialogue)
    │       └── ChatEntry (character observation)
    ├── ConversationTurn #2 (2nd communication round)
    │   └── Entries: List<ConversationEntry>
    │       ├── ChatEntry (user message)
    │       ├── ActionEntry
    │       └── ...
    └── ...
```

### TalkSceneChat/Response

#### DialogueSegment

A class representing a segment of conversation in the AI's response.

```csharp
public class DialogueSegment
{
    [JsonProperty("type")]
    public string Type { get; set; }         // "dialogue" or "observation"

    [JsonProperty("content")]
    public string Content { get; set; }      // Dialogue or description text

    [JsonProperty("expression")]
    public string Expression { get; set; }  // Expression name (chosen from Available Expressions)

    [JsonProperty("pose")]
    public string CharaMotion { get; set; } // Pose name (chosen from Available Poses)

    public bool IsValid();
    public DialogueSegment Clone();
}
```

#### TalkSceneResponse

A class that stores the complete response from the AI. `TalkSceneUI` deserializes it using `FromJson()`.

```csharp
public class TalkSceneResponse
{
    [JsonProperty("conversation_segments")]
    public List<DialogueSegment> ConversationSegments { get; set; }

    [JsonProperty("impression_on_user")]
    public string ImpressionOnUser { get; set; }

    [JsonProperty("is_aroused_by_conversation")]
    public string IsArousedByConversation { get; set; }  // "yes" or "no"

    [JsonProperty("post_conversation_action")]
    public string PostConversationAction { get; set; }

    public static TalkSceneResponse FromJson(string jsonString);
    public bool IsValid();
    public TalkSceneResponse Clone();
    public List<DialogueSegment> GetDialogues();
    public List<DialogueSegment> GetObservations();
}
```

**`FromJson()` Processing Flow:**

1. **JSON Cleaning (`CleanJsonResponse()`)**: Strips Markdown code blocks (`` ```json ``` `` or `` ``` ``` ``) from AI output if present, and extracts the range from `{` to `}` to obtain a pure JSON string.
2. **Deserialization**: Converts to object via `JsonConvert.DeserializeObject<TalkSceneResponse>()`.
3. **Emoji Removal (`RemoveEmoji()`)**: Removes emoji from the `content` of each segment. Target ranges include major BMP emoji/symbol ranges (`U+2300–U+23FF`, `U+2600–U+27BF`, `U+2B00–U+2BFF`, etc.) and supplementary plane surrogate pairs (`U+1F000` and above).
4. **Validation**: Validates all of the following and throws an exception if any are missing:
   - `conversation_segments` is non-empty
   - `impression_on_user`, `is_aroused_by_conversation`, `post_conversation_action` are non-empty
   - Each segment's `type`, `content`, `expression`, `pose` are non-empty

---

## How to Add a New Log Type

New log types can be added **without modifying any existing code**:
```csharp
// 1. Create a class inheriting from MainGameLog (this is all you need!)
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

// 2. Usage (no changes to existing code required)
saveData.AddLogForHeroine(heroine, new DateLog { Location = "Park", DateResult = "Success" });

// 3. Type-specific retrieval is also available
var dateLogs = logCollection.GetLogsByType<DateLog>();
```

---

## How to Add a New AI Client

Thanks to the Provider Pattern, new AI clients can be added **without modifying the plugin core**.

### Steps

#### 1. Create Directory
```
Core/Communication/Clients/OpenAI/
├── OpenAIClient.cs
├── OpenAIResponseExtractor.cs
└── OpenAIClientProvider.cs
```

#### 2. Implement Interfaces

Implement `ICommunicationClient`, `IResponseExtractor`, and `IClientProvider` in each file.

#### 3. Register in ClientRegistry

Just add one line to the static constructor of `ClientRegistry.cs`:
```csharp
static ClientRegistry()
{
    RegisterProvider(new OllamaClientProvider());
    RegisterProvider(new LMStudioClientProvider());
    RegisterProvider(new OpenAIClientProvider());  // ← Add this one line
}
```

**The plugin core (`AICharacterBridgePlugin.cs`) requires 0 changes** to make the new client available.

---

## How to Add a New Action in the TalkSceneChat Module

Thanks to separation of concerns, adding new actions follows a clear procedure:
```csharp
// 1. Add to ALL_ACTIONS in TalkSceneActionFilter.cs
private static readonly List<string> ALL_ACTIONS = new List<string>
{
    // ...
    "new_action_name"  // ← Add here
};

// 2. Add to GetActionDisplayText in TalkSceneActionFilter.cs
public string GetActionDisplayText(string actionName)
{
    switch (actionName)
    {
        case "new_action_name": return "New Action Display Text";  // ← Add here
    }
}

// 3. Add implementation in TalkSceneActionExecutor.cs
public bool ExecuteAction(string actionName, TalkScene talkScene)
{
    switch (actionName)
    {
        case "new_action_name": return ExecuteNewAction(talkScene);  // ← Add here
    }
}

private bool ExecuteNewAction(TalkScene talkScene) { /* implementation */ }
```

---

## UI Components

### CharacterCardEditorUI (Key: "K")
Unified editing for WorldSetting, Player, and all Heroine CharacterCards.

**Features:**
- Left panel: Select World / Player / Heroine
- Right panel: Tab-based editing
  - **Description / Name / Personality**: Edit basic fields
  - **Coordinate**: Edit Clothes / Appearance descriptions for each outfit preset (0–6)
- JSON import/export with file type validation via the `spec` field
- During TalkScene, the conversation partner is shown at the top of the list

**Coordinate Tab Behavior:**
- Select preset number (0–6) and field (Clothes / Appearance) to edit
- Editing data is saved as `CoordinateData` inside `CharacterCard`'s `data.extensions`
- Reflected to save data on pressing the Apply button

### TalkSceneUI (Key: "L")
AI chat during the main game's TalkScene. Implemented as an ImguiWindow.

**Features:**
- **Chat Tab**: Message input, chat execution, special action execution, Resend Last button
- **Log Tab**: TalkScene history display (per turn), log deletion
- **Context Tab**: Per-heroine context_note editing (Apply Changes / Reset / Clear)
- **Prompt Tab**: Prompt template editing and reset

**Impact on Favorability, Intimacy, and Arousal:**

Favorability (`favor`) change based on `impression_on_user` value (active when `Enable Favorability Update = true`):

| `impression_on_user` | `favor` Change | Notes |
|---|---|---|
| `very_bad` | -4 | Minimum 0 |
| `bad` | -2 | Minimum 0 |
| `neutral` | No change | |
| `good` | +4 | If `favor >= 100 && isGirlfriend`, increases `intimacy += 1` instead (max 100) |
| `very_good` | +6 | If `favor >= 100 && isGirlfriend`, increases `intimacy += 1` instead (max 100) |

When `is_aroused_by_conversation` is `"yes"` (active when `Enable Arousal Update = true`):
- `lewdness += 4` (maximum 100)

**Architecture:**
- Implemented as an ImguiWindow
- Pure presentation layer; control logic is delegated to TalkSceneChatModule
- Visibility controlled by the `enabled` property

---

## Data Management

### Where Data Is Stored

| Data | Storage Location | Managing Class |
|--------|----------|-----------|
| WorldSetting | Game save (core) | GameController |
| Player CharacterCard | Game save (core · RAW JSON) | GameController |
| Heroine CharacterCard | Game save (core · RAW JSON) | GameController |
| CoordinateData | Inside CharacterCard extensions | CharacterCardEditorUI (write) / CharacterCardResolver (read-only) |
| MainGameLog | Game save (core) | GameController |
| CustomPromptTemplate | Game save (TalkSceneChat slot) | TalkSceneChatGameController |
| HeroineChatSettings (context_note, etc.) | Game save (TalkSceneChat slot) | TalkSceneChatGameController |
| General settings | BepInEx config file | AICharacterBridgePlugin |
| Client-specific settings | BepInEx config file | Each ClientProvider |
| Module settings | BepInEx config file | Each module |

### AICharacterBridgeSaveData (Core Save Data)

Core data saved in the game save. Does not include TalkSceneChat-specific data.
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

## Configuration

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

**LLM Options Format:**
- Written in JSON format. The enclosing `{}` are not required.
- Example (Ollama):
  ```
  "top_k": 40,
  "top_p": 0.9,
  "temperature": 0.8
  ```
- Example (LM Studio):
  ```
  "temperature": 0.8,
  "top_p": 0.9,
  "max_output_tokens": 500
  ```

**About Ollama's Think Option:**
- `Default`: Does not include the `think` field in the request.
- `True`: Adds `"think": true` as a top-level field in the request.
- `False`: Adds `"think": false` as a top-level field in the request.
- Used with models that support extended thinking, such as QwQ and DeepSeek-R1.
- `think` is added as a top-level request field, not inside the `options` object.

**Note on LM Studio LLM Options:**
- Use `max_output_tokens` (not `max_tokens`) for the token limit.

---

## How to Use

### 1. Setting Up CharacterCard & World Info
1. Press the **"K" key** during the main game
2. Select the target from the left panel (World / Player / Heroine)
3. Enter information using the tabs (Description / Name / Personality)
4. Press **Apply** to save, then save the game to persist

### 2. Setting Per-Outfit-Preset Descriptions
1. Press **"K"** → select the target Player or Heroine
2. Open the **Coordinate** tab
3. Select the preset number (0–6) and field (Clothes / Appearance) and enter descriptions
4. Press **Apply** to save
5. By writing `{{clothes}}` / `{{appearance}}` in CharacterCard fields, they will be automatically substituted during prompt generation

### 3. Chatting with AI
1. Press the **"L" key** during a TalkScene (the session starts automatically)
2. If needed, open the **Context tab**, enter a context_note, and press **Apply Changes** to confirm
3. In the **Chat tab**, type a message and click the **Talk** button
4. The AI's response is automatically recorded as a turn
5. If a special action is suggested, click the green button to execute it
6. When the TalkScene ends, logs are automatically saved and the UI closes automatically

### 4. Checking Logs
1. Open the **Log tab** of TalkSceneUI
2. View past TalkScene logs and the current session log
3. Use **Delete All Logs** to clear logs if needed

### 5. JSON Export/Import
- **Save JSON**: Exports in the format matching the current selection type
  - With World selected: exports in `WorldSetting` format (`spec: "world_setting"`)
  - With Player/Heroine selected: exports in Character Card V2/V3 format
- **Load JSON**: Validates file type via the `spec` field before importing
  - With World selected: only accepts files with `spec = "world_setting"`
  - With Character selected: only accepts files with `spec = "chara_card_v2"` / `"chara_card_v3"`
  - Load is rejected on type mismatch and the currently edited data is not changed

---

## CharacterCard Spec Support

### Supported Versions
- **Character Card V2**: Full support
- **Character Card V3**: Full support

### Default on New Creation
- When creating new cards from within the plugin, **V2 format** is used

### Data Integrity Guarantee
- Information in the input JSON file will not be lost through editing → saving → export
- Fields not used by the plugin (`mes_example`, `first_mes`, `scenario`, etc.) are fully retained as `RawJson`
- No conversion between V2 and V3 is performed; the original version is preserved

---

## Design Principles

1. **Single Responsibility Principle**: Each class has a clear, well-defined responsibility
2. **Data Integrity**: Retaining CharacterCard RAW JSON prevents information loss
3. **Extensibility**: New log types and AI clients can be added without modifying existing code
4. **Type Safety**: Log types are managed by the type system rather than an enum
5. **Cohesion**: Related data (CharacterCard and logs) are managed together
6. **Reusability**: Data classes themselves carry formatting functionality
7. **Maintainability**: Clear separation between domain logic and general-purpose processing
8. **Optimization**: Reduced redundancy for lower-performance LLMs (context-dependent log omission)
9. **Module Independence**: Each feature is an independent module; easy to add or remove
10. **Provider Pattern**: Full externalization and self-management of the AI communication layer
11. **Game Independence**: The Core layer does not depend on game-specific APIs, maintaining generality
12. **Separation of Concerns**: Clear separation between control layer (Module) and presentation layer (UI)
13. **Layered Data Access**: Clear separation of formatting, state retrieval, data retrieval, and placeholder resolution
14. **Automatic Lifecycle Management**: Sessions are fully synchronized with TalkScene, eliminating manual management
15. **Single Source of Truth**: Data truth is centralized in one place
16. **Structured Conversation Management**: Data is managed per turn, making the flow of conversation explicit
17. **Logic Consolidation**: Log formatting logic centralized via TalkSceneLogFormatter
18. **Non-Destructive Placeholder Resolution**: CharacterCardResolver performs replacement on a clone, protecting original data
19. **Guaranteed Ordered Replacement**: Replacement is guaranteed to execute in the index order of the ReplaceEntry list
20. **Self-Contained Module Save**: Each module has a dedicated GameCustomFunctionController and does not depend on core save data
21. **Template and Builder Responsibility Separation**: Prompt templates contain only placeholders; tag structures are added by the Builder. This keeps template descriptions clean while allowing tag changes to be managed centrally in code

---

**Document Version**: 34.0  
**Supported Plugin Version**: AI Character Bridge v0.0.1  
**Last Updated**: June 2026
