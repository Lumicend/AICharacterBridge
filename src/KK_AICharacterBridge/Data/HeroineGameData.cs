using System;
using System.Collections.Generic;
using AICharacterBridge.Core.Data;
using Newtonsoft.Json;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// メインゲーム内のヒロインに関するすべてのデータを保持するクラス。
    /// CharacterCardとログを統合管理します。
    /// A class that holds all data related to a heroine in the main game.
    /// Manages CharacterCard and logs in an integrated manner.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroineGameData
    {
        /// <summary>Game.Instance.HeroineList内のインデックス</summary>
        [JsonProperty("heroine_index")]
        public int HeroineIndex { get; set; }

        /// <summary>CharacterCard（RAW JSON文字列として保存）</summary>
        [JsonProperty("character_card_json")]
        public string CharacterCardJson { get; set; }

        /// <summary>ログのリスト</summary>
        [JsonProperty("logs", ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<MainGameLog> Logs { get; set; }

        /// <summary>ログコレクションのランタイムキャッシュ（シリアライズされない）</summary>
        [JsonIgnore]
        private MainGameLogCollection _logCollectionCache;

        public HeroineGameData()
        {
            HeroineIndex = -1;
            CharacterCardJson = "";
            Logs = new List<MainGameLog>();
            _logCollectionCache = null;
        }

        public HeroineGameData(int heroineIndex)
        {
            HeroineIndex = heroineIndex;
            CharacterCardJson = "";
            Logs = new List<MainGameLog>();
            _logCollectionCache = null;
        }

        #region CharacterCard 操作

        /// <summary>
        /// CharacterCardを取得します。
        /// Gets the CharacterCard.
        /// </summary>
        public CharacterCard GetCharacterCard()
        {
            if (string.IsNullOrEmpty(CharacterCardJson))
            {
                return null;
            }

            try
            {
                return CharacterCard.FromJson(CharacterCardJson);
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    $"Failed to parse CharacterCard for heroine index {HeroineIndex}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// CharacterCardを設定します。
        /// Sets the CharacterCard.
        /// </summary>
        public void SetCharacterCard(CharacterCard card)
        {
            if (card == null)
            {
                CharacterCardJson = "";
                return;
            }

            CharacterCardJson = card.RawJson;
        }

        /// <summary>
        /// CharacterCardが存在するかチェックします。
        /// Checks if a CharacterCard exists.
        /// </summary>
        public bool HasCharacterCard()
        {
            if (string.IsNullOrEmpty(CharacterCardJson))
                return false;

            try
            {
                var card = CharacterCard.FromJson(CharacterCardJson);
                return card != null && !card.IsDefault();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// CharacterCardがデフォルト値かどうかを判定します。
        /// Determines if the CharacterCard is a default value.
        /// </summary>
        public bool IsCharacterCardDefault()
        {
            if (string.IsNullOrEmpty(CharacterCardJson))
                return true;

            try
            {
                var card = CharacterCard.FromJson(CharacterCardJson);
                return card == null || card.IsDefault();
            }
            catch
            {
                return true;
            }
        }

        #endregion

        #region Log 操作

        /// <summary>
        /// ログコレクションを取得します（キャッシュ機能付き）。
        /// Gets the log collection (with caching).
        /// </summary>
        public MainGameLogCollection GetLogCollection()
        {
            // キャッシュがあればそれを返す
            if (_logCollectionCache != null)
            {
                return _logCollectionCache;
            }

            // キャッシュを構築
            _logCollectionCache = new MainGameLogCollection();

            if (Logs != null)
            {
                foreach (var log in Logs)
                {
                    if (log != null && log.IsValid())
                    {
                        _logCollectionCache.AddLog(log);
                    }
                }
            }

            return _logCollectionCache;
        }

        /// <summary>
        /// ログコレクションを設定します。
        /// Sets the log collection.
        /// </summary>
        public void SetLogCollection(MainGameLogCollection collection)
        {
            if (collection == null)
            {
                Logs = new List<MainGameLog>();
                _logCollectionCache = null;
                return;
            }

            Logs = collection.GetAllLogs();
            _logCollectionCache = collection;
        }

        /// <summary>
        /// ログの総数を取得します。
        /// Gets the total log count.
        /// </summary>
        [JsonIgnore]
        public int LogCount => Logs?.Count ?? 0;

        /// <summary>
        /// ログが存在するかチェックします。
        /// Checks if logs exist.
        /// </summary>
        public bool HasLogs()
        {
            return Logs != null && Logs.Count > 0;
        }

        /// <summary>
        /// すべてのログの経過日数を1日増やします。
        /// Increments elapsed days for all logs by 1.
        /// </summary>
        public void IncrementAllLogDays()
        {
            if (Logs == null) return;

            foreach (var log in Logs)
            {
                log?.IncrementElapsedDays();
            }

            // キャッシュは同じログインスタンスを参照しているため、
            // Logsをインクリメントすれば自動的にキャッシュも更新される
            // （キャッシュに対する明示的な更新は不要）
        }

        /// <summary>
        /// ログコレクションに対してログ上限を適用します。
        /// Enforces log limit on the log collection.
        /// </summary>
        public void EnforceLogLimit(int maxLogs)
        {
            var collection = GetLogCollection();
            collection.EnforceLogLimit(maxLogs);

            // 変更をLogsリストに反映
            Logs = collection.GetAllLogs();
        }

        /// <summary>
        /// 特定の型のログに対してログ上限を適用します。
        /// Enforces log limit for a specific log type.
        /// </summary>
        public void EnforceLogLimitByType<T>(int maxLogs) where T : MainGameLog
        {
            var collection = GetLogCollection();
            collection.EnforceLogLimitByType<T>(maxLogs);

            // 変更をLogsリストに反映
            Logs = collection.GetAllLogs();
        }

        #endregion

        #region 統計情報

        /// <summary>
        /// ログの統計情報を取得します。
        /// Gets log statistics.
        /// </summary>
        public string GetLogStatistics()
        {
            var collection = GetLogCollection();
            return collection.GetStatisticsSummary();
        }

        /// <summary>
        /// ログの型ごとの数を取得します。
        /// Gets the count of logs by type.
        /// </summary>
        public Dictionary<string, int> GetLogCountByType()
        {
            var collection = GetLogCollection();
            return collection.GetLogCountByType();
        }

        #endregion

        #region シリアライゼーション補助

        /// <summary>
        /// セーブ前の準備：ログコレクションのキャッシュをLogsリストに反映します。
        /// Preparation before save: Reflects log collection cache to Logs list.
        /// </summary>
        public void PrepareForSave()
        {
            if (_logCollectionCache != null)
            {
                Logs = _logCollectionCache.GetAllLogs();
            }
        }

        /// <summary>
        /// ロード後の復元：キャッシュをクリアします（次回GetLogCollection時に再構築）。
        /// Restoration after load: Clears cache (will be rebuilt on next GetLogCollection call).
        /// </summary>
        public void RestoreAfterLoad()
        {
            _logCollectionCache = null;
        }

        #endregion

        #region ユーティリティ

        /// <summary>
        /// このデータが空（CharacterCardもログもない）かどうかを判定します。
        /// Determines if this data is empty (no CharacterCard and no logs).
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(CharacterCardJson) &&
                   (Logs == null || Logs.Count == 0);
        }

        /// <summary>
        /// ディープコピーを作成します。
        /// Creates a deep copy.
        /// </summary>
        public HeroineGameData Clone()
        {
            var clone = new HeroineGameData(HeroineIndex)
            {
                CharacterCardJson = this.CharacterCardJson
            };

            if (this.Logs != null)
            {
                foreach (var log in this.Logs)
                {
                    if (log != null)
                    {
                        clone.Logs.Add(log.Clone());
                    }
                }
            }

            return clone;
        }

        public override string ToString()
        {
            return $"HeroineGameData [Index: {HeroineIndex}, " +
                   $"HasCard: {HasCharacterCard()}, " +
                   $"Logs: {LogCount}]";
        }

        #endregion
    }
}
