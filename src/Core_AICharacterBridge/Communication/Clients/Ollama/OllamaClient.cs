using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AICharacterBridge.Core.Communication.Interfaces;

namespace AICharacterBridge.Core.Communication.Clients.Ollama
{
    /// <summary>
    /// Ollama APIと通信するための具体的なクライアント実装。
    /// A concrete client implementation for communicating with the Ollama API.
    /// </summary>
    public class OllamaClient : ICommunicationClient
    {
        private readonly Uri _endpoint;

        private string _model;
        private int _timeoutSeconds;

        /// <summary>
        /// LLM生成オプション。
        /// Configure() で設定された JObject をそのまま保持する。
        /// LLM generation options.
        /// Holds the JObject set via Configure() as-is.
        /// </summary>
        private JObject _llmOptions;

        /// <summary>
        /// think オプションの値。
        /// "Default": ペイロードに含めない / Not included in payload.
        /// "True":    ["think"] = true をトップレベルに追加 / Added as top-level ["think"] = true.
        /// "False":   ["think"] = false をトップレベルに追加 / Added as top-level ["think"] = false.
        /// </summary>
        private string _thinkOption;

        public OllamaClient() : this("http://localhost:11434/api/generate")
        {
        }

        public OllamaClient(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentNullException(nameof(endpoint));

            _endpoint = new Uri(endpoint);
            _model = "";
            _timeoutSeconds = 300;
            _llmOptions = new JObject();
            _thinkOption = "Default";
        }

        public string GetName() => "Ollama";

        /// <inheritdoc/>
        public void Configure(string model, int timeoutSeconds, JObject llmOptions)
        {
            if (string.IsNullOrEmpty(model))
                throw new ArgumentException("Model cannot be null or empty.", nameof(model));

            if (timeoutSeconds <= 0)
                throw new ArgumentException("TimeoutSeconds must be greater than 0.", nameof(timeoutSeconds));

            _model = model;
            _timeoutSeconds = timeoutSeconds;
            _llmOptions = llmOptions ?? new JObject();
        }

        /// <summary>
        /// think オプションを設定します。
        /// Sets the think option.
        /// </summary>
        /// <param name="thinkOption">
        /// "Default": ペイロードに含めない / Not included in payload.
        /// "True":    トップレベルに ["think"] = true を追加 / Adds top-level ["think"] = true.
        /// "False":   トップレベルに ["think"] = false を追加 / Adds top-level ["think"] = false.
        /// </param>
        public void SetThinkOption(string thinkOption)
        {
            _thinkOption = thinkOption ?? "Default";
        }

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
                    string errorMessage = $"Ollama Error (Code {webRequest.responseCode}): {webRequest.error}";

                    if (!string.IsNullOrEmpty(webRequest.downloadHandler?.text))
                        errorMessage += $"\nResponse: {webRequest.downloadHandler.text}";

                    onError?.Invoke(new Exception(errorMessage));
                }
                else
                {
                    string response = webRequest.downloadHandler?.text;
                    if (string.IsNullOrEmpty(response))
                        onError?.Invoke(new Exception("Received empty response from Ollama."));
                    else
                        onSuccess?.Invoke(response);
                }
            }
        }

        /// <summary>
        /// プロンプトから JSON ペイロードを構築します。
        /// LLMオプションは "options" オブジェクトにネストして格納します。
        /// think オプションは "Default" 以外の場合にトップレベルフィールドとして追加します。
        /// Builds the JSON payload from the prompt.
        /// LLM options are nested under the "options" object.
        /// The think option is added as a top-level field when not "Default".
        /// </summary>
        private string BuildPayload(string prompt)
        {
            var payload = new JObject
            {
                ["model"] = _model,
                ["prompt"] = prompt,
                ["stream"] = false
            };

            // think オプションを "Default" 以外の場合にトップレベルに追加する
            // Add think option as a top-level field when not "Default"
            if (_thinkOption == "True")
                payload["think"] = true;
            else if (_thinkOption == "False")
                payload["think"] = false;

            // LLMオプションを "options" オブジェクトとしてネストする
            // Nest LLM options as the "options" object
            if (_llmOptions != null && _llmOptions.Count > 0)
                payload["options"] = _llmOptions;

            return payload.ToString(Formatting.None);
        }
    }
}
