using System;
using System.Collections;
using BepInEx.Configuration;

namespace AICharacterBridge.Core.Communication.Interfaces
{
    /// <summary>
    /// AIクライアントプロバイダーのインターフェース。
    /// 各クライアントが設定・初期化・通信を自己管理します。
    /// Interface for AI client providers.
    /// Each client manages its own configuration, initialization, and communication.
    /// </summary>
    public interface IClientProvider
    {
        /// <summary>
        /// クライアント名を取得します。
        /// Gets the client name.
        /// </summary>
        string GetName();

        /// <summary>
        /// 設定を登録します（初期化時に1回呼ばれます）。
        /// Registers configuration (called once during initialization).
        /// </summary>
        /// <param name="config">BepInEx設定ファイル</param>
        void RegisterConfiguration(ConfigFile config);

        /// <summary>
        /// プロンプトをAIに送信します。
        /// 設定は内部で自動適用されるため、呼び出し側はClientOptionsを意識しません。
        /// Sends a prompt to the AI.
        /// Settings are automatically applied internally, so callers don't need to worry about ClientOptions.
        /// </summary>
        /// <param name="prompt">送信するプロンプト文字列</param>
        /// <param name="onSuccess">成功時に呼び出されるコールバック（抽出されたメッセージを受け取る）</param>
        /// <param name="onError">エラー時に呼び出されるコールバック</param>
        IEnumerator SendPrompt(
            string prompt,
            Action<string> onSuccess,
            Action<Exception> onError);
    }
}
