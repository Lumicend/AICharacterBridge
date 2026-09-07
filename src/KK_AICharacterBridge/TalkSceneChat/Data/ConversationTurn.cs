using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// AIとの1回の通信における会話のやり取り(ターン)を表すクラス。
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
    }
}
