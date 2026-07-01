using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// メインゲームログのコレクションを管理するクラス。
    /// Manages a collection of main game logs.
    /// </summary>
    public class MainGameLogCollection
    {
        private List<MainGameLog> _logs;

        public MainGameLogCollection()
        {
            _logs = new List<MainGameLog>();
        }

        #region 基本操作

        /// <summary>
        /// ログを追加します。
        /// Adds a log to the collection.
        /// </summary>
        public void AddLog(MainGameLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            if (!log.IsValid())
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[MainGameLogCollection] Attempted to add invalid log: {log.GetLogTypeName()}");
                return;
            }

            _logs.Add(log);
        }

        /// <summary>
        /// ログを削除します。
        /// Removes a log from the collection.
        /// </summary>
        public bool RemoveLog(MainGameLog log)
        {
            if (log == null)
            {
                return false;
            }

            return _logs.Remove(log);
        }

        /// <summary>
        /// 指定したインデックスのログを削除します。
        /// Removes a log at the specified index.
        /// </summary>
        public void RemoveLogAt(int index)
        {
            if (index >= 0 && index < _logs.Count)
            {
                _logs.RemoveAt(index);
            }
        }

        /// <summary>
        /// すべてのログをクリアします。
        /// Clears all logs.
        /// </summary>
        public void ClearLogs()
        {
            _logs.Clear();
        }

        /// <summary>
        /// すべてのログを取得します。
        /// Gets all logs.
        /// </summary>
        public List<MainGameLog> GetAllLogs()
        {
            return new List<MainGameLog>(_logs);
        }

        /// <summary>
        /// ログの総数を取得します。
        /// Gets the total count of logs.
        /// </summary>
        public int GetLogCount()
        {
            return _logs.Count;
        }

        #endregion

        #region 型別取得

        /// <summary>
        /// 指定した型のログのみを取得します。
        /// Gets logs of a specific type.
        /// </summary>
        public List<T> GetLogsByType<T>() where T : MainGameLog
        {
            var result = new List<T>();
            foreach (var log in _logs)
            {
                if (log is T typedLog)
                {
                    result.Add(typedLog);
                }
            }
            return result;
        }

        /// <summary>
        /// 指定した型のログが存在するかチェックします。
        /// Checks if logs of a specific type exist.
        /// </summary>
        public bool HasLogsOfType<T>() where T : MainGameLog
        {
            foreach (var log in _logs)
            {
                if (log is T)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 時間管理

        /// <summary>
        /// すべてのログの経過日数を1日増やします。
        /// Increments elapsed days for all logs by 1.
        /// </summary>
        public void IncrementAllLogDays()
        {
            foreach (var log in _logs)
            {
                log.IncrementElapsedDays();
            }
        }

        #endregion

        #region 容量管理

        /// <summary>
        /// ログの上限を適用し、超過分の古いログを削除します。
        /// Enforces log limit and removes oldest logs if exceeded.
        /// </summary>
        public void EnforceLogLimit(int maxLogs)
        {
            if (maxLogs <= 0)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                    "[MainGameLogCollection] Log limit is 0 or negative. Clearing all logs.");
                ClearLogs();
                return;
            }

            while (_logs.Count > maxLogs)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogDebug(
                    $"[MainGameLogCollection] Log limit exceeded ({_logs.Count}/{maxLogs}). Removing oldest log.");
                _logs.RemoveAt(0); // 最も古いログ（0番目）を削除
            }
        }

        /// <summary>
        /// 指定した型のログに対して上限を適用します。
        /// Enforces log limit for a specific log type.
        /// </summary>
        public void EnforceLogLimitByType<T>(int maxLogs) where T : MainGameLog
        {
            if (maxLogs <= 0)
            {
                // 指定した型のログをすべて削除
                _logs.RemoveAll(log => log is T);
                return;
            }

            var typedLogs = new List<T>();
            var indices = new List<int>();

            for (int i = 0; i < _logs.Count; i++)
            {
                if (_logs[i] is T typedLog)
                {
                    typedLogs.Add(typedLog);
                    indices.Add(i);
                }
            }

            if (typedLogs.Count <= maxLogs)
            {
                return;
            }

            // 超過分の古いログを削除
            int removeCount = typedLogs.Count - maxLogs;
            for (int i = 0; i < removeCount; i++)
            {
                _logs.RemoveAt(indices[i] - i); // インデックス調整
            }
        }

        #endregion

        #region フィルタリング

        /// <summary>
        /// 指定した日数より古いログを取得します。
        /// Gets logs older than the specified number of days.
        /// </summary>
        public List<MainGameLog> GetLogsOlderThan(int days)
        {
            var result = new List<MainGameLog>();
            foreach (var log in _logs)
            {
                if (log.ElapsedDays > days)
                {
                    result.Add(log);
                }
            }
            return result;
        }

        /// <summary>
        /// 指定した日数より新しいログを取得します。
        /// Gets logs newer than the specified number of days.
        /// </summary>
        public List<MainGameLog> GetLogsNewerThan(int days)
        {
            var result = new List<MainGameLog>();
            foreach (var log in _logs)
            {
                if (log.ElapsedDays < days)
                {
                    result.Add(log);
                }
            }
            return result;
        }

        /// <summary>
        /// 指定した日数の範囲内のログを取得します。
        /// Gets logs within the specified day range.
        /// </summary>
        public List<MainGameLog> GetLogsInRange(int minDays, int maxDays)
        {
            var result = new List<MainGameLog>();
            foreach (var log in _logs)
            {
                if (log.ElapsedDays >= minDays && log.ElapsedDays <= maxDays)
                {
                    result.Add(log);
                }
            }
            return result;
        }

        #endregion

        #region プロンプトフォーマット

        /// <summary>
        /// すべてのログをAIプロンプト用の文字列にフォーマットします。
        /// Formats all logs into a string for AI prompts.
        /// </summary>
        public string FormatForPrompt()
        {
            if (_logs == null || _logs.Count == 0)
                return "(No history recorded)";

            var sb = new StringBuilder();
            //sb.AppendLine("=== Character History ===");
            //sb.AppendLine();

            int? currentDay = null;

            for (int i = 0; i < _logs.Count; i++)
            {
                var log = _logs[i];

                // 日付が変わったらセクションヘッダーを出力
                if (currentDay != log.ElapsedDays)
                {
                    if (currentDay != null) // 最初のログではない場合
                        sb.AppendLine();

                    string daysAgo = log.ElapsedDays == 0
                        ? "Today"
                        : log.ElapsedDays == 1
                            ? "1 day ago"
                            : $"{log.ElapsedDays} days ago";
                    sb.AppendLine($"--- {daysAgo} ---");
                    currentDay = log.ElapsedDays;
                }

                // 各ログに全体情報を渡してフォーマット
                string formatted = log.FormatForPrompt(this, i);
                sb.AppendLine(formatted);
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region 統計情報

        /// <summary>
        /// ログの型ごとの数を取得します。
        /// Gets the count of logs by type.
        /// </summary>
        public Dictionary<string, int> GetLogCountByType()
        {
            var counts = new Dictionary<string, int>();

            foreach (var log in _logs)
            {
                string typeName = log.GetLogTypeName();
                if (!counts.ContainsKey(typeName))
                {
                    counts[typeName] = 0;
                }
                counts[typeName]++;
            }

            return counts;
        }

        /// <summary>
        /// 統計情報のサマリーを取得します。
        /// Gets a summary of statistics.
        /// </summary>
        public string GetStatisticsSummary()
        {
            var counts = GetLogCountByType();
            var summary = new StringBuilder();
            summary.AppendLine($"Total logs: {_logs.Count}");

            foreach (var kvp in counts)
            {
                summary.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }

            return summary.ToString().TrimEnd();
        }

        #endregion

        #region ディープコピー

        /// <summary>
        /// このコレクションのディープコピーを作成します。
        /// Creates a deep copy of this collection.
        /// </summary>
        public MainGameLogCollection Clone()
        {
            var clone = new MainGameLogCollection();
            foreach (var log in _logs)
            {
                clone.AddLog(log.Clone());
            }
            return clone;
        }

        #endregion
    }
}
