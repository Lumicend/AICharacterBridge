using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// キャラクターの表情を定義するデータクラス
    /// Character expression definition data class
    /// </summary>
    [Serializable]
    public class ExpressionData
    {
        /// <summary>AIが選択時に使用する表情名（例: "smile", "sad", "angry"）</summary>
        public string Name { get; set; }

        /// <summary>眉のパターン番号</summary>
        public string Eyebrow { get; set; }

        /// <summary>目のパターン番号</summary>
        public string Eyes { get; set; }

        /// <summary>口のパターン番号</summary>
        public string Mouth { get; set; }

        /// <summary>眉の開き具合（0.0～1.0、空文字列で変更なし）</summary>
        public string EyebrowOpen { get; set; }

        /// <summary>目の開き具合（0.0～1.0、空文字列で変更なし）</summary>
        public string EyesOpen { get; set; }

        /// <summary>口の開き具合（0.0～1.0、空文字列で変更なし）</summary>
        public string MouthOpen { get; set; }

        /// <summary>視線の設定（空文字列で変更なし）</summary>
        public string Gaze { get; set; }

        /// <summary>頬の赤み（0.0～1.0、空文字列で変更なし）</summary>
        public string CheekBlush { get; set; }

        /// <summary>ハイライト（空文字列で変更なし）</summary>
        public string Highlight { get; set; }

        /// <summary>涙の量（0.0～1.0、空文字列で変更なし）</summary>
        public string Tears { get; set; }

        /// <summary>瞬きの設定（空文字列で変更なし）</summary>
        public string Blink { get; set; }

        public ExpressionData()
        {
            Name = "";
            Eyebrow = "0";
            Eyes = "0";
            Mouth = "0";
            EyebrowOpen = "";
            EyesOpen = "";
            MouthOpen = "";
            Gaze = "";
            CheekBlush = "";
            Highlight = "";
            Tears = "";
            Blink = "";
        }

        /// <summary>
        /// CharaExpressionコマンドの引数配列に変換
        /// Convert to CharaExpression command arguments
        /// </summary>
        public string[] ToCommandArgs()
        {
            return new[]
            {
                Eyebrow,
                Eyes,
                Mouth,
                EyebrowOpen,
                EyesOpen,
                MouthOpen,
                Gaze,
                CheekBlush,
                Highlight,
                Tears,
                Blink
            };
        }

        /// <summary>
        /// 妥当性を検証
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(Eyebrow) &&
                   !string.IsNullOrEmpty(Eyes) &&
                   !string.IsNullOrEmpty(Mouth);
        }

        /// <summary>
        /// ディープコピーを作成
        /// </summary>
        public ExpressionData Clone()
        {
            return new ExpressionData
            {
                Name = this.Name,
                Eyebrow = this.Eyebrow,
                Eyes = this.Eyes,
                Mouth = this.Mouth,
                EyebrowOpen = this.EyebrowOpen,
                EyesOpen = this.EyesOpen,
                MouthOpen = this.MouthOpen,
                Gaze = this.Gaze,
                CheekBlush = this.CheekBlush,
                Highlight = this.Highlight,
                Tears = this.Tears,
                Blink = this.Blink
            };
        }

        /// <summary>
        /// ExpressionDataのリストをJSON配列形式（名前のみ）にシリアライズします。
        /// Serializes a list of ExpressionData to JSON array format (names only).
        /// </summary>
        /// <param name="expressions">表情データのリスト</param>
        /// <returns>JSON配列形式の文字列（例: ["smile", "sad", "angry"]）</returns>
        public static string ToJsonList(List<ExpressionData> expressions)
        {
            if (expressions == null || expressions.Count == 0)
                return "[]";

            var names = new List<string>();
            foreach (var expr in expressions)
            {
                if (!string.IsNullOrEmpty(expr.Name))
                    names.Add(expr.Name);
            }

            return JsonConvert.SerializeObject(names, Formatting.Indented);
        }
    }
}
