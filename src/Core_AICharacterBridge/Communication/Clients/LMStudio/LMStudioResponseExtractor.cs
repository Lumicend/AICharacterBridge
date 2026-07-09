using System;
using Newtonsoft.Json.Linq;
using AICharacterBridge.Core.Communication.Interfaces;

namespace AICharacterBridge.Core.Communication.Clients.LMStudio
{
    /// <summary>
    /// LM Studio の /v1/responses レスポンスからメッセージを抽出する実装。
    /// Implementation for extracting messages from LM Studio's /v1/responses API responses.
    ///
    /// レスポンス構造 / Response structure:
    /// {
    ///   "output": [
    ///     {
    ///       "type": "reasoning",
    ///       "content": [
    ///         { "type": "reasoning_text", "text": "(思考過程 / reasoning text)" }
    ///       ]
    ///     },
    ///     {
    ///       "type": "message",
    ///       "content": [
    ///         { "type": "output_text", "text": "AIの回答テキスト / AI response text" }
    ///       ]
    ///     }
    ///   ]
    /// }
    ///
    /// 注意 / Note:
    ///   推論(Thinking)機能を持つモデルを使用する場合、"output" 配列には
    ///   "type": "reasoning" の要素（思考過程）が "type": "message" の要素（本回答）より
    ///   前に挿入されることがある。
    ///   そのため output[0] を無条件に本回答とみなすと、思考過程のテキストを
    ///   誤って抽出してしまう。
    ///   本実装では "type" フィールドを確認し、"message" 要素の中から
    ///   "output_text" タイプの content を明示的に探して抽出する。
    ///
    ///   When using a model with reasoning/"thinking" capability, the "output" array
    ///   may contain a "type": "reasoning" element (the reasoning/thinking text)
    ///   before the "type": "message" element (the actual answer).
    ///   Blindly treating output[0] as the final answer would therefore extract
    ///   the reasoning text by mistake.
    ///   This implementation explicitly looks for the "message" type element,
    ///   and within it, the "output_text" type content.
    /// </summary>
    public class LMStudioResponseExtractor : IResponseExtractor
    {
        /// <inheritdoc/>
        public string GetName() => "LM Studio";

        /// <inheritdoc/>
        public string ExtractMessage(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse))
                throw new ArgumentException("Raw response cannot be null or empty.", nameof(rawResponse));

            try
            {
                var json = JObject.Parse(rawResponse);

                // エラーフィールドのチェック
                // Check for error field
                var errorToken = json["error"];
                if (errorToken != null && errorToken.Type != JTokenType.Null)
                    throw new Exception($"LM Studio API Error: {json["error"]}");

                // output 配列の取得
                // Get output array
                var output = json["output"] as JArray;
                if (output == null || output.Count == 0)
                    throw new Exception(
                        "LM Studio response 'output' array is missing or empty.");

                // "type": "message" の要素を探す（"reasoning" 等の要素は無視する）
                // Find the element with "type": "message" (ignoring "reasoning" etc.)
                JObject messageItem = FindOutputItemByType(output, "message");

                if (messageItem == null)
                {
                    // "message" が見つからない場合、後方互換のため output[0] にフォールバックする。
                    // Fall back to output[0] for backward compatibility if "message" is not found.
                    AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                        "[LMStudioResponseExtractor] No output item with type 'message' found. " +
                        "Falling back to output[0]. This may indicate the response only contains " +
                        "reasoning content (e.g., truncated by max_output_tokens).");

                    messageItem = output[0] as JObject;
                }

                if (messageItem == null)
                    throw new Exception(
                        "LM Studio response 'output[0]' is not a valid object.");

                // message要素のcontent配列を取得
                // Get the content array of the message item
                var content = messageItem["content"] as JArray;
                if (content == null || content.Count == 0)
                    throw new Exception(
                        "LM Studio response message item's 'content' array is missing or empty.");

                // "type": "output_text" の content を探す（見つからない場合は先頭要素にフォールバック）
                // Find content with "type": "output_text" (fall back to the first element if not found)
                string text = FindTextByContentType(content, "output_text");

                if (string.IsNullOrEmpty(text))
                    throw new Exception(
                        "LM Studio response message item's text content is missing or empty. " +
                        "This may indicate the response was truncated before the final answer " +
                        "was generated (e.g., max_output_tokens reached during reasoning). " +
                        "Try increasing 'max_output_tokens' in the LLM Options.");

                return text;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to extract message from LM Studio response: {ex.Message}" +
                    $"\nRaw response: {rawResponse}",
                    ex);
            }
        }

        // =====================================================================
        // Private helpers
        // =====================================================================

        /// <summary>
        /// output 配列から指定した type を持つ要素を検索します。
        /// 見つからない場合は null を返します。
        /// Searches the output array for an item with the specified type.
        /// Returns null if not found.
        /// </summary>
        private static JObject FindOutputItemByType(JArray output, string type)
        {
            foreach (var item in output)
            {
                if (item is JObject obj && obj["type"]?.ToString() == type)
                    return obj;
            }
            return null;
        }

        /// <summary>
        /// content 配列から指定した type を持つ要素の text を検索します。
        /// 見つからない場合、content[0] の text にフォールバックします。
        /// Searches the content array for an item with the specified type and returns its text.
        /// Falls back to content[0]'s text if not found.
        /// </summary>
        private static string FindTextByContentType(JArray content, string type)
        {
            foreach (var item in content)
            {
                if (item is JObject obj && obj["type"]?.ToString() == type)
                    return obj["text"]?.ToString();
            }

            // フォールバック: 先頭要素の text を使用
            // Fallback: use the first element's text
            return content[0]?["text"]?.ToString();
        }
    }
}
