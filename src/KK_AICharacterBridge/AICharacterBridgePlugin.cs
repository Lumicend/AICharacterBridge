using AICharacterBridge.Core.Communication;
using AICharacterBridge.Core.Data;
using AICharacterBridge.Core.Utilities;
using AICharacterBridge.TalkSceneChat;
using AICharacterBridge.UI;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace AICharacterBridge
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class AICharacterBridgePlugin : BaseUnityPlugin
    {
        public const string GUID = "com.lumicend.aicharacterbridge.kk";
        public const string PluginName = "AI Character Bridge";
        public const string Version = "0.0.1";

        // ===================================================================
        // AI Character Card extensions の名前空間・キー定数
        // Namespace and key constants for AI Character Card extensions
        //
        // CharacterCard の data.extensions 内に以下の構造でデータを格納する。
        // Data is stored in CharacterCard's data.extensions with the following structure:
        //   "extensions": {
        //     "ai_character_bridge_kk": {        // ExtensionNamespace
        //       "coordinate_data": { ... }       // CoordinateDataKey
        //     }
        //   }
        // ===================================================================

        /// <summary>
        /// CharacterCard の extensions 内で使用する名前空間キー。
        /// Namespace key used within CharacterCard extensions.
        /// </summary>
        public const string ExtensionNamespace = "ai_character_bridge_kk";

        /// <summary>
        /// extensions の名前空間内で CoordinateData を格納するキー。
        /// Key for storing CoordinateData within the extensions namespace.
        /// </summary>
        public const string CoordinateDataKey = "coordinate_data";

        // ===================================================================

        public static AICharacterBridgePlugin Instance { get; private set; }
        public new ManualLogSource Logger => base.Logger;

        // UI Components
        private CharacterCardEditorUI _characterCardEditorUI;

        // Configuration
        public static ConfigEntry<string> ClientType { get; private set; }
        public static ConfigEntry<string> Language { get; private set; }
        public static ConfigEntry<int> MaxLogsPerHeroine { get; private set; }
        public static ConfigEntry<KeyboardShortcut> ToggleCharacterCardEditorUIKey { get; private set; }


        private void Awake()
        {
            Instance = this;

            InitializeConfiguration();
            InitializeNetworking();
            RegisterControllers();
            InitializeUI();
            InitializeModules();

        }

        private void Update()
        {
            // Character Card Editor UI のトグル処理
            // TalkScene UI のトグル処理は TalkSceneChatModule で実行
            if (ToggleCharacterCardEditorUIKey.Value.IsDown())
            {
                if (_characterCardEditorUI.enabled)
                {
                    CharacterCardEditorUI.Disable();
                }
                else
                {
                    // TalkScene中かチェック
                    var talkScene = UnityEngine.Object.FindObjectOfType<TalkScene>();
                    if (talkScene != null && talkScene.targetHeroine != null)
                    {
                        // 会話相手を優先表示
                        var priorityList = new List<SaveData.Heroine> { talkScene.targetHeroine };
                        CharacterCardEditorUI.Enable(priorityList);
                    }
                    else
                    {
                        // 通常表示
                        CharacterCardEditorUI.Enable();
                    }
                }
            }
        }

        #region Initialization

        private void InitializeConfiguration()
        {
            Language = Config.Bind("General", "Language", "English",
                "The language for the AI to respond in.");

            MaxLogsPerHeroine = Config.Bind("General", "Max Logs Per Heroine", 20,
                "Maximum number of conversation logs to keep per heroine. Set to 0 to disable logging.");

            ToggleCharacterCardEditorUIKey = Config.Bind("Keyboard Shortcuts", "Toggle Character Card Editor UI",
                new KeyboardShortcut(KeyCode.K),
                "Press this key to open/close the Character Card & World Editor UI.");

            // クライアント選択（動的に取得）
            var availableClients = ClientRegistry.GetAvailableClientNames();
            ClientType = Config.Bind("General", "AI Client Type", availableClients[0],
                new ConfigDescription(
                    "The type of AI client to use for communication.",
                    new AcceptableValueList<string>(availableClients)));

            // 各クライアントの設定を一括登録
            ClientRegistry.RegisterAllConfigurationsTo(Config);
        }

        private void InitializeNetworking()
        {
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to enable TLS 1.2: {ex.Message}");
            }
        }

        private void RegisterControllers()
        {
            GameAPI.RegisterExtraBehaviour<GameController>(GUID);
        }

        private void InitializeUI()
        {
            _characterCardEditorUI = gameObject.AddComponent<CharacterCardEditorUI>();
        }

        private void InitializeModules()
        {
            // TalkSceneChatモジュールの初期化
            // Initialize the TalkSceneChat module
            TalkSceneChatModule.Initialize(gameObject, Config);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 構築済みのプロンプトをAIに送信し、抽出されたメッセージを取得します。
        /// Sends a constructed prompt to the AI and retrieves the extracted message.
        /// </summary>
        /// <param name="prompt">送信するプロンプト文字列</param>
        /// <param name="onSuccess">成功時に呼び出されるコールバック（抽出されたメッセージを受け取る）</param>
        /// <param name="onError">エラー時に呼び出されるコールバック</param>
        public IEnumerator SendPromptToAI(
            string prompt,
            Action<string> onSuccess,
            Action<Exception> onError)
        {
            yield return ClientRegistry.SendPrompt(
                ClientType.Value,
                prompt,
                onSuccess,
                onError
            );
        }

        #endregion

    }
}
