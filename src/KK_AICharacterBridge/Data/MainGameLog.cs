using System;
using Newtonsoft.Json;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// メインゲーム内でのヒロインに関連するログの基底クラス。
    /// Base class for logs related to a heroine in the main game.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class MainGameLog
    {
        /// <summary>ログを記録してから経過した(ゲーム内)日数</summary>
        [JsonProperty("elapsed_days")]
        public int ElapsedDays { get; set; }

        protected MainGameLog()
        {
            ElapsedDays = 0;
        }

        /// <summary>
        /// このログの型名を取得します。
        /// Gets the type name of this log.
        /// </summary>
        public virtual string GetLogTypeName()
        {
            return GetType().Name;
        }

        /// <summary>
        /// このログをAIプロンプト用の文字列にフォーマットします。
        /// Formats this log into a string for AI prompts.
        /// </summary>
        /// <param name="collection">このログを含むコレクション全体</param>
        /// <param name="index">このログのコレクション内でのインデックス</param>
        public abstract string FormatForPrompt(MainGameLogCollection collection, int index);

        /// <summary>
        /// ログの妥当性を検証します。
        /// Validates the log.
        /// </summary>
        public abstract bool IsValid();

        /// <summary>
        /// ディープコピーを作成します。
        /// Creates a deep copy.
        /// </summary>
        public abstract MainGameLog Clone();

        /// <summary>
        /// 経過日数を1日増やします。
        /// Increments the elapsed days by 1.
        /// </summary>
        public virtual void IncrementElapsedDays()
        {
            ElapsedDays++;
        }

        public override string ToString()
        {
            return $"[{GetLogTypeName()}] {ElapsedDays} days ago";
        }
    }
}
