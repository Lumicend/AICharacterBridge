using System;
using Newtonsoft.Json;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// 会話内の1つのエントリー（発言またはアクション）を表す基底クラス。
    /// Base class representing a single entry (utterance or action) in a conversation.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ConversationEntry
    {
        /// <summary>エントリーの種類</summary>
        [JsonProperty("entry_type")]
        public ConversationEntryType EntryType { get; protected set; }

        protected ConversationEntry(ConversationEntryType entryType)
        {
            EntryType = entryType;
        }

        /// <summary>
        /// エントリーの妥当性を検証します。
        /// Validates the entry.
        /// </summary>
        public abstract bool IsValid();

        /// <summary>
        /// ディープコピーを作成します。
        /// Creates a deep copy.
        /// </summary>
        public abstract ConversationEntry Clone();
    }
}
