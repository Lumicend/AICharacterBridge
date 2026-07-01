using ActionGame;
using ExtensibleSaveFormat;
using KKAPI.MainGame;
using UnityEngine;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// TalkSceneChatモジュール専用のセーブデータを管理するゲームコントローラー。
    /// GameCustomFunctionController を継承し、ExtensibleSaveFormat の独立したスロットに
    /// TalkSceneChat固有のデータを保存・復元します。
    /// Game controller dedicated to managing save data for the TalkSceneChat module.
    /// Inherits GameCustomFunctionController and stores/restores TalkSceneChat-specific data
    /// in an independent ExtensibleSaveFormat slot.
    /// </summary>
    public class TalkSceneChatGameController : GameCustomFunctionController
    {
        /// <summary>
        /// 現在のゲームセーブに紐づく TalkSceneChat のデータ。
        /// TalkSceneChat data associated with the current game save.
        /// </summary>
        public static TalkSceneChatSaveData CurrentSaveData { get; private set; }

        /// <summary>
        /// ExtensibleSaveFormat 内のデータキー。
        /// Data key within ExtensibleSaveFormat.
        /// </summary>
        private const string SaveDataKey = "TalkSceneChatData";

        protected override void OnGameLoad(GameSaveLoadEventArgs args)
        {
            var data = GetExtendedData();

            if (data != null && data.data.TryGetValue(SaveDataKey, out var loadedObject))
            {
                try
                {
                    string jsonString = loadedObject.ToString();
                    CurrentSaveData = TalkSceneChatSaveData.FromJson(jsonString);
                    CurrentSaveData.RestoreAfterLoad();

                    LogInfo("TalkSceneChat save data loaded successfully.");
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to load TalkSceneChat save data: {ex.Message}");
                    CurrentSaveData = new TalkSceneChatSaveData();
                }
            }
            else
            {
                // セーブデータが存在しない場合は新規作成
                // No save data found – initialize with defaults
                LogInfo("No TalkSceneChat save data found. Initializing with default values.");
                CurrentSaveData = new TalkSceneChatSaveData();
            }
        }

        protected override void OnGameSave(GameSaveLoadEventArgs args)
        {
            if (CurrentSaveData == null)
            {
                CurrentSaveData = new TalkSceneChatSaveData();
            }

            try
            {
                CurrentSaveData.PrepareForSave();

                string jsonString = CurrentSaveData.ToJson();

                var pluginData = new PluginData();
                pluginData.data[SaveDataKey] = jsonString;
                SetExtendedData(pluginData);

                LogInfo("TalkSceneChat save data saved successfully.");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to save TalkSceneChat data: {ex.Message}");
            }
        }

        protected override void OnNewGame()
        {
            // ニューゲーム時にデフォルト値で初期化
            // Initialize with defaults on new game
            CurrentSaveData = new TalkSceneChatSaveData();
            LogInfo("New game started. TalkSceneChat data initialized with default values.");
        }

        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr) { }
        protected override void OnEndH(MonoBehaviour proc, HFlag hFlag, bool vr) { }
        protected override void OnDayChange(Cycle.Week day) { }
        protected override void OnPeriodChange(Cycle.Type period) { }

        #region Helper Methods

        private void LogInfo(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogInfo($"[TalkSceneChatGameController] {message}");
        }

        private void LogError(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogError($"[TalkSceneChatGameController] {message}");
        }

        #endregion
    }
}
