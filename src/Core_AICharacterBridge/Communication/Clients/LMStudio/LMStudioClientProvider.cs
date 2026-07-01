using AICharacterBridge.Core.Communication.Interfaces;
using AICharacterBridge.Core.Utilities;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;

namespace AICharacterBridge.Core.Communication.Clients.LMStudio
{
    /// <summary>
    /// LM Studio クライアントのプロバイダー実装。
    /// 設定・初期化・通信を自己完結的に管理します。
    /// LM Studio client provider implementation.
    /// Manages configuration, initialization, and communication in a self-contained manner.
    ///
    /// エンドポイント / Endpoint:
    ///   {BaseUrl}/v1/responses
    ///
    /// LLMオプションの注意点 / Note on LLM options:
    ///   トークン上限は "max_tokens" ではなく "max_output_tokens" を使用してください。
    ///   Use "max_output_tokens" (not "max_tokens") for the token limit.
    /// </summary>
    public class LMStudioClientProvider : IClientProvider
    {
        private readonly LMStudioResponseExtractor _extractor;

        /// <summary>
        /// LM Studio サーバーのベースURL。
        /// Base URL of the LM Studio server.
        /// </summary>
        public static ConfigEntry<string> BaseUrl { get; private set; }

        /// <summary>
        /// 使用するモデル名。空欄の場合は LM Studio が起動中のモデルを自動で使用します。
        /// Model name to use. If empty, LM Studio automatically uses the currently loaded model.
        /// </summary>
        public static ConfigEntry<string> Model { get; private set; }

        /// <summary>
        /// リクエストのタイムアウト秒数。
        /// Request timeout in seconds.
        /// </summary>
        public static ConfigEntry<int> TimeoutSeconds { get; private set; }

        /// <summary>
        /// LLM生成オプション（JSON形式、中括弧なし）。
        /// LM Studio API の /v1/responses エンドポイントへのトップレベルフィールドとして展開されます。
        /// トークン上限は "max_tokens" ではなく "max_output_tokens" を使用してください。
        /// LLM generation options in JSON format (without enclosing braces).
        /// Flattened as top-level fields in the /v1/responses endpoint request.
        /// Use "max_output_tokens" (not "max_tokens") for the token limit.
        /// 例 / Example:
        ///   "temperature": 0.8,
        ///   "top_p": 0.9,
        ///   "max_output_tokens": 500
        /// </summary>
        public static ConfigEntry<string> LlmOptionsText { get; private set; }

        /// <summary>
        /// コンストラクタ。
        /// Constructor.
        /// </summary>
        public LMStudioClientProvider()
        {
            _extractor = new LMStudioResponseExtractor();
        }

        /// <inheritdoc/>
        public string GetName() => "LM Studio";

        /// <inheritdoc/>
        public void RegisterConfiguration(ConfigFile config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            BaseUrl = config.Bind(
                "Client - LM Studio",
                "1. Base URL",
                "http://localhost:1234",
                "The base URL of the LM Studio local server (e.g., 'http://localhost:1234').");

            Model = config.Bind(
                "Client - LM Studio",
                "2. Model Name",
                "",
                "The name of the AI model to use. " +
                "Can be left empty; LM Studio will use whatever model is currently loaded.");

            TimeoutSeconds = config.Bind(
                "Client - LM Studio",
                "3. Timeout (seconds)",
                300,
                "Request timeout in seconds. Default: 300 (5 minutes).");

            LlmOptionsText = config.Bind(
                "Client - LM Studio",
                "4. LLM Options",
                "",
                new ConfigDescription(
                    "LLM generation options in JSON format (without enclosing braces).\n" +
                    "These are added as top-level fields in the /v1/responses request.\n" +
                    "Note: Use 'max_output_tokens' (not 'max_tokens') for the token limit.\n" +
                    "Example:\n" +
                    "\"temperature\": 0.8,\n" +
                    "\"top_p\": 0.9,\n" +
                    "\"max_output_tokens\": 500",
                    null,
                    new ConfigurationManagerAttributes { CustomDrawer = TextAreaDrawer }
                )
            );
        }

        /// <inheritdoc/>
        public IEnumerator SendPrompt(
            string prompt,
            Action<string> onSuccess,
            Action<Exception> onError)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                onError?.Invoke(new ArgumentException("Prompt cannot be null or empty.", nameof(prompt)));
                yield break;
            }

            // 設定の検証
            // Validate configuration
            try
            {
                ValidateConfiguration();
            }
            catch (Exception ex)
            {
                onError?.Invoke(new Exception($"Invalid configuration: {ex.Message}", ex));
                yield break;
            }

            // LLMオプションをパースしてクライアントを構成する
            // Parse LLM options and configure the client
            JObject options;
            try
            {
                options = ParseLlmOptions(LlmOptionsText.Value);
            }
            catch (Exception ex)
            {
                onError?.Invoke(new Exception($"Failed to parse LLM options: {ex.Message}", ex));
                yield break;
            }

            // 現在の BaseUrl からエンドポイント URL を構築し、クライアントを生成する
            // BaseUrl は実行時に変更される可能性があるため、毎回インスタンスを生成する
            // Build the endpoint URL from the current BaseUrl and create a client.
            // A new instance is created each call because BaseUrl may change at runtime.
            string endpoint = BuildEndpointUrl(BaseUrl.Value);
            var client = new LMStudioClient(endpoint);
            client.Configure(Model.Value, TimeoutSeconds.Value, options);

            // 通信実行
            // Execute communication
            string rawResponse = null;
            Exception error = null;

            yield return client.Post(
                prompt,
                response => rawResponse = response,
                ex => error = ex);

            // 結果処理
            // Process result
            if (error != null)
            {
                onError?.Invoke(error);
            }
            else if (rawResponse != null)
            {
                try
                {
                    string message = _extractor.ExtractMessage(rawResponse);
                    onSuccess?.Invoke(message);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(new Exception($"Failed to extract message: {ex.Message}", ex));
                }
            }
            else
            {
                onError?.Invoke(new Exception("Received null response from LM Studio."));
            }
        }

        // =====================================================================
        // Private helpers
        // =====================================================================

        /// <summary>
        /// BaseUrl からエンドポイント URL を構築します。
        /// Builds the full endpoint URL from BaseUrl.
        /// 例 / Example: "http://localhost:1234" → "http://localhost:1234/v1/responses"
        /// </summary>
        private string BuildEndpointUrl(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                baseUrl = "http://localhost:1234";

            return baseUrl.TrimEnd('/') + "/v1/responses";
        }

        /// <summary>
        /// 設定の妥当性を検証します。
        /// Validates configuration.
        /// </summary>
        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(BaseUrl.Value))
                throw new ArgumentException("Base URL cannot be null or empty.");

            if (TimeoutSeconds.Value <= 0)
                throw new ArgumentException("Timeout seconds must be greater than 0.");
        }

        /// <summary>
        /// テキスト形式（中括弧なしのJSONキー・バリュー列）を JObject にパースします。
        /// 入力文字列の前後に "{" と "}" を補ってからパースします。
        /// Parses a text string (JSON key-value pairs without enclosing braces) into a JObject.
        /// Wraps the input with "{" and "}" before parsing.
        /// </summary>
        /// <param name="optionsText">
        /// 中括弧なしのJSONテキスト（例: "\"temperature\": 0.8,\n\"top_p\": 0.9"）
        /// JSON text without enclosing braces (e.g., "\"temperature\": 0.8,\n\"top_p\": 0.9")
        /// </param>
        /// <returns>パース済みの JObject。入力が空の場合は空の JObject。</returns>
        private JObject ParseLlmOptions(string optionsText)
        {
            if (string.IsNullOrEmpty(optionsText) || string.IsNullOrEmpty(optionsText.Trim()))
                return new JObject();

            string json = "{" + optionsText.Trim() + "}";
            return JObject.Parse(json);
        }

        /// <summary>
        /// ConfigurationManager 用のテキストエリア描画メソッド。
        /// Text area drawer for ConfigurationManager.
        /// </summary>
        private static void TextAreaDrawer(ConfigEntryBase entry)
        {
            var text = (string)entry.BoxedValue;
            var newText = GUILayout.TextArea(text, GUILayout.Height(80));
            if (newText != text)
                entry.BoxedValue = newText;
        }
    }
}
