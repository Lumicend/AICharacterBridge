using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AICharacterBridge.Core.Data
{
    /// <summary>
    /// 世界設定情報を格納するデータクラス。
    /// Character Card V2 のデータ構造に準拠した形式（spec / spec_version / data）で管理します。
    ///
    /// JSON 構造例 / JSON structure example:
    /// {
    ///   "spec": "world_setting",
    ///   "spec_version": "1.0",
    ///   "data": {
    ///     "description": "..."
    ///   }
    /// }
    ///
    /// A data class that stores world setting information.
    /// Managed in a format conforming to the Character Card V2 data structure
    /// (spec / spec_version / data).
    /// </summary>
    [Serializable]
    public class WorldSetting
    {
        // =====================================================================
        // 定数 / Constants
        // =====================================================================

        /// <summary>
        /// このデータクラスの spec 識別子。
        /// FromJson() でファイルの種別を検証するために使用します。
        /// Spec identifier for this data class.
        /// Used by FromJson() to validate the file type.
        /// </summary>
        public const string SpecIdentifier = "world_setting";

        /// <summary>
        /// 現在の spec_version の値。
        /// Current spec_version value.
        /// </summary>
        public const string CurrentSpecVersion = "1.0";

        // =====================================================================
        // プロパティ / Properties
        // =====================================================================

        /// <summary>
        /// データ種別の識別子。常に "world_setting" を保持します。
        /// Data type identifier. Always holds "world_setting".
        /// </summary>
        [JsonProperty("spec")]
        public string Spec { get; set; }

        /// <summary>
        /// データ形式のバージョン。
        /// Data format version.
        /// </summary>
        [JsonProperty("spec_version")]
        public string SpecVersion { get; set; }

        /// <summary>
        /// 世界設定の実データを格納するオブジェクト。
        /// Object holding the actual world setting data.
        /// </summary>
        [JsonProperty("data")]
        public WorldSettingData Data { get; set; }

        // =====================================================================
        // コンストラクタ / Constructor
        // =====================================================================

        /// <summary>
        /// デフォルト値でインスタンスを初期化します。
        /// Initializes the instance with default values.
        /// </summary>
        public WorldSetting()
        {
            Spec = SpecIdentifier;
            SpecVersion = CurrentSpecVersion;
            Data = new WorldSettingData();
        }

        // =====================================================================
        // データアクセス / Data access
        // =====================================================================

        /// <summary>
        /// 説明文を取得します。
        /// Gets the description.
        /// </summary>
        /// <returns>説明文。未設定の場合は空文字列。/ Description, or empty string if not set.</returns>
        public string GetDescription() => Data?.Description ?? "";

        /// <summary>
        /// 説明文を設定します。
        /// Sets the description.
        /// </summary>
        /// <param name="description">設定する説明文 / Description to set</param>
        public void SetDescription(string description)
        {
            if (Data == null)
                Data = new WorldSettingData();

            Data.Description = description ?? "";
        }

        // =====================================================================
        // ユーティリティ / Utilities
        // =====================================================================

        /// <summary>
        /// デフォルト値（説明文が未設定）かどうかを判定します。
        /// Determines whether this instance holds default values (description not set).
        /// </summary>
        public bool IsDefault() => string.IsNullOrEmpty(GetDescription());

        /// <summary>
        /// ディープコピーを作成します。
        /// Creates a deep copy.
        /// </summary>
        public WorldSetting Clone()
        {
            return new WorldSetting
            {
                Spec = this.Spec,
                SpecVersion = this.SpecVersion,
                Data = new WorldSettingData
                {
                    Description = this.GetDescription()
                }
            };
        }

        /// <summary>
        /// AIプロンプト用の文字列にフォーマットします。
        /// 説明文が未設定の場合は "None" を返します。
        /// Formats the world setting as a string for AI prompts.
        /// Returns "None" if the description is not set.
        /// </summary>
        public string FormatForPrompt()
        {
            if (IsDefault())
                return "None";

            return GetDescription();
        }

        // =====================================================================
        // シリアライゼーション / Serialization
        // =====================================================================

        /// <summary>
        /// このオブジェクトをJSON文字列にシリアライズします。
        /// Serializes this object to a JSON string.
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// JSON文字列から WorldSetting インスタンスを生成します。
        /// "spec" フィールドが "world_setting" であることを検証します。
        /// 検証に失敗した場合は例外をスローします（ロードは行いません）。
        ///
        /// Creates a WorldSetting instance from a JSON string.
        /// Validates that the "spec" field equals "world_setting".
        /// Throws an exception on validation failure (load is not performed).
        /// </summary>
        /// <param name="json">JSON文字列 / JSON string</param>
        /// <returns>生成された WorldSetting インスタンス / Created WorldSetting instance</returns>
        /// <exception cref="ArgumentException">
        /// json が null または空の場合 / When json is null or empty
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// spec が "world_setting" でない場合 / When spec is not "world_setting"
        /// </exception>
        /// <exception cref="Exception">
        /// JSON 解析またはデシリアライズに失敗した場合 / When JSON parsing or deserialization fails
        /// </exception>
        public static WorldSetting FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("JSON cannot be null or empty.", nameof(json));

            try
            {
                // spec フィールドを先読みして種別を検証する
                // Pre-read the spec field to validate the data type
                var parsed = JObject.Parse(json);
                var spec = parsed["spec"]?.ToString();

                if (spec != SpecIdentifier)
                {
                    throw new NotSupportedException(
                        $"Invalid spec: '{spec ?? "null"}'. Expected '{SpecIdentifier}'. " +
                        "Please select a WorldSetting JSON file.");
                }

                var setting = JsonConvert.DeserializeObject<WorldSetting>(json);

                if (setting == null)
                    throw new Exception("Deserialization returned null.");

                // data フィールドが null の場合はデフォルト値で補完
                // Fill with default value if the data field is null
                if (setting.Data == null)
                    setting.Data = new WorldSettingData();

                return setting;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse WorldSetting JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// デフォルト値の新しい WorldSetting インスタンスを作成します。
        /// Creates a new WorldSetting instance with default values.
        /// </summary>
        public static WorldSetting CreateNew()
        {
            return new WorldSetting();
        }
    }

    // =========================================================================

    /// <summary>
    /// WorldSetting の実データを格納するクラス。
    /// Class holding the actual data fields of a WorldSetting.
    /// </summary>
    [Serializable]
    public class WorldSettingData
    {
        /// <summary>
        /// 世界設定の説明文。
        /// World setting description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        public WorldSettingData()
        {
            Description = "";
        }
    }
}
