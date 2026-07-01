using System;
using Newtonsoft.Json;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// 1回の発言（ユーザーのメッセージ、キャラのセリフ、描写など）を表すエントリー。
    /// Entry representing a single utterance (user message, character dialogue, observation, etc.).
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ChatEntry : ConversationEntry
    {
        /// <summary>発言者（"user" または "character"）</summary>
        [JsonProperty("speaker")]
        public string Speaker { get; set; }

        /// <summary>発言しているキャラクターの実際の名前（例："太郎"、"桜"）</summary>
        [JsonProperty("character_name")]
        public string CharacterName { get; set; }

        /// <summary>発言の種類（"message", "dialogue", "observation"）</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>発言内容</summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        public ChatEntry() : base(ConversationEntryType.Chat)
        {
            Speaker = "";
            CharacterName = "";
            Type = "";
            Content = "";
        }

        public ChatEntry(string speaker, string characterName, string type, string content)
            : base(ConversationEntryType.Chat)
        {
            Speaker = speaker;
            CharacterName = characterName ?? "";
            Type = type;
            Content = content;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(Speaker) &&
                   CharacterName != null &&
                   !string.IsNullOrEmpty(Type) &&
                   !string.IsNullOrEmpty(Content);
        }

        public override ConversationEntry Clone()
        {
            return new ChatEntry(Speaker, CharacterName, Type, Content);
        }
    }
}
