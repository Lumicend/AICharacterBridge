using AICharacterBridge.Core.Data;
using AICharacterBridge.Data;
using Manager;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AICharacterBridge
{
    /// <summary>
    /// メインゲームのセーブデータに保存するAI Character Bridge関連のコアデータ。
    /// TalkSceneChat固有のデータは TalkSceneChatSaveData で管理します。
    /// Core AI Character Bridge data saved in the main game save data.
    /// TalkSceneChat-specific data is managed by TalkSceneChatSaveData.
    /// </summary>
    public class AICharacterBridgeSaveData
    {
        /// <summary>セーブデータのバージョン / Save data version</summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>Heroineごとのデータ（CharacterCardとログを統合管理） / Per-heroine data (CharacterCard and logs)</summary>
        [JsonProperty("heroine_data_list")]
        public List<HeroineGameData> HeroineDataList { get; set; }

        /// <summary>ランタイム用のマップ（高速アクセス用、シリアライズされない） / Runtime map (fast access, not serialized)</summary>
        [JsonIgnore]
        private Dictionary<SaveData.Heroine, HeroineGameData> _heroineDataMap;

        /// <summary>PlayerのCharacterCard（RAW JSON文字列として保存） / Player's CharacterCard (stored as raw JSON string)</summary>
        [JsonProperty("player_character_card_json")]
        public string PlayerCharacterCardJson { get; set; }

        /// <summary>世界設定情報 / World setting</summary>
        [JsonProperty("world_setting")]
        public WorldSetting WorldSetting { get; set; }

        /// <summary>
        /// デフォルト値でインスタンスを初期化します。
        /// Initializes the instance with default values.
        /// </summary>
        public AICharacterBridgeSaveData()
        {
            Version = "2.0";
            HeroineDataList = new List<HeroineGameData>();
            _heroineDataMap = new Dictionary<SaveData.Heroine, HeroineGameData>();
            PlayerCharacterCardJson = "";
            WorldSetting = new WorldSetting();
        }

        #region Heroineデータ管理 / Heroine Data Management

        /// <summary>
        /// 指定したHeroineのゲームデータを取得します。存在しない場合は新規作成します。
        /// Gets the game data for the specified Heroine, creating new data if not found.
        /// </summary>
        private HeroineGameData GetHeroineData(SaveData.Heroine heroine)
        {
            if (heroine == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[AICharacterBridgeSaveData] Cannot get data for null heroine.");
                return null;
            }

            if (!_heroineDataMap.ContainsKey(heroine))
            {
                _heroineDataMap[heroine] = new HeroineGameData();
            }

            return _heroineDataMap[heroine];
        }

        #endregion

        #region ログ管理メソッド / Log Management Methods

        /// <summary>
        /// 指定したHeroineのログコレクションを取得します。
        /// Gets the log collection for the specified Heroine.
        /// </summary>
        public MainGameLogCollection GetLogsForHeroine(SaveData.Heroine heroine)
        {
            var data = GetHeroineData(heroine);
            if (data == null)
            {
                return new MainGameLogCollection();
            }

            return data.GetLogCollection();
        }

        /// <summary>
        /// 指定したHeroineにログを追加します。
        /// ログ上限を超えている場合は最も古いログを削除します。
        /// Adds a log for the specified Heroine.
        /// If log limit is exceeded, oldest logs are removed.
        /// </summary>
        public void AddLogForHeroine(SaveData.Heroine heroine, MainGameLog log)
        {
            if (heroine == null || log == null) return;

            // ログ上限を取得（0の場合は保存しない）
            // Get log limit (skip saving if 0)
            int maxLogs = AICharacterBridgePlugin.MaxLogsPerHeroine.Value;
            if (maxLogs <= 0)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                    $"Logging is disabled (MaxLogsPerHeroine = {maxLogs}). Skipping log save.");
                return;
            }

            var data = GetHeroineData(heroine);
            if (data == null) return;

            var collection = data.GetLogCollection();
            collection.AddLog(log);
            data.EnforceLogLimit(maxLogs);

            AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                $"Added {log.GetLogTypeName()} log for {heroine.charFile.parameter.fullname}. " +
                $"Total logs: {data.LogCount}/{maxLogs}");
        }

        /// <summary>
        /// 全てのHeroineのログの経過日数を1日増やします。
        /// Increments elapsed days for all Heroine logs by 1.
        /// </summary>
        public void IncrementAllLogDays()
        {
            foreach (var data in HeroineDataList)
            {
                data.IncrementAllLogDays();
            }
        }

        #endregion

        #region CharacterCard管理メソッド / CharacterCard Management Methods

        /// <summary>
        /// 指定したHeroineのCharacterCardを取得します。
        /// Gets the CharacterCard for the specified Heroine.
        /// </summary>
        public CharacterCard GetCharacterCardForHeroine(SaveData.Heroine heroine)
        {
            var data = GetHeroineData(heroine);
            if (data == null)
            {
                return CharacterCard.CreateNew();
            }

            var card = data.GetCharacterCard();
            return card ?? CharacterCard.CreateNew();
        }

        /// <summary>
        /// 指定したHeroineのCharacterCardを設定します。
        /// Sets the CharacterCard for the specified Heroine.
        /// </summary>
        public void SetCharacterCardForHeroine(SaveData.Heroine heroine, CharacterCard card)
        {
            if (heroine == null || card == null) return;

            var data = GetHeroineData(heroine);
            if (data == null) return;

            data.SetCharacterCard(card);
        }

        /// <summary>
        /// PlayerのCharacterCardを取得します。
        /// Gets the Player's CharacterCard.
        /// </summary>
        public CharacterCard GetPlayerCharacterCard()
        {
            if (string.IsNullOrEmpty(PlayerCharacterCardJson))
            {
                return CharacterCard.CreateNew();
            }

            try
            {
                return CharacterCard.FromJson(PlayerCharacterCardJson);
            }
            catch (System.Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    $"Failed to parse Player CharacterCard: {ex.Message}");
                return CharacterCard.CreateNew();
            }
        }

        /// <summary>
        /// PlayerのCharacterCardを設定します。
        /// Sets the Player's CharacterCard.
        /// </summary>
        public void SetPlayerCharacterCard(CharacterCard card)
        {
            if (card == null)
            {
                PlayerCharacterCardJson = "";
                return;
            }
            PlayerCharacterCardJson = card.RawJson;
        }

        /// <summary>
        /// WorldSettingを取得します。
        /// Gets the WorldSetting.
        /// </summary>
        public WorldSetting GetWorldSetting()
        {
            return WorldSetting ?? new WorldSetting();
        }

        /// <summary>
        /// WorldSettingを設定します。
        /// Sets the WorldSetting.
        /// </summary>
        public void SetWorldSetting(WorldSetting worldSetting)
        {
            if (worldSetting == null) return;
            WorldSetting = worldSetting;
        }

        #endregion

        #region シリアライゼーション / Serialization

        /// <summary>
        /// セーブ前の準備：DictionaryをList形式に変換します。
        /// Preparation before save: Converts Dictionary to List format.
        /// </summary>
        public void PrepareForSave()
        {
            HeroineDataList.Clear();

            var game = Singleton<Game>.Instance;
            if (game == null) return;

            foreach (var kvp in _heroineDataMap)
            {
                var heroine = kvp.Key;
                var data = kvp.Value;

                int heroineIndex = game.HeroineList.IndexOf(heroine);
                if (heroineIndex >= 0 && !data.IsEmpty())
                {
                    data.HeroineIndex = heroineIndex;
                    data.PrepareForSave();
                    HeroineDataList.Add(data);
                }
            }
        }

        /// <summary>
        /// ロード後の復元：List形式からDictionaryに変換します。
        /// Restoration after load: Converts List format to Dictionary.
        /// </summary>
        public void RestoreAfterLoad()
        {
            _heroineDataMap.Clear();

            var game = Singleton<Game>.Instance;
            if (game == null) return;

            foreach (var data in HeroineDataList)
            {
                if (data.HeroineIndex >= 0 && data.HeroineIndex < game.HeroineList.Count)
                {
                    var heroine = game.HeroineList[data.HeroineIndex];
                    data.RestoreAfterLoad();
                    _heroineDataMap[heroine] = data;
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
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        /// <summary>
        /// JSON文字列からオブジェクトをデシリアライズします。
        /// Deserializes an object from a JSON string.
        /// </summary>
        public static AICharacterBridgeSaveData FromJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return new AICharacterBridgeSaveData();

            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                var saveData = JsonConvert.DeserializeObject<AICharacterBridgeSaveData>(jsonString, settings);

                if (saveData == null)
                    return new AICharacterBridgeSaveData();

                if (saveData.HeroineDataList == null)
                    saveData.HeroineDataList = new List<HeroineGameData>();

                if (saveData.PlayerCharacterCardJson == null)
                    saveData.PlayerCharacterCardJson = "";

                // WorldSetting が null の場合は新規作成する。
                // Data フィールドが null の場合も補完する（不完全なデシリアライズへの対応）。
                // Create a new WorldSetting if null.
                // Also fill in Data if null (handles incomplete deserialization).
                if (saveData.WorldSetting == null)
                    saveData.WorldSetting = new WorldSetting();
                else if (saveData.WorldSetting.Data == null)
                    saveData.WorldSetting.Data = new WorldSettingData();

                saveData._heroineDataMap = new Dictionary<SaveData.Heroine, HeroineGameData>();

                return saveData;
            }
            catch (System.Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    $"Failed to deserialize AICharacterBridgeSaveData: {ex.Message}");
                return new AICharacterBridgeSaveData();
            }
        }

        #endregion
    }
}
