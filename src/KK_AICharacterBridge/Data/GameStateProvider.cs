using ActionGame;
using Manager;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// 現在のゲーム状態を取得するための静的クラス
    /// Static class for retrieving current game state
    /// </summary>
    public static class GameStateProvider
    {

        /// <summary>
        /// 学校名を取得します（将来的に設定可能にする予定）
        /// Gets the school name (planned to be configurable in the future)
        /// </summary>
        public static string GetSchoolName()
        {
            var game = Singleton<Game>.Instance;
            if (game?.saveData != null && !string.IsNullOrEmpty(game.saveData.accademyName))
            {
                return game.saveData.accademyName;
            }
            return "school";
        }

        /// <summary>
        /// 現在の時間帯を取得します。
        /// Gets the current time period.
        /// </summary>
        /// <returns>時間帯の文字列（例: "LunchTime", "AfterSchool"）</returns>
        public static string GetCurrentTimePeriod()
        {
            try
            {
                var cycle = Singleton<Cycle>.Instance;
                if (cycle != null)
                {
                    return cycle.nowType.ToString();
                }
            }
            catch (System.Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[GameStateProvider] Failed to get time period: {ex.Message}");
            }
            return "Unknown";
        }

        /// <summary>
        /// 現在の曜日を取得します。
        /// Gets the current day of the week.
        /// </summary>
        /// <returns>曜日の文字列（例: "Monday", "Tuesday"）</returns>
        public static string GetCurrentWeek()
        {
            try
            {
                var cycle = Singleton<Cycle>.Instance;
                if (cycle != null)
                {
                    return cycle.nowWeek.ToString();
                }
            }
            catch (System.Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[GameStateProvider] Failed to get week: {ex.Message}");
            }
            return "Unknown";
        }

        /// <summary>
        /// 現在の場所を取得します。
        /// Gets the current location.
        /// </summary>
        /// <returns>場所の文字列（例: "Classroom 1-1", "Library"）</returns>
        public static string GetCurrentLocation()
        {
            try
            {
                var actionScene = Singleton<ActionScene>.Instance;
                if (actionScene != null && actionScene.Map != null)
                {
                    var mapInfo = actionScene.Map.infoDic;
                    if (mapInfo != null && mapInfo.Count > 0)
                    {
                        var mapId = actionScene.Map.no;
                        if (mapInfo.TryGetValue(mapId, out var info) && info != null)
                        {
                            return info.MapName ?? $"Map {mapId}";
                        }
                        return $"Map {mapId}";
                    }
                }

                // TalkSceneの場合
                var talkScene = UnityEngine.Object.FindObjectOfType<TalkScene>();
                if (talkScene != null)
                {
                    return "Talk Scene";
                }
            }
            catch (System.Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[GameStateProvider] Failed to get location: {ex.Message}");
            }
            return "Unknown Location";
        }
    }
}
