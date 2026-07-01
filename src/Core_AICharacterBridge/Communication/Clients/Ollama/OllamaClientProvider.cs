using AICharacterBridge.Core.Communication.Interfaces;
using AICharacterBridge.Core.Utilities;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;

namespace AICharacterBridge.Core.Communication.Clients.Ollama
{
    /// <summary>
    /// Ollamaクライアントのプロバイダー実装。
    /// 設定・初期化・通信を自己完結的に管理します。
    /// Ollama client provider implementation.
    /// Manages configuration, initialization, and communication in a self-contained manner.
    /// </summary>
    public class OllamaClientProvider : IClientProvider
    {
        private readonly OllamaClient _client;
        private readonly OllamaResponseExtractor _extractor;

        public static ConfigEntry<string> Model { get; private set; }
        public static ConfigEntry<int> TimeoutSeconds { get; private set; }

        /// <summary>
        /// think オプションの設定。
        /// Think option configuration.
        /// "Default": リクエストに含めない / Not included in request.
        /// "True":    ["think"] = true をトップレベルに追加 / Adds top-level ["think"] = true.
        /// "False":   ["think"] = false をトップレベルに追加 / Adds top-level ["think"] = false.
        /// </summary>
        public static ConfigEntry<string> ThinkOption { get; private set; }

        /// <summary>
        /// LLM生成オプション（JSON形式、中括弧なし）。
        /// Ollama API の "options" オブジェクトの中身をそのまま記述します。
        /// LLM generation options in JSON format (without enclosing braces).
        /// Write the contents of Ollama API's "options" object directly.
        /// 例 / Example:
        ///   "top_k": 40,
        ///   "top_p": 0.9,
        ///   "temperature": 0.8
        /// </summary>
        public static ConfigEntry<string> LlmOptionsText { get; private set; }

        public OllamaClientProvider()
        {
            _client = new OllamaClient();
            _extractor = new OllamaResponseExtractor();
        }

        public string GetName() => "Ollama";

        public void RegisterConfiguration(ConfigFile config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Model = config.Bind(
                "Client - Ollama",
                "1. Model Name",
                "",
                "The name of the AI model to use (e.g., 'gemma:2b', 'llama3:8b').");

            TimeoutSeconds = config.Bind(
                "Client - Ollama",
                "2. Timeout (seconds)",
                300,
                "Request timeout in seconds. Default: 300 (5 minutes).");

            // think オプション: "Default" / "True" / "False" のドロップダウン選択
            // Think option: dropdown selection from "Default" / "True" / "False"
            ThinkOption = config.Bind(
                "Client - Ollama",
                "3. Think Option",
                "Default",
                new ConfigDescription(
                    "Controls the 'think' field in the Ollama request (top-level field).\n" +
                    "Default: Not included in the request.\n" +
                    "True:    Adds [\"think\"] = true to the request.\n" +
                    "False:   Adds [\"think\"] = false to the request.\n" +
                    "Useful for models that support extended thinking (e.g., QwQ, DeepSeek-R1).",
                    new AcceptableValueList<string>("Default", "True", "False")));

            LlmOptionsText = config.Bind(
                "Client - Ollama",
                "4. LLM Options",
                "",
                new ConfigDescription(
                    "LLM generation options in JSON format (without enclosing braces).\n" +
                    "These are passed as the contents of Ollama API's \"options\" object.\n" +
                    "Example:\n" +
                    "\"top_k\": 40,\n" +
                    "\"top_p\": 0.9,\n" +
                    "\"temperature\": 0.8",
                    null,
                    new ConfigurationManagerAttributes { CustomDrawer = TextAreaDrawer }
                )
            );
        }

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

            _client.Configure(Model.Value, TimeoutSeconds.Value, options);

            // think オプションをクライアントに設定する
            // Apply the think option to the client
            _client.SetThinkOption(ThinkOption.Value);

            // 通信実行
            // Execute communication
            string rawResponse = null;
            Exception error = null;

            yield return _client.Post(
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
                onError?.Invoke(new Exception("Received null response from Ollama."));
            }
        }

        // =====================================================================
        // Private helpers
        // =====================================================================

        /// <summary>
        /// 設定の妥当性を検証します。
        /// Validates configuration.
        /// </summary>
        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(Model.Value))
                throw new ArgumentException("Model name cannot be null or empty.");

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
