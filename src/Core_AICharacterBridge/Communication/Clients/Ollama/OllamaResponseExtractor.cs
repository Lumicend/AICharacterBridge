using System;
using Newtonsoft.Json.Linq;
using AICharacterBridge.Core.Communication.Interfaces;

namespace AICharacterBridge.Core.Communication.Clients.Ollama
{
    /// <summary>
    /// Implementation for extracting messages from Ollama API responses.
    /// </summary>
    public class OllamaResponseExtractor : IResponseExtractor
    {
        public string GetName() => "Ollama";

        public string ExtractMessage(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse))
                throw new ArgumentException("Raw response cannot be null or empty.", nameof(rawResponse));

            try
            {
                var json = JObject.Parse(rawResponse);

                // エラーチェック
                var errorToken = json["error"];
                if (errorToken != null && errorToken.Type != JTokenType.Null)
                {
                    throw new Exception($"Ollama API Error: {json["error"]}");
                }

                // レスポンスの取得
                if (json["response"] != null)
                {
                    string message = json["response"].ToString();

                    if (string.IsNullOrEmpty(message))
                        throw new Exception("Ollama response contains empty message.");

                    return message;
                }

                // responseフィールドがない場合は生のレスポンスを返す
                return rawResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract message from Ollama response: {ex.Message}\nRaw response: {rawResponse}", ex);
            }
        }
    }
}
