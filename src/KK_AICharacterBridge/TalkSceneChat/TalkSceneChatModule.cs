using AICharacterBridge.Data;
using AICharacterBridge.TalkSceneChat.UI;
using BepInEx.Configuration;
using KKAPI.MainGame;
using Manager;
using UnityEngine;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// TalkSceneChatモジュールの初期化と管理を担当するMonoBehaviourクラス。
    /// MonoBehaviour-based class handling initialization and management for TalkSceneChat module.
    /// </summary>
    public class TalkSceneChatModule : MonoBehaviour
    {
        #region Singleton

        /// <summary>モジュールのシングルトンインスタンス / Module singleton instance</summary>
        public static TalkSceneChatModule Instance { get; private set; }

        #endregion

        #region Constants

        /// <summary>
        /// TalkSceneChatモジュール専用のGUID。
        /// ExtensibleSaveFormat の独立したスロットに使用します。
        /// GUID dedicated to the TalkSceneChat module.
        /// Used as an independent slot in ExtensibleSaveFormat.
        /// </summary>
        public const string ModuleGUID = AICharacterBridgePlugin.GUID + ".talkscenechat";

        #endregion

        #region Configuration

        /// <summary>TalkScene UI表示切り替えキー / TalkScene UI toggle key</summary>
        public static ConfigEntry<KeyboardShortcut> ToggleUIKey { get; private set; }

        /// <summary>
        /// 会話の内容がヒロインの好感度に影響を与えるかどうか。
        /// Whether conversation content affects the heroine's favorability.
        /// </summary>
        public static ConfigEntry<bool> EnableFavorabilityUpdate { get; private set; }

        /// <summary>
        /// 会話の内容がヒロインの性的興奮度に影響を与えるかどうか。
        /// Whether conversation content affects the heroine's arousal level.
        /// </summary>
        public static ConfigEntry<bool> EnableArousalUpdate { get; private set; }

        #endregion

        #region Private Fields

        /// <summary>現在のTalkSceneへの参照 / Reference to current TalkScene</summary>
        private TalkScene _currentTalkScene;

        /// <summary>セッションマネージャー / Session manager</summary>
        private TalkSceneSessionManager _sessionManager;

        #endregion

        #region Public API

        /// <summary>
        /// モジュールを初期化します。
        /// このメソッドはAICharacterBridgePlugin.InitializeModules()から呼び出されます。
        /// Initializes the TalkSceneChat module.
        /// This method is called from AICharacterBridgePlugin.InitializeModules().
        /// </summary>
        /// <param name="gameObject">モジュールコンポーネントを追加するGameObject / GameObject to attach module components</param>
        /// <param name="config">BepInEx設定ファイル / BepInEx config file</param>
        public static void Initialize(
            GameObject gameObject,
            ConfigFile config)
        {
            if (Instance != null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneChatModule] Module already initialized. Skipping.");
                return;
            }

            // TalkSceneChat専用のゲームコントローラーを登録
            // Register the dedicated game controller for TalkSceneChat
            GameAPI.RegisterExtraBehaviour<TalkSceneChatGameController>(ModuleGUID);

            // モジュールコンポーネントを追加
            // Add module components
            var module = gameObject.AddComponent<TalkSceneChatModule>();

            // TalkSceneUIコンポーネントを追加
            // Add TalkSceneUI component
            gameObject.AddComponent<TalkSceneUI>();

            // 設定の登録
            // Register configuration
            RegisterConfiguration(config);

            AICharacterBridgePlugin.Instance?.Logger.LogInfo("TalkSceneChat module initialized.");
        }

        /// <summary>
        /// セッションマネージャーへの参照を取得します。
        /// Gets a reference to the session manager.
        /// </summary>
        public TalkSceneSessionManager GetSessionManager()
        {
            return _sessionManager;
        }

        /// <summary>
        /// 現在のTalkSceneへの参照を取得します。
        /// Gets a reference to the current TalkScene.
        /// </summary>
        public TalkScene GetCurrentTalkScene()
        {
            return _currentTalkScene;
        }

        /// <summary>
        /// 現在のHeroineへの参照を取得します。
        /// Gets a reference to the current Heroine.
        /// </summary>
        public SaveData.Heroine GetCurrentHeroine()
        {
            return _currentTalkScene?.targetHeroine;
        }

        /// <summary>
        /// 現在のセッションから最後の会話ターンを削除します。
        /// Removes the last conversation turn from the current session.
        /// </summary>
        /// <returns>削除に成功した場合 true / True if successfully removed</returns>
        public bool RemoveLastTurn()
        {
            if (_sessionManager == null || !_sessionManager.IsSessionActive)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneChatModule] Cannot remove last turn: No active session.");
                return false;
            }

            return _sessionManager.RemoveLastTurn();
        }

        /// <summary>
        /// 現在のセッションをクリアします（UIから呼び出し用）。
        /// Clears the current session log (for UI calls).
        /// </summary>
        public void ClearCurrentSessionLog()
        {
            _sessionManager?.ClearActiveSessionLog();
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // シングルトンの初期化
            // Initialize singleton
            if (Instance != null && Instance != this)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneChatModule] Multiple instances detected. Destroying duplicate.");
                Destroy(this);
                return;
            }

            Instance = this;

            // セッションマネージャーの初期化
            // Initialize session manager
            _sessionManager = new TalkSceneSessionManager();
        }

        private void Update()
        {
            HandleUIToggle();
            CheckTalkSceneStatus();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// UIトグルキーの入力を処理します。
        /// Handles UI toggle key input.
        /// </summary>
        private void HandleUIToggle()
        {
            if (ToggleUIKey.Value.IsDown())
            {
                ToggleTalkSceneUI();
            }
        }

        /// <summary>
        /// TalkSceneの状態をチェックし、開始/終了時の処理を行います。
        /// Checks TalkScene status and handles session start/end.
        /// </summary>
        private void CheckTalkSceneStatus()
        {
            var talkScene = UnityEngine.Object.FindObjectOfType<TalkScene>();

            // TalkSceneが新しく開始された場合
            // TalkScene newly started
            if (_sessionManager.IsSessionActive == false && talkScene != null && talkScene.targetHeroine != null)
            {
                OnTalkSceneStart(talkScene);
                _currentTalkScene = talkScene;
            }
            // TalkSceneが終了した場合
            // TalkScene ended
            else if (_sessionManager.IsSessionActive == true && talkScene == null)
            {
                OnTalkSceneEnd();
                _currentTalkScene = null;
            }
        }

        #endregion

        #region TalkScene Management

        /// <summary>
        /// TalkScene開始時の処理を行います。
        /// Handles initialization when TalkScene starts.
        /// </summary>
        private void OnTalkSceneStart(TalkScene talkScene)
        {
            if (talkScene == null || talkScene.targetHeroine == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneChatModule] TalkScene started but target heroine is null.");
                return;
            }

            ResetUISessionState();

            if (_sessionManager != null && !_sessionManager.IsSessionActive)
            {
                _sessionManager.StartSession(
                    talkScene.targetHeroine,
                    GameStateProvider.GetCurrentTimePeriod(),
                    GameStateProvider.GetCurrentWeek(),
                    GameStateProvider.GetCurrentLocation()
                );

                AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                    "[TalkSceneChatModule] Session automatically started.");
            }
        }

        /// <summary>
        /// TalkScene終了時の処理を行います。
        /// Handles cleanup when TalkScene ends.
        /// </summary>
        private void OnTalkSceneEnd()
        {

            var existingUI = TalkSceneUI.Instance;
            if (existingUI != null && existingUI.enabled)
            {
                existingUI.enabled = false;
                AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                    "[TalkSceneChatModule] TalkScene UI closed due to TalkScene end.");
            }

            ResetUISessionState();

            if (_sessionManager != null && _sessionManager.IsSessionActive)
            {
                _sessionManager.EndSession();
                AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                    "[TalkSceneChatModule] Session automatically ended and saved.");
            }
        }

        /// <summary>
        /// TalkSceneUIのセッション固有の実行時状態をリセットします。
        /// UIインスタンスが存在しない場合は何もしません。
        /// Resets the session-specific runtime state of TalkSceneUI.
        /// Does nothing if the UI instance does not exist.
        /// </summary>
        private void ResetUISessionState()
        {
            var existingUI = TalkSceneUI.Instance;
            if (existingUI == null)
                return;

            existingUI.ResetSessionState();
        }

        /// <summary>
        /// TalkScene UIの表示/非表示を切り替えます。
        /// Toggles TalkScene UI visibility.
        /// </summary>
        private void ToggleTalkSceneUI()
        {
            if (_currentTalkScene == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneChatModule] AI TalkScene UI can only be opened during a talk scene.");
                return;
            }

            var existingUI = TalkSceneUI.Instance;
            if (existingUI == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneChatModule] TalkSceneUI instance not found.");
                return;
            }

            // UIが閉じている場合のみ、追加の初期化を実行
            // Perform additional initialization only when UI is currently closed
            if (!existingUI.enabled)
            {
                InitializePromptTemplate(existingUI);
                InitializeContextNote(existingUI);
            }

            existingUI.enabled = !existingUI.enabled;
        }

        /// <summary>
        /// プロンプトテンプレートを初期化します。
        /// TalkSceneChatGameController.CurrentSaveData からカスタムテンプレートを取得し、
        /// 存在しない場合はデフォルトテンプレートを使用します。
        /// Initializes the prompt template.
        /// Retrieves the custom template from TalkSceneChatGameController.CurrentSaveData,
        /// falling back to the default template if not set.
        /// </summary>
        private void InitializePromptTemplate(TalkSceneUI ui)
        {
            var saveData = TalkSceneChatGameController.CurrentSaveData;

            string promptTemplate;
            if (saveData != null && !string.IsNullOrEmpty(saveData.CustomPromptTemplate))
            {
                promptTemplate = saveData.CustomPromptTemplate;
            }
            else
            {
                promptTemplate = TalkSceneDefaultTemplate.GetTemplate();
            }

            ui.SetPromptTemplate(promptTemplate);
        }

        /// <summary>
        /// context_note を初期化します。
        /// 現在のヒロインの context_note を TalkSceneChatGameController.CurrentSaveData から取得し、
        /// UIに反映します。
        /// Initializes the context_note.
        /// Retrieves the context_note for the current heroine from TalkSceneChatGameController.CurrentSaveData
        /// and applies it to the UI.
        /// </summary>
        private void InitializeContextNote(TalkSceneUI ui)
        {
            var heroine = _currentTalkScene?.targetHeroine;
            if (heroine == null)
                return;

            var saveData = TalkSceneChatGameController.CurrentSaveData;
            if (saveData == null)
                return;

            string contextNote = saveData.GetContextNote(heroine);
            ui.SetContextNote(contextNote);

        }

        #endregion

        #region Configuration

        /// <summary>
        /// モジュール用の設定を登録します。
        /// Registers module configuration.
        /// </summary>
        private static void RegisterConfiguration(ConfigFile config)
        {
            ToggleUIKey = config.Bind(
                "TalkSceneChat",
                "Toggle UI Key",
                new KeyboardShortcut(KeyCode.L),
                "Press this key during a talk scene to open/close the AI Talk Scene UI.");

            EnableFavorabilityUpdate = config.Bind(
                "TalkSceneChat",
                "Enable Favorability Update",
                true,
                "If enabled, the AI's evaluation of the conversation will affect the heroine's favorability (favor).");

            EnableArousalUpdate = config.Bind(
                "TalkSceneChat",
                "Enable Arousal Update",
                true,
                "If enabled, the AI's evaluation of the conversation will affect the heroine's arousal level (lewdness).");
        }

        #endregion
    }
}
