# AI Character Bridge — Start Guide

This guide walks you through everything from installing the plugin to successfully having an AI-driven conversation with an in-game character.

---

## Table of Contents

1. [What You'll Need](#1-what-youll-need)
2. [Installation](#2-installation)
3. [Testing the Setup](#3-testing-the-setup)
   - [3-1. Preparation (Before Launching the Game)](#3-1-preparation-before-launching-the-game)
   - [3-2. In-Game Setup](#3-2-in-game-setup)
   - [3-3. Conversation Test](#3-3-conversation-test)

---

## 1. What You'll Need

Please prepare all of the following.

| Name | Notes |
|------|------|
| **AICharacterBridge** | This plugin |
| **BepInEx** | Mod loader |
| **KKAPI** | API for Koikatsu |
| **ExtensibleSaveFormat** | Extended save data |
| **BepInEx.ConfigurationManager** | GUI-based plugin configuration |
| **Newtonsoft Json.NET for Unity3D** | Bundled with this plugin ([GitHub](https://github.com/SaladLab/Json.Net.Unity3D)) |
| **Local LLM tool** | This plugin supports [Ollama](https://ollama.com/), and optionally [LM Studio](https://lmstudio.ai/) |
| **Local LLM model** | Any model that runs with the above tool |

> **Note: About local LLMs**
> For instructions on installing and using local LLM tools, please refer to each tool's official documentation. After downloading a model, we recommend confirming it works (by chatting with it) using the tool's standard features before proceeding into the game.
> The steps below assume you are using **Ollama**.

---

## 2. Installation

### 2-1. Installing Dependency Plugins

Install the following plugins according to the installation instructions in each repository.

- [BepInEx](https://github.com/BepInEx/BepInEx)
- [KKAPI](https://github.com/IllusionMods/IllusionModdingAPI)
- [ExtensibleSaveFormat](https://github.com/IllusionMods/BepisPlugins) (included in BepisPlugins)
- [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager)

### 2-2. Installing AICharacterBridge

1. Download the latest ZIP file from the [releases page](https://github.com/Lumicend/AICharacterBridge/releases/latest).
2. Extract the ZIP and copy its contents directly into your Koikatsu game folder.

---

## 3. Testing the Setup

### 3-1. Preparation (Before Launching the Game)

#### Download Test Character Cards

This plugin manages character personality data in [Character Card V2](https://github.com/malfoyslastname/character-card-spec-v2) / [Character Card V3](https://github.com/kwaroran/character-card-spec-v3) JSON format.

For testing purposes, the following sample data is provided. Please download it in advance.

- [Sosuke_Kashiwagi.json](https://github.com/Lumicend/AICharacterBridge/blob/master/docs/SampleCards/Sosuke_Kashiwagi.json): Sample data for the player
- [Chika_Haruno.json](https://github.com/Lumicend/AICharacterBridge/blob/master/docs/SampleCards/Chika_Haruno.json): Sample data for a heroine

> **Note: What is a Character Card?**
> A Character Card is a JSON file describing a character's name, personality, speech patterns, background, and so on. It is managed independently of the in-game character, so the character's in-game name and personality are not affected. In this test, the registered heroine will be assigned the personality of "Chika Haruno" for AI purposes.

#### Note Down the Model Name You'll Use

Before launching the game, check the name of the model you'll be using with Ollama.

1. Press **Windows key + R**, type `cmd`, and open the Command Prompt.
2. Run the following command:
   ```
   ollama list
   ```
3. A list of downloaded models will be displayed. Copy the value in the **Name** column for the model you want to use.

> **Important**: Keep Ollama running while you play the game. Ollama must be running in the background when you press the Talk button.

---

### 3-2. In-Game Setup

#### Step 1: Configure the Plugin Options

1. Launch the game.
2. From the title screen or any scene, press **F1** (the default key for BepInEx.ConfigurationManager) to open the plugin configuration screen.
3. Confirm that "**AI Character Bridge**" appears in the list.
4. Expand the "**Client - Ollama**" section, and enter the model name you noted earlier into the **"1. Model Name"** field.
5. Close the configuration screen.

#### Step 2: Create Test Save Data

1. Start a new game.
   - The school name and male character settings can be left at their defaults.
   - **Register at least one heroine** (you'll be talking to her in a later step).
2. Once the opening event ends and you're free to act, go straight home.
   - It's recommended to save your game at this point so you can quickly restart later if needed.

#### Step 3: Configure the Character's Personality Data

Once you're back home, press **"K"** (default key setting) to open the Character Card & World Editor window.

The window is laid out as follows:

- **Left panel**: Selection of the editing target (World / Player / heroine roster)
- **Right panel**: Editing of the selected target's various data (Description / Name / Personality / Message Example / Coordinate)

---

**Setting the Player's Personality**

1. Click "**Player**" in the left panel.
2. Click the "**Load JSON**" button at the top of the right panel, and select the downloaded `Sosuke_Kashiwagi.json` to load it.
3. After confirming the data has been applied to each tab, click the "**Apply**" button to confirm.

> **Note**: If you select a different target or close the window **before** pressing "Apply," your changes will be discarded. Be sure to press Apply before proceeding.

---

**Setting the Heroine's Personality**

1. From the heroine roster in the left panel, click the heroine you registered in Step 2.
2. Click the "**Load JSON**" button and select `Chika_Haruno.json` to load it.
3. Click the "**Apply**" button to confirm.

---

This completes the setup. You can now close the Character Card & World Editor window.

> **Tip**: If you'd like to keep this setup for the next time you launch the game, save your game now. If you're only going to run the conversation test right away, saving is not necessary.

#### Step 4: Advance to the Next Day

Advance to the next day and head to school.

---

### 3-3. Conversation Test

#### Step 1: Talk to the Heroine

Once you're free to move around the school, go to where the heroine you set up is located and talk to her.

#### Step 2: End the Conversation Event

Talking to her will start the normal conversation event. Progress through it by selecting choices until you reach the state where the action menu is displayed (i.e., waiting for an action).

#### Step 3: Open the TalkScene UI

**By default, press "L"** to open the TalkScene UI window.

> **Note**: The window will not open when you press L unless you are currently in a TalkScene.
> Also, if the Player's or the heroine's CharacterCard has not been set, the window will still open, but **pressing the Talk button will do nothing**. If this happens, check that Apply was completed in Step 3 above.

The window has the following four tabs.

| Tab | Description |
|------|------|
| **Chat** | Enter and send messages |
| **Log** | View and delete conversation history |
| **Context** | Set per-heroine supplementary information (Context Note) |
| **Prompt** | View and edit the prompt template sent to the AI |

#### Step 4: Send a Message

1. Confirm that the "**Chat**" tab is selected.
2. Enter what you want to say in the text box.
   Example: `Hello, Chika!`
3. Press the "**Talk**" button to send the data to Ollama.

> **Points to note**
> - The "Talk" button is disabled while waiting for a response.
> - It's recommended not to operate the game while waiting for a response. You can still move around, but doing so may cause unstable behavior.
> - Response time varies significantly depending on your PC specs, the model in use, and Ollama's model loading status. The first request may take longer while the model is being loaded.

#### Step 5: Check the Response

If everything is working correctly, Chika's response will be displayed on screen as dialogue. Depending on the content of the conversation, the heroine's favorability parameter will also change slightly.

That's it — the basic operation test for AI Character Bridge is complete! 🎉

---

## Troubleshooting

| Symptom | What to Check |
|------|-------------|
| The window doesn't open when pressing L | Make sure you are currently in a TalkScene |
| Nothing happens when pressing Talk | ① Check that Ollama is running (i.e., `ollama serve` is active). ② Check that CharacterCards have been set and applied for both the Player and the heroine |
| No response is returned / it times out | Check that the model name is set correctly. Try running `ollama run <model name>` in the Command Prompt to confirm the model works |
| The response looks wrong / comes back in English | Check that "**General > Language**" in the plugin settings is set correctly (e.g., `Japanese`) |
