using AICharacterBridge.TalkSceneChat.Data;
using Manager;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// TalkSceneChatモジュールのセーブデータ。
    /// カスタムプロンプトテンプレートとヒロインごとのチャット設定を管理します。
    /// Save data for the TalkSceneChat module.
    /// Manages the custom prompt template and per-heroine chat settings.
    /// </summary>
    public class TalkSceneChatSaveData
    {
        /// <summary>
        /// カスタムプロンプトテンプレート（nullまたは空の場合はデフォルトを使用）。
        /// Custom prompt template (uses default when null or empty).
        /// </summary>
        [JsonProperty("custom_prompt_template")]
        public string CustomPromptTemplate { get; set; }

        /// <summary>
        /// ヒロインごとのチャット設定リスト（シリアライズ用）。
        /// List of per-heroine chat settings (for serialization).
        /// </summary>
        [JsonProperty("heroine_settings_list")]
        public List<HeroineChatSettings> HeroineSettingsList { get; set; }

        /// <summary>
        /// ランタイム用の高速アクセスマップ（シリアライズされない）。
        /// Runtime fast-access map (not serialized).
        /// </summary>
        [JsonIgnore]
        private Dictionary<SaveData.Heroine, HeroineChatSettings> _settingsMap;

        /// <summary>
        /// デフォルト値でインスタンスを初期化します。
        /// Initializes the instance with default values.
        /// </summary>
        public TalkSceneChatSaveData()
        {
            CustomPromptTemplate = null;
            HeroineSettingsList = new List<HeroineChatSettings>();
            _settingsMap = new Dictionary<SaveData.Heroine, HeroineChatSettings>();
        }

        #region ヒロイン設定管理 / Heroine Settings Management

        /// <summary>
        /// 指定したヒロインの設定を取得します。存在しない場合は新規作成します。
        /// Gets the settings for the specified heroine, creating new settings if not found.
        /// </summary>
        private HeroineChatSettings GetOrCreateSettings(SaveData.Heroine heroine)
        {
            if (heroine == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneChatSaveData] Cannot get settings for null heroine.");
                return null;
            }

            if (!_settingsMap.ContainsKey(heroine))
            {
                _settingsMap[heroine] = new HeroineChatSettings();
            }

            return _settingsMap[heroine];
        }

        /// <summary>
        /// 指定したヒロインの context_note を取得します。
        /// Gets the context_note for the specified heroine.
        /// </summary>
        /// <param name="heroine">対象のヒロイン / Target heroine</param>
        /// <returns>context_note 文字列（未設定の場合は空文字） / context_note string (empty if not set)</returns>
        public string GetContextNote(SaveData.Heroine heroine)
        {
            var settings = GetOrCreateSettings(heroine);
            return settings?.ContextNote ?? "";
        }

        /// <summary>
        /// 指定したヒロインの context_note を設定します。
        /// Sets the context_note for the specified heroine.
        /// </summary>
        /// <param name="heroine">対象のヒロイン / Target heroine</param>
        /// <param name="note">設定する context_note / context_note to set</param>
        public void SetContextNote(SaveData.Heroine heroine, string note)
        {
            if (heroine == null) return;

            var settings = GetOrCreateSettings(heroine);
            if (settings == null) return;

            settings.ContextNote = note ?? "";
        }

        #endregion

        #region シリアライゼーション / Serialization

        /// <summary>
        /// セーブ前の準備：ランタイムマップをシリアライズ用リストに変換します。
        /// Preparation before save: Converts the runtime map to a serializable list.
        /// </summary>
        public void PrepareForSave()
        {
            HeroineSettingsList.Clear();

            var game = Singleton<Game>.Instance;
            if (game == null) return;

            foreach (var kvp in _settingsMap)
            {
                var heroine = kvp.Key;
                var settings = kvp.Value;

                // 空データは保存しない
                // Skip empty data
                if (settings.IsEmpty()) continue;

                int heroineIndex = game.HeroineList.IndexOf(heroine);
                if (heroineIndex >= 0)
                {
                    settings.HeroineIndex = heroineIndex;
                    HeroineSettingsList.Add(settings);
                }
            }
        }

        /// <summary>
        /// ロード後の復元：シリアライズ用リストからランタイムマップを再構築します。
        /// Restoration after load: Rebuilds the runtime map from the serialized list.
        /// </summary>
        public void RestoreAfterLoad()
        {
            _settingsMap.Clear();

            var game = Singleton<Game>.Instance;
            if (game == null) return;

            foreach (var settings in HeroineSettingsList)
            {
                if (settings.HeroineIndex >= 0 && settings.HeroineIndex < game.HeroineList.Count)
                {
                    var heroine = game.HeroineList[settings.HeroineIndex];
                    _settingsMap[heroine] = settings;
                }
            }
        }

        /// <summary>
        /// このオブジェクトをJSON文字列にシリアライズします。
        /// Serializes this object to a JSON string.
        /// </summary>
        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        /// <summary>
        /// JSON文字列からオブジェクトをデシリアライズします。
        /// Deserializes an object from a JSON string.
        /// </summary>
        /// <param name="jsonString">JSON文字列 / JSON string</param>
        /// <returns>デシリアライズされたインスタンス（失敗時はデフォルト値） / Deserialized instance (default on failure)</returns>
        public static TalkSceneChatSaveData FromJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return new TalkSceneChatSaveData();

            try
            {
                var saveData = JsonConvert.DeserializeObject<TalkSceneChatSaveData>(jsonString);

                if (saveData == null)
                    return new TalkSceneChatSaveData();

                if (saveData.HeroineSettingsList == null)
                    saveData.HeroineSettingsList = new List<HeroineChatSettings>();

                // ランタイムマップを初期化（RestoreAfterLoad で再構築される）
                // Initialize runtime map (rebuilt by RestoreAfterLoad)
                saveData._settingsMap = new Dictionary<SaveData.Heroine, HeroineChatSettings>();

                return saveData;
            }
            catch (System.Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    $"[TalkSceneChatSaveData] Failed to deserialize: {ex.Message}");
                return new TalkSceneChatSaveData();
            }
        }

        #endregion
    }
}
