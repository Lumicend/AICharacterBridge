# AI Character Bridge - Project Overview

**Target Game**: Koikatsu
**Last Updated**: July 2026

---

## Project Overview

### Purpose

AI Character Bridge is a BepInEx plugin that **connects AI to in-game characters** in Unity-based games (specifically Illusion's Koikatsu), enabling natural, context-aware, and sophisticated dialogue.

### Key Features

- **Extensible AI communication via the provider pattern**: New AI clients (OpenAI, Claude, etc.) can be added easily
- **CharacterCard & WorldSetting system**: Supports the Character Card V2/V3 specifications, managing character personality data and world settings centrally within the game's save data
- **Outfit preset support**: Manages clothes/appearance description text for each of the game's 7 outfit presets alongside the CharacterCard, and automatically reflects it in the prompt
- **Integrated editor UI**: Allows editing of all CharacterCards and WorldSettings during the main game (including per-outfit-preset description editing)
- **Extensible log system**: New log types can be added without modifying existing code
- **Integrated data management**: Manages each heroine's CharacterCard and logs in a unified way
- **Turn-based conversation management**: Manages a single round of AI communication as one turn, structuring the flow of conversation
- **Automatic session management**: Automatically saves conversation logs in sync with the start/end of TalkScene
- **User-approved action execution**: Special actions proposed by the AI are only executed after user approval
- **General-purpose prompt construction system**: A highly extensible template variable replacement mechanism that unifies ordering control, plain replacement, and tagged replacement
- **Optimized log formatting**: Provides past logs in a format that's easy to understand even for lower-performance LLMs
- **Module-independent save mechanism**: Each module self-containedly saves its data to an independent slot in ExtensibleSaveFormat
- **Modular design**: Manages features as independent modules, making it easy to add or remove them
- **Robust AI response handling**: Extracts JSON object candidates via brace matching that accounts for string literals (validated from the end of the response backward), removes emoji, and validates required fields — absorbing stray braces that appear in descriptive text and variance in AI output

### Tech Stack

- **BepInEx**: Mod loader
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
├── Core/                              # Game-agnostic generic components
│   ├── Data/                          # Generic data models
│   │   ├── CharacterCard.cs          # CharacterCard base class, V2/V3 implementations
│   │   └── WorldSetting.cs
│   ├── Communication/                 # AI communication layer (provider pattern)
│   │   ├── Interfaces/               # Communication interfaces
│   │   │   ├── IClientProvider.cs    # Provider interface (external API)
│   │   │   ├── ICommunicationClient.cs # Communication client interface
│   │   │   └── IResponseExtractor.cs # Response extraction interface
│   │   ├── ClientRegistry.cs         # Client provider management
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
├── Data/                              # Data models / data access layer
│   ├── GameDataFormatter.cs          # Data conversion (formatting) only
│   ├── GameStateProvider.cs          # Retrieves current game state
│   ├── CharacterCardProvider.cs      # CharacterCard retrieval and initialization (raw data)
│   ├── CharacterCardResolver.cs      # Resolves CharacterCard placeholders
│   ├── CoordinateData.cs             # Clothes/appearance descriptions per outfit preset
│   ├── MainGameLog.cs                # Main game log base class
│   ├── MainGameLogCollection.cs      # Log collection management
│   ├── HeroineGameData.cs            # Integrated heroine data (Card + Logs)
│   ├── ExpressionData.cs             # Expression data
│   ├── CharaMotionData.cs            # Motion data
│   ├── ExpressionPresets.cs          # Expression presets
│   └── CharaMotionPresets.cs         # Motion presets
│
├── TalkSceneChat/                     # TalkSceneChat module
│   ├── TalkSceneChatModule.cs        # Module body (MonoBehaviour)
│   ├── TalkSceneChatGameController.cs # Module-specific save controller
│   ├── TalkSceneChatSaveData.cs      # Module-specific save data
│   ├── TalkSceneSessionManager.cs    # Conversation session management
│   ├── TalkSceneLogFormatter.cs      # Log formatting logic
│   ├── TalkSceneActionFilter.cs      # Determines available actions
│   ├── TalkSceneActionExecutor.cs    # Executes special actions
│   ├── TalkSceneEventExecutor.cs     # Builds/executes ADV events
│   ├── TalkScenePromptBuilder.cs     # Prompt construction
│   ├── Data/                          # TalkSceneChat-specific data
│   │   ├── TalkSceneLog.cs
│   │   ├── ConversationTurn.cs       # Conversation turn (unit of one communication)
│   │   ├── ConversationEntry.cs
│   │   ├── ConversationEntryType.cs
│   │   ├── ChatEntry.cs
│   │   ├── ActionEntry.cs
│   │   └── HeroineChatSettings.cs    # Heroine-specific chat settings (context_note, etc.)
│   ├── Response/                      # Response processing
│   │   ├── TalkSceneResponse.cs      # Parses/validates AI response (JSON candidate extraction method)
│   │   └── DialogueSegment.cs
│   ├── UI/                            # TalkSceneChat-specific UI
│   │   └── TalkSceneUI.cs
│   └── TalkSceneDefaultTemplate.cs   # Default prompt (tag structure applied by the Builder)
│
├── UI/                                # Shared UI components
│   └── CharacterCardEditorUI.cs
│
├── AICharacterBridgePlugin.cs
├── GameController.cs
└── AICharacterBridgeSaveData.cs
```

---

## Module Design

### Overview

The plugin manages features on a **per-module** basis. Each module is independent, allowing it to be added or removed with minimal impact on the plugin core.

### Module Structure

Each module has the following composition:
```
ModuleName/
├── ModuleNameModule.cs          # Module body (MonoBehaviour)
├── ModuleNameGameController.cs  # Module-specific save controller
├── ModuleNameSaveData.cs        # Module-specific save data
├── Data/                         # Module-specific data
├── UI/                           # Module-specific UI
└── Other required components
```

A module's save mechanism is handled by a dedicated controller that inherits from `GameCustomFunctionController`, saving data to an independent slot in ExtensibleSaveFormat keyed by `ModuleGUID` (`AICharacterBridgePlugin.GUID + ".modulename"`). This completely separates module-specific data from the core save data (`AICharacterBridgeSaveData`).

### TalkSceneChat Module

A module that provides AI dialogue functionality during conversation scenes. It implements automatic session management fully synchronized with the TalkScene lifecycle.

#### Architecture
```
TalkSceneChatGameController (save management / GameCustomFunctionController)
├── Independent ExtensibleSaveFormat slot (GUID: "...kk.talkscenechat")
├── Saves/restores TalkSceneChatSaveData
└── Exposes TalkSceneChatSaveData.CurrentSaveData

TalkSceneChatModule (control layer / MonoBehaviour)
├── Monitors TalkScene start/end (Update())
├── Automatic session start/end
├── UI open/close control (enabled property)
└── Provides reference access to SessionManager

TalkSceneSessionManager (session management)
├── Session state management (IsSessionActive)
├── Holding and manipulating ActiveSessionLog
├── Adding turns
└── Log saving logic

TalkSceneLogFormatter (log formatting)
├── Integrates past logs and current session log
├── Used by both UI and prompt builder
└── Single responsibility via static methods

TalkSceneUI (display layer / ImguiWindow)
├── Rendering only
├── Accepts user input
└── Accesses the session via the module

TalkScenePromptBuilder (prompt construction)
├── Retrieves cards with placeholders resolved via CharacterCardResolver
├── Retrieves WorldSetting (from core save data)
├── Formats logs using TalkSceneLogFormatter
├── Retrieves context_note (from TalkSceneChatGameController.CurrentSaveData)
├── Filters available actions
└── Builds the ReplaceEntry list and applies bulk replacement via PromptReplacer.ReplaceAll
    ├── Plain: for inline use within the template (including cases where the enclosing tag is written directly)
    ├── Tagged block: when the template only contains {{key}} and the Builder attaches the tag
    └── Tagged block + note: when the tag requires a note attribute

TalkSceneActionFilter (action determination)
├── Determines available actions based on game state
└── Generates action button text

TalkSceneActionExecutor (action execution)
├── Executes special actions
└── Modifies game state

TalkSceneEventExecutor (event execution)
├── Builds ADV events
└── Executes events
```

#### Session Management Flow
```
1. TalkScene starts (targetHeroine already set)
   ↓
2. Detected by TalkSceneChatModule
   → SessionManager.StartSession() automatically executed
   → ActiveSessionLog created
   ↓
3. User performs a chat exchange (UI open/close is optional)
   ↓
4. TalkSceneUI → creates ConversationTurn → SessionManager.AddTurn()
   → Accumulated into ActiveSessionLog
   ↓
5. TalkScene ends
   ↓
6. Detected by TalkSceneChatModule
   → SessionManager.EndSession() automatically executed
   → Log saved to core save data
   → UI automatically closes (enabled = false)
```

#### Key Components

**TalkSceneChatGameController.cs (GameCustomFunctionController)**
- Saves/restores TalkSceneChat-specific data to/from an independent ExtensibleSaveFormat slot
- Exposes `CurrentSaveData` (`TalkSceneChatSaveData`) as a global access point
- Uses `TalkSceneChatModule.ModuleGUID` (`"...kk.talkscenechat"`) as the slot key

**TalkSceneChatSaveData.cs**
- `CustomPromptTemplate`: Custom prompt template string (uses `TalkSceneDefaultTemplate.GetTemplate()` if unset)
- `HeroineSettingsList`: Per-heroine chat settings list (for serialization)
- `GetContextNote(heroine)` / `SetContextNote(heroine, note)`: Access to per-heroine `context_note`
- Follows the same `PrepareForSave` / `RestoreAfterLoad` pattern as `HeroineGameData` / `AICharacterBridgeSaveData`

**TalkSceneChatModule.cs (MonoBehaviour)**
- `ModuleGUID` constant (`AICharacterBridgePlugin.GUID + ".talkscenechat"`)
- Calls `GameAPI.RegisterExtraBehaviour<TalkSceneChatGameController>(ModuleGUID)` inside `Initialize()`
- Monitors TalkScene state and manages the session lifecycle via Update()
- When the UI is opened, calls `InitializePromptTemplate()` and `InitializeContextNote()` to reflect each initial value in the UI
  - `InitializePromptTemplate()`: Uses `TalkSceneChatSaveData.CustomPromptTemplate` if set, otherwise falls back to `TalkSceneDefaultTemplate.GetTemplate()`

**TalkSceneSessionManager.cs**
- Session state management (IsSessionActive)
- Manipulating ActiveSessionLog
- Adding and managing turns
- Automatic log saving (to core save data)

**TalkSceneLogFormatter.cs**
- Integrated formatting of past logs and the current session log
- Shared common use by both UI and prompt builder
- Achieves single responsibility via static methods

**TalkSceneUI.cs (ImguiWindow)**
- A pure display layer
- Reading/writing `CustomPromptTemplate` goes through `TalkSceneChatGameController.CurrentSaveData`
  - "Apply Changes" in the Prompt tab: if the text being edited matches `TalkSceneDefaultTemplate.GetTemplate()`, sets `CustomPromptTemplate` to `null` to revert to default; otherwise saves the text as `CustomPromptTemplate`
  - "Reset" in the Prompt tab: overwrites the text being edited with `TalkSceneDefaultTemplate.GetTemplate()` (reflected to save data only via "Apply Changes")
- Reading/writing `context_note` goes through `TalkSceneChatGameController.CurrentSaveData`
- Reading/writing logs goes through `GameController.CurrentSaveData` (logs are managed by core data)

**Configuration Items:**
- `Toggle UI Key`: UI display toggle key (default: "L" key)
- `Enable Favorability Update`: Whether conversation content affects the heroine's favorability (default: true)
- `Enable Arousal Update`: Whether conversation content affects the heroine's arousal level (default: true)

**Integration into the Plugin:**
```csharp
// AICharacterBridgePlugin.cs
private void InitializeModules()
{
    TalkSceneChatModule.Initialize(gameObject, Config);
    // Add other future modules here
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
    DateSystemModule.Initialize(gameObject, Config);  // ← added
}
---


## Communication Layer Design (Core/Communication)

### Adoption of the Provider Pattern

The communication layer adopts the **provider pattern**, where each AI client self-manages its configuration, initialization, and communication.

### Architecture Diagram
```
Plugin core
    ↓
ClientRegistry (registry)
    ↓
IClientProvider (external API)
    ↓ used internally
ICommunicationClient + IResponseExtractor
```

### Interface Design

#### IClientProvider (external API)

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
- Fully self-manages everything from configuration registration to communication
- No intermediate data structures like ClientOptions are needed
- The plugin side doesn't need to be aware of configuration details

#### ICommunicationClient (internal API)

Handles low-level communication.
```csharp
public interface ICommunicationClient
{
    string GetName();
    void Configure(string model, int timeoutSeconds, JObject llmOptions);
    IEnumerator Post(string prompt, Action<string> onSuccess, Action<Exception> onError);
}
```

#### IResponseExtractor (internal API)

Extracts the message from the AI response.
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
        // Add one line here when adding a new client
    }
    
    public static void RegisterAllConfigurationsTo(ConfigFile config);
    public static IEnumerator SendPrompt(string clientName, string prompt, ...);
}
```

### List of Implemented Clients

| Client | Endpoint | Prompt Key | Response Extraction Path |
|---|---|---|---|
| Ollama | `/api/generate` | `"prompt"` | `response` |
| LM Studio | `/v1/responses` | `"input"` | `output[0].content[0].text` |

#### Key Implementation Differences Between Ollama and LM Studio

| Comparison | Ollama | LM Studio |
|---|---|---|
| How LLM options are passed | Nested under `"options": { ... }` | Flattened as top-level fields |
| LLM option configuration format | JSON format (no enclosing braces). Written as-is as the contents of the Ollama API's `options` object | JSON format (no enclosing braces). Written as-is as the top-level fields of `/v1/responses` |
| Token limit parameter name | `max_tokens` | `max_output_tokens` |
| Model name | Required (cannot be empty) | Empty string allowed (automatically uses the currently loaded model) |
| Client instance creation | Held as a Provider field | Created on each `SendPrompt` call (since BaseUrl may change at runtime) |
| think option | Configurable. Selected via a "Default" / "True" / "False" dropdown. Added as a top-level field (set via `IClientProvider`'s dedicated `SetThinkOption()` method) | Not present |

---

## Data Models

### Core/Data

#### CharacterCard

Character or user personality data. Supports the Character Card V2/V3 specifications.
```csharp
public abstract class CharacterCard
{
    [JsonProperty("raw_json")]
    public string RawJson { get; set; }  // Holds the complete JSON

    [JsonIgnore]
    protected JObject ParsedData { get; set; }  // Parsed cache
    
    // Common field access
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

**Key design policies:**
- Fully retains the RAW JSON to prevent data loss
- Supports both V2 and V3
- Defaults to V2 when creating new cards
- Retains unused fields as well, so no information is lost through input/output
- Data inside `data.extensions` is managed hierarchically by namespace key; only the specified key is ever touched

**JSON structure of extensions:**
```json
{
  "extensions": {
    "ai_character_bridge_kk": {
      "coordinate_data": { ... }
    }
  }
}
```

**Constants related to extensions (AICharacterBridgePlugin.cs):**
```csharp
public const string ExtensionNamespace = "ai_character_bridge_kk";
public const string CoordinateDataKey  = "coordinate_data";
```

#### WorldSetting

The game's world setting data. Uses a `spec / spec_version / data` structure similar to Character Card V2.

**JSON structure:**
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
    // spec identifier constants (used to validate the file type)
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
    
    // Factory (FromJson validates the spec field and throws
    // NotSupportedException if it is not "world_setting")
    public static WorldSetting FromJson(string json);
    public static WorldSetting CreateNew();
}
```

**Design characteristics:**
- The `spec` field explicitly identifies the file type
- `FromJson()` validates `spec` before loading, preventing accidental loading of the wrong file
- Data access goes through `GetDescription()` / `SetDescription()`, protecting callers from internal structure changes
- Unlike `CharacterCard`, this is a format fully managed by the plugin, so RAW JSON is not retained

### Core/Prompt - Prompt Construction System

#### ReplaceEntry

A class representing a replacement entry for a prompt template. Supports both plain replacement and tagged replacement (inline or block).

```csharp
public class ReplaceEntry
{
    public string Key      { get; }  // Placeholder key (e.g., "user_name")
    public string Value    { get; }  // Value to replace with
    public bool   IsTagged { get; }  // Whether this is a tagged replacement
    public string TagName  { get; }  // Tag name (used only when IsTagged == true)
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

**Choosing a factory method:**

| Method | Resulting Entry |
|---|---|
| `Plain("key", value)` | Plain replacement. `{{key}}` → `value` |
| `Tagged("key", value, "tag")` | Tagged replacement (block format). `{{key}}` → `<tag>\nvalue\n</tag>` |
| `Tagged("key", value, "tag", block: false)` | Tagged replacement (inline format). `{{key}}` → `<tag>value</tag>` |
| `Tagged("key", value, "tag", "note")` | Tagged replacement (block format, with note attribute). `{{key}}` → `<tag note="note">\nvalue\n</tag>` |

**About the `block` parameter:**
When `block = true` (default), a newline is inserted between the opening and closing tags. Suitable for wrapping multi-line text. `block = false` produces an inline replacement, used when embedding a short value within an existing tag.

**Empty value handling for tagged replacement:**
If `value` is `null` or empty, the line containing `{{key}}` is removed entirely, including surrounding newlines. This ensures that optional information fields like `{{world_setting}}`, when not filled in, don't leave extra blank lines in the template.

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
    // If value is null or empty: the line containing the placeholder is removed
    public static string ReplaceWithTag(string template, string key, string value, string tagName, bool block = true);
    
    // Single tagged replacement (with note)
    // block=true (default): {{key}} → <tagName note="note">\nvalue\n</tagName>
    // block=false:          {{key}} → <tagName note="note">value</tagName>
    // If value is null or empty: the line containing the placeholder is removed
    public static string ReplaceWithTag(string template, string key, string value, string tagName, string note, bool block = true);
    
    // Ordered bulk replacement: processes a List<ReplaceEntry> in sequence
    public static string ReplaceAll(string template, List<ReplaceEntry> entries);
}
```

**Usage guidelines:**
- Use `ReplaceAll` for multiple replacements
- The **order of the `ReplaceEntry` list determines the order of replacement execution**. Control via list position when order matters
- Use `Replace` / `ReplaceWithTag` for one-off replacements

### Data - Data Access Layer

#### GameDataFormatter

A static class that converts in-game data into human/AI-readable strings. Handles **conversion (formatting) only**.
```csharp
public static class GameDataFormatter
{
    public static string FormatTimePeriod(string timePeriod);
    public static string FormatLocation(string location);
    public static string FormatWeek(string week);
}
```

#### GameStateProvider

A static class that retrieves the current game state.
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

A static class responsible for retrieving CharacterCard data. Placeholders remain unresolved raw data.
Use `CharacterCardResolver` when constructing prompts.
```csharp
public static class CharacterCardProvider
{
    // Get Player's CharacterCard (placeholders unresolved)
    public static CharacterCard GetPlayerCharacterCard();
    
    // Get Heroine's CharacterCard (placeholders unresolved)
    public static CharacterCard GetHeroineCharacterCard(SaveData.Heroine heroine);
}
```

#### CharacterCardResolver

A static class that resolves placeholders within a CharacterCard based on the current game state.
**Use this when constructing prompts.**

```csharp
public static class CharacterCardResolver
{
    // Gets the player card and resolves {{clothes}}/{{appearance}}
    // The outfit index is obtained from Singleton<Game>.Instance.Player.changeClothesType
    // -1 (auto) is temporarily treated as index 0
    public static CharacterCard GetResolvedPlayerCard();
    
    // Gets the heroine card and resolves {{clothes}}/{{appearance}}
    // The outfit index is obtained from heroine.NowCoordinate
    public static CharacterCard GetResolvedHeroineCard(SaveData.Heroine heroine);
}
```

**Design characteristics:**
- The original CharacterCard is never modified (a clone is generated from `RawJson` for replacement)
- Uses `PromptReplacer.ReplaceAll` and `ReplaceEntry.Plain` to replace each field
- Fields subject to replacement: Name / Description / Personality / MessageExample / FirstMessage / Scenario
- If `CoordinateData` is unset, replaces with an empty string (preserving existing behavior)

**Supported placeholders:**

| Placeholder | Replacement Content |
|---|---|
| `{{clothes}}` | Clothes description corresponding to the current outfit preset |
| `{{appearance}}` | Appearance description corresponding to the current outfit preset |

#### CoordinateData

A class holding clothes/appearance description text for each outfit preset (Coordinate).
Stored as JSON inside `CharacterCard`'s `data.extensions`.

```csharp
public class CoordinateData
{
    public const int PresetCount = 7;
    
    public List<string> Appearance { get; set; }  // 7 elements, appearance description per preset
    public List<string> Clothes { get; set; }     // 7 elements, clothes description per preset
    
    public string GetAppearance(int presetIndex);
    public string GetClothes(int presetIndex);
    public void SetAppearance(int presetIndex, string value);
    public void SetClothes(int presetIndex, string value);
    public void EnsurePresetCount();  // Fills missing entries after loading
    public CoordinateData Clone();
}
```

**Outfit preset numbers:**

| Index | Outfit |
|---|---|
| 0 | School uniform (on campus) |
| 1 | School uniform (after school) |
| 2 | Gym clothes |
| 3 | Swimsuit |
| 4 | Club activities |
| 5 | Casual wear |
| 6 | Sleepwear |

### TalkSceneChat - Save Data

#### TalkSceneChatSaveData

Save data dedicated to the TalkSceneChat module, managed by `TalkSceneChatGameController`.

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

Per-heroine TalkSceneChat settings data, held in a list by `TalkSceneChatSaveData`.

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

A static class responsible for formatting TalkScene logs. Used by both the UI and the prompt builder.
```csharp
public static class TalkSceneLogFormatter
{
    // Integrates past logs with the current session log into a formatted string
    // sessionManager is optional (can be used to display past logs only)
    public static string FormatLogs(
        SaveData.Heroine heroine,
        TalkSceneSessionManager sessionManager = null);
}
```

### Data - Log System

#### MainGameLog (base class)

Base class for logs related to a heroine within the main game.
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

**Design characteristics:**
- Does not use a `LogType` enum; managed via the type system instead
- New log types work simply by inheriting `MainGameLog`
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

A class that integrates the management of each heroine's CharacterCard and logs. Managed by the core save data (`AICharacterBridgeSaveData`).
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

A class representing a single conversation exchange (turn) within one round of AI communication.
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

Records an entire exchange from a single TalkScene.
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

**Data structure:**
```
TalkSceneLog
└── ConversationTurns: List<ConversationTurn>
    ├── ConversationTurn #1 (1st communication)
    │   └── Entries: List<ConversationEntry>
    │       ├── ChatEntry (user message)
    │       ├── ChatEntry (character dialogue)
    │       └── ChatEntry (character observation)
    ├── ConversationTurn #2 (2nd communication)
    │   └── Entries: List<ConversationEntry>
    │       ├── ChatEntry (user message)
    │       ├── ActionEntry
    │       └── ...
    └── ...
```

### TalkSceneChat/Response

#### DialogueSegment

A class representing a segment of conversation (dialogue or observation) contained in the AI's response.

```csharp
public class DialogueSegment
{
    [JsonProperty("type")]
    public string Type { get; set; }         // "dialogue" or "observation"

    [JsonProperty("content")]
    public string Content { get; set; }      // The dialogue or descriptive text

    [JsonProperty("expression")]
    public string Expression { get; set; }  // Expression name (chosen from Available Expressions)

    [JsonProperty("pose")]
    public string CharaMotion { get; set; } // Pose name (chosen from Available Poses)
    
    public bool IsValid();
    public DialogueSegment Clone();
}
```

#### TalkSceneResponse

A class that stores the complete response from the AI. Deserialized via `FromJson()` and used by `TalkSceneUI`.

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

**Processing flow of `FromJson()`:**

The AI's response may contain descriptive text in addition to the JSON body itself, and if a lone `{}`-like brace appears within that descriptive text, naive string searching can misidentify the JSON. For this reason, `FromJson()` adopts a two-step approach: "extract candidates → validate from the end."

1. **Extract JSON object candidates (`FindJsonObjectCandidates()`)**: Scans the entire response text and, using brace matching that excludes braces inside string literals (`"..."`, including escapes) from the count, identifies all candidates for "complete, top-level (non-nested) JSON objects." Stray `}` with no matching `{` are ignored.
2. **Validate from the last candidate (`ParseAndValidate()`)**: Deserializes the extracted candidates one at a time, starting from the end of the response, and validates the required fields. The first candidate to pass validation is adopted. If a candidate fails validation (missing required field, deserialization failure, etc.), it falls back to the previous candidate.
3. **Deserialization**: Each candidate is converted to an object via `JsonConvert.DeserializeObject<TalkSceneResponse>()`.
4. **Emoji removal (`RemoveEmoji()`)**: Removes emoji from each segment's `content`. Target ranges include major BMP emoji/symbol ranges (`U+2300–U+23FF`, `U+2600–U+27BF`, `U+2B00–U+2BFF`, etc.) and supplementary-plane surrogate pairs (`U+1F000` and above).
5. **Validation**: Validates all of the following, throwing an exception if anything is missing (this candidate is then rejected and the process falls back per step 2):
   - `conversation_segments` is non-empty
   - `impression_on_user`, `is_aroused_by_conversation`, and `post_conversation_action` are non-empty
   - Each segment's `type`, `content`, `expression`, and `pose` are non-empty

---

## How to Add a New Log Type

New log types can be added **without modifying existing code**:
```csharp
// 1. Create a class inheriting MainGameLog (that's it!)
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

// 2. Use it (no changes to existing code required)
saveData.AddLogForHeroine(heroine, new DateLog { Location = "Park", DateResult = "Success" });

// 3. Type-specific retrieval is also possible
var dateLogs = logCollection.GetLogsByType<DateLog>();
```

---

## How to Add a New AI Client

Thanks to the provider pattern, new AI clients can be added **without modifying the plugin core**.

### Steps

#### 1. Create a directory
```
Core/Communication/Clients/OpenAI/
├── OpenAIClient.cs
├── OpenAIResponseExtractor.cs
└── OpenAIClientProvider.cs
```

#### 2. Implement the interfaces

Implement `ICommunicationClient`, `IResponseExtractor`, and `IClientProvider` in each file.

#### 3. Register with ClientRegistry

Just add one line to the static constructor in `ClientRegistry.cs`:
```csharp
static ClientRegistry()
{
    RegisterProvider(new OllamaClientProvider());
    RegisterProvider(new LMStudioClientProvider());
    RegisterProvider(new OpenAIClientProvider());  // ← add this line
}
```

The new client becomes usable with **zero changes** to the plugin core (`AICharacterBridgePlugin.cs`).

---

## How to Add a New Action to the TalkSceneChat Module

Thanks to the separation of responsibilities, adding an action is a clear, well-defined process:
```csharp
// 1. Add to ALL_ACTIONS in TalkSceneActionFilter.cs
private static readonly List<string> ALL_ACTIONS = new List<string>
{
    // ...
    "new_action_name"  // ← add
};

// 2. Add to GetActionDisplayText in TalkSceneActionFilter.cs
public string GetActionDisplayText(string actionName)
{
    switch (actionName)
    {
        case "new_action_name": return "New Action Display Text";  // ← add
    }
}

// 3. Add the implementation in TalkSceneActionExecutor.cs
public bool ExecuteAction(string actionName, TalkScene talkScene)
{
    switch (actionName)
    {
        case "new_action_name": return ExecuteNewAction(talkScene);  // ← add
    }
}

private bool ExecuteNewAction(TalkScene talkScene) { /* implementation */ }
```

---

## UI Components

### CharacterCardEditorUI ("K" key)
Integrated editor for WorldSetting, Player, and all Heroines' CharacterCards.

**Features:**
- Left panel: Select World / Player / Heroine
- Right panel: Tabbed editing
  - **Description / Name / Personality**: Editing of basic fields
  - **Coordinate**: Edit Clothes / Appearance descriptions for each outfit preset (0–6)
- JSON import/export (with type validation via the `spec` field)
- Prioritizes the current conversation partner for display during TalkScene

**Behavior of the Coordinate tab:**
- Select a preset number (0–6) and a field (Clothes / Appearance) to edit
- Edited data is stored as `CoordinateData` inside `CharacterCard`'s `data.extensions`
- Reflected to save data when the Apply button is pressed

### TalkSceneUI ("L" key)
Chat with the AI during the main game's TalkScene. Implemented as an ImguiWindow-based UI.

**Features:**
- **Chat tab**: Message input, chat execution, special action execution, Resend Last button
- **Log tab**: Displays TalkScene history (per turn), log deletion feature
- **Context tab**: Per-heroine context_note editing (Apply Changes / Reset / Clear)
- **Prompt tab**: Prompt template editing, reset feature

**Effects on favorability, intimacy, and arousal:**

Change in favorability (`favor`) based on the value of `impression_on_user` (effective when `Enable Favorability Update = true`):

| `impression_on_user` | `favor` change | Notes |
|---|---|---|
| `very_bad` | -4 | Minimum 0 |
| `bad` | -2 | Minimum 0 |
| `neutral` | No change | |
| `good` | +4 | If `favor >= 100 && isGirlfriend`, `intimacy += 1` instead (max 100) |
| `very_good` | +6 | If `favor >= 100 && isGirlfriend`, `intimacy += 1` instead (max 100) |

When `is_aroused_by_conversation` is `"yes"` (effective when `Enable Arousal Update = true`):
- `lewdness += 4` (max 100)

**Architecture:**
- ImguiWindow-based implementation (referencing KKABMX_AdvancedGUI)
- A pure display layer; control logic is delegated to TalkSceneChatModule
- Display is controlled via the `enabled` property

---

## Data Management

### Data Storage Locations

| Data | Storage Location | Managing Class |
|--------|----------|-----------|
| WorldSetting | Game save (core) | GameController |
| Player CharacterCard | Game save (core, RAW JSON) | GameController |
| Heroine CharacterCard | Game save (core, RAW JSON) | GameController |
| CoordinateData | Inside CharacterCard's extensions | CharacterCardEditorUI (write) / CharacterCardResolver (read-only) |
| MainGameLog | Game save (core) | GameController |
| CustomPromptTemplate | Game save (TalkSceneChat slot) | TalkSceneChatGameController |
| HeroineChatSettings (context_note, etc.) | Game save (TalkSceneChat slot) | TalkSceneChatGameController |
| General settings | BepInEx config file | AICharacterBridgePlugin |
| Client-specific settings | BepInEx config file | Each ClientProvider |
| Module settings | BepInEx config file | Each module |

### AICharacterBridgeSaveData (core save data)

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

**Format for LLM Options:**
- Written in JSON format. The enclosing `{}` for the whole object is not required.
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
- `True`: Adds `"think": true` to the top level of the request.
- `False`: Adds `"think": false` to the top level of the request.
- Used with models that support thinking features, such as QwQ or DeepSeek-R1.
- `think` is added as a top-level request field, not inside the `options` object.

**Note on LM Studio's LLM Options:**
- Use `max_output_tokens` (not `max_tokens`) for the token limit.

---

## Usage

### 1. Configuring CharacterCard & World Info
1. Press the **"K" key** during the main game
2. Select the item to edit from the list on the left (World / Player / Heroine)
3. Enter information using the tabs (Description / Name / Personality)
4. Save with the **Apply** button, then save the game to persist it

### 2. Configuring per-outfit-preset descriptions
1. **"K" key** → select the target Player or Heroine
2. Open the **Coordinate** tab
3. Select a preset number (0–6) and field (Clothes / Appearance) and enter the description
4. Save with the **Apply** button
5. Writing `{{clothes}}` / `{{appearance}}` in each CharacterCard field will cause them to be automatically replaced when the prompt is generated

### 3. Chatting with the AI
1. Press the **"L" key** during a TalkScene (the session is already started automatically)
2. If needed, enter a context_note in the **Context tab** and confirm with **Apply Changes**
3. Type a message in the **Chat tab** and click the **Talk** button
4. The AI's response is automatically recorded as one turn
5. If a special action is proposed, click the green button to execute it
6. When the TalkScene ends, the log is automatically saved and the UI closes automatically

### 4. Checking logs
1. Open the **Log tab** in TalkSceneUI
2. Review past TalkScene logs and the current session log
3. Use **Delete All Logs** to clear logs if needed

### 5. JSON export/import
- **Save JSON**: Exports in a format matching the currently selected type
  - When World is selected: exports in `WorldSetting` format (`spec: "world_setting"`)
  - When Player / Heroine is selected: exports in Character Card V2/V3 format
- **Load JSON**: Validates the type via the `spec` field before importing
  - When World is selected, only files with `spec = "world_setting"` are accepted
  - When Character is selected, only files with `spec = "chara_card_v2"` / `"chara_card_v3"` are accepted
  - If the type doesn't match, the load is rejected and the data being edited remains unchanged

---

## CharacterCard Specification Support

### Supported Versions
- **Character Card V2**: Fully supported
- **Character Card V3**: Fully supported

### Default for New Creation
- When creating a new card from the plugin, it is created in **V2 format**

### Data Integrity Guarantee
- Information from an imported JSON file is never lost through the edit → save → export cycle
- Fields not used by the plugin (`mes_example`, `first_mes`, `scenario`, etc.) are also fully retained as `RawJson`
- No mutual conversion between V2 and V3 is performed; the original version is preserved

---

## Design Principles

1. **Single Responsibility Principle**: Each class has a clear responsibility
2. **Data integrity**: Retaining CharacterCard's RAW JSON prevents information loss
3. **Extensibility**: New log types or AI clients can be added without modifying existing code
4. **Type safety**: Log types are managed via the type system rather than an enum
5. **Cohesion**: Related data (CharacterCard and logs) is managed in an integrated manner
6. **Reusability**: Data classes themselves provide formatting functionality
7. **Maintainability**: Clear separation between domain logic and generic processing
8. **Optimization**: Reduced redundancy for lower-performance LLMs (context-dependent omission of information)
9. **Module independence**: Each feature is independent as a module, making addition/removal easy
10. **Provider pattern**: Full externalization and self-management of the AI communication layer
11. **Game independence**: The Core layer does not depend on game-specific APIs, maintaining generality
12. **Separation of responsibilities**: Clear separation between the control layer (Module) and the display layer (UI)
13. **Layered data access**: Clear separation of formatting, state retrieval, data retrieval, and placeholder resolution
14. **Automatic lifecycle management**: Sessions are fully synchronized with TalkScene, eliminating manual management
15. **Single Source of Truth**: The source of truth for data is consolidated in one place
16. **Structured conversation management**: Data is managed per turn, clarifying the flow of conversation
17. **Consolidated logic**: Log formatting logic is centralized in TalkSceneLogFormatter
18. **Non-destructive placeholder resolution**: CharacterCardResolver performs replacement on a clone, protecting the original data
19. **Order-guaranteed replacement**: Replacement is guaranteed to execute in the index order of the ReplaceEntry list
20. **Module self-contained saving**: Each module has its own dedicated GameCustomFunctionController and does not depend on the core save data
21. **Separation of template and Builder responsibilities**: The prompt template contains only placeholders; the tag structure is applied by the Builder side. This keeps the template's description clean while allowing tag changes to be managed centrally in code

---

**Document Version**: 35.0
**Corresponding Plugin Version**: AI Character Bridge v0.0.1
**Last Updated**: July 2026
