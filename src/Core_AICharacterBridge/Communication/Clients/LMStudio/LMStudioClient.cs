using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AICharacterBridge.Core.Communication.Interfaces;

namespace AICharacterBridge.Core.Communication.Clients.LMStudio
{
    /// <summary>
    /// LM Studio の /v1/responses エンドポイントと通信するための具体的なクライアント実装。
    /// A concrete client implementation for communicating with LM Studio's /v1/responses endpoint.
    ///
    /// リクエスト形式 / Request format:
    ///   { "model": "...", "input": "prompt", "stream": false, "temperature": 0.8, ... }
    ///   LLMオプションはトップレベルフィールドとして展開される。
    ///   LLM options are flattened as top-level fields.
    ///
    /// レスポンス取り出しパス / Response extraction path:
    ///   output[0].content[0].text
    /// </summary>
    public class LMStudioClient : ICommunicationClient
    {
        private readonly Uri _endpoint;

        /// <summary>モデル名（空文字列を許容） / Model name (empty string allowed)</summary>
        private string _model;

        /// <summary>タイムアウト秒数 / Timeout in seconds</summary>
        private int _timeoutSeconds;

        /// <summary>
        /// LLM生成オプション。
        /// Configure() で設定された JObject をそのまま保持する。
        /// LLM generation options.
        /// Holds the JObject set via Configure() as-is.
        /// </summary>
        private JObject _llmOptions;

        /// <summary>
        /// デフォルトエンドポイントでインスタンスを生成します。
        /// Creates an instance with the default endpoint.
        /// </summary>
        public LMStudioClient() : this("http://localhost:1234/v1/responses")
        {
        }

        /// <summary>
        /// 指定エンドポイントでインスタンスを生成します。
        /// Creates an instance with the specified endpoint.
        /// </summary>
        /// <param name="endpoint">完全なエンドポイントURL / Full endpoint URL</param>
        public LMStudioClient(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentNullException(nameof(endpoint));

            _endpoint = new Uri(endpoint);
            _model = "";
            _timeoutSeconds = 300;
            _llmOptions = new JObject();
        }

        /// <inheritdoc/>
        public string GetName() => "LM Studio";

        /// <inheritdoc/>
        /// <remarks>
        /// Ollama と異なり、LM Studio ではモデル名を空文字列にできます。
        /// LM Studio が起動中のモデルを自動で使用するためです。
        /// Unlike Ollama, LM Studio allows an empty model name,
        /// because it automatically uses whatever model is currently loaded.
        /// </remarks>
        public void Configure(string model, int timeoutSeconds, JObject llmOptions)
        {
            if (timeoutSeconds <= 0)
                throw new ArgumentException("TimeoutSeconds must be greater than 0.", nameof(timeoutSeconds));

            _model = model ?? "";
            _timeoutSeconds = timeoutSeconds;
            _llmOptions = llmOptions ?? new JObject();
        }

        /// <inheritdoc/>
        public IEnumerator Post(string prompt, Action<string> onSuccess, Action<Exception> onError)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                onError?.Invoke(new ArgumentException("Prompt cannot be null or empty.", nameof(prompt)));
                yield break;
            }

            string payload = BuildPayload(prompt);

            using (var webRequest = new UnityWebRequest(_endpoint.ToString(), "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = _timeoutSeconds;

#pragma warning disable 0618
                yield return webRequest.Send();
#pragma warning restore 0618

#if UNITY_2017_1_OR_NEWER
                if (webRequest.isNetworkError || webRequest.isHttpError)
#else
                if (webRequest.isError)
#endif
                {
                    string errorMessage =
                        $"LM Studio Error (Code {webRequest.responseCode}): {webRequest.error}";

                    if (!string.IsNullOrEmpty(webRequest.downloadHandler?.text))
                        errorMessage += $"\nResponse: {webRequest.downloadHandler.text}";

                    onError?.Invoke(new Exception(errorMessage));
                }
                else
                {
                    string response = webRequest.downloadHandler?.text;
                    if (string.IsNullOrEmpty(response))
                        onError?.Invoke(new Exception("Received empty response from LM Studio."));
                    else
                        onSuccess?.Invoke(response);
                }
            }
        }

        /// <summary>
        /// プロンプトから JSON ペイロードを構築します。
        /// LLMオプションはトップレベルフィールドとして展開します。
        /// Builds the JSON payload from the prompt.
        /// LLM options are flattened as top-level fields.
        ///
        /// 注: /v1/responses はオプションをトップレベルフィールドとして受け取ります。
        ///     Ollama の /api/generate が "options" オブジェクトにネストするのとは異なります。
        /// Note: /v1/responses receives options as top-level fields,
        ///       unlike Ollama's /api/generate which nests them under "options".
        /// </summary>
        private string BuildPayload(string prompt)
        {
            var payload = new JObject
            {
                ["model"] = _model,
                ["input"] = prompt,
                ["stream"] = false   // ストリーミング固定無効 / Streaming always disabled
            };

            // LLMオプションをトップレベルに展開する
            // Flatten LLM options to top-level fields
            if (_llmOptions != null && _llmOptions.Count > 0)
            {
                foreach (var prop in _llmOptions.Properties())
                    payload[prop.Name] = prop.Value;
            }

            return payload.ToString(Formatting.None);
        }
    }
}
