using AICharacterBridge.TalkSceneChat.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AICharacterBridge.TalkSceneChat.Response
{
    /// <summary>
    /// TalkSceneチャットにおけるAIからの完全な応答を格納するクラス。
    /// A class that stores the complete response from the AI in TalkScene chat.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class TalkSceneResponse
    {
        /// <summary>会話のセグメント(セリフや描写)のリスト。</summary>
        [JsonProperty("conversation_segments")]
        public List<DialogueSegment> ConversationSegments { get; set; }

        /// <summary>キャラクターがユーザーに対して抱いた印象。</summary>
        [JsonProperty("impression_on_user")]
        public string ImpressionOnUser { get; set; }

        /// <summary>
        /// この会話によってキャラクターが性的に興奮したかどうか。
        /// Whether the character was sexually aroused by this conversation.
        /// "yes" または "no" の値をとる。
        /// </summary>
        [JsonProperty("is_aroused_by_conversation")]
        public string IsArousedByConversation { get; set; }

        /// <summary>会話の次に行うべきキャラクターのアクション。</summary>
        [JsonProperty("post_conversation_action")]
        public string PostConversationAction { get; set; }

        public TalkSceneResponse()
        {
            ConversationSegments = new List<DialogueSegment>();
            ImpressionOnUser = "";
            IsArousedByConversation = "";
            PostConversationAction = "";
        }

        /// <summary>
        /// JSON文字列からTalkSceneResponseオブジェクトをデシリアライズします。
        /// Deserializes a TalkSceneResponse object from a JSON string.
        /// </summary>
        /// <param name="jsonString">JSON文字列</param>
        /// <returns>デシリアライズされたTalkSceneResponseオブジェクト</returns>
        /// <exception cref="ArgumentException">JSON文字列がnullまたは空の場合</exception>
        /// <exception cref="JsonException">JSON解析に失敗した場合</exception>
        /// <exception cref="FormatException">必須フィールドが不足している場合</exception>
        public static TalkSceneResponse FromJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(jsonString));

            // レスポンスクリーニング処理
            string cleanedJson = CleanJsonResponse(jsonString);

            try
            {
                var response = JsonConvert.DeserializeObject<TalkSceneResponse>(cleanedJson);

                if (response == null)
                    throw new FormatException("Failed to deserialize TalkSceneResponse: result is null.");

                // 必須フィールドの検証
                if (response.ConversationSegments == null || response.ConversationSegments.Count == 0)
                    throw new FormatException("'conversation_segments' is missing or empty in AI response.");

                if (string.IsNullOrEmpty(response.ImpressionOnUser))
                    throw new FormatException("'impression_on_user' is missing or empty in AI response.");

                if (string.IsNullOrEmpty(response.IsArousedByConversation))
                    throw new FormatException("'is_aroused_by_conversation' is missing or empty in AI response.");

                if (string.IsNullOrEmpty(response.PostConversationAction))
                    throw new FormatException("'post_conversation_action' is missing or empty in AI response.");

                // 各セグメントの Content から絵文字を除去する。
                // Remove emoji from Content in each segment.
                // バリデーションより前に実行することで、除去後の文字列に対して検証が行われる。
                // Executed before validation so that the check runs against the sanitized string.
                foreach (var segment in response.ConversationSegments)
                {
                    if (segment == null) continue;

                    string original = segment.Content;
                    segment.Content = RemoveEmoji(segment.Content);

                    // 除去が発生した場合はデバッグログを出力
                    // Log if any emoji were actually removed
                    if (original != segment.Content)
                    {
                        AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                            $"[TalkSceneResponse] Emoji removed from segment content. " +
                            $"Before: \"{original}\" / After: \"{segment.Content}\"");
                    }
                }

                // 各セグメントの検証
                foreach (var segment in response.ConversationSegments)
                {
                    if (string.IsNullOrEmpty(segment.Type))
                        throw new FormatException("A segment is missing 'type'.");

                    if (string.IsNullOrEmpty(segment.Content))
                        throw new FormatException("A segment is missing 'content'.");

                    if (string.IsNullOrEmpty(segment.Expression))
                        throw new FormatException("A segment is missing 'expression'.");

                    if (string.IsNullOrEmpty(segment.CharaMotion))
                        throw new FormatException("A segment is missing 'pose'.");
                }

                return response;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse AI response (JSON parse error): {ex.Message}\nRaw response: {jsonString}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse AI response: {ex.Message}\nRaw response: {jsonString}", ex);
            }
        }

        /// <summary>
        /// AIレスポンスをクリーニングして純粋なJSON文字列を抽出します。
        /// Markdownコードブロック（```json ... ```）や余分な文字を除去します。
        /// Cleans AI response to extract pure JSON string.
        /// Removes Markdown code blocks (```json ... ```) and extraneous characters.
        /// </summary>
        /// <param name="response">AIからの生レスポンス</param>
        /// <returns>クリーニングされたJSON文字列</returns>
        private static string CleanJsonResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return response;

            // 前後の空白・改行を削除
            response = response.Trim();

            // 1. Markdownコードブロックの除去
            // ```json ... ``` または ``` ... ``` 形式に対応

            // 先頭の ```json を削除
            if (response.StartsWith("```json"))
            {
                response = response.Substring(7); // "```json" の長さ = 7
                AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                    "[TalkSceneResponse] Removed leading '```json' from response");
            }
            // 先頭の ``` を削除（jsonが付いていない場合）
            else if (response.StartsWith("```"))
            {
                response = response.Substring(3); // "```" の長さ = 3
                AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                    "[TalkSceneResponse] Removed leading '```' from response");
            }

            // 再度トリム（コードブロック除去後の空白対策）
            response = response.Trim();

            // 末尾の ``` を削除
            if (response.EndsWith("```"))
            {
                response = response.Substring(0, response.Length - 3);
                AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                    "[TalkSceneResponse] Removed trailing '```' from response");
            }

            // 再度トリム
            response = response.Trim();

            // 2. JSON開始位置と終了位置を探す（念のための安全策）
            int jsonStart = response.IndexOf('{');
            int jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                // JSON部分のみを抽出
                string extractedJson = response.Substring(jsonStart, jsonEnd - jsonStart + 1);

                // 抽出前後で変化があった場合はログ出力
                if (extractedJson != response)
                {
                    AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                        $"[TalkSceneResponse] Extracted JSON from position {jsonStart} to {jsonEnd}");
                }

                return extractedJson;
            }
            else
            {
                // JSON構造が見つからない場合は警告を出して元の文字列を返す
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneResponse] Could not find valid JSON structure ('{' and '}') in response");
            }

            return response;
        }

        /// <summary>
        /// 文字列から絵文字を除去します。
        /// BMP範囲の主要な絵文字・記号と、補助面（サロゲートペア）の絵文字を対象とします。
        /// Removes emoji from the given string.
        /// Targets major BMP emoji/symbol ranges and supplementary plane (surrogate pair) emoji.
        ///
        /// 対象範囲 / Target ranges:
        ///   U+2300–U+23FF : Miscellaneous Technical
        ///   U+2600–U+26FF : Miscellaneous Symbols
        ///   U+2700–U+27BF : Dingbats
        ///   U+2B00–U+2BFF : Miscellaneous Symbols and Arrows
        ///   U+200D        : Zero Width Joiner（絵文字シーケンス結合子 / Emoji sequence joiner）
        ///   U+FE0F        : Variation Selector-16（絵文字スタイル指定 / Emoji style selector）
        ///   U+20E3        : Combining Enclosing Keycap
        ///   U+1F000以降   : 補助面の絵文字（サロゲートペア / Supplementary plane emoji via surrogate pairs）
        /// </summary>
        /// <param name="text">処理対象の文字列 / String to process</param>
        /// <returns>絵文字を除去した文字列 / String with emoji removed</returns>
        private static string RemoveEmoji(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // BMP範囲の主要な絵文字・記号を除去
            // Remove major BMP emoji and symbol ranges
            text = Regex.Replace(
                text,
                @"[\u2300-\u23FF\u2600-\u27BF\u2B00-\u2BFF\u200D\uFE0F\u20E3]",
                "");

            // 補助面の絵文字（サロゲートペア）を除去
            // Remove supplementary plane emoji represented as surrogate pairs
            // U+1F000 以降のコードポイントは事実上すべて絵文字・記号として扱う
            // Code points at U+1F000 and above are treated as emoji/symbols
            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (char.IsHighSurrogate(c))
                {
                    if (i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    {
                        int codePoint = char.ConvertToUtf32(c, text[i + 1]);
                        if (codePoint >= 0x1F000)
                        {
                            // 絵文字範囲のサロゲートペアをスキップ
                            // Skip surrogate pair in emoji range
                            i++;
                            continue;
                        }
                        sb.Append(c);
                        sb.Append(text[i + 1]);
                        i++;
                    }
                    // 孤立したハイサロゲートは除去（不正なUTF-16）
                    // Skip lone high surrogate (invalid UTF-16)
                }
                else if (!char.IsLowSurrogate(c))
                {
                    sb.Append(c);
                }
                // 孤立したローサロゲートは除去（不正なUTF-16）
                // Skip lone low surrogate (invalid UTF-16)
            }

            return sb.ToString();
        }

        /// <summary>
        /// レスポンスの妥当性を検証
        /// </summary>
        public bool IsValid()
        {
            if (ConversationSegments == null || ConversationSegments.Count == 0)
                return false;

            foreach (var segment in ConversationSegments)
            {
                if (!segment.IsValid())
                    return false;
            }

            return !string.IsNullOrEmpty(ImpressionOnUser) &&
                   !string.IsNullOrEmpty(IsArousedByConversation) &&
                   !string.IsNullOrEmpty(PostConversationAction);
        }

        /// <summary>
        /// レスポンスのディープコピーを作成
        /// </summary>
        public TalkSceneResponse Clone()
        {
            var clone = new TalkSceneResponse
            {
                ImpressionOnUser = this.ImpressionOnUser,
                IsArousedByConversation = this.IsArousedByConversation,
                PostConversationAction = this.PostConversationAction,
                ConversationSegments = new List<DialogueSegment>()
            };

            foreach (var segment in this.ConversationSegments)
            {
                clone.ConversationSegments.Add(segment.Clone());
            }

            return clone;
        }

        /// <summary>
        /// すべてのセリフ(dialogueタイプのセグメント)を取得
        /// </summary>
        public List<DialogueSegment> GetDialogues()
        {
            return ConversationSegments.FindAll(s => s.Type == "dialogue");
        }

        /// <summary>
        /// すべての描写(observationタイプのセグメント)を取得
        /// </summary>
        public List<DialogueSegment> GetObservations()
        {
            return ConversationSegments.FindAll(s => s.Type == "observation");
        }
    }
}
