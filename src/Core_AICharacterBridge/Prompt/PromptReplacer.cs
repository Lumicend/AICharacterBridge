using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AICharacterBridge.Core.Prompt
{
    /// <summary>
    /// プロンプトテンプレートの変数置換を行う静的クラス。
    /// Static class that performs variable replacement in prompt templates.
    ///
    /// 使用方法 / Usage:
    ///   単一置換     : PromptReplacer.Replace(template, key, value)
    ///   タグ付き置換 : PromptReplacer.ReplaceWithTag(template, key, value, tagName [, note] [, block])
    ///   一括置換     : PromptReplacer.ReplaceAll(template, entries)
    ///
    /// タグ付き置換について / About tagged replacement:
    ///   値が null または空文字の場合、プレースホルダーを含む行を前後の改行ごと削除します。
    ///   If the value is null or empty, the line containing the placeholder
    ///   (including surrounding newlines) is removed entirely.
    ///
    ///   block=false (インライン) : &lt;tag&gt;value&lt;/tag&gt;
    ///   block=true  (ブロック)  :
    ///     &lt;tag&gt;
    ///     value
    ///     &lt;/tag&gt;
    /// </summary>
    public static class PromptReplacer
    {
        // =====================================================================
        // Public API
        // =====================================================================

        /// <summary>
        /// テンプレート内の単一プレースホルダーを通常置換します。
        /// Replaces a single placeholder in the template with a plain value.
        /// </summary>
        /// <param name="template">プロンプトテンプレート文字列 / Prompt template string</param>
        /// <param name="key">プレースホルダーのキー（例: "user_name"）/ Placeholder key (e.g., "user_name")</param>
        /// <param name="value">置換する値 / Replacement value</param>
        /// <returns>置換済みプロンプト文字列 / Prompt string with placeholder replaced</returns>
        public static string Replace(string template, string key, string value)
        {
            ValidateTemplate(template);
            ValidateKey(key);

            return template.Replace("{{" + key + "}}", value ?? "");
        }

        /// <summary>
        /// テンプレート内の単一プレースホルダーをタグ付きで置換します（note なし）。
        /// 値が null または空文字の場合、プレースホルダーを含む行を前後の改行ごと削除します。
        /// Replaces a single placeholder in the template with a tagged value (without note).
        /// If value is null or empty, removes the placeholder line including surrounding newlines.
        /// </summary>
        /// <param name="template">プロンプトテンプレート文字列 / Prompt template string</param>
        /// <param name="key">プレースホルダーのキー / Placeholder key</param>
        /// <param name="value">置換する値（null または空文字で行削除）/ Replacement value (null or empty removes line)</param>
        /// <param name="tagName">タグ名 / Tag name</param>
        /// <param name="block">
        ///   true（デフォルト）: ブロック形式（開閉タグ間に改行を挿入）。
        ///   false: インライン形式。
        ///   true (default): block format. false: inline format.
        /// </param>
        /// <returns>置換済みプロンプト文字列 / Prompt string with placeholder replaced</returns>
        public static string ReplaceWithTag(string template, string key, string value, string tagName, bool block = true)
            => ReplaceWithTag(template, key, value, tagName, null, block);

        /// <summary>
        /// テンプレート内の単一プレースホルダーをタグ付きで置換します（note あり）。
        /// 値が null または空文字の場合、プレースホルダーを含む行を前後の改行ごと削除します。
        /// Replaces a single placeholder in the template with a tagged value (with note).
        /// If value is null or empty, removes the placeholder line including surrounding newlines.
        /// </summary>
        /// <param name="template">プロンプトテンプレート文字列 / Prompt template string</param>
        /// <param name="key">プレースホルダーのキー / Placeholder key</param>
        /// <param name="value">置換する値（null または空文字で行削除）/ Replacement value (null or empty removes line)</param>
        /// <param name="tagName">タグ名 / Tag name</param>
        /// <param name="note">タグの note 属性（null で省略）/ Tag note attribute (null to omit)</param>
        /// <param name="block">
        ///   true（デフォルト）: ブロック形式（開閉タグ間に改行を挿入）。
        ///   false: インライン形式。
        ///   true (default): block format. false: inline format.
        /// </param>
        /// <returns>置換済みプロンプト文字列 / Prompt string with placeholder replaced</returns>
        public static string ReplaceWithTag(string template, string key, string value, string tagName, string note, bool block = true)
        {
            ValidateTemplate(template);
            ValidateKey(key);

            // 値が空の場合: プレースホルダーを含む行（前後の改行を含む）を削除する
            // If value is empty: remove the line containing the placeholder (including surrounding newlines)
            if (string.IsNullOrEmpty(value))
                return RemovePlaceholderLine(template, key);

            // タグ付きの置換文字列を生成する
            // Generate the tagged replacement string
            string openTag = string.IsNullOrEmpty(note)
                ? $"<{tagName}>"
                : $"<{tagName} note=\"{note}\">";

            string closeTag = $"</{tagName}>";

            string replacement = block
                ? $"{openTag}\n{value}\n{closeTag}"
                : $"{openTag}{value}{closeTag}";

            return template.Replace("{{" + key + "}}", replacement);
        }

        /// <summary>
        /// 順序付き ReplaceEntry リストを使ってテンプレートの全プレースホルダーを一括置換します。
        /// リストの順序通りに置換が実行されます。
        /// Replaces all placeholders in the template using an ordered list of ReplaceEntry.
        /// Replacements are applied in the order they appear in the list.
        /// </summary>
        /// <param name="template">プロンプトテンプレート文字列 / Prompt template string</param>
        /// <param name="entries">順序付き置換エントリーリスト / Ordered list of replacement entries</param>
        /// <returns>全プレースホルダーが置換済みのプロンプト文字列 / Prompt string with all placeholders replaced</returns>
        public static string ReplaceAll(string template, List<ReplaceEntry> entries)
        {
            ValidateTemplate(template);

            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            string result = template;

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry?.Key))
                    continue;

                if (entry.IsTagged)
                    result = ReplaceWithTag(result, entry.Key, entry.Value, entry.TagName, entry.Note, entry.IsBlock);
                else
                    result = Replace(result, entry.Key, entry.Value);
            }

            return result;
        }

        // =====================================================================
        // Private helpers
        // =====================================================================

        /// <summary>
        /// テンプレートからプレースホルダーを含む行を前後の改行ごと削除します。
        /// 前後両方に改行がある場合は1つを保持し、空行の二重化を防ぎます。
        /// Removes the line containing the placeholder from the template,
        /// including surrounding newlines.
        /// If both sides have newlines, one is preserved to prevent double blank lines.
        /// </summary>
        private static string RemovePlaceholderLine(string template, string key)
        {
            // {{key}} を正規表現でエスケープしてパターンを構築する
            // Build a regex pattern with the escaped placeholder
            string escapedPlaceholder = Regex.Escape("{{" + key + "}}");
            string pattern = @"(\r\n|\r|\n)?" + escapedPlaceholder + @"(\r\n|\r|\n)?";

            return Regex.Replace(template, pattern, m =>
            {
                // 前後両方に改行がある場合は1つ残す（空行の二重化を防ぐ）
                // If both sides have newlines, keep one to prevent double blank lines
                if (m.Groups[1].Success && m.Groups[2].Success)
                    return m.Groups[1].Value;
                return "";
            });
        }

        /// <summary>テンプレートの検証 / Validates the template argument</summary>
        private static void ValidateTemplate(string template)
        {
            if (string.IsNullOrEmpty(template))
                throw new ArgumentException("Template cannot be null or empty.", nameof(template));
        }

        /// <summary>キーの検証 / Validates the key argument</summary>
        private static void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }
    }
}
