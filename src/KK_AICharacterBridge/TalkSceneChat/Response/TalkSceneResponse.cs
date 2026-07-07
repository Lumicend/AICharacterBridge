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
        /// テキスト内の完全なトップレベルJSONオブジェクトの位置（開始位置と長さ）を表す内部構造体。
        /// Represents the position (start index and length) of a complete top-level
        /// JSON object found within a larger text.
        /// </summary>
        private struct JsonSpan
        {
            public readonly int Start;
            public readonly int Length;

            public JsonSpan(int start, int length)
            {
                Start = start;
                Length = length;
            }
        }

        /// <summary>
        /// JSON文字列からTalkSceneResponseオブジェクトをデシリアライズします。
        /// Deserializes a TalkSceneResponse object from a JSON string.
        /// </summary>
        /// <param name="jsonString">JSON文字列（AIからの生レスポンス）/ JSON string (raw response from the AI)</param>
        /// <returns>デシリアライズされたTalkSceneResponseオブジェクト / Deserialized TalkSceneResponse object</returns>
        /// <exception cref="ArgumentException">JSON文字列がnullまたは空の場合</exception>
        /// <exception cref="Exception">
        /// JSONオブジェクトの候補が1つも見つからない場合、
        /// またはすべての候補がデシリアライズ・検証に失敗した場合
        /// </exception>
        public static TalkSceneResponse FromJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(jsonString));

            // 応答内の完全なトップレベルJSONオブジェクト候補をすべて抽出する。
            // Extract all candidate top-level JSON objects from the response.
            List<JsonSpan> candidates = FindJsonObjectCandidates(jsonString);

            if (candidates.Count == 0)
            {
                throw new Exception(
                    "No JSON object structure ('{' ... '}') found in AI response.\n" +
                    $"Raw response: {jsonString}");
            }

            // 末尾の候補から順に検証する。
            // Validate candidates starting from the last one and moving backward.
            Exception lastError = null;

            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                JsonSpan span = candidates[i];
                string candidateJson = jsonString.Substring(span.Start, span.Length);

                try
                {
                    return ParseAndValidate(candidateJson);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                        $"[TalkSceneResponse] JSON candidate #{i} " +
                        $"(chars {span.Start}-{span.Start + span.Length - 1}) failed validation: {ex.Message}");
                }
            }

            throw new Exception(
                $"Failed to parse AI response: none of the {candidates.Count} JSON candidate(s) passed validation.\n" +
                $"Last error: {lastError?.Message}\nRaw response: {jsonString}",
                lastError);
        }

        /// <summary>
        /// テキスト全体を走査し、文字列リテラル内の中括弧を除外しながら、
        /// トップレベル（ネストしていない）の完全なJSONオブジェクトの範囲をすべて抽出します。
        /// Scans the entire text and extracts the spans of all complete top-level
        /// (non-nested) JSON objects, while ignoring braces that appear inside
        /// string literals.
        ///
        /// Algorithm overview:
        ///   - While inside a double-quoted string literal, '{' and '}' are not counted.
        ///   - Backslash escapes (\", \\, etc.) inside strings are handled correctly.
        ///   - Outside of string literals, '{' increases depth by 1 and '}' decreases it by 1.
        ///   - The position where depth transitions from 0 to 1 is recorded as a
        ///     candidate's start; the position where it returns from 1 to 0 closes
        ///     that candidate.
        ///   - A stray '}' with no matching '{' is ignored (treated as malformed
        ///     structure and skipped).
        /// </summary>
        /// <param name="text">走査対象の文字列 / Text to scan</param>
        /// <returns>検出されたJSONオブジェクト候補のリスト（テキスト中の出現順）/ List of detected JSON object candidates, in order of appearance</returns>
        private static List<JsonSpan> FindJsonObjectCandidates(string text)
        {
            var candidates = new List<JsonSpan>();

            bool inString = false;
            bool escapeNext = false;
            int depth = 0;
            int currentStart = -1;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (inString)
                {
                    if (escapeNext)
                    {
                        // エスケープされた文字はそのまま読み飛ばす
                        // Skip the escaped character as-is
                        escapeNext = false;
                    }
                    else if (c == '\\')
                    {
                        escapeNext = true;
                    }
                    else if (c == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == '{')
                {
                    if (depth == 0)
                        currentStart = i;

                    depth++;
                }
                else if (c == '}')
                {
                    if (depth > 0)
                    {
                        depth--;

                        if (depth == 0 && currentStart >= 0)
                        {
                            candidates.Add(new JsonSpan(currentStart, i - currentStart + 1));
                            currentStart = -1;
                        }
                    }
                    // depth が既に 0 の状態での余分な '}' は無視する
                    // Ignore an extra '}' encountered while depth is already 0
                }
            }

            return candidates;
        }

        /// <summary>
        /// JSON候補文字列をデシリアライズし、必須フィールドの検証と絵文字除去を行います。
        /// 検証に失敗した場合は例外をスローします（呼び出し側で次の候補にフォールバックするため）。
        ///
        /// Deserializes a candidate JSON string, validates required fields, and
        /// removes emoji from segment content.
        /// Throws an exception on validation failure so the caller can fall back
        /// to the next candidate.
        /// </summary>
        /// <param name="candidateJson">JSONオブジェクト候補の文字列 / Candidate JSON object string</param>
        /// <returns>検証済みの TalkSceneResponse インスタンス / Validated TalkSceneResponse instance</returns>
        /// <exception cref="FormatException">必須フィールドが不足している場合 / When a required field is missing</exception>
        /// <exception cref="JsonException">JSON解析に失敗した場合 / When JSON parsing fails</exception>
        private static TalkSceneResponse ParseAndValidate(string candidateJson)
        {
            var response = JsonConvert.DeserializeObject<TalkSceneResponse>(candidateJson);

            if (response == null)
                throw new FormatException("Deserialization returned null.");

            // 必須フィールドの検証
            // Validate required fields
            if (response.ConversationSegments == null || response.ConversationSegments.Count == 0)
                throw new FormatException("'conversation_segments' is missing or empty in AI response.");

            if (string.IsNullOrEmpty(response.ImpressionOnUser))
                throw new FormatException("'impression_on_user' is missing or empty in AI response.");

            if (string.IsNullOrEmpty(response.IsArousedByConversation))
                throw new FormatException("'is_aroused_by_conversation' is missing or empty in AI response.");

            if (string.IsNullOrEmpty(response.PostConversationAction))
                throw new FormatException("'post_conversation_action' is missing or empty in AI response.");

            // 各セグメントの Content から絵文字を除去する。
            // バリデーションより前に実行することで、除去後の文字列に対して検証が行われる。
            // Remove emoji from Content in each segment.
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
            // Validate each segment
            foreach (var segment in response.ConversationSegments)
            {
                if (segment == null)
                    throw new FormatException("A segment is null.");

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
