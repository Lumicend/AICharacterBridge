using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// 衣装プリセット（Coordinate）ごとの容姿・服装の説明文を保持するクラス。
    /// Holds appearance and clothes description strings for each outfit preset (Coordinate).
    ///
    /// リストのインデックスはゲーム本編の Coordinate プリセット番号に対応する。
    /// List indices correspond to in-game Coordinate preset numbers:
    ///   0 = 学生服(校内) / School uniform (on campus)
    ///   1 = 学生服(下校) / School uniform (after school)
    ///   2 = 体操着         / Gym clothes
    ///   3 = 水着           / Swimsuit
    ///   4 = 部活           / Club activities
    ///   5 = 私服           / Casual wear
    ///   6 = お泊り         / Sleepwear
    /// </summary>
    [Serializable]
    public class CoordinateData
    {
        /// <summary>プリセット数（固定）/ Number of presets (fixed)</summary>
        public const int PresetCount = 7;

        /// <summary>
        /// 各プリセットに対応した容姿の説明文リスト。
        /// List of appearance description strings for each preset.
        /// インデックスはプリセット番号（0〜6）に対応する。
        /// Indices correspond to preset numbers (0–6).
        ///
        /// ObjectCreationHandling.Replace を指定することで、デシリアライズ時に
        /// コンストラクタで初期化済みのリストに追記されるのを防ぐ。
        /// ObjectCreationHandling.Replace prevents Json.Net from appending to the
        /// constructor-initialized list during deserialization.
        /// </summary>
        [JsonProperty("appearance", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<string> Appearance { get; set; }

        /// <summary>
        /// 各プリセットに対応した服装の説明文リスト。
        /// List of clothes description strings for each preset.
        /// インデックスはプリセット番号（0〜6）に対応する。
        /// Indices correspond to preset numbers (0–6).
        ///
        /// ObjectCreationHandling.Replace を指定することで、デシリアライズ時に
        /// コンストラクタで初期化済みのリストに追記されるのを防ぐ。
        /// ObjectCreationHandling.Replace prevents Json.Net from appending to the
        /// constructor-initialized list during deserialization.
        /// </summary>
        [JsonProperty("clothes", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<string> Clothes { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ。
        /// 全プリセットの説明文を空文字列で初期化する。
        /// Default constructor.
        /// Initializes all preset description strings to empty strings.
        /// </summary>
        public CoordinateData()
        {
            Appearance = new List<string> { "", "", "", "", "", "", "" };
            Clothes = new List<string> { "", "", "", "", "", "", "" };
        }

        /// <summary>
        /// 指定したプリセットインデックスの容姿説明文を取得する。
        /// インデックスが範囲外の場合は空文字列を返す。
        /// Gets the appearance description for the specified preset index.
        /// Returns an empty string if the index is out of range.
        /// </summary>
        /// <param name="presetIndex">プリセット番号（0〜6）/ Preset number (0–6)</param>
        public string GetAppearance(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= Appearance.Count)
                return "";
            return Appearance[presetIndex] ?? "";
        }

        /// <summary>
        /// 指定したプリセットインデックスの服装説明文を取得する。
        /// インデックスが範囲外の場合は空文字列を返す。
        /// Gets the clothes description for the specified preset index.
        /// Returns an empty string if the index is out of range.
        /// </summary>
        /// <param name="presetIndex">プリセット番号（0〜6）/ Preset number (0–6)</param>
        public string GetClothes(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= Clothes.Count)
                return "";
            return Clothes[presetIndex] ?? "";
        }

        /// <summary>
        /// 指定したプリセットインデックスの容姿説明文を設定する。
        /// Sets the appearance description for the specified preset index.
        /// </summary>
        /// <param name="presetIndex">プリセット番号（0〜6）/ Preset number (0–6)</param>
        /// <param name="value">設定する文字列 / String to set</param>
        public void SetAppearance(int presetIndex, string value)
        {
            if (presetIndex < 0 || presetIndex >= Appearance.Count) return;
            Appearance[presetIndex] = value ?? "";
        }

        /// <summary>
        /// 指定したプリセットインデックスの服装説明文を設定する。
        /// Sets the clothes description for the specified preset index.
        /// </summary>
        /// <param name="presetIndex">プリセット番号（0〜6）/ Preset number (0–6)</param>
        /// <param name="value">設定する文字列 / String to set</param>
        public void SetClothes(int presetIndex, string value)
        {
            if (presetIndex < 0 || presetIndex >= Clothes.Count) return;
            Clothes[presetIndex] = value ?? "";
        }

        /// <summary>
        /// ロード後に要素数が PresetCount と一致しない場合、不足分を空文字列で補完する。
        /// After loading, if the element count does not match PresetCount,
        /// fills missing entries with empty strings.
        /// </summary>
        public void EnsurePresetCount()
        {
            while (Appearance.Count < PresetCount) Appearance.Add("");
            while (Clothes.Count < PresetCount) Clothes.Add("");
        }

        /// <summary>
        /// すべてのプリセットの Appearance および Clothes が空文字列かどうかを確認する。
        /// Checks whether all preset Appearance and Clothes entries are empty strings.
        ///
        /// このメソッドは、CoordinateData を CharacterCard の extensions に書き込む前に
        /// 「書き込む価値があるデータかどうか」を判定するために使用する。
        /// This method is used to determine whether the data is worth writing
        /// to CharacterCard extensions before saving.
        /// </summary>
        /// <returns>
        /// すべてのフィールドが空文字列の場合 true、1つでも値がある場合 false。
        /// True if all fields are empty strings; false if at least one field has a value.
        /// </returns>
        public bool IsEmpty()
        {
            if (Appearance != null)
            {
                foreach (var item in Appearance)
                {
                    if (!string.IsNullOrEmpty(item))
                        return false;
                }
            }

            if (Clothes != null)
            {
                foreach (var item in Clothes)
                {
                    if (!string.IsNullOrEmpty(item))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ディープコピーを作成する。
        /// Creates a deep copy.
        /// </summary>
        public CoordinateData Clone()
        {
            var clone = new CoordinateData();
            for (int i = 0; i < PresetCount; i++)
            {
                clone.Appearance[i] = Appearance.Count > i ? (Appearance[i] ?? "") : "";
                clone.Clothes[i] = Clothes.Count > i ? (Clothes[i] ?? "") : "";
            }
            return clone;
        }
    }
}
