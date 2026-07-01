# AI Character Bridge — User Manual

## About This Document

This manual provides detailed explanations of all features in AI Character Bridge.  
If you are setting up the plugin for the first time, please refer to the **Start Guide** first.

---

## Table of Contents

1. [Keyboard Shortcuts](#1-keyboard-shortcuts)
2. [Plugin Settings (F1 Key)](#2-plugin-settings-f1-key)
3. [Character Card & World Editor (K Key)](#3-character-card--world-editor-k-key)
4. [About Character Cards](#4-about-character-cards)
5. [About World Setting](#5-about-world-setting)
6. [TalkScene Chat (L Key)](#6-talkscene-chat-l-key)
7. [Conversation Logs](#7-conversation-logs)
8. [Prompt Templates](#8-prompt-templates)
9. [Appendix A: Available Expressions & Poses](#appendix-a-available-expressions--poses)
10. [Appendix B: Troubleshooting (Detailed)](#appendix-b-troubleshooting-detailed)

---

## 1. Keyboard Shortcuts

| Key | Function | Available In |
|-----|----------|-------------|
| **K** | Open/close Character Card & World Editor | Main game only |
| **L** | Open/close TalkScene Chat UI | During TalkScene (conversation scenes) only |
| **F1** | Open/close BepInEx plugin settings | Anytime |

> **Note**: The K and L keys can be changed in the plugin settings.

---

## 2. Plugin Settings (F1 Key)

Press the F1 key to open the plugin settings screen and expand the **"AI Character Bridge"** section to see the following settings.

### 2-1. General Section

| Setting | Default | Description |
|---------|---------|-------------|
| **AI Client Type** | Ollama | Select the AI client to use. Currently `Ollama` and `LM Studio` are available |
| **Language** | English | Specify the language for AI responses (e.g., `Japanese`, `English`) |
| **Max Logs Per Heroine** | 20 | Maximum number of conversation logs to save per heroine. Setting to `0` disables log saving |

### 2-2. Keyboard Shortcuts Section

| Setting | Default | Description |
|---------|---------|-------------|
| **Toggle Character Card Editor UI** | K | Key to open/close the Character Card & World Editor |

The key for TalkScene Chat UI is configured in the "TalkSceneChat" section described below.

### 2-3. Client - Ollama Section

| Setting | Default | Description |
|---------|---------|-------------|
| **1. Model Name** | (empty) | Ollama model name to use (e.g., `gemma3:12b`). **Must be configured** |
| **2. Timeout (seconds)** | 300 | Request timeout in seconds |
| **3. Think Option** | Default | Controls whether the `think` field is included in requests (see below) |
| **4. LLM Options** | (empty) | LLM generation options in JSON format (see below) |

**About Think Option**

Used with models that support extended thinking, such as QwQ or DeepSeek-R1.

| Value | Behavior |
|-------|----------|
| `Default` | Does not include the `think` field in requests |
| `True` | Adds `"think": true` to requests |
| `False` | Adds `"think": false` to requests (disables thinking to speed up responses) |

**LLM Options Format and Example**

The enclosing `{}` is **not required**. Write the fields of the Ollama API `options` object directly.

```
"temperature": 0.8,
"top_k": 40,
"top_p": 0.9
```

### 2-4. Client - LM Studio Section

| Setting | Default | Description |
|---------|---------|-------------|
| **1. Base URL** | `http://localhost:1234` | Base URL of the LM Studio server |
| **2. Model Name** | (empty) | Model name to use. If left empty, the model currently loaded in LM Studio will be used automatically |
| **3. Timeout (seconds)** | 300 | Request timeout in seconds |
| **4. LLM Options** | (empty) | LLM generation options in JSON format (see below) |

**LM Studio LLM Options Example**

```
"temperature": 0.8,
"top_p": 0.9,
"max_output_tokens": 500
```

> **Important**: For LM Studio, use **`max_output_tokens`** instead of `max_tokens` for the token limit.

### 2-5. TalkSceneChat Section

| Setting | Default | Description |
|---------|---------|-------------|
| **Toggle UI Key** | L | Key to open/close TalkScene Chat UI |
| **Enable Favorability Update** | true | Whether to reflect conversation content in the heroine's favorability (favor) |
| **Enable Arousal Update** | true | Whether to reflect conversation content in the heroine's arousal level (lewdness) |

---

## 3. Character Card & World Editor (K Key)

Press the **K key** (default) to open the window. **Only available during the main game (free roam).**

> **Note**: Pressing the K key during a TalkScene (conversation scene) will open the window with the heroine you are currently talking to shown at the top of the list.

### 3-1. Window Layout

The window is divided into two panels.

- **Left panel (Selector)**: Select the target to edit
  - `World`: Edit the World Setting
  - `Player`: Edit the player's Character Card
  - List of heroine names (in `#number - heroine name` format): Edit each heroine's Character Card

- **Right panel (Editor)**: Edit the data for the selected target

When an asterisk `*` appears in the window title bar, it indicates there are unsaved changes.

### 3-2. Right Panel — Editor Tabs

When **World** is selected, only the **"Description"** tab is shown.  
When **Player** or **Heroine** is selected, the following four tabs are available:

| Tab | Content |
|-----|---------|
| **Description** | Character description and background |
| **Name** | The character's name as recognized by the AI |
| **Personality** | Character personality traits |
| **Coordinate** | Clothes and appearance descriptions per outfit preset |

#### Description Tab

A field for writing basic character settings and descriptions. You can freely describe a character's background, physical features, relationships, and so on.  
When World is selected, write the game's world setting and background here (see "[5. About World Setting](#5-about-world-setting)" for details).

#### Name Tab

Set the character's name as it will be recognized by the AI. The name in this field is used as the character's name within the prompt.

> **Note**: If this field is left empty, the character's in-game name from Koikatsu will be used automatically.

#### Personality Tab

Describe the character's personality traits. Writing about their attitude, values, and speech patterns will make the AI's responses more consistent.

#### Coordinate Tab

You can set two types of descriptions — **Clothes** (clothing description) and **Appearance** (appearance description) — for each of the seven outfit presets (0–6).

| Preset Number | In-Game Outfit |
|---------------|---------------|
| 0 | School uniform (on campus) |
| 1 | School uniform (after school) |
| 2 | Gym clothes |
| 3 | Swimsuit |
| 4 | Club activity |
| 5 | Casual wear |
| 6 | Sleepwear |

**How to Use:**

1. Select a preset number using the top row of buttons (`0: School` through `3: Swimsuit`) or the bottom row (`4: Club Activity` through `6: Sleepwear`)
2. Select the field to edit using the `Clothes` / `Appearance` buttons
3. Enter the description in the text area
4. Switching to another preset or field will automatically save your current input temporarily
5. Pressing **Apply** reflects all preset and field data to the save data

The descriptions set here can be referenced in any Character Card text field using the `{{clothes}}` / `{{appearance}}` placeholders, which will be automatically substituted during prompt generation (see "[4-4. Placeholder Feature](#4-4-placeholder-feature)" for details).

### 3-3. File I/O (Load JSON / Save JSON)

**Load JSON:**  
You can import Character Card V2/V3 format JSON files or World Setting JSON files. After loading, the content in each tab will be updated, but **changes will not be reflected in save data until you press Apply.**

**Save JSON:**  
Exports the currently edited data as a JSON file. The file is saved with a timestamped filename in the following directory. After saving, Explorer will automatically open to show the save location.

| Target | Save Directory |
|--------|---------------|
| Player / Heroine Character Card | `(same folder as plugin DLL)/CharacterCards/` |
| World Setting | `(same folder as plugin DLL)/WorldSettings/` |

> **Note**: Save JSON for Character Cards outputs complete data including the outfit descriptions (CoordinateData) configured in the Coordinate tab.

### 3-4. Apply Button and Clear Button

| Button | Action |
|--------|--------|
| **Apply** | Reflects the right panel's edits to save data. **Changes will be lost if you don't press this** |
| **Clear** | Resets the right panel content to an empty default state (not reflected in save data until Apply is pressed) |

> **Important**:
> - Changes made before pressing Apply will not be reflected in save data even if you close the window or select a different target
> - There is no Undo function after pressing Clear
> - Even after reflecting to save data with Apply, **the settings will be lost when you quit the game unless you save the game itself**

---

## 4. About Character Cards

### 4-1. What is a Character Card?

A Character Card is data that conveys a character's personality, background, and speech patterns to the AI. It conforms to the **Character Card V2/V3** general-purpose standard, allowing you to import character cards created for other AI chat applications directly.

Set this for both the player character and heroines. The Talk button in TalkScene Chat will not function unless both are configured.

### 4-2. Supported Formats

| Version | Description |
|---------|-------------|
| **Character Card V2** | Standard format. Used when creating new cards in Character Card & World Editor |
| **Character Card V3** | Extended version of V2. Supports additional fields such as nicknames |

### 4-3. Field Roles

| Field | Tab in Character Card & World Editor | Effect on AI |
|-------|--------------------------------------|-------------|
| **name** | Name | Used as the character's name within the prompt |
| **description** | Description | Included in the prompt as character background and description |
| **personality** | Personality | Included in the prompt as personality traits |
| **mes_example** | — (not directly editable from UI) | Message examples |
| **first_mes** | — (not directly editable from UI) | First message |
| **scenario** | — (not directly editable from UI) | Scenario setting |

> **Note**: `mes_example`, `first_mes`, and `scenario` are stored without any data loss, but are not included in the default prompt template. When imported via Load JSON, they are preserved as-is.

### 4-4. Placeholder Feature

The following two placeholders can be written in any text field of a Character Card (Description, Personality, etc.).

| Placeholder | Substituted Content |
|-------------|---------------------|
| `{{clothes}}` | The Clothes description for the current outfit preset (set in the Coordinate tab) |
| `{{appearance}}` | The Appearance description for the current outfit preset (set in the Coordinate tab) |

These are automatically substituted with the description corresponding to the current in-game outfit preset number during prompt generation. If not set, they are replaced with an empty string.

**Example (in a Description field):**
```
[Appearance]
{{appearance}}

[Clothing]
{{clothes}}
```

If you set the Appearance for preset 0 (School) to "Black ponytail, brown eyes, slender build" and the Clothes to "School uniform with white blouse and navy skirt" in the Coordinate tab, those descriptions will be automatically inserted into the prompt whenever the character is at school.

### 4-5. Compatibility with `{{user}}` / `{{char}}` Placeholders

Character cards created for other tools often contain placeholders in the format `{{user}}` or `{{char}}`. This plugin automatically recognizes these and substitutes the player's name and the character's name respectively.

---

## 5. About World Setting

### 5-1. What is World Setting?

World Setting is data that conveys the game's world setting and shared background to the AI. It is applied to conversations with all heroines.

In Character Card & World Editor, select **"World"** and write the content in the Description tab. If the content is empty, the World Setting will be excluded from the prompt.

### 5-2. Example

```
The setting is the world of Koikatsu. A school romance story centered on the academy attended by the male protagonist.
Focuses on deepening everyday interactions with heroine classmates.
Based on general modern Japanese school culture and values. No flashy sci-fi or fantasy elements.
```

The more detail you provide, the better the AI can understand the game's context and generate appropriate responses. Feel free to write whatever suits your desired world setting.

---

## 6. TalkScene Chat (L Key)

Press the **L key** (default) to open the window. **Only available during a TalkScene (conversation scene).**

### 6-1. Automatic Session Management

TalkScene Chat sessions are automatically managed in full synchronization with the start and end of TalkScenes.

| Timing | Behavior |
|--------|----------|
| When TalkScene starts | A conversation session starts automatically |
| When TalkScene ends | The session ends automatically, the conversation log is saved, and the UI closes automatically |

The L key can be used to open and close the UI any number of times during a TalkScene. Closing the UI does not discard session data (conversation logs, etc.).

**Behavior when UI is opened:**

When the UI is newly opened, the following data is automatically loaded:

- **Prompt tab**: Saved custom template (or the default template if none has been saved)
- **Context tab**: Context Note saved for the current heroine

### 6-2. Chat Tab

The main tab for exchanging messages with the AI.

**How to Use:**

1. Type your message in the text box
2. Press the **Talk** button to send the request to the AI
3. When the AI's response arrives, it is displayed on screen as in-game dialogue

**Button Descriptions:**

| Button | Description |
|--------|-------------|
| **Talk** | Sends the entered message to the AI. Disabled when the message is empty |
| **Resend Last** | Restores the most recently sent message to the text box. Press Talk again to resend |
| **Special Action button (green)** | Becomes active when the AI proposes a special action. Click to execute the action (see "[6-6. Special Actions](#6-6-special-actions)" for details) |

**During AI Processing:**

While waiting for the AI's response, "AI is thinking..." is displayed and the Talk button is disabled. It is recommended to avoid operating the game while waiting for a response.

**What is automatically saved when pressing Talk:**

- The prompt template currently being edited in the Prompt tab
- The Context Note currently entered in the Context tab

These are saved when you press the Talk button even without explicitly pressing "Apply Changes."

### 6-3. Log Tab

View past conversation logs and the current session log.

**Button Descriptions:**

| Button | Description |
|--------|-------------|
| **Delete Last Turn** | Deletes the most recent turn (one exchange) from the current session |
| **Delete All Logs** | Deletes all past logs and the current session log for the current heroine |

> **Note**: Deleted content cannot be restored.

**Log Display Format:**

Past logs are grouped and displayed by how many days ago they occurred. The currently active session is shown at the bottom under "`--- Now ---`."

```
--- 2 days ago ---
[Afternoon Classes, conversation at school, Library, Wednesday]
Sosuke: "Want to study together in the library?"
Chika: "Sure! Let's do it after school today."

--- Now ---
[Lunch Time, conversation at school, Cafeteria, Friday]
(The conversation is just starting)
```

### 6-4. Context Tab (Context Note)

Set supplementary information about the current conversation (**Context Note**). What you write here is conveyed to the AI as a supplementary explanation of the current situation, enabling more contextually appropriate responses.

**Example:**

```
Today is the day before the school festival. Yesterday there was a small argument about the class event.
The player wants to repair the awkward atmosphere.
```

**Button Descriptions:**

| Button | Description |
|--------|-------------|
| **Apply Changes** | Saves the entered content to save data |
| **Reset** | Reloads the content saved in save data into the text box (discards unsaved input) |
| **Clear** | Clears the text box content (not reflected in save data until Apply Changes is pressed) |

Context Note content is saved **independently for each heroine**.

> **Note**: When you press the Talk button, the Context Note at that point is automatically saved. You do not need to press "Apply Changes" every time. However, if you close the UI and reopen it, the values are reloaded from save data. If you close the UI without pressing Apply, any unsaved input will be lost.

### 6-5. Prompt Tab

View and edit the prompt template sent to the AI.

**Button Descriptions:**

| Button | Description |
|--------|-------------|
| **Apply Changes** | Saves the edited template to save data |
| **Reset** | Restores the template to the default (built into the plugin) |

Each time you press the Talk button, the template at that point is automatically saved.

For information on customizing the prompt template, see "[8. Prompt Templates](#8-prompt-templates)."

### 6-6. Special Actions

The AI's responses include a proposal for a **"Post-Conversation Action"** based on the flow of the conversation. When this action is anything other than continuing the conversation (`continue_conversation`), the action button in the Chat tab turns **green** and can be clicked to execute it.

**List of Available Special Actions:**

| Display Text | Description | Condition |
|-------------|-------------|-----------|
| Have Lunch Together | Have lunch together | During lunch time and haven't had lunch together today |
| Join Club Activity Together | Do club activities together | During club activity time and heroine is a club member |
| Go Home Together | Go home together | During after school time |
| Study Together | Study together | — |
| Exercise Together | Exercise together | — |
| Accompany Player | Accompany the player | — |
| Make Date Reservation | Make a date arrangement | Heroine does not have a date reservation yet |
| Consent to H Scene | Proceed to H scene | — |

Actions that do not meet the conditions for the current game situation are excluded from the AI's options.

When an action is executed, it is also recorded in the conversation log.

> **Note**: Even when the action button turns green, you are free to choose when to click it. You can also ignore the proposal and continue the conversation.

### 6-7. Effects on Favorability, Intimacy, and Arousal

**Effect on Favorability (favor)** — Only active when `Enable Favorability Update` is `true`

The heroine's favorability changes based on the impression evaluation toward the user (`impression_on_user`) included in the AI's response.

| Impression | Change to favor | Notes |
|------------|----------------|-------|
| `very_bad` | −4 | Minimum value 0 |
| `bad` | −2 | Minimum value 0 |
| `neutral` | ±0 | |
| `good` | +4 | If favor ≥ 100 and girlfriend (`isGirlfriend`), intimacy +1 instead (max 100) |
| `very_good` | +6 | If favor ≥ 100 and girlfriend (`isGirlfriend`), intimacy +1 instead (max 100) |

**Effect on Arousal (lewdness)** — Only active when `Enable Arousal Update` is `true`

When the AI evaluation (`is_aroused_by_conversation`) is `yes`, the heroine's arousal (lewdness) increases by **+4** (maximum 100).

---

## 7. Conversation Logs

### 7-1. How Logs Work

Conversations conducted in TalkScene Chat are automatically saved as logs when the TalkScene ends. Logs are managed per heroine and stored in the game's save data (persisted when the game is saved).

Saved logs include the following information:

- The day of the week, time period, and location when the conversation took place
- User and character utterances (dialogue and observations)
- Special actions that were executed (if any)

### 7-2. Log Limit Setting

You can set the maximum number of saved logs with `Max Logs Per Heroine` (default: 20). When the limit is exceeded, the oldest logs are automatically deleted first.

Setting to `0` disables log saving.

### 7-3. How Logs Affect Prompts

Saved logs are automatically inserted as conversation history (`{{chat_log}}`) in the prompt for subsequent TalkScenes. This allows the AI to generate responses with memory of past conversations.

As logs accumulate, prompts become longer and the number of input tokens to the LLM increases. If you are using a model with a small context window or if performance is degrading, reduce the Max Logs Per Heroine value or delete unnecessary logs in the Log tab.

---

## 8. Prompt Templates

### 8-1. What is a Prompt Template?

A prompt template is a template for the prompt (instruction text) sent to the AI. Various data is written in the template in `{{placeholder_name}}` format and substituted with actual data at the time of sending.

You can view and edit the content in the **Prompt tab** of TalkScene Chat.

### 8-2. Placeholder List

| Placeholder | Substituted Content | Behavior When Empty |
|-------------|---------------------|---------------------|
| `{{language}}` | Response language (value from General > Language setting) | — |
| `{{world_setting}}` | World Setting description | Entire line is removed |
| `{{user_name}}` | Player's Character Card Name | — |
| `{{user_description}}` | Player's Character Card Description | Entire line is removed |
| `{{user_personality}}` | Player's Character Card Personality | Entire line is removed |
| `{{char_name}}` | Heroine's Character Card Name | — |
| `{{char_description}}` | Heroine's Character Card Description | Entire line is removed |
| `{{char_personality}}` | Heroine's Character Card Personality | Entire line is removed |
| `{{time_period}}` | Current in-game time period | — |
| `{{week}}` | Current in-game day of the week | — |
| `{{location}}` | Current in-game location | — |
| `{{school_name}}` | School name (retrieved from game save data) | — |
| `{{context_note}}` | Context Note content | Entire line is removed |
| `{{chat_log}}` | Conversation history (past logs + current session log) | — |
| `{{user_message}}` | Message entered by the user | — |
| `{{available_expressions}}` | List of available expression names (JSON array format) | — |
| `{{available_chara_motions}}` | List of available pose names (JSON array format) | — |
| `{{available_impressions}}` | List of available impression evaluation values (JSON array format) | — |
| `{{available_post_actions}}` | List of available post-conversation actions (JSON array format) | — |
| `{{user}}` | Compatibility alias for `{{user_name}}` | — |
| `{{char}}` | Compatibility alias for `{{char_name}}` | — |

### 8-3. Customizing Templates

You can freely edit the template in the Prompt tab. Use the placeholders listed above to customize the instructions to the AI.

Use the **Reset button** to restore the default template.

Custom templates are saved in the game's save data. They are saved by pressing "Apply Changes" or the Talk button.

> **Note**: Template customization is an advanced feature. The following placeholders are important for the AI to return responses in the correct JSON format. Be careful when deleting or modifying them.
> - `{{user_message}}`
> - `{{available_expressions}}`
> - `{{available_chara_motions}}`
> - `{{available_impressions}}`
> - `{{available_post_actions}}`

### 8-4. Using Context Note

The `{{context_note}}` placeholder is a mechanism for flexibly adding supplementary information about the current conversation. In the default template, it is placed within the `<current_context>` section, where it is conveyed to the AI alongside the game's time and location information.

When Context Note is empty, the `{{context_note}}` line is automatically removed from the prompt without leaving any extra blank lines.

---

## Appendix A: Available Expressions & Poses

The following lists show the expressions and poses available for AI selection with default settings.

### Expression List (Available Expressions)

| Expression Name | Description |
|-----------------|-------------|
| `normal` | Normal |
| `smile` | Smile |
| `big_smile` | Big smile |
| `gentle_smile` | Gentle smile |
| `sad` | Sad |
| `angry` | Angry |
| `surprised` | Surprised |
| `shy` | Shy / Embarrassed |
| `serious` | Serious |
| `thinking` | Thinking |
| `proud` | Proud / Confident |
| `troubled` | Troubled |
| `excited` | Excited / Nervous |
| `bored` | Bored |
| `eyes_closed` | Eyes closed |

### Pose List (Available Poses)

| Pose Name | Description |
|-----------|-------------|
| `standing_normal` | Normal standing pose |
| `standing_arms_crossed` | Standing with arms crossed |

---

## Appendix B: Troubleshooting (Detailed)

| Symptom | Check / Solution |
|---------|-----------------|
| **Pressing K key does not open the window** | Make sure you are in the main game. It cannot be used on the title screen or character creation screen |
| **Pressing L key does not open the window** | Make sure you are in a TalkScene (conversation scene). Try when the action menu is displayed |
| **Pressing Talk button does nothing** | ① Check that Ollama / LM Studio is running. ② Check that Character Cards have been set and Applied for both the player and the heroine |
| **Response doesn't come back / times out** | ① Check that the model name is configured correctly. ② Try increasing the Timeout setting (large models take more time to generate). ③ Try `ollama run <model name>` in the command prompt to check if the model is working |
| **Responses come back in English** | Change `General > Language` in the F1 settings |
| **Response format is incorrect / errors occur** | The model may lack sufficient instruction-following ability. Try a more capable model. Models with 7B or more parameters are recommended |
| **Heroine's favorability doesn't change** | Check that `TalkSceneChat > Enable Favorability Update` is set to `true` in the F1 settings |
| **Character Card settings disappeared** | Check that you are saving the game itself after pressing the Apply button. Settings will be lost if you quit the game without saving |
| **Context Note or Prompt changes are not saved** | Press "Apply Changes" or the Talk button before closing the UI |
| **Special action button is not shown** | It is not shown when the AI selects `continue_conversation`. Special actions may not be proposed depending on the flow of the conversation |
