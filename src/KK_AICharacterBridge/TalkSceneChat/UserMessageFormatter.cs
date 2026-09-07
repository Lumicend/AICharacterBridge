using System;
using System.Collections.Generic;
using System.Text;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// ユーザーが入力した会話メッセージを、プロンプトおよびログで共通して使用する
    /// 統一記法(セリフ / 動作描写)に正規化する静的クラス。
    ///
    /// 記法:
    ///   セリフ:                 "..." (二重引用符)
    ///   動作・情景・心情描写:   *...* (アスタリスク)
    ///
    /// エスケープ:
    ///   \*  → 区切り文字として扱わず、文字通りの "*" として出力する
    ///   \"  → 引用符変換の対象とせず、文字通りの "\"" として出力する
    ///
    /// 処理は以下の5段階で行う:
    ///   Step1. エスケープの退避(\* と \" をプレースホルダーに置き換える)
    ///   Step2. アスタリスクによる区間分割(奇数個ならスキップし全体を1区間扱い)
    ///   Step3. セリフ候補区間ごとの引用符変換(区間内の"が偶数個の場合のみ'に変換)
    ///   Step4. 区間の再構築(セリフ候補区間は"..."、動作描写区間は*...*で囲み、半角スペースで連結)
    ///   Step5. エスケープの復元
    ///
    /// Static class that normalizes a user-typed chat message into the unified
    /// narration format (spoken words / actions) shared by prompts and logs.
    ///
    /// Format:
    ///   Spoken words:                  "..." (double quotes)
    ///   Actions/scenery/emotions:      *...* (asterisks)
    ///
    /// Escaping:
    ///   \*  → treated as a literal "*", not a delimiter
    ///   \"  → treated as a literal "\"", excluded from quote conversion
    ///
    /// Processing is performed in 5 stages:
    ///   Step1. Protect escapes (replace \* and \" with placeholders)
    ///   Step2. Split into segments by asterisk (skip splitting if the count is odd; treat whole text as one segment)
    ///   Step3. Convert quotes within each speech-candidate segment (only when the in-segment count of " is even)
    ///   Step4. Reassemble segments (wrap speech segments in "...", narration segments in *...*, join with a single space)
    ///   Step5. Restore escapes
    /// </summary>
    public static class UserMessageFormatter
    {
        // =====================================================================
        // エスケープ退避用のプレースホルダー文字
        // Placeholder characters used to temporarily protect escaped sequences.
        //
        // Unicode 私用領域(Private Use Area)の文字を使用し、通常のユーザー入力
        // では実質的に出現しないことを保証する。
        //
        // Uses characters from the Unicode Private Use Area to guarantee they
        // will not realistically appear in normal user input.
        // =====================================================================

        private const char EscapedAsteriskPlaceholder = '\uE000';
        private const char EscapedQuotePlaceholder = '\uE001';

        /// <summary>
        /// ユーザーメッセージを正規化します。
        /// null または空文字の場合は、そのまま(空文字として)返します。
        ///
        /// Normalizes a user message.
        /// Returns the input unchanged (as an empty string) when null or empty.
        /// </summary>
        /// <param name="rawInput">ユーザーが入力した生のメッセージ / Raw message as typed by the user</param>
        /// <returns>正規化済みのメッセージ文字列 / Normalized message string</returns>
        public static string Normalize(string rawInput)
        {
            if (string.IsNullOrEmpty(rawInput))
                return rawInput ?? "";

            // Step 1: エスケープの退避
            // Step 1: Protect escaped sequences
            string protectedText = ProtectEscapes(rawInput);

            // Step 2: アスタリスクによる区間分割
            // Step 2: Split into segments by unescaped asterisks
            List<Segment> segments = SplitIntoSegments(protectedText);

            // Step 3: セリフ候補区間ごとの引用符変換
            // Step 3: Convert quotes within each speech-candidate segment
            foreach (var segment in segments)
            {
                if (!segment.IsNarration)
                    segment.Content = ConvertQuotesIfBalanced(segment.Content);
            }

            // Step 4: 区間の再構築
            // Step 4: Reassemble segments
            string joined = Reassemble(segments);

            // Step 5: エスケープの復元
            // Step 5: Restore escaped sequences
            return RestoreEscapes(joined);
        }

        // =====================================================================
        // Step 1 / Step 5: エスケープの退避・復元
        // Step 1 / Step 5: Escape protection and restoration
        // =====================================================================

        /// <summary>
        /// "\*" と "\"" を、後続の処理で区切り文字・引用符として扱われないよう
        /// プレースホルダー文字に置き換えます。
        ///
        /// Replaces "\*" and "\"" with placeholder characters so that subsequent
        /// processing does not treat them as delimiters/quotes.
        /// </summary>
        private static string ProtectEscapes(string input)
        {
            var sb = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\\' && i + 1 < input.Length && input[i + 1] == '*')
                {
                    sb.Append(EscapedAsteriskPlaceholder);
                    i++; // "\*" の2文字分をまとめて消費する / Consume both characters of "\*"
                }
                else if (input[i] == '\\' && i + 1 < input.Length && input[i + 1] == '"')
                {
                    sb.Append(EscapedQuotePlaceholder);
                    i++; // "\"" の2文字分をまとめて消費する / Consume both characters of "\""
                }
                else
                {
                    sb.Append(input[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// プレースホルダー文字を、文字通りの "*" および "\"" に復元します。
        /// Restores placeholder characters back to literal "*" and "\"".
        /// </summary>
        private static string RestoreEscapes(string input)
        {
            return input
                .Replace(EscapedAsteriskPlaceholder, '*')
                .Replace(EscapedQuotePlaceholder, '"');
        }

        // =====================================================================
        // Step 2: アスタリスクによる区間分割
        // Step 2: Splitting into segments by asterisk
        // =====================================================================

        /// <summary>
        /// 会話区間(セリフ候補区間、または動作描写区間)を表す内部クラス。
        /// Internal class representing a single conversation segment
        /// (either a speech candidate or a narration segment).
        /// </summary>
        private class Segment
        {
            /// <summary>true の場合、動作・情景・心情描写(*...*)区間 / True if this is a narration (*...*) segment</summary>
            public readonly bool IsNarration;

            /// <summary>区間の中身(区切り文字を含まない) / Segment content (delimiters excluded)</summary>
            public string Content;

            public Segment(bool isNarration, string content)
            {
                IsNarration = isNarration;
                Content = content;
            }
        }

        /// <summary>
        /// エスケープ退避済みのテキストを、アスタリスクを区切りとして
        /// セリフ候補区間と動作描写区間に分割します。
        ///
        /// アスタリスクの総数が奇数(閉じ忘れ相当)の場合は分割を行わず、
        /// テキスト全体を単一のセリフ候補区間として扱います(フォールバック)。
        ///
        /// Splits escape-protected text into speech-candidate and narration
        /// segments, using asterisks as delimiters.
        ///
        /// If the total number of asterisks is odd (indicating an unclosed
        /// span), splitting is skipped and the entire text is treated as a
        /// single speech-candidate segment (fallback).
        /// </summary>
        private static List<Segment> SplitIntoSegments(string protectedText)
        {
            var segments = new List<Segment>();

            int asteriskCount = CountChar(protectedText, '*');

            if (asteriskCount % 2 != 0)
            {
                // 閉じ忘れ: 全体を単一のセリフ候補区間として扱う
                // Unclosed span: treat the entire text as a single speech-candidate segment
                segments.Add(new Segment(isNarration: false, content: protectedText));
                return segments;
            }

            string[] parts = protectedText.Split('*');

            for (int i = 0; i < parts.Length; i++)
            {
                // 偶数インデックス(0,2,4...) = セリフ候補区間
                // 奇数インデックス(1,3,5...) = 動作描写区間
                //
                // Even indices (0,2,4...) = speech-candidate segments
                // Odd indices (1,3,5...)  = narration segments
                bool isNarration = (i % 2 == 1);
                segments.Add(new Segment(isNarration, parts[i]));
            }

            return segments;
        }

        // =====================================================================
        // Step 3: 引用符変換
        // Step 3: Quote conversion
        // =====================================================================

        /// <summary>
        /// セリフ候補区間内の(エスケープされていない)" を、区間内の出現数が
        /// 偶数の場合のみ単一引用符(')に変換します。
        /// 奇数の場合は変換を行わず、元のテキストのまま返します(フォールバック)。
        ///
        /// 注意: この処理は「区間全体がユーザー自身の手による引用符で完全に
        /// 囲まれている(例: 区間全体が "わあ！" のような形)」場合であっても
        /// 特別扱いをしません。区間内の " は常に機械的に ' へ変換され、
        /// その上で Step4 によって区間全体がさらに外側の "..." で囲まれます。
        /// これは、区間の一部にネストした引用が現れるケース(例: 上司に"しっかりしろ"
        /// と言われた)と、区間全体が引用符で囲まれているケースを、記号の並びだけ
        /// から区別する確実な方法がないための設計上の判断です。
        ///
        /// Converts unescaped double quotes within a speech-candidate segment to
        /// single quotes, but only when the total count within the segment is
        /// even. If odd, leaves the segment unchanged (fallback).
        ///
        /// Note: this does not special-case the situation where the entire
        /// segment happens to already be fully wrapped in the user's own quotes
        /// (e.g. a segment consisting solely of "Wow!"). Quotes within a segment
        /// are always mechanically converted to single quotes, and the whole
        /// segment is then wrapped in an outer "..." by Step4. This is a
        /// deliberate design choice: there is no reliable way to distinguish a
        /// nested quotation appearing mid-segment (e.g. My boss told me "shape up")
        /// from a segment that is entirely pre-wrapped in quotes, based on the
        /// arrangement of the symbols alone.
        /// </summary>
        private static string ConvertQuotesIfBalanced(string segmentContent)
        {
            int quoteCount = CountChar(segmentContent, '"');

            if (quoteCount % 2 != 0)
                return segmentContent;

            return segmentContent.Replace('"', '\'');
        }

        // =====================================================================
        // Step 4: 区間の再構築
        // Step 4: Segment reassembly
        // =====================================================================

        /// <summary>
        /// 区間リストから最終的なメッセージ文字列を再構築します。
        ///
        /// 動作描写区間はアスタリスクで囲み、空でないセリフ候補区間は二重引用符で
        /// 囲み、それぞれを半角スペース1つで連結します。
        /// 区間の前後の空白文字は、連結用スペースと重複するため事前に除去します。
        ///
        /// 動作描写区間は、中身が空(例: "**" の場合)であっても "**" のまま保持
        /// します。これは、誤って通常のアスタリスクの連続入力を動作描写として
        /// 補足してしまった場合でも、情報を破壊しない(元の記号列を維持する)方針
        /// によるものです。
        ///
        /// Reassembles the final message string from the segment list.
        ///
        /// Narration segments are wrapped in asterisks, non-empty speech
        /// segments are wrapped in double quotes, and all pieces are joined
        /// with a single half-width space. Leading/trailing whitespace within
        /// each segment is trimmed beforehand, since it would otherwise
        /// duplicate the join-space.
        ///
        /// Narration segments are kept even when their content is empty (e.g.
        /// "**"), preserving the original symbols rather than discarding them,
        /// in case of an accidental double-asterisk input.
        /// </summary>
        private static string Reassemble(List<Segment> segments)
        {
            var pieces = new List<string>();

            foreach (var segment in segments)
            {
                string trimmed = segment.Content != null ? segment.Content.Trim() : "";

                if (segment.IsNarration)
                {
                    pieces.Add("*" + trimmed + "*");
                }
                else if (!string.IsNullOrEmpty(trimmed))
                {
                    pieces.Add("\"" + trimmed + "\"");
                }
            }

            return string.Join(" ", pieces.ToArray());
        }

        // =====================================================================
        // ユーティリティ
        // Utilities
        // =====================================================================

        /// <summary>
        /// 文字列内の指定文字の出現数を数えます。
        /// Counts the number of occurrences of the specified character in a string.
        /// </summary>
        private static int CountChar(string text, char target)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int count = 0;
            foreach (char c in text)
            {
                if (c == target)
                    count++;
            }
            return count;
        }
    }
}
