using System;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// 会話エントリーの種類を示すEnum。
    /// Enum indicating the type of conversation entry.
    /// </summary>
    [Serializable]
    public enum ConversationEntryType
    {
        /// <summary>チャットメッセージ（ユーザーまたはキャラクターの発言）</summary>
        Chat,

        /// <summary>アクション（特殊なアクションの実行）</summary>
        Action

        // 将来的な拡張:
        // SystemMessage,  // システムメッセージ
        // Observation,    // ナレーション・観察
        // Emotion         // 感情の変化
    }
}
