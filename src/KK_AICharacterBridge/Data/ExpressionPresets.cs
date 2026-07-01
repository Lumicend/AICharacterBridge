using System.Collections.Generic;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// よく使う表情のプリセット
    /// Preset expressions for common use
    /// </summary>
    public static class ExpressionPresets
    {
        /// <summary>
        /// 通常の表情
        /// Normal expression
        /// </summary>
        public static ExpressionData Normal => new ExpressionData
        {
            Name = "normal",
            Eyebrow = "0",  // 通常
            Eyes = "0",     // 通常
            Mouth = "0"     // 通常
        };

        /// <summary>
        /// 笑顔
        /// Smile
        /// </summary>
        public static ExpressionData Smile => new ExpressionData
        {
            Name = "smile",
            Eyebrow = "0",      // 通常
            Eyes = "2",         // 笑顔
            Mouth = "1",        // 笑顔
            EyesOpen = "1"
        };

        /// <summary>
        /// 大きな笑顔
        /// Big smile
        /// </summary>
        public static ExpressionData BigSmile => new ExpressionData
        {
            Name = "big_smile",
            Eyebrow = "0",      // 通常
            Eyes = "3",         // 笑顔両目閉じ
            Mouth = "2",        // 嬉しい
            EyesOpen = "0"
        };

        /// <summary>
        /// 微笑み
        /// Gentle smile
        /// </summary>
        public static ExpressionData GentleSmile => new ExpressionData
        {
            Name = "gentle_smile",
            Eyebrow = "0",      // 通常
            Eyes = "4",         // 微笑
            Mouth = "1",        // 笑顔
            EyesOpen = "0.8"
        };

        // ネガティブな表情

        /// <summary>
        /// 悲しい
        /// Sad
        /// </summary>
        public static ExpressionData Sad => new ExpressionData
        {
            Name = "sad",
            Eyebrow = "2",      // 困り
            Eyes = "15",        // 悲しい
            Mouth = "13",       // 寂しい
            Tears = "0.5"
        };

        /// <summary>
        /// 泣いている
        /// Crying
        /// </summary>
        public static ExpressionData Crying => new ExpressionData
        {
            Name = "crying",
            Eyebrow = "2",      // 困り
            Eyes = "16",        // 泣き
            Mouth = "13",       // 寂しい
            Tears = "1",
            EyesOpen = "0.3"
        };

        /// <summary>
        /// 怒り
        /// Angry
        /// </summary>
        public static ExpressionData Angry => new ExpressionData
        {
            Name = "angry",
            Eyebrow = "1",      // 怒り
            Eyes = "9",         // 怒り
            Mouth = "8",        // 怒り
            EyebrowOpen = "0.7"
        };

        /// <summary>
        /// 困る
        /// Troubled
        /// </summary>
        public static ExpressionData Troubled => new ExpressionData
        {
            Name = "troubled",
            Eyebrow = "2",      // 困り
            Eyes = "19",        // 困る
            Mouth = "15"        // 不満
        };

        /// <summary>
        /// 落胆
        /// Disappointed
        /// </summary>
        public static ExpressionData Disappointed => new ExpressionData
        {
            Name = "disappointed",
            Eyebrow = "13",     // 落胆
            Eyes = "18",        // 落胆
            Mouth = "13"        // 寂しい
        };

        /// <summary>
        /// 嫌悪
        /// Disgust
        /// </summary>
        public static ExpressionData Disgust => new ExpressionData
        {
            Name = "disgust",
            Eyebrow = "1",      // 怒り
            Eyes = "13",        // 嫌悪
            Mouth = "12"        // 嫌悪
        };

        // 感情表現

        /// <summary>
        /// 驚き
        /// Surprised
        /// </summary>
        public static ExpressionData Surprised => new ExpressionData
        {
            Name = "surprised",
            Eyebrow = "12",     // 驚き
            Eyes = "0",         // 通常
            Mouth = "17",       // 驚き
            EyesOpen = "1",
            EyebrowOpen = "1",
            MouthOpen = "0.8"
        };

        /// <summary>
        /// 照れ・恥ずかしい
        /// Shy / Embarrassed
        /// </summary>
        public static ExpressionData Shy => new ExpressionData
        {
            Name = "shy",
            Eyebrow = "2",      // 困り
            Eyes = "8",         // 照れ
            Mouth = "7",        // ドキドキ
            CheekBlush = "0.8",
            Gaze = "down"
        };

        /// <summary>
        /// 照れ笑い
        /// Embarrassed smile
        /// </summary>
        public static ExpressionData EmbarrassedSmile => new ExpressionData
        {
            Name = "embarrassed_smile",
            Eyebrow = "2",      // 困り
            Eyes = "8",         // 照れ
            Mouth = "1",        // 笑顔
            CheekBlush = "1"
        };

        /// <summary>
        /// ドキドキ
        /// Excited / Nervous
        /// </summary>
        public static ExpressionData Excited => new ExpressionData
        {
            Name = "excited",
            Eyebrow = "0",      // 通常
            Eyes = "8",         // 照れ
            Mouth = "5",        // ドキドキ
            CheekBlush = "0.6"
        };

        /// <summary>
        /// 焦り
        /// Panicked
        /// </summary>
        public static ExpressionData Panicked => new ExpressionData
        {
            Name = "panicked",
            Eyebrow = "2",      // 困り
            Eyes = "17",        // 焦り
            Mouth = "14"        // 焦り
        };

        /// <summary>
        /// 不安
        /// Anxious
        /// </summary>
        public static ExpressionData Anxious => new ExpressionData
        {
            Name = "anxious",
            Eyebrow = "11",     // 不安
            Eyes = "7",         // 切ない
            Mouth = "0"         // 通常
        };

        // その他の表情

        /// <summary>
        /// 真剣
        /// Serious
        /// </summary>
        public static ExpressionData Serious => new ExpressionData
        {
            Name = "serious",
            Eyebrow = "10",     // 真剣
            Eyes = "10",        // 真剣
            Mouth = "10"        // 真剣1
        };

        /// <summary>
        /// 思案中
        /// Thinking
        /// </summary>
        public static ExpressionData Thinking => new ExpressionData
        {
            Name = "thinking",
            Eyebrow = "6",      // 思案
            Eyes = "14",        // 思案
            Mouth = "0"         // 通常
        };

        /// <summary>
        /// 得意げ
        /// Proud / Confident
        /// </summary>
        public static ExpressionData Proud => new ExpressionData
        {
            Name = "proud",
            Eyebrow = "14",     // 得意げ
            Eyes = "20",        // 得意げ
            Mouth = "19"        // 得意げ
        };

        /// <summary>
        /// つまらない
        /// Bored
        /// </summary>
        public static ExpressionData Bored => new ExpressionData
        {
            Name = "bored",
            Eyebrow = "3",      // つまらない
            Eyes = "11",        // つまらない
            Mouth = "0"         // 通常
        };

        /// <summary>
        /// 呆れ
        /// Exasperated
        /// </summary>
        public static ExpressionData Exasperated => new ExpressionData
        {
            Name = "exasperated",
            Eyebrow = "3",      // つまらない
            Eyes = "11",        // つまらない
            Mouth = "16"        // 呆れ
        };

        /// <summary>
        /// 疑問
        /// Questioning
        /// </summary>
        public static ExpressionData Questioning => new ExpressionData
        {
            Name = "questioning",
            Eyebrow = "4",      // 疑問
            Eyes = "0",         // 通常
            Mouth = "0"         // 通常
        };

        /// <summary>
        /// 苦しい
        /// Pained
        /// </summary>
        public static ExpressionData Pained => new ExpressionData
        {
            Name = "pained",
            Eyebrow = "2",      // 困り
            Eyes = "12",        // 苦しい
            Mouth = "0",        // 通常
            EyesOpen = "0.5"
        };

        /// <summary>
        /// 切ない
        /// Longing
        /// </summary>
        public static ExpressionData Longing => new ExpressionData
        {
            Name = "longing",
            Eyebrow = "2",      // 困り
            Eyes = "7",         // 切ない
            Mouth = "13"        // 寂しい
        };

        /// <summary>
        /// ウインク（右目）
        /// Wink (right eye)
        /// </summary>
        public static ExpressionData WinkRight => new ExpressionData
        {
            Name = "wink_right",
            Eyebrow = "15",     // ウィンク
            Eyes = "5",         // ウインク
            Mouth = "1"         // 笑顔
        };

        /// <summary>
        /// ウインク（左目）
        /// Wink (left eye)
        /// </summary>
        public static ExpressionData WinkLeft => new ExpressionData
        {
            Name = "wink_left",
            Eyebrow = "16",     // ウィンク
            Eyes = "6",         // ウインク
            Mouth = "1"         // 笑顔
        };

        /// <summary>
        /// 目を閉じる
        /// Eyes closed
        /// </summary>
        public static ExpressionData EyesClosed => new ExpressionData
        {
            Name = "eyes_closed",
            Eyebrow = "0",      // 通常
            Eyes = "1",         // 両目閉じ
            Mouth = "0",        // 通常
            EyesOpen = "0"
        };

        /// <summary>
        /// デフォルトの表情セットを取得
        /// </summary>
        public static List<ExpressionData> GetDefaultSet()
        {
            return new List<ExpressionData>
            {
                Normal,
                Smile,
                BigSmile,
                GentleSmile,
                Sad,
                Angry,
                Surprised,
                Shy,
                Serious,
                Thinking,
                Proud,
                Troubled,
                Excited,
                Bored,
                EyesClosed
            };
        }
    }
}
