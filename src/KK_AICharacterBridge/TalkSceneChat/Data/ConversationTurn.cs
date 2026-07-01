using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// AIとの1回の通信における会話のやり取り（ターン）を表すクラス。
    /// ユーザーの発言とAIの応答をひとまとめにして管理します。
    /// Represents a single conversation turn (one communication with AI).
    /// Groups user messages and AI responses together.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ConversationTurn
    {
        /// <summary>このターンに含まれるエントリーのリスト</summary>
        [JsonProperty("entries", ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<ConversationEntry> Entries { get; set; }

        public ConversationTurn()
        {
            Entries = new List<ConversationEntry>();
        }

        /// <summary>
        /// エントリーを追加します。
        /// Adds an entry to this turn.
        /// </summary>
        /// <param name="entry">追加するエントリー</param>
        public void AddEntry(ConversationEntry entry)
        {
            if (entry != null && entry.IsValid())
            {
                Entries.Add(entry);
            }
        }

        /// <summary>
        /// エントリー数を取得します。
        /// Gets the entry count.
        /// </summary>
        [JsonIgnore]
        public int Count => Entries?.Count ?? 0;

        /// <summary>
        /// このターンが有効かどうかを判定します。
        /// Determines if this turn is valid.
        /// </summary>
        public bool IsValid()
        {
            return Entries != null && Entries.Count > 0;
        }

        /// <summary>
        /// ディープコピーを作成します。
        /// Creates a deep copy.
        /// </summary>
        public ConversationTurn Clone()
        {
            var clone = new ConversationTurn();

            if (this.Entries != null)
            {
                foreach (var entry in this.Entries)
                {
                    if (entry != null)
                    {
                        clone.AddEntry(entry.Clone());
                    }
                }
            }

            return clone;
        }

        /// <summary>
        /// このターンを読みやすい形式でフォーマットします。
        /// Formats this turn in a readable format.
        /// </summary>
        /// <returns>フォーマットされた文字列</returns>
        public string FormatForDisplay()
        {
            var sb = new StringBuilder();

            if (Entries == null || Entries.Count == 0)
            {
                sb.AppendLine("(Empty turn)");
                return sb.ToString();
            }

            foreach (var entry in Entries)
            {
                if (entry is ChatEntry chatEntry)
                {
                    if (chatEntry.Speaker == "user")
                    {
                        sb.AppendLine($"User: {chatEntry.Content}");
                    }
                    else if (chatEntry.Speaker == "character")
                    {
                        if (chatEntry.Type == "dialogue")
                        {
                            sb.AppendLine($"Character: {chatEntry.Content}");
                        }
                        else if (chatEntry.Type == "observation")
                        {
                            sb.AppendLine($"(Observation: {chatEntry.Content})");
                        }
                    }
                }
                else if (entry is ActionEntry actionEntry)
                {
                    sb.AppendLine($"[Action: {actionEntry.Action}]");
                }
            }

            return sb.ToString();
        }
    }
}
