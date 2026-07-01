using AICharacterBridge.TalkSceneChat.Data;
using Manager;
using System;
using System.Collections.Generic;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// TalkSceneでの会話セッション管理を担当するクラス。
    /// セッションの開始・終了、ターン追加、ログ保存を管理します。
    /// Manages conversation sessions in TalkScene.
    /// Handles session start/end, turn additions, and log saving.
    /// </summary>
    public class TalkSceneSessionManager
    {
        /// <summary>セッションがアクティブかどうか</summary>
        public bool IsSessionActive { get; private set; }

        /// <summary>現在アクティブなセッションログ</summary>
        public TalkSceneLog ActiveSessionLog { get; private set; }

        /// <summary>現在の対話相手のHeroine</summary>
        public SaveData.Heroine CurrentHeroine { get; private set; }

        public TalkSceneSessionManager()
        {
            ClearSession();
        }

        /// <summary>
        /// 新しいセッションを開始します。
        /// Starts a new session.
        /// </summary>
        /// <param name="heroine">対話相手のHeroine</param>
        /// <param name="timePeriod">時間帯</param>
        /// <param name="week">曜日</param>
        /// <param name="location">場所</param>
        public void StartSession(SaveData.Heroine heroine, string timePeriod, string week, string location)
        {
            if (heroine == null)
                throw new ArgumentNullException(nameof(heroine));

            if (IsSessionActive)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneSessionManager] Session already active. Ending previous session.");
                EndSession(saveLog: false);
            }

            ActiveSessionLog = new TalkSceneLog
            {
                ElapsedDays = 0,
                TimePeriod = timePeriod ?? "",
                Week = week ?? "",
                Location = location ?? ""
            };

            CurrentHeroine = heroine;
            IsSessionActive = true;

            AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                $"[TalkSceneSessionManager] Started new session at {location}");
        }

        /// <summary>
        /// セッションに会話ターンを追加します。
        /// Adds a conversation turn to the current session.
        /// </summary>
        /// <param name="turn">追加する会話ターン</param>
        /// <returns>追加に成功したかどうか</returns>
        public bool AddTurn(ConversationTurn turn)
        {
            if (turn == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneSessionManager] Cannot add null turn");
                return false;
            }

            if (!turn.IsValid())
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneSessionManager] Cannot add invalid turn");
                return false;
            }

            if (!IsSessionActive || ActiveSessionLog == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneSessionManager] No active session. Cannot add turn.");
                return false;
            }

            ActiveSessionLog.AddTurn(turn);

            AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                $"[TalkSceneSessionManager] Added conversation turn. Total turns: {ActiveSessionLog.TurnCount}");

            return true;
        }

        /// <summary>
        /// 最後の会話ターンを削除します。
        /// Removes the last conversation turn.
        /// </summary>
        /// <returns>削除に成功した場合true</returns>
        public bool RemoveLastTurn()
        {
            if (!IsSessionActive || ActiveSessionLog == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneSessionManager] No active session. Cannot remove turn.");
                return false;
            }

            if (ActiveSessionLog.TurnCount == 0)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneSessionManager] No turns to remove.");
                return false;
            }

            var turns = ActiveSessionLog.ConversationTurns;
            turns.RemoveAt(turns.Count - 1);

            AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                $"[TalkSceneSessionManager] Removed last turn. Remaining turns: {ActiveSessionLog.TurnCount}");

            return true;
        }

        /// <summary>
        /// 現在のアクティブセッションをクリアします（保存せずに破棄）
        /// Clears the current active session (discards without saving)
        /// </summary>
        public void ClearActiveSessionLog()
        {
            if (!IsSessionActive)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                    "[TalkSceneSessionManager] No active session to clear.");
                return;
            }

            ActiveSessionLog = new TalkSceneLog
            {
                ElapsedDays = 0,
                TimePeriod = ActiveSessionLog?.TimePeriod ?? "",
                Week = ActiveSessionLog?.Week ?? "",
                Location = ActiveSessionLog?.Location ?? ""
            };

            AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                "[TalkSceneSessionManager] Active session cleared (data discarded).");
        }

        /// <summary>
        /// セッションを終了し、ログを保存します。
        /// Ends the current session and saves the log.
        /// </summary>
        /// <param name="saveLog">ログを保存するかどうか（デフォルト: true）</param>
        public void EndSession(bool saveLog = true)
        {
            if (!IsSessionActive)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                    "[TalkSceneSessionManager] No active session to end");
                return;
            }

            // ターンが空の場合は保存しない
            if (ActiveSessionLog == null || ActiveSessionLog.TurnCount == 0)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                    "[TalkSceneSessionManager] Session ended with no turns. Not saving.");
                ClearSession();
                return;
            }

            // ログの保存
            if (saveLog)
            {
                var saveData = GameController.CurrentSaveData;
                if (saveData != null && CurrentHeroine != null)
                {
                    saveData.AddLogForHeroine(CurrentHeroine, ActiveSessionLog);
                    AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                        $"[TalkSceneSessionManager] Saved session: {ActiveSessionLog.TurnCount} turns, " +
                        $"{ActiveSessionLog.Count} total entries ");
                }
                else
                {
                    AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                        "[TalkSceneSessionManager] Cannot save session: SaveData or Heroine is null");
                }
            }

            ClearSession();
        }

        /// <summary>
        /// セッション状態をクリアします。
        /// Clears the session state.
        /// </summary>
        private void ClearSession()
        {
            IsSessionActive = false;
            ActiveSessionLog = null;
            CurrentHeroine = null;
        }

        /// <summary>
        /// 現在のセッション情報を取得します。
        /// Gets information about the current session.
        /// </summary>
        /// <returns>セッション情報の文字列</returns>
        public string GetSessionInfo()
        {
            if (!IsSessionActive || ActiveSessionLog == null)
                return "No active session";

            return $"Active session: {ActiveSessionLog.TurnCount} turns ({ActiveSessionLog.Count} entries), " +
                   $"Location: {ActiveSessionLog.Location}, " +
                   $"Time: {ActiveSessionLog.TimePeriod}";
        }
    }
}