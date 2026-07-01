using ActionGame;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.MainGame;
using UnityEngine;

namespace AICharacterBridge
{
    /// <summary>
    /// メインゲームにおけるAI Character Bridgeのセーブデータを管理するコントローラー
    /// Controller that manages AI Character Bridge save data in the main game.
    /// </summary>
    public class GameController : GameCustomFunctionController
    {
        /// <summary>
        /// 現在のゲームセーブに紐づくAI Character Bridgeのデータ
        /// The AI Character Bridge data associated with the current game save.
        /// </summary>
        public static AICharacterBridgeSaveData CurrentSaveData { get; private set; }

        protected override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            // セーブデータから拡張データを読み込む
            var data = GetExtendedData();

            if (data != null && data.data.TryGetValue("AICharacterBridgeData", out var loadedObject))
            {
                try
                {
                    string jsonString = loadedObject.ToString();
                    CurrentSaveData = AICharacterBridgeSaveData.FromJson(jsonString);

                    // ロード後にDictionaryを復元
                    CurrentSaveData.RestoreAfterLoad();

                    LogInfo("AI Character Bridge save data loaded successfully.");
                }
                catch (System.Exception e)
                {
                    LogError($"Failed to load AI Character Bridge save data: {e.Message}");
                    CurrentSaveData = new AICharacterBridgeSaveData();
                }
            }
            else
            {
                // セーブデータが存在しない場合は新規作成
                LogInfo("No AI Character Bridge save data found. Initializing with default values.");
                CurrentSaveData = new AICharacterBridgeSaveData();
            }
        }

        protected override void OnGameSave(GameSaveLoadEventArgs args)
        {
            // 現在のデータをJSON文字列に変換
            if (CurrentSaveData == null)
            {
                CurrentSaveData = new AICharacterBridgeSaveData();
            }

            try
            {
                // セーブ前にDictionaryをListに変換
                CurrentSaveData.PrepareForSave();

                string jsonString = CurrentSaveData.ToJson();

                // 拡張データとして保存
                var data = new PluginData();
                data.data["AICharacterBridgeData"] = jsonString;
                SetExtendedData(data);

                LogInfo("AI Character Bridge save data saved successfully.");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to save AI Character Bridge data: {ex.Message}");
            }
        }

        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            // Hシーン開始時の処理（必要に応じて実装）
        }

        protected override void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            // Hシーン終了時の処理（必要に応じて実装）
        }

        protected override void OnDayChange(Cycle.Week day)
        {
            // 日付変更時に全ての会話ログの経過日数を更新
            if (CurrentSaveData != null)
            {
                CurrentSaveData.IncrementAllLogDays();
                LogDebug($"Day changed to {day}. All conversation logs aged by 1 day.");
            }
        }

        protected override void OnPeriodChange(Cycle.Type period)
        {
            // 時間帯変更時の処理（必要に応じて実装）
        }

        protected override void OnNewGame()
        {
            // ニューゲーム時にデフォルト値で初期化
            CurrentSaveData = new AICharacterBridgeSaveData();
            LogInfo("New game started. AI Character Bridge data initialized with default values.");
        }

        #region Helper Methods

        private void LogInfo(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogInfo($"[GameController] {message}");
        }

        private void LogDebug(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogDebug($"[GameController] {message}");
        }

        private void LogError(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogError($"[GameController] {message}");
        }

        #endregion

        #region Public API

        /// <summary>
        /// セーブデータを手動で保存（通常は自動保存されるため不要）
        /// </summary>
        public void ForceSave()
        {
            if (CurrentSaveData == null)
            {
                LogError("Cannot force save: CurrentSaveData is null.");
                return;
            }

            try
            {
                CurrentSaveData.PrepareForSave();
                string jsonString = CurrentSaveData.ToJson();
                var data = new PluginData();
                data.data["AICharacterBridgeData"] = jsonString;
                SetExtendedData(data);
                LogInfo("Force save completed.");
            }
            catch (System.Exception ex)
            {
                LogError($"Force save failed: {ex.Message}");
            }
        }

        #endregion
    }
}
