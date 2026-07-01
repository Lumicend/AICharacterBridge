using System.Collections.Generic;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// よく使うモーション（ポーズ）のプリセット
    /// Preset motions (poses) for common use
    /// </summary>
    public static class CharaMotionPresets
    {
        // 基本的な立ちポーズ

        /// <summary>
        /// 通常の立ちポーズ（デフォルト）
        /// Normal standing pose (default)
        /// </summary>
        public static CharaMotionData StandNormal => new CharaMotionData
        {
            Name = "standing_normal",
            State = "Stand_01_00"
        };

        /// <summary>
        /// 腕組みをした立ちポーズ
        /// Standing with arms crossed
        /// </summary>
        public static CharaMotionData StandArmsCrossed => new CharaMotionData
        {
            Name = "standing_arms_crossed",
            State = "Stand_27_00"
        };


        /// <summary>
        /// デフォルトのモーションセットを取得
        /// Get default motion set
        /// </summary>
        public static List<CharaMotionData> GetDefaultSet()
        {
            return new List<CharaMotionData>
            {
                StandNormal,
                StandArmsCrossed
            };
        }
    }
}
