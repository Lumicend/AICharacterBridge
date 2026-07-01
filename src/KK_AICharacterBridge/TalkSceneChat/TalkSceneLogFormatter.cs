using AICharacterBridge.Data;
using AICharacterBridge.TalkSceneChat.Data;
using Manager;
using System;
using System.Text;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// TalkSceneログのフォーマットを担当する静的クラス。
    /// UIおよびプロンプト構築の両方で使用されます。
    /// Static class responsible for formatting TalkScene logs.
    /// Used by both UI and prompt building.
    /// </summary>
    public static class TalkSceneLogFormatter
    {
        /// <summary>
        /// Heroineのログをフォーマットします。
        /// 過去のログと現在進行中のセッションログを統合してフォーマットします。
        /// Formats logs.
        /// Integrates past logs and current active session log.
        /// </summary>
        /// <param name="heroine">対象のHeroine</param>
        /// <param name="sessionManager">セッションマネージャー（省略可能）</param>
        /// <returns>フォーマットされたログ文字列</returns>
        public static string FormatLogs(
            SaveData.Heroine heroine,
            TalkSceneSessionManager sessionManager = null)
        {
            string chatLog = "None";

            var saveData = GameController.CurrentSaveData;
            if (saveData == null)
                return chatLog;

            try
            {
                // 過去のログを取得
                var logCollection = saveData.GetLogsForHeroine(heroine);
                var pastLogs = logCollection.GetAllLogs();

                // 過去のログをフォーマット
                if (pastLogs != null && pastLogs.Count > 0)
                {
                    var tempCollection = new MainGameLogCollection();
                    foreach (var log in pastLogs)
                    {
                        tempCollection.AddLog(log);
                    }
                    chatLog = tempCollection.FormatForPrompt();
                }

                // 現在のセッションログがある場合は追加
                if (sessionManager != null &&
                    sessionManager.IsSessionActive &&
                    sessionManager.ActiveSessionLog != null)
                {
                    var currentLog = sessionManager.ActiveSessionLog;
                    if (currentLog.Count >= 0)
                    {
                        if (!string.IsNullOrEmpty(chatLog) && chatLog != "None")
                            chatLog += "\n\n";
                        else
                            chatLog = "";

                        chatLog += "--- Now ---\n";
                        chatLog += currentLog.FormatAsCurrentConversation();
                    }
                }
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[TalkSceneLogFormatter] Failed to format logs: {ex.Message}");
                chatLog = "None";
            }

            return chatLog;
        }
    }
}
