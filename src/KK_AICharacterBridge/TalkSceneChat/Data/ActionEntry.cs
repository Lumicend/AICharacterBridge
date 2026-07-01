using System;
using Newtonsoft.Json;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// 特殊なアクション（一緒に昼食、デート予約など）を表すエントリー。
    /// Entry representing a special action (lunch together, date reservation, etc.).
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ActionEntry : ConversationEntry
    {
        /// <summary>実行された特殊なアクション</summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        public ActionEntry() : base(ConversationEntryType.Action)
        {
            Action = "";
        }

        public ActionEntry(string action) : base(ConversationEntryType.Action)
        {
            Action = action;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(Action);
        }

        public override ConversationEntry Clone()
        {
            return new ActionEntry(Action);
        }
    }
}
