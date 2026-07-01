using System;
using Newtonsoft.Json;

namespace AICharacterBridge.TalkSceneChat.Data
{
    /// <summary>
    /// TalkSceneChatモジュールにおけるヒロイン固有の設定データ。
    /// Per-heroine settings data for the TalkSceneChat module.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroineChatSettings
    {
        /// <summary>
        /// Game.Instance.HeroineList 内のインデックス（シリアライズ用）。
        /// Index in Game.Instance.HeroineList (for serialization).
        /// </summary>
        [JsonProperty("heroine_index")]
        public int HeroineIndex { get; set; }

        /// <summary>
        /// 現在の状況に関するユーザー補足メモ。
        /// プロンプトの {{context_note}} プレースホルダーに展開される。
        /// User-supplied supplementary note about the current context.
        /// Expanded into the {{context_note}} placeholder in the prompt.
        /// </summary>
        [JsonProperty("context_note")]
        public string ContextNote { get; set; }

        /// <summary>
        /// デフォルト値でインスタンスを初期化します。
        /// Initializes the instance with default values.
        /// </summary>
        public HeroineChatSettings()
        {
            HeroineIndex = -1;
            ContextNote = "";
        }

        /// <summary>
        /// 指定インデックスでインスタンスを初期化します。
        /// Initializes the instance with the specified index.
        /// </summary>
        /// <param name="heroineIndex">ヒロインのインデックス / Heroine index</param>
        public HeroineChatSettings(int heroineIndex)
        {
            HeroineIndex = heroineIndex;
            ContextNote = "";
        }

        /// <summary>
        /// このデータが保存する価値を持つか（空でないか）を判定します。
        /// Determines whether this data is worth persisting (i.e., not empty).
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(ContextNote);
        }

        public override string ToString()
        {
            return $"HeroineChatSettings [Index: {HeroineIndex}, HasContextNote: {!string.IsNullOrEmpty(ContextNote)}]";
        }
    }
}
