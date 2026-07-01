using ActionGame;
using AICharacterBridge.Core.Data;
using AICharacterBridge.Data;
using AICharacterBridge.TalkSceneChat.Data;
using AICharacterBridge.TalkSceneChat.Response;
using KKAPI.Utilities;
using Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICharacterBridge.TalkSceneChat.UI
{
    /// <summary>
    /// メインゲームの会話シーン中にAIとの対話を行うためのIMGUIウィンドウを管理するクラス。
    /// Manages the IMGUI window for AI chat during the main game's talk scene.
    /// </summary>
    public sealed class TalkSceneUI : ImguiWindow<TalkSceneUI>
    {
        #region Constants

        private const float BUTTON_HEIGHT = 30f;
        private const float SPACING = 5f;

        #endregion

        #region Tab Management

        private enum Tab { Chat, Log, Context, Prompt }
        private Tab _currentTab = Tab.Chat;
        private readonly string[] _tabNames = { "Chat", "Log", "Context", "Prompt" };

        #endregion

        #region Scroll Positions

        private Vector2 _chatInputScrollPos = Vector2.zero;
        private Vector2 _logScrollPos = Vector2.zero;
        private Vector2 _contextNoteScrollPos = Vector2.zero;
        private Vector2 _promptScrollPos = Vector2.zero;

        #endregion

        #region Input Data

        private string _userMessage = "";
        private string _contextNoteText = "";
        private string _promptTemplate = "";
        private string _lastSentMessage = "";

        #endregion

        #region Runtime State

        private bool _isChatRunning = false;
        private string _pendingAction = "";

        #endregion

        #region GUI Styles

        private GUIStyle _tabButtonStyle;
        private GUIStyle _selectedTabButtonStyle;
        private GUIStyle _textAreaStyle;

        #endregion

        #region Core Components

        private TalkSceneActionFilter _actionFilter;
        private TalkSceneActionExecutor _actionExecutor;
        private TalkSceneEventExecutor _eventExecutor;
        private TalkScenePromptBuilder _promptBuilder;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            // コアコンポーネントの初期化
            // Initialize core components
            _actionFilter = new TalkSceneActionFilter();
            _actionExecutor = new TalkSceneActionExecutor();
            _eventExecutor = new TalkSceneEventExecutor();
        }

        #endregion

        #region ImguiWindow Implementation

        protected override Rect GetDefaultWindowRect(Rect screenRect)
        {
            const int width = 700;
            const int height = 550;
            return new Rect(
                (screenRect.width - width) / 2,
                (screenRect.height - height) / 2,
                width,
                height
            );
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Title = "AI Character Bridge - TalkScene Chat";
            MinimumSize = new Vector2(600, 400);
        }

        protected override void DrawContents()
        {
            InitializeStyles();

            GUILayout.BeginVertical();
            {
                DrawTabBar();

                GUILayout.Space(SPACING);

                switch (_currentTab)
                {
                    case Tab.Chat: DrawChatTab(); break;
                    case Tab.Log: DrawLogTab(); break;
                    case Tab.Context: DrawContextTab(); break;
                    case Tab.Prompt: DrawPromptTab(); break;
                }
            }
            GUILayout.EndVertical();
        }

        #endregion

        #region GUI Styles Initialization

        private void InitializeStyles()
        {
            if (_tabButtonStyle == null)
            {
                _tabButtonStyle = new GUIStyle(GUI.skin.button);

                _selectedTabButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.yellow }
                };

                _textAreaStyle = new GUIStyle(GUI.skin.textArea)
                {
                    wordWrap = true
                };
            }
        }

        #endregion

        #region Tab Bar

        private void DrawTabBar()
        {
            GUILayout.BeginHorizontal();
            {
                for (int i = 0; i < _tabNames.Length; i++)
                {
                    var tab = (Tab)i;
                    var style = _currentTab == tab ? _selectedTabButtonStyle : _tabButtonStyle;

                    if (GUILayout.Button(_tabNames[i], style, GUILayout.Height(25)))
                    {
                        OnTabChanged(tab);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void OnTabChanged(Tab newTab)
        {
            _currentTab = newTab;
        }

        #endregion

        #region Chat Tab

        private void DrawChatTab()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Your Message:");
                _chatInputScrollPos = GUILayout.BeginScrollView(_chatInputScrollPos, false, true, GUILayout.ExpandHeight(true));
                {
                    _userMessage = GUILayout.TextArea(_userMessage, _textAreaStyle, GUILayout.ExpandHeight(true));
                }
                GUILayout.EndScrollView();

                GUILayout.Space(SPACING);

                DrawChatButtons();

                if (_isChatRunning)
                {
                    GUILayout.Label("AI is thinking...");
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawChatButtons()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(BUTTON_HEIGHT));
            {
                GUI.enabled = !_isChatRunning && !string.IsNullOrEmpty(_lastSentMessage);
                if (GUILayout.Button("Resend Last", GUILayout.Width(100)))
                {
                    ResendLastMessage();
                }
                GUI.enabled = true;

                DrawSpecialActionButton();

                GUILayout.FlexibleSpace();

                GUI.enabled = !_isChatRunning && !string.IsNullOrEmpty(_userMessage?.Trim());
                if (GUILayout.Button("Talk", GUILayout.Width(80)))
                {
                    StartTalkSceneChat();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSpecialActionButton()
        {
            var module = TalkSceneChatModule.Instance;
            var talkScene = module?.GetCurrentTalkScene();

            bool hasAction = !string.IsNullOrEmpty(_pendingAction) && _actionFilter.IsSpecialAction(_pendingAction);
            string buttonText = hasAction ? _actionFilter.GetActionDisplayText(_pendingAction) : "Special Action (Unavailable)";

            GUI.enabled = hasAction && !_isChatRunning && talkScene != null;

            var originalColor = GUI.backgroundColor;
            if (hasAction)
            {
                GUI.backgroundColor = Color.green;
            }

            if (GUILayout.Button(buttonText, GUILayout.Width(200)))
            {
                ExecutePendingAction();
            }

            GUI.backgroundColor = originalColor;
            GUI.enabled = true;
        }

        #endregion

        #region Log Tab

        private void DrawLogTab()
        {
            var module = TalkSceneChatModule.Instance;
            var heroine = module?.GetCurrentHeroine();
            var sessionManager = module?.GetSessionManager();

            if (heroine == null)
            {
                GUILayout.Label("No heroine data available.");
                return;
            }

            GUILayout.BeginVertical();
            {
                _logScrollPos = GUILayout.BeginScrollView(_logScrollPos, false, true, GUILayout.ExpandHeight(true));
                {
                    string logText = TalkSceneLogFormatter.FormatLogs(heroine, sessionManager);
                    GUILayout.TextArea(logText, _textAreaStyle, GUILayout.ExpandHeight(true));
                }
                GUILayout.EndScrollView();

                GUILayout.Space(SPACING);

                DrawLogButtons();
            }
            GUILayout.EndVertical();
        }

        private void DrawLogButtons()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(BUTTON_HEIGHT));
            {
                if (GUILayout.Button("Delete Last Turn", GUILayout.Width(140)))
                {
                    DeleteLastTurn();
                }

                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1, 0.4f, 0.4f, 1);
                if (GUILayout.Button("Delete All Logs", GUILayout.Width(120)))
                {
                    ResetTalkSceneLogs();
                }
                GUI.backgroundColor = originalColor;
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Context Note Tab

        /// <summary>
        /// Context Note タブの内容を描画します。
        /// Draws the Context Note tab content.
        /// </summary>
        private void DrawContextTab()
        {
            var heroine = TalkSceneChatModule.Instance?.GetCurrentHeroine();

            if (heroine == null)
            {
                GUILayout.Label("No heroine data available.");
                return;
            }

            GUILayout.BeginVertical();
            {
                GUILayout.Label("Context Note:");
                _contextNoteScrollPos = GUILayout.BeginScrollView(_contextNoteScrollPos, false, true, GUILayout.ExpandHeight(true));
                {
                    _contextNoteText = GUILayout.TextArea(_contextNoteText, _textAreaStyle, GUILayout.ExpandHeight(true));
                }
                GUILayout.EndScrollView();

                GUILayout.Space(SPACING);

                DrawContextNoteButtons(heroine);
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Context Note タブのボタン群を描画します。
        /// Draws the buttons for the Context Note tab.
        /// </summary>
        /// <param name="heroine">現在のヒロイン / Current heroine</param>
        private void DrawContextNoteButtons(SaveData.Heroine heroine)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(BUTTON_HEIGHT));
            {
                if (GUILayout.Button("Apply Changes", GUILayout.Width(130)))
                {
                    ApplyContextNote(heroine);
                }

                if (GUILayout.Button("Reset", GUILayout.Width(80)))
                {
                    LoadContextNote(heroine);
                }

                if (GUILayout.Button("Clear", GUILayout.Width(80)))
                {
                    _contextNoteText = "";
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 指定したヒロインの context_note をセーブデータから読み込み、編集用テキストに反映します。
        /// Loads the context_note for the specified heroine from save data into the editing text field.
        /// </summary>
        /// <param name="heroine">対象のヒロイン / Target heroine</param>
        private void LoadContextNote(SaveData.Heroine heroine)
        {
            if (heroine == null) return;

            var saveData = TalkSceneChatGameController.CurrentSaveData;
            if (saveData == null)
            {
                LogWarning("Cannot load context note: TalkSceneChatSaveData is null.");
                return;
            }

            _contextNoteText = saveData.GetContextNote(heroine);
        }

        /// <summary>
        /// 編集中の context_note をセーブデータに書き込みます。
        /// Writes the current context_note text to save data.
        /// </summary>
        /// <param name="heroine">対象のヒロイン / Target heroine</param>
        private void ApplyContextNote(SaveData.Heroine heroine)
        {
            if (heroine == null) return;

            var saveData = TalkSceneChatGameController.CurrentSaveData;
            if (saveData == null)
            {
                LogWarning("Cannot apply context note: TalkSceneChatSaveData is null.");
                return;
            }

            saveData.SetContextNote(heroine, _contextNoteText);
            LogInfo($"Context note applied for {heroine.charFile?.parameter?.fullname ?? "Unknown"}.");
        }

        #endregion

        #region Public API

        /// <summary>
        /// プロンプトテンプレートを設定します（TalkSceneChatModuleから呼ばれる）。
        /// Sets the prompt template (called from TalkSceneChatModule).
        /// </summary>
        public void SetPromptTemplate(string template)
        {
            _promptTemplate = template ?? "";
        }

        /// <summary>
        /// context_note の初期値を設定します（TalkSceneChatModuleから呼ばれる）。
        /// Sets the initial value of context_note (called from TalkSceneChatModule).
        /// </summary>
        /// <param name="note">設定する context_note / context_note to set</param>
        public void SetContextNote(string note)
        {
            _contextNoteText = note ?? "";
        }

        /// <summary>
        /// TalkSceneの開始・終了に伴うセッション固有の実行時状態をリセットします。
        /// TalkSceneChatModule の OnTalkSceneStart および OnTalkSceneEnd から呼び出されます。
        /// Resets session-specific runtime state on TalkScene start or end.
        /// Called from TalkSceneChatModule's OnTalkSceneStart and OnTalkSceneEnd.
        /// </summary>
        public void ResetSessionState()
        {
            _userMessage = "";
            _contextNoteText = "";
            _promptTemplate = "";
            _lastSentMessage = "";
            _pendingAction = "";
            _isChatRunning = false;
        }

        #endregion

        #region Chat Execution

        private void StartTalkSceneChat()
        {
            var module = TalkSceneChatModule.Instance;
            if (module == null || module.GetCurrentTalkScene() == null || module.GetCurrentHeroine() == null)
            {
                LogError("TalkScene or Heroine not found.");
                return;
            }

            _lastSentMessage = _userMessage;

            // チャット開始時に現在の設定を自動保存
            // Auto-save current settings when chat starts
            SaveCurrentSettings();

            AICharacterBridgePlugin.Instance.StartCoroutine(TalkSceneChatCoroutine());
        }

        private void ResendLastMessage()
        {
            if (!string.IsNullOrEmpty(_lastSentMessage))
            {
                _userMessage = _lastSentMessage;
            }
        }

        /// <summary>
        /// 現在の設定（プロンプトテンプレートおよびcontext_note）をTalkSceneChatSaveDataに保存します。
        /// Saves current settings (prompt template and context_note) to TalkSceneChatSaveData.
        /// </summary>
        private void SaveCurrentSettings()
        {
            var saveData = TalkSceneChatGameController.CurrentSaveData;
            if (saveData == null)
            {
                LogWarning("Cannot save settings: TalkSceneChatSaveData is null.");
                return;
            }

            // プロンプトテンプレートの保存
            // Save prompt template
            saveData.CustomPromptTemplate = _promptTemplate;

            // context_note の保存
            // Save context_note
            var heroine = TalkSceneChatModule.Instance?.GetCurrentHeroine();
            if (heroine != null)
            {
                saveData.SetContextNote(heroine, _contextNoteText);
            }

        }

        private IEnumerator TalkSceneChatCoroutine()
        {
            _isChatRunning = true;
            LogInfo("Starting AI chat...");

            var module = TalkSceneChatModule.Instance;
            if (module == null)
            {
                LogError("TalkSceneChatModule instance not found.");
                _isChatRunning = false;
                yield break;
            }

            var sessionManager = module.GetSessionManager();
            var heroine = module.GetCurrentHeroine();
            var talkScene = module.GetCurrentTalkScene();

            if (sessionManager == null || heroine == null || talkScene == null)
            {
                LogError("Required components not found.");
                _isChatRunning = false;
                yield break;
            }

            // プロンプトビルダーの遅延初期化
            // Lazy initialization of the prompt builder
            if (_promptBuilder == null)
            {
                _promptBuilder = new TalkScenePromptBuilder(sessionManager, _actionFilter);
            }

            Exception caughtError = null;
            string extractedMessage = null;
            string prompt = null;
            List<ExpressionData> expressions = null;
            List<CharaMotionData> charaMotions = null;
            TalkSceneResponse response = null;

            // フェーズ1: データ準備
            // Phase 1: Data preparation
            try
            {
                expressions = ExpressionPresets.GetDefaultSet();
                charaMotions = CharaMotionPresets.GetDefaultSet();

                if (!sessionManager.IsSessionActive)
                {
                    LogWarning("Session not active. This should not happen.");
                }

                // コアセーブデータから WorldSetting を取得
                // Retrieve WorldSetting from core save data
                var coreData = GameController.CurrentSaveData;
                WorldSetting worldSetting = coreData?.GetWorldSetting();

                prompt = _promptBuilder.BuildPrompt(
                    _promptTemplate,
                    _userMessage,
                    worldSetting,
                    heroine,
                    expressions,
                    charaMotions
                );

                if (string.IsNullOrEmpty(prompt))
                {
                    LogError("Failed to build prompt.");
                    _isChatRunning = false;
                    yield break;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error during preparation phase: {ex.Message}");
                _isChatRunning = false;
                yield break;
            }

            // フェーズ2: AI通信
            // Phase 2: AI communication
            yield return AICharacterBridgePlugin.Instance.SendPromptToAI(
                prompt,
                message => extractedMessage = message,
                ex => caughtError = ex
            );

            if (caughtError != null)
            {
                LogError($"AI processing error: {caughtError.Message}");
                if (caughtError.InnerException != null)
                {
                    LogError($"Inner exception: {caughtError.InnerException.Message}");
                }
                _isChatRunning = false;
                yield break;
            }

            if (string.IsNullOrEmpty(extractedMessage))
            {
                LogWarning("Received null or empty response from AI.");
                _isChatRunning = false;
                yield break;
            }

            // フェーズ3: レスポンス処理
            // Phase 3: Response processing
            try
            {
                response = TalkSceneResponse.FromJson(extractedMessage);

                LogInfo("===== AI Response =====");
                foreach (var segment in response.ConversationSegments)
                {
                    LogInfo($"- Type: {segment.Type}, Expression: {segment.Expression}, Pose: {segment.CharaMotion}");
                    LogInfo($"  Content: {segment.Content}");
                }
                LogInfo("--- Additional Info ---");
                LogInfo($"Impression: {response.ImpressionOnUser}");
                LogInfo($"Arousal: {response.IsArousedByConversation}");
                LogInfo($"Next Action: {response.PostConversationAction}");
                LogInfo("========================");

                var turn = new ConversationTurn();

                var userCard = CharacterCardProvider.GetPlayerCharacterCard();
                var heroineCard = CharacterCardProvider.GetHeroineCharacterCard(heroine);
                string userName = userCard?.GetName() ?? "User";
                string heroineName = heroineCard?.GetName() ?? heroine.charFile?.parameter?.fullname ?? "Character";

                turn.AddEntry(new ChatEntry("user", userName, "message", _userMessage));

                foreach (var segment in response.ConversationSegments)
                {
                    turn.AddEntry(new ChatEntry("character", heroineName, segment.Type, segment.Content));
                }

                sessionManager.AddTurn(turn);
            }
            catch (Exception ex)
            {
                LogError($"Error during response processing: {ex.Message}");
                _isChatRunning = false;
                yield break;
            }

            // フェーズ4: ゲーム内イベント実行
            // Phase 4: In-game event execution
            enabled = false;

            yield return _eventExecutor.ExecuteAIEvent(
                talkScene,
                response,
                expressions,
                charaMotions
            );

            enabled = true;

            // フェーズ5: 最終処理（アクション処理・好感度更新・性的興奮度更新）
            // Phase 5: Post-processing (action queuing, favorability/arousal updates)
            _userMessage = "";

            try
            {
                if (_actionFilter.IsSpecialAction(response.PostConversationAction))
                {
                    LogInfo($"Post-conversation action queued: {response.PostConversationAction}");
                    _pendingAction = response.PostConversationAction;
                }
                else
                {
                    _pendingAction = "";
                }

                // 好感度の更新（コンフィグで有効な場合のみ実行）
                // Update favorability only when enabled in config
                if (TalkSceneChatModule.EnableFavorabilityUpdate.Value)
                {
                    UpdateFavorability(response.ImpressionOnUser);
                }

                // 性的興奮度の更新（コンフィグで有効な場合のみ実行）
                // Update arousal only when enabled in config
                if (TalkSceneChatModule.EnableArousalUpdate.Value)
                {
                    UpdateArousal(response.IsArousedByConversation);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error during final processing: {ex.Message}");
            }
            finally
            {
                _isChatRunning = false;
                LogInfo("Chat completed.");
            }
        }

        /// <summary>
        /// 会話の印象評価に基づいてヒロインの好感度(favor)を更新します。
        /// Updates the heroine's favorability (favor) based on the impression evaluation of the conversation.
        /// </summary>
        private void UpdateFavorability(string impression)
        {
            var module = TalkSceneChatModule.Instance;
            var talkScene = module?.GetCurrentTalkScene();
            if (talkScene == null || talkScene.targetHeroine == null)
                return;

            var heroine = talkScene.targetHeroine;

            switch (impression)
            {
                case "very_bad":
                    heroine.favor = Math.Max(heroine.favor - 4, 0);
                    break;
                case "bad":
                    heroine.favor = Math.Max(heroine.favor - 2, 0);
                    break;
                case "neutral":
                    break;
                case "good":
                    if (heroine.favor >= 100 && heroine.isGirlfriend)
                        heroine.intimacy = Math.Min(heroine.intimacy + 1, 100);
                    else
                        heroine.favor = Math.Min(heroine.favor + 4, 100);
                    break;
                case "very_good":
                    if (heroine.favor >= 100 && heroine.isGirlfriend)
                        heroine.intimacy = Math.Min(heroine.intimacy + 1, 100);
                    else
                        heroine.favor = Math.Min(heroine.favor + 6, 100);
                    break;
            }
        }

        /// <summary>
        /// 会話による性的興奮評価に基づいてヒロインの性的興奮度(lewdness)を更新します。
        /// Updates the heroine's arousal (lewdness) based on the arousal evaluation of the conversation.
        /// </summary>
        private void UpdateArousal(string isAroused)
        {
            if (isAroused != "yes")
                return;

            var module = TalkSceneChatModule.Instance;
            var talkScene = module?.GetCurrentTalkScene();
            if (talkScene == null || talkScene.targetHeroine == null)
                return;

            var heroine = talkScene.targetHeroine;
            heroine.lewdness = Math.Min(heroine.lewdness + 4, 100);

            LogInfo($"Arousal updated: lewdness = {heroine.lewdness}");
        }

        #endregion

        #region Action Execution

        private void ExecutePendingAction()
        {
            var module = TalkSceneChatModule.Instance;
            var talkScene = module?.GetCurrentTalkScene();
            var heroine = module?.GetCurrentHeroine();
            var sessionManager = module?.GetSessionManager();

            if (string.IsNullOrEmpty(_pendingAction) || talkScene == null || heroine == null || sessionManager == null)
            {
                LogWarning("Cannot execute action: Invalid state");
                return;
            }

            LogInfo($"Executing pending action: {_pendingAction}");

            bool success = _actionExecutor.ExecuteAction(_pendingAction, talkScene);

            if (success)
            {
                var actionTurn = new ConversationTurn();
                actionTurn.AddEntry(new ActionEntry(_pendingAction));
                sessionManager.AddTurn(actionTurn);
            }

            _pendingAction = "";
        }

        #endregion

        #region Prompt Management

        /// <summary>
        /// UIで編集されたプロンプトテンプレートをTalkSceneChatSaveDataに反映します。
        /// デフォルトと同一の場合はnullを設定してデフォルトに戻します。
        /// Applies the prompt template edited in the UI to TalkSceneChatSaveData.
        /// Sets null (reverts to default) if the template matches the default.
        /// </summary>
        private void ApplyPromptTemplate()
        {
            var saveData = TalkSceneChatGameController.CurrentSaveData;
            if (saveData == null)
            {
                LogWarning("Cannot apply prompt template: TalkSceneChatSaveData is null.");
                return;
            }

            // デフォルトテンプレートは静的クラスから直接取得する
            // Get the default template directly from the static class
            var defaultTemplate = TalkSceneDefaultTemplate.GetTemplate();

            if (string.IsNullOrEmpty(_promptTemplate) || _promptTemplate == defaultTemplate)
            {
                saveData.CustomPromptTemplate = null;
                LogInfo("Prompt template reset to default.");
            }
            else
            {
                saveData.CustomPromptTemplate = _promptTemplate;
                LogInfo("Custom prompt template applied successfully.");
            }
        }

        /// <summary>
        /// プロンプトテンプレートをデフォルトにリセットします（編集用テキストのみ）。
        /// セーブデータへの反映は ApplyPromptTemplate() で行います。
        /// Resets the prompt template to default (editing text only).
        /// Reflecting the change to save data is done via ApplyPromptTemplate().
        /// </summary>
        private void ResetPromptTemplate()
        {
            _promptTemplate = TalkSceneDefaultTemplate.GetTemplate();
            LogInfo("Prompt template reset to default.");
        }

        #endregion

        #region Prompt Tab

        private void DrawPromptTab()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Prompt Template:");
                _promptScrollPos = GUILayout.BeginScrollView(_promptScrollPos, false, true, GUILayout.ExpandHeight(true));
                {
                    _promptTemplate = GUILayout.TextArea(_promptTemplate, _textAreaStyle, GUILayout.ExpandHeight(true));
                }
                GUILayout.EndScrollView();

                GUILayout.Space(SPACING);

                DrawPromptButtons();
            }
            GUILayout.EndVertical();
        }

        private void DrawPromptButtons()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(BUTTON_HEIGHT));
            {
                if (GUILayout.Button("Apply Changes", GUILayout.Width(130)))
                {
                    ApplyPromptTemplate();
                }

                if (GUILayout.Button("Reset", GUILayout.Width(80)))
                {
                    ResetPromptTemplate();
                }
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Log Management

        private void DeleteLastTurn()
        {
            var module = TalkSceneChatModule.Instance;
            if (module == null)
            {
                LogWarning("Cannot delete last turn: TalkSceneChatModule not found.");
                return;
            }

            bool success = module.RemoveLastTurn();
            if (success)
                LogInfo("Last conversation turn deleted successfully.");
            else
                LogWarning("Failed to delete last turn: No active session or no turns to delete.");
        }

        private void ResetTalkSceneLogs()
        {
            var module = TalkSceneChatModule.Instance;
            var currentHeroine = module?.GetCurrentHeroine();

            if (currentHeroine == null)
            {
                LogWarning("Cannot reset logs: No heroine selected.");
                return;
            }

            // ログはコアセーブデータで管理されているため GameController 経由でアクセス
            // Logs are managed in core save data, accessed via GameController
            var coreData = GameController.CurrentSaveData;
            if (coreData == null)
            {
                LogWarning("Cannot reset logs: Core SaveData is null.");
                return;
            }

            var logCollection = coreData.GetLogsForHeroine(currentHeroine);
            logCollection.ClearLogs();
            module.ClearCurrentSessionLog();

            LogInfo($"TalkScene logs and current session cleared for {currentHeroine.charFile.parameter.fullname}");
        }

        #endregion

        #region Logging Helpers

        private void LogInfo(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogInfo($"[TalkSceneUI] {message}");
        }

        private void LogDebug(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogDebug($"[TalkSceneUI] {message}");
        }

        private void LogWarning(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogWarning($"[TalkSceneUI] {message}");
        }

        private void LogError(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogError($"[TalkSceneUI] {message}");
        }

        #endregion
    }
}
