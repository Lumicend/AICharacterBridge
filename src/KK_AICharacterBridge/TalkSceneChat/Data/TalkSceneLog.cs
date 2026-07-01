using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using AICharacterBridge.Data;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// 1回のTalkSceneでの完全な会話のやり取り（ユーザーの発言、AIの応答、アクション実行など）を記録するログデータクラス。
    /// A log data class recording a complete chat session in a single TalkScene.
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
        /// すべてのエントリーをフラット化して取得します（内部処理用）。
        /// Gets all entries in a flattened list (for internal processing).
        /// </summary>
        private List<ConversationEntry> GetAllEntries()
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
        /// このログをAIプロンプト用の文字列にフォーマットします。
        /// Formats this log into a string for AI prompts.
        /// </summary>
        public override string FormatForPrompt(MainGameLogCollection collection, int index)
        {
            var sb = new StringBuilder();

            // 前のログを取得（同じ日の場合のみ）
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
                    var headerParts = new List<string>();

                    if (!string.IsNullOrEmpty(TimePeriod))
                        headerParts.Add(GameDataFormatter.FormatTimePeriod(TimePeriod));

                    headerParts.Add($"conversation at {GameStateProvider.GetSchoolName()}");

                    if (!string.IsNullOrEmpty(Location))
                        headerParts.Add(GameDataFormatter.FormatLocation(Location));

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

            // エントリーのフォーマット（フラット化して処理）
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
        /// Formats a single entry.
        /// </summary>
        private string FormatEntry(ConversationEntry entry)
        {
            if (entry is ChatEntry chatEntry)
            {
                if (chatEntry.Speaker == "user")
                {
                    return $"{chatEntry.CharacterName}: \"{chatEntry.Content}\"";
                }
                else if (chatEntry.Speaker == "character")
                {
                    if (chatEntry.Type == "dialogue")
                    {
                        return $"{chatEntry.CharacterName}: \"{chatEntry.Content}\"";
                    }
                    else if (chatEntry.Type == "observation")
                    {
                        return $"{chatEntry.CharacterName}: ({chatEntry.Content})";
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
        /// アクションを読みやすい形式に変換します。
        /// Converts action to readable format.
        /// </summary>
        private string FormatAction(string action)
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

        /// <summary>
        /// 現在進行中の会話をAIプロンプト用の文字列にフォーマットします。
        /// Formats the current ongoing conversation into a string for AI prompts.
        /// </summary>
        /// <param name="currentWeek">現在の曜日</param>
        /// <param name="currentTimePeriod">現在の時間帯</param>
        /// <param name="currentLocation">現在の場所</param>
        public string FormatAsCurrentConversation()
        {
            var sb = new StringBuilder();
            var currentWeek = GameStateProvider.GetCurrentWeek();
            var currentTimePeriod = GameStateProvider.GetCurrentTimePeriod();
            var currentLocation = GameStateProvider.GetCurrentLocation();
            // 周辺情報を常に出力（省略なし）
            var headerParts = new List<string>();

            if (!string.IsNullOrEmpty(currentTimePeriod))
                headerParts.Add(GameDataFormatter.FormatTimePeriod(currentTimePeriod));

            headerParts.Add($"conversation at {GameStateProvider.GetSchoolName()}");

            if (!string.IsNullOrEmpty(currentLocation))
                headerParts.Add(GameDataFormatter.FormatLocation(currentLocation));

            if (!string.IsNullOrEmpty(currentWeek))
                headerParts.Add(GameDataFormatter.FormatWeek(currentWeek));

            sb.AppendLine($"[{string.Join(", ", headerParts.ToArray())}]");

            // エントリーが空の場合
            if (ConversationTurns == null || ConversationTurns.Count == 0)
            {
                sb.AppendLine("(The conversation is just starting)");
            }
            else
            {
                // エントリーのフォーマット（フラット化して処理）
                var allEntries = GetAllEntries();
                foreach (var entry in allEntries)
                {
                    string formatted = FormatEntry(entry);
                    if (!string.IsNullOrEmpty(formatted))
                    {
                        sb.AppendLine(formatted);
                    }
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
