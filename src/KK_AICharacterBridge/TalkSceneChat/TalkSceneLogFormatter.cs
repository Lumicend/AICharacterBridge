using AICharacterBridge.Data;
using AICharacterBridge.TalkSceneChat.Data;
using Manager;
using System;
using System.Collections.Generic;
using System.Text;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// TalkSceneログのフォーマットを担当する静的クラス。
    /// UIおよびプロンプト構築の両方で使用されます。
    ///
    /// 責務範囲について / About responsibility scope:
    ///   過去セッション(確定済みログ)のフォーマットは TalkSceneLog.FormatForPrompt に委譲するが、
    ///   進行中セッション(CurrentConversation)のフォーマットはこのクラスが直接担当する。
    ///   CurrentConversation は MainGameLog の抽象化(ポリモーフィズム)を必要としない
    ///   別種の処理であるため(呼び出し側は最初から TalkSceneLog だと知っている)、
    ///   TalkSceneLog 側に置く必要性がないと判断したため。
    ///   これに伴い、進行中セッションに付随する context_note(ooc_note)の取得・埋め込みも
    ///   このクラスが担当する。
    ///
    /// Static class responsible for formatting TalkScene logs.
    /// Used by both UI and prompt building.
    ///
    /// About responsibility scope:
    ///   Formatting of past sessions (confirmed logs) is delegated to
    ///   TalkSceneLog.FormatForPrompt, but formatting of the in-progress
    ///   session (CurrentConversation) is handled directly by this class.
    ///   CurrentConversation formatting does not need the MainGameLog
    ///   abstraction (polymorphism), since the caller already knows the
    ///   concrete type is TalkSceneLog, so there was no reason to keep it on
    ///   TalkSceneLog itself.
    ///   Accordingly, retrieving and embedding the context_note (ooc_note)
    ///   attached to the in-progress session is also handled by this class.
    /// </summary>
    public static class TalkSceneLogFormatter
    {
        /// <summary>
        /// 過去セッション(確定済みログ)のみをAIプロンプト用の文字列にフォーマットします。
        /// ログが存在しない場合は "None" を返します。
        ///
        /// Formats only past sessions (confirmed logs) into a string for AI prompts.
        /// Returns "None" if no logs exist.
        /// </summary>
        /// <param name="heroine">対象のHeroine / Target heroine</param>
        /// <returns>フォーマットされた過去ログ文字列 / Formatted past log string</returns>
        public static string FormatPastLogs(SaveData.Heroine heroine)
        {
            var saveData = GameController.CurrentSaveData;
            if (saveData == null)
                return "None";

            try
            {
                var logCollection = saveData.GetLogsForHeroine(heroine);
                var pastLogs = logCollection.GetAllLogs();

                if (pastLogs == null || pastLogs.Count == 0)
                    return "None";

                return logCollection.FormatForPrompt();
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[TalkSceneLogFormatter] Failed to format past logs: {ex.Message}");
                return "None";
            }
        }

        /// <summary>
        /// 進行中セッション(CurrentConversation)をAIプロンプト用の文字列にフォーマットします。
        ///
        /// ヘッダー(時間帯・場所等)は常にゲームの「現在の状態」を反映して生成されます
        /// (ActiveSessionLog開始時点のTimePeriod/Locationではなく、GameStateProviderから都度取得)。
        /// context_note が設定されている場合、ヘッダー直後に &lt;ooc_note&gt; として埋め込まれます。
        /// pendingUserMessage を指定すると、まだ ActiveSessionLog に確定登録されていない
        /// ユーザーの今回の発言を、末尾の1行として追加します(プロンプト構築用)。
        /// 指定しない場合、確定済みエントリーのみが出力されます(UI表示用)。
        ///
        /// Formats the in-progress session (CurrentConversation) into a string for AI prompts.
        ///
        /// The header (time period, location, etc.) always reflects the game's
        /// current state (fetched fresh from GameStateProvider each call, rather
        /// than the TimePeriod/Location recorded when ActiveSessionLog started).
        /// If a context_note is set, it is embedded as &lt;ooc_note&gt; right after
        /// the header.
        /// When pendingUserMessage is supplied, the user's current turn — which
        /// has not yet been committed to ActiveSessionLog — is appended as the
        /// final line (used for prompt construction). When omitted, only
        /// already-committed entries are output (used for UI display).
        /// </summary>
        /// <param name="heroine">対象のHeroine / Target heroine</param>
        /// <param name="sessionManager">セッションマネージャー(nullの場合は確定エントリーなし扱い) / Session manager (treated as no committed entries when null)</param>
        /// <param name="pendingUserMessage">未確定のユーザーの今回の発言(省略可) / The user's current, not-yet-committed turn (optional)</param>
        /// <param name="userName">pendingUserMessage 用の表示名(省略時は "User") / Display name for pendingUserMessage (defaults to "User" if omitted)</param>
        /// <returns>フォーマットされたCurrentConversation文字列 / Formatted CurrentConversation string</returns>
        public static string FormatCurrentConversation(
            SaveData.Heroine heroine,
            TalkSceneSessionManager sessionManager,
            string pendingUserMessage = null,
            string userName = null)
        {
            var sb = new StringBuilder();

            try
            {
                // ヘッダー生成(常に現在のゲーム状態を反映する)
                // 並び順は TalkSceneLog.FormatForPrompt の過去ログヘッダーと揃える:
                // TimePeriod, School, Location, Week
                //
                // Header generation (always reflects the current game state).
                // Order is kept in sync with the past-log header in
                // TalkSceneLog.FormatForPrompt: TimePeriod, School, Location, Week
                var headerParts = new List<string>();

                string timePeriod = GameStateProvider.GetCurrentTimePeriod();
                if (!string.IsNullOrEmpty(timePeriod))
                    headerParts.Add(GameDataFormatter.FormatTimePeriod(timePeriod));

                headerParts.Add($"conversation at {GameStateProvider.GetSchoolName()}");

                string location = GameStateProvider.GetCurrentLocation();
                if (!string.IsNullOrEmpty(location))
                    headerParts.Add(GameDataFormatter.FormatLocation(location));

                string week = GameStateProvider.GetCurrentWeek();
                if (!string.IsNullOrEmpty(week))
                    headerParts.Add(GameDataFormatter.FormatWeek(week));

                sb.AppendLine($"[{string.Join(", ", headerParts.ToArray())}]");

                // context_note (ooc_note) の埋め込み。空の場合は行ごと省略する。
                // Embed context_note (ooc_note). Omitted entirely when empty.
                string contextNote = TalkSceneChatGameController.CurrentSaveData?.GetContextNote(heroine) ?? "";
                if (!string.IsNullOrEmpty(contextNote))
                {
                    sb.AppendLine($"<ooc_note>{contextNote}</ooc_note>");
                }

                bool hasAnyLine = false;

                // 確定済みエントリー(ActiveSessionLog)のフォーマット
                // Format committed entries (ActiveSessionLog)
                if (sessionManager != null &&
                    sessionManager.IsSessionActive &&
                    sessionManager.ActiveSessionLog != null)
                {
                    var entries = sessionManager.ActiveSessionLog.GetAllEntries();
                    foreach (var entry in entries)
                    {
                        string formatted = TalkSceneLog.FormatEntry(entry);
                        if (!string.IsNullOrEmpty(formatted))
                        {
                            sb.AppendLine(formatted);
                            hasAnyLine = true;
                        }
                    }
                }

                // 未確定のユーザー発言を末尾行として追加
                // Append the not-yet-committed user turn as the final line
                if (!string.IsNullOrEmpty(pendingUserMessage))
                {
                    // 保存はせず、文字列化のためだけに使う一時オブジェクト
                    // Temporary object used only for stringification; never saved
                    var pendingEntry = new ChatEntry("user", userName ?? "User", "message", pendingUserMessage);
                    string formatted = TalkSceneLog.FormatEntry(pendingEntry);
                    if (!string.IsNullOrEmpty(formatted))
                    {
                        sb.AppendLine(formatted);
                        hasAnyLine = true;
                    }
                }

                if (!hasAnyLine)
                {
                    sb.AppendLine("(The conversation is just starting)");
                }
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[TalkSceneLogFormatter] Failed to format current conversation: {ex.Message}");
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Heroineのログをフォーマットします。
        /// 過去のログ(FormatPastLogs)と進行中セッション(FormatCurrentConversation)を
        /// "--- Now ---" 区切りで統合した、UI表示用の文字列を返します。
        ///
        /// プロンプト構築時は FormatPastLogs / FormatCurrentConversation を
        /// それぞれ個別に呼び出すため、このメソッドは使用しません
        /// (TalkScenePromptBuilder を参照)。
        ///
        /// Formats logs for a heroine.
        /// Returns a UI-display string that combines past logs
        /// (FormatPastLogs) and the in-progress session
        /// (FormatCurrentConversation), separated by a "--- Now ---" marker.
        ///
        /// This method is not used during prompt construction, which calls
        /// FormatPastLogs / FormatCurrentConversation separately instead
        /// (see TalkScenePromptBuilder).
        /// </summary>
        /// <param name="heroine">対象のHeroine</param>
        /// <param name="sessionManager">セッションマネージャー(省略可能)</param>
        /// <returns>フォーマットされたログ文字列</returns>
        public static string FormatLogs(
            SaveData.Heroine heroine,
            TalkSceneSessionManager sessionManager = null)
        {
            string pastLogs = FormatPastLogs(heroine);
            string currentConversation = FormatCurrentConversation(heroine, sessionManager);

            var sb = new StringBuilder();

            // 過去ログが "None"(未記録)の場合は、見た目の冗長さを避けるため省略する
            // Omit past logs when "None" (no records) to avoid a redundant-looking display
            if (!string.IsNullOrEmpty(pastLogs) && pastLogs != "None")
            {
                sb.Append(pastLogs);
                sb.Append("\n\n");
            }

            sb.Append("--- Now ---\n");
            sb.Append(currentConversation);

            return sb.ToString();
        }
    }
}
