using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AICharacterBridge.Core.Prompt
{
    /// <summary>
    /// プロンプトテンプレートの置換エントリーを表すクラス。
    /// 通常置換とタグ付き置換（インライン／ブロック）の両方をサポートします。
    /// Represents a replacement entry for prompt templates.
    /// Supports plain replacements and tagged replacements (inline or block).
    /// </summary>
    public class ReplaceEntry
    {
        // =====================================================================
        // Properties
        // =====================================================================

        /// <summary>
        /// プレースホルダーのキー（例: "user_name"）。
        /// Placeholder key (e.g., "user_name").
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 置換する値。
        /// Value to replace with.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// タグ付き置換かどうか。
        /// Whether this is a tagged replacement.
        /// </summary>
        public bool IsTagged { get; }

        /// <summary>
        /// タグ名（IsTagged == true のときのみ使用）。
        /// Tag name (used only when IsTagged is true).
        /// </summary>
        public string TagName { get; }

        /// <summary>
        /// タグの note 属性（オプション）。
        /// Tag note attribute (optional).
        /// </summary>
        public string Note { get; }

        /// <summary>
        /// ブロック形式のタグ付き置換かどうか。
        /// true のとき、開タグと閉タグの間に改行を挿入する。
        ///
        ///   block=false (インライン): &lt;tag&gt;value&lt;/tag&gt;
        ///   block=true  (ブロック):
        ///     &lt;tag&gt;
        ///     value
        ///     &lt;/tag&gt;
        ///
        /// Whether to use block-style tag formatting.
        /// When true, newlines are inserted between the tag and its content.
        /// </summary>
        public bool IsBlock { get; }

        // =====================================================================
        // Constructor (private)
        // =====================================================================

        private ReplaceEntry(string key, string value, bool isTagged, string tagName, string note, bool isBlock)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value ?? "";
            IsTagged = isTagged;
            TagName = tagName;
            Note = note;
            IsBlock = isBlock;
        }

        // =====================================================================
        // Factory methods
        // =====================================================================

        /// <summary>
        /// 通常置換エントリーを生成します。
        /// Creates a plain replacement entry.
        /// </summary>
        /// <param name="key">プレースホルダーのキー / Placeholder key</param>
        /// <param name="value">置換する値 / Replacement value</param>
        /// <returns>通常置換エントリー / Plain replacement entry</returns>
        public static ReplaceEntry Plain(string key, string value)
            => new ReplaceEntry(key, value ?? "", false, null, null, false);

        /// <summary>
        /// タグ付き置換エントリーを生成します（note なし）。
        /// 値が null または空文字の場合、プレースホルダーを含む行ごと削除します。
        /// Creates a tagged replacement entry (without note).
        /// If value is null or empty, removes the placeholder line including surrounding newlines.
        /// </summary>
        /// <param name="key">プレースホルダーのキー / Placeholder key</param>
        /// <param name="value">置換する値（null または空文字の場合、プレースホルダーを含む行ごと削除）
        ///   Replacement value (null or empty removes the placeholder line)</param>
        /// <param name="tagName">タグ名 / Tag name</param>
        /// <param name="block">
        ///   true（デフォルト）: ブロック形式（開閉タグ間に改行を挿入）。
        ///   false: インライン形式（改行なし）。
        ///   true (default): block format (newlines between tags and content).
        ///   false: inline format (no newlines).
        /// </param>
        /// <returns>タグ付き置換エントリー / Tagged replacement entry</returns>
        public static ReplaceEntry Tagged(string key, string value, string tagName, bool block = true)
            => new ReplaceEntry(key, value ?? "", true, tagName ?? key, null, block);

        /// <summary>
        /// タグ付き置換エントリーを生成します（note あり）。
        /// 値が null または空文字の場合、プレースホルダーを含む行ごと削除します。
        /// Creates a tagged replacement entry (with note).
        /// If value is null or empty, removes the placeholder line including surrounding newlines.
        /// </summary>
        /// <param name="key">プレースホルダーのキー / Placeholder key</param>
        /// <param name="value">置換する値（null または空文字の場合、プレースホルダーを含む行ごと削除）
        ///   Replacement value (null or empty removes the placeholder line)</param>
        /// <param name="tagName">タグ名 / Tag name</param>
        /// <param name="note">タグの note 属性 / Tag note attribute</param>
        /// <param name="block">
        ///   true（デフォルト）: ブロック形式（開閉タグ間に改行を挿入）。
        ///   false: インライン形式（改行なし）。
        ///   true (default): block format (newlines between tags and content).
        ///   false: inline format (no newlines).
        /// </param>
        /// <returns>タグ付き置換エントリー / Tagged replacement entry</returns>
        public static ReplaceEntry Tagged(string key, string value, string tagName, string note, bool block = true)
            => new ReplaceEntry(key, value ?? "", true, tagName ?? key, note, block);

        // =====================================================================
        // Utilities
        // =====================================================================

        /// <summary>
        /// 文字列リストを JSON 配列形式にフォーマットします。
        /// Formats a string list into JSON array format.
        /// </summary>
        /// <param name="items">文字列のリスト / List of strings</param>
        /// <returns>JSON 配列形式の文字列（例: ["item1", "item2"]）/ JSON array string</returns>
        public static string FormatStringListAsJson(List<string> items)
        {
            if (items == null || items.Count == 0)
                return "[]";

            var filtered = new List<string>();
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item))
                    filtered.Add(item);
            }

            return JsonConvert.SerializeObject(filtered, Formatting.Indented);
        }

        /// <summary>デバッグ用文字列表現 / Debug string representation</summary>
        public override string ToString()
        {
            if (IsTagged)
            {
                string format = IsBlock ? "block" : "inline";
                return $"[Tagged/{format}] {{{{{Key}}}}} -> <{TagName}>{Value}</{TagName}>";
            }
            return $"[Plain]  {{{{{Key}}}}} -> \"{Value}\"";
        }
    }
}
