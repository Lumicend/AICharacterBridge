using System;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace AICharacterBridge.Core.Communication.Interfaces
{
    /// <summary>
    /// AIサービスとの通信を行うクライアントのインターフェース。
    /// Interface for clients that communicate with an AI service.
    /// </summary>
    public interface ICommunicationClient
    {
        /// <summary>
        /// クライアントの名前を取得します。
        /// Gets the name of the client.
        /// </summary>
        string GetName();

        /// <summary>
        /// クライアントの設定を構成します。
        /// Configures the client settings.
        /// </summary>
        /// <param name="model">使用するモデル名 / Model name to use</param>
        /// <param name="timeoutSeconds">タイムアウト時間（秒）/ Timeout in seconds</param>
        /// <param name="llmOptions">
        /// LLM生成オプション（JObjectとして渡す）。
        /// null の場合は空のオプションとして扱われる。
        /// LLM generation options passed as a JObject.
        /// Treated as empty options when null.
        /// </param>
        void Configure(string model, int timeoutSeconds, JObject llmOptions);

        /// <summary>
        /// AIサービスにリクエストを送信し、レスポンスを取得します。
        /// 設定は事前にConfigure()で設定されている必要があります。
        /// Sends a request to the AI service and retrieves the response.
        /// Settings must be configured via Configure() beforehand.
        /// </summary>
        /// <param name="prompt">送信するプロンプト文字列 / Prompt string to send</param>
        /// <param name="onSuccess">成功時に呼び出されるコールバック / Callback invoked on success</param>
        /// <param name="onError">エラー時に呼び出されるコールバック / Callback invoked on error</param>
        IEnumerator Post(string prompt, Action<string> onSuccess, Action<Exception> onError);
    }
}
