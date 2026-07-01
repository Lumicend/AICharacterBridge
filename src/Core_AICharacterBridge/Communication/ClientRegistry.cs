using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using AICharacterBridge.Core.Communication.Interfaces;
using AICharacterBridge.Core.Communication.Clients.Ollama;
using AICharacterBridge.Core.Communication.Clients.LMStudio;

namespace AICharacterBridge.Core.Communication
{
    /// <summary>
    /// すべてのクライアントプロバイダーを管理するレジストリ。
    /// Registry that manages all client providers.
    /// </summary>
    public static class ClientRegistry
    {
        private static readonly Dictionary<string, IClientProvider> _providers;

        static ClientRegistry()
        {
            _providers = new Dictionary<string, IClientProvider>();

            // デフォルトクライアントを手動登録
            // Manually register default clients
            RegisterProvider(new OllamaClientProvider());
            RegisterProvider(new LMStudioClientProvider());

            // 将来的に他のクライアントを追加する場合はここに追記
            // To add more clients in the future, add them here
            // RegisterProvider(new OpenAIClientProvider());
            // RegisterProvider(new ClaudeClientProvider());
        }

        /// <summary>
        /// プロバイダーを登録します。
        /// Registers a provider.
        /// </summary>
        /// <param name="provider">登録するプロバイダー / Provider to register</param>
        public static void RegisterProvider(IClientProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            string name = provider.GetName();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Provider name cannot be null or empty.");

            if (_providers.ContainsKey(name))
                return;

            _providers[name] = provider;
        }

        /// <summary>
        /// すべてのプロバイダーの設定を一括登録します。
        /// Registers configurations for all providers.
        /// </summary>
        /// <param name="config">BepInEx設定ファイル / BepInEx config file</param>
        public static void RegisterAllConfigurationsTo(ConfigFile config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            foreach (var provider in _providers.Values)
            {
                provider.RegisterConfiguration(config);
            }
        }

        /// <summary>
        /// 利用可能なクライアント名を取得します。
        /// Gets available client names.
        /// </summary>
        /// <returns>クライアント名の配列 / Array of client names</returns>
        public static string[] GetAvailableClientNames()
        {
            var names = new string[_providers.Keys.Count];
            _providers.Keys.CopyTo(names, 0);
            return names;
        }

        /// <summary>
        /// 指定した名前のプロバイダーを取得します。
        /// Gets a provider by name.
        /// </summary>
        /// <param name="name">クライアント名 / Client name</param>
        /// <returns>プロバイダーインスタンス / Provider instance</returns>
        public static IClientProvider GetProvider(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Client name cannot be null or empty.", nameof(name));

            if (!_providers.TryGetValue(name, out var provider))
            {
                var availableNames = GetAvailableClientNames();
                throw new KeyNotFoundException(
                    $"Client provider '{name}' not found. " +
                    $"Available providers: {string.Join(", ", availableNames)}");
            }

            return provider;
        }

        /// <summary>
        /// プロンプトを送信します（プロバイダーへ委譲）。
        /// Sends a prompt (delegates to provider).
        /// </summary>
        /// <param name="clientName">使用するクライアント名 / Client name to use</param>
        /// <param name="prompt">送信するプロンプト文字列 / Prompt string to send</param>
        /// <param name="onSuccess">成功時に呼び出されるコールバック / Callback invoked on success</param>
        /// <param name="onError">エラー時に呼び出されるコールバック / Callback invoked on error</param>
        public static IEnumerator SendPrompt(
            string clientName,
            string prompt,
            Action<string> onSuccess,
            Action<Exception> onError)
        {
            IClientProvider provider = null;

            try
            {
                provider = GetProvider(clientName);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                yield break;
            }

            yield return provider.SendPrompt(prompt, onSuccess, onError);
        }
    }
}
