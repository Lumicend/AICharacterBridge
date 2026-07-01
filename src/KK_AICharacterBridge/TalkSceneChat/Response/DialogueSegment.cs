using System;
using Newtonsoft.Json;

namespace AICharacterBridge.TalkSceneChat.Response
{
    /// <summary>
    /// AIの応答に含まれる会話の断片(セグメント)を表すクラス。
    /// Represents a segment of conversation in the AI's response.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class DialogueSegment
    {
        /// <summary>セグメントの種類 ("dialogue" または "observation")。</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>セリフや描写のテキスト内容。</summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>このセグメント時点でのキャラクターの表情。</summary>
        [JsonProperty("expression")]
        public string Expression { get; set; }

        /// <summary>このセグメント時点でのキャラクターのモーション(ポーズ)。</summary>
        [JsonProperty("pose")]
        public string CharaMotion { get; set; }

        public DialogueSegment()
        {
            Type = "";
            Content = "";
            Expression = "";
            CharaMotion = "";
        }

        /// <summary>
        /// セグメントの妥当性を検証
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Type) &&
                   !string.IsNullOrEmpty(Content) &&
                   !string.IsNullOrEmpty(Expression) &&
                   !string.IsNullOrEmpty(CharaMotion);
        }

        /// <summary>
        /// セグメントのディープコピーを作成
        /// </summary>
        public DialogueSegment Clone()
        {
            return new DialogueSegment
            {
                Type = this.Type,
                Content = this.Content,
                Expression = this.Expression,
                CharaMotion = this.CharaMotion
            };
        }

        public override string ToString()
        {
            return $"[{Type}] {Content} (Expression: {Expression}, Pose: {CharaMotion})";
        }
    }
}
