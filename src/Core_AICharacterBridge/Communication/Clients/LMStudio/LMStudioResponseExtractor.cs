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
    ///       "type": "message",
    ///       "content": [
    ///         {
    ///           "type": "output_text",
    ///           "text": "AIの回答テキスト / AI response text"
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }
    ///
    /// 取り出しパス: output[0].content[0].text
    /// Extraction path: output[0].content[0].text
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

                // output[0].content 配列の取得
                // Get output[0].content array
                var content = output[0]["content"] as JArray;
                if (content == null || content.Count == 0)
                    throw new Exception(
                        "LM Studio response 'output[0].content' array is missing or empty.");

                // output[0].content[0].text の取得
                // Get output[0].content[0].text
                string text = content[0]["text"]?.ToString();
                if (string.IsNullOrEmpty(text))
                    throw new Exception(
                        "LM Studio response 'output[0].content[0].text' is missing or empty.");

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
    }
}
