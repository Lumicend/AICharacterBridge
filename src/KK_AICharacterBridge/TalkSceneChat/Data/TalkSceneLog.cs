using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using AICharacterBridge.Data;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// 1回のTalkSceneでの完全な会話のやり取り(ユーザーの発言、AIの応答、アクション実行など)を記録するログデータクラス。
    ///
    /// 責務範囲について / About responsibility scope:
    ///   このクラスは「確定済み(保存済み)の過去セッション」のデータ保持と、
    ///   そのプロンプト用フォーマット(FormatForPrompt)のみを担当する。
    ///   進行中のセッション(CurrentConversation)のフォーマットは、
    ///   MainGameLogの抽象化を必要としない別種の処理であるため、
    ///   TalkSceneLogFormatter 側が担当する(旧 FormatAsCurrentConversation は削除済み)。
    ///
    /// A log data class recording a complete chat session in a single TalkScene.
    ///
    /// About responsibility scope:
    ///   This class is responsible only for holding data of a confirmed
    ///   (already saved) past session, and formatting it for prompts
    ///   (FormatForPrompt). Formatting of the in-progress session
    ///   (CurrentConversation) is a different kind of processing that does not
    ///   need the MainGameLog abstraction, so it is now handled by
    ///   TalkSceneLogFormatter instead (the old FormatAsCurrentConversation
    ///   method has been removed).
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class TalkSceneLog : MainGameLog
    {
        /// <summary>この会話セッションに含まれる全ての会話ターンのリスト。</summary>
        [JsonProperty("conversation_turns", ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<ConversationTurn> ConversationTurns { get; set; }

        /// <summary>会話を行った(ゲーム内の)曜日</summary>
        [JsonProperty("week")]
        public string Week { get; set; }

        /// <summary>会話を行った(ゲーム内の)時間帯</summary>
        [JsonProperty("time_period")]
        public string TimePeriod { get; set; }

        /// <summary>会話が行われた場所。</summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        public TalkSceneLog() : base()
        {
            ConversationTurns = new List<ConversationTurn>();
            Week = "";
            TimePeriod = "";
            Location = "";
        }

        /// <summary>
        /// 会話ターンを追加します。
        /// Adds a conversation turn.
        /// </summary>
        public void AddTurn(ConversationTurn turn)
        {
            if (turn != null && turn.IsValid())
            {
                ConversationTurns.Add(turn);
            }
        }

        /// <summary>
        /// すべてのターンをクリアします。
        /// Clears all turns.
        /// </summary>
        public void Clear()
        {
            ConversationTurns.Clear();
        }

        /// <summary>
        /// ターン数を取得します。
        /// Gets the turn count.
        /// </summary>
        [JsonIgnore]
        public int TurnCount => ConversationTurns?.Count ?? 0;

        /// <summary>
        /// すべてのエントリーをフラット化して取得します。
        /// TalkSceneLogFormatter が進行中セッション(CurrentConversation)を
        /// フォーマットする際にも使用するため public 公開している。
        ///
        /// Gets all entries in a flattened list.
        /// Made public so that TalkSceneLogFormatter can also use this when
        /// formatting the in-progress session (CurrentConversation).
        /// </summary>
        public List<ConversationEntry> GetAllEntries()
        {
            var allEntries = new List<ConversationEntry>();

            if (ConversationTurns != null)
            {
                foreach (var turn in ConversationTurns)
                {
                    if (turn != null && turn.Entries != null)
                    {
                        allEntries.AddRange(turn.Entries);
                    }
                }
            }

            return allEntries;
        }

        /// <summary>
        /// エントリー総数を取得します。
        /// Gets the total entry count.
        /// </summary>
        [JsonIgnore]
        public int Count
        {
            get
            {
                int count = 0;
                if (ConversationTurns != null)
                {
                    foreach (var turn in ConversationTurns)
                    {
                        count += turn.Count;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// このログ(過去セッション)をAIプロンプト用の文字列にフォーマットします。
        /// Formats this log (a past session) into a string for AI prompts.
        /// </summary>
        public override string FormatForPrompt(MainGameLogCollection collection, int index)
        {
            var sb = new StringBuilder();

            // 前のログを取得(同じ日の場合のみ)
            TalkSceneLog previousLog = null;
            if (index > 0)
            {
                var allLogs = collection.GetAllLogs();
                var prev = allLogs[index - 1];

                // 前のログが同じ日のTalkSceneLogの場合のみ参照
                if (prev is TalkSceneLog talkLog && talkLog.ElapsedDays == this.ElapsedDays)
                {
                    previousLog = talkLog;
                }
            }

            // ヘッダー出力の判定
            bool needsFullHeader = previousLog == null ||
                                   previousLog.TimePeriod != this.TimePeriod ||
                                   previousLog.Location != this.Location;

            bool needsLocationHeader = previousLog != null &&
                                       previousLog.TimePeriod == this.TimePeriod &&
                                       previousLog.Location != this.Location;

            if (needsFullHeader)
            {
                if (previousLog == null || previousLog.TimePeriod != this.TimePeriod)
                {
                    // 完全なヘッダーを出力
                    // 並び順は CurrentConversation 側のヘッダー(TalkSceneLogFormatter)と揃える:
                    // TimePeriod, School, Location, Week
                    // Output a full header.
                    // Order is kept in sync with the CurrentConversation header
                    // (TalkSceneLogFormatter): TimePeriod, School, Location, Week
                    var headerParts = new List<string>();

                    if (!string.IsNullOrEmpty(TimePeriod))
                        headerParts.Add(GameDataFormatter.FormatTimePeriod(TimePeriod));

                    headerParts.Add($"conversation at {GameStateProvider.GetSchoolName()}");

                    if (!string.IsNullOrEmpty(Location))
                        headerParts.Add(GameDataFormatter.FormatLocation(Location));

                    if (!string.IsNullOrEmpty(Week))
                        headerParts.Add(GameDataFormatter.FormatWeek(Week));

                    sb.AppendLine($"[{string.Join(", ", headerParts.ToArray())}]");
                }
                else if (needsLocationHeader)
                {
                    // 場所のみ変更された場合
                    sb.AppendLine($"[Shortly after, at {GameDataFormatter.FormatLocation(Location)}]");
                }
            }
            else
            {
                // 同じ時間帯、同じ場所の場合
                sb.AppendLine("[Shortly after]");
            }

            // エントリーのフォーマット(フラット化して処理)
            var allEntries = GetAllEntries();
            foreach (var entry in allEntries)
            {
                string formatted = FormatEntry(entry);
                if (!string.IsNullOrEmpty(formatted))
                {
                    sb.AppendLine(formatted);
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 単一のエントリーをフォーマットします。
        ///
        /// 過去ログ(FormatForPrompt)と、TalkSceneLogFormatter が担当する
        /// 進行中セッション(CurrentConversation)のフォーマットの両方から使用される
        /// 共通処理のため、internal static として公開している。
        /// これにより「セリフは"..."で囲む」等の整形ルールを1箇所に集約できる。
        ///
        /// 記述フォーマット(統一記法)に基づく整形ルール:
        ///   ユーザー発言          : そのまま出力する(UserMessageFormatter により
        ///                           送信時点で既に "..." / *...* 形式へ正規化済みのため、
        ///                           ここで追加の引用符付与は行わない)
        ///   キャラクターのセリフ  : "..." で囲む(AIのJSON出力の content 自体は
        ///                           無印テキストのため、ここでプラグイン側が付与する)
        ///   キャラクターの描写    : *...* で囲む
        ///
        /// 改行の扱い / Newline handling:
        ///   Content 内に改行が含まれる場合、1エントリー1行という前提が崩れ、
        ///   「末尾の行に応答する」という規約と整合しなくなる。
        ///   そのため出力直前に改行を半角スペースへ変換する(保存データ自体は変更しない)。
        ///
        /// Formats a single entry.
        ///
        /// This is shared logic used both by past-log formatting (FormatForPrompt)
        /// and by the in-progress session (CurrentConversation) formatting handled
        /// by TalkSceneLogFormatter, so it is exposed as internal static.
        /// This keeps formatting rules (e.g. wrapping dialogue in "...") centralized
        /// in one place.
        ///
        /// Formatting rules based on the unified narration format:
        ///   User utterance      : output as-is (already normalized into the
        ///                         "..."/*...* format at send time by
        ///                         UserMessageFormatter; no additional quoting here)
        ///   Character dialogue  : wrapped in "..." (the AI's JSON "content" field
        ///                         itself is plain text, so the plugin adds quotes)
        ///   Character observation: wrapped in *...*
        ///
        /// Newline handling:
        ///   If Content contains newlines, the "one entry = one line" assumption
        ///   breaks down, which conflicts with the "respond to the final line"
        ///   convention. Newlines are therefore converted to spaces right before
        ///   output (the stored data itself is left unchanged).
        /// </summary>
        internal static string FormatEntry(ConversationEntry entry)
        {
            if (entry is ChatEntry chatEntry)
            {
                string content = SquashNewlines(chatEntry.Content);

                if (chatEntry.Speaker == "user")
                {
                    return $"{chatEntry.CharacterName}: {content}";
                }
                else if (chatEntry.Speaker == "character")
                {
                    if (chatEntry.Type == "dialogue")
                    {
                        return $"{chatEntry.CharacterName}: \"{content}\"";
                    }
                    else if (chatEntry.Type == "observation")
                    {
                        return $"{chatEntry.CharacterName}: *{content}*";
                    }
                }
            }
            else if (entry is ActionEntry actionEntry)
            {
                return $"→ {FormatAction(actionEntry.Action)}";
            }
            return "";
        }

        /// <summary>
        /// 文字列内の改行(\r\n, \r, \n)をすべて半角スペースに変換します。
        /// Content内に改行が含まれていても「1エントリー1行」の前提を崩さないための処理。
        ///
        /// Converts all newlines (\r\n, \r, \n) in a string into single spaces.
        /// Ensures the "one entry = one line" assumption holds even if Content
        /// contains newlines.
        /// </summary>
        /// <param name="text">対象文字列 / Target string</param>
        /// <returns>改行をスペースに変換した文字列 / String with newlines converted to spaces</returns>
        private static string SquashNewlines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
        }

        /// <summary>
        /// アクションを読みやすい形式に変換します。
        /// Converts action to readable format.
        /// </summary>
        private static string FormatAction(string action)
        {
            var actionMap = new Dictionary<string, string>
            {
                { "accept_lunch_together", "Had lunch together" },
                { "accept_study_together", "Studied together" },
                { "accept_recreate_together", "Exercised together" },
                { "accept_club_activity_together", "Did club activities together" },
                { "accept_go_home_together", "Went home together" },
                { "accept_date_reservation", "Made plans for a date" },
                { "accept_accompany_player", "Accompanied {user_name}" },
                { "consent_to_sex", "Became intimate" },
                { "accept_confession_become_lovers", "Became lovers" },
                { "accept_club_recruitment", "Joined the club" }
            };

            if (string.IsNullOrEmpty(action))
                return "Unknown action occurred";

            if (actionMap.TryGetValue(action, out string formatted))
                return formatted;

            return action.Replace("_", " ");
        }

        public override bool IsValid()
        {
            return ConversationTurns != null && ConversationTurns.Count > 0;
        }

        public override MainGameLog Clone()
        {
            var clone = new TalkSceneLog
            {
                ElapsedDays = this.ElapsedDays,
                Week = this.Week,
                TimePeriod = this.TimePeriod,
                Location = this.Location
            };

            if (this.ConversationTurns != null)
            {
                foreach (var turn in this.ConversationTurns)
                {
                    if (turn != null)
                    {
                        clone.AddTurn(turn.Clone());
                    }
                }
            }

            return clone;
        }
    }
}
