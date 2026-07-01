using ActionGame;
using ActionGame.Communication;
using Manager;
using System;
using System.Collections;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// 特殊アクションの実行を担当するクラス。
    /// ゲーム状態の変更やイベントのトリガーを行います。
    /// Handles execution of special actions.
    /// Performs game state changes and triggers events.
    /// </summary>
    public class TalkSceneActionExecutor
    {
        /// <summary>
        /// 特殊アクションを実行します。
        /// Executes a special action.
        /// </summary>
        /// <param name="actionName">実行するアクション名 / Name of the action to execute</param>
        /// <param name="talkScene">現在のTalkScene / Current TalkScene</param>
        /// <returns>実行が成功したかどうか / Whether execution succeeded</returns>
        public bool ExecuteAction(string actionName, TalkScene talkScene)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneActionExecutor] Action name is null or empty");
                return false;
            }

            if (talkScene == null || talkScene.targetHeroine == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneActionExecutor] TalkScene or target heroine is null");
                return false;
            }

            AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                $"[TalkSceneActionExecutor] Executing action: {actionName}");

            try
            {
                switch (actionName)
                {
                    case "consent_to_sex":
                        return ExecuteConsentToSex(talkScene);

                    case "accept_lunch_together":
                        return ExecuteAcceptLunchTogether(talkScene);

                    case "accept_club_activity_together":
                        return ExecuteAcceptClubActivityTogether(talkScene);

                    case "accept_go_home_together":
                        return ExecuteAcceptGoHomeTogether(talkScene);

                    case "accept_study_together":
                        return ExecuteAcceptStudyTogether(talkScene);

                    case "accept_recreate_together":
                        return ExecuteAcceptRecreateTogether(talkScene);

                    case "accept_accompany_player":
                        return ExecuteAcceptAccompanyPlayer(talkScene);

                    case "accept_date_reservation":
                        return ExecuteAcceptDateReservation(talkScene);

                    default:
                        AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                            $"[TalkSceneActionExecutor] Unknown action: {actionName}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    $"[TalkSceneActionExecutor] Failed to execute action '{actionName}': {ex.Message}");
                return false;
            }
        }

        // =====================================================================
        // Private action implementations
        // =====================================================================

        /// <summary>
        /// HシーンへのTalkScene終了アクション。
        /// Ends the TalkScene and proceeds to the H scene.
        /// </summary>
        private bool ExecuteConsentToSex(TalkScene talkScene)
        {
            talkScene.resultInfo.result = (ResultEnum)2;
            talkScene.resultInfo.isFirst = false;
            talkScene.endADVNo = 0;
            StartTalkEnd(talkScene);
            return true;
        }

        /// <summary>
        /// 一緒に昼食アクション。
        /// Accepts having lunch together.
        /// </summary>
        private bool ExecuteAcceptLunchTogether(TalkScene talkScene)
        {
            var game = Singleton<Game>.Instance;
            if (game == null || game.actScene == null || game.actScene.actCtrl == null)
                return false;

            talkScene.targetHeroine.isLunch = true;
            game.actScene.actCtrl.SetDesire(talkScene.targetHeroine, 3, 0);
            talkScene.resultInfo.result = (ResultEnum)3;
            talkScene.resultInfo.isFirst = false;
            talkScene.endADVNo = 0;
            StartTalkEnd(talkScene);
            return true;
        }

        /// <summary>
        /// 一緒に部活アクション。
        /// Accepts doing club activities together.
        /// </summary>
        private bool ExecuteAcceptClubActivityTogether(TalkScene talkScene)
        {
            talkScene.resultInfo.result = (ResultEnum)4;
            talkScene.resultInfo.isFirst = false;
            talkScene.endADVNo = 0;
            StartTalkEnd(talkScene);
            return true;
        }

        /// <summary>
        /// 一緒に帰宅アクション。
        /// Accepts going home together.
        /// </summary>
        private bool ExecuteAcceptGoHomeTogether(TalkScene talkScene)
        {
            talkScene.resultInfo.result = (ResultEnum)5;
            talkScene.resultInfo.isFirst = false;
            talkScene.endADVNo = 0;
            StartTalkEnd(talkScene);
            return true;
        }

        /// <summary>
        /// デート予約アクション。
        /// Reserves a date without ending the TalkScene immediately.
        /// </summary>
        private bool ExecuteAcceptDateReservation(TalkScene talkScene)
        {
            var game = Singleton<Game>.Instance;
            if (game == null || game.actScene == null || game.actScene.Cycle == null)
                return false;

            talkScene.targetHeroine.isDate = true;
            game.actScene.Cycle.dateHeroine = talkScene.targetHeroine;
            return true;
        }

        /// <summary>
        /// 一緒に勉強アクション。
        /// Accepts studying together.
        /// </summary>
        private bool ExecuteAcceptStudyTogether(TalkScene talkScene)
        {
            var game = Singleton<Game>.Instance;
            if (game == null || game.Player == null)
                return false;

            game.Player.intellect = game.Player.intellect + 3;
            talkScene.resultInfo.result = (ResultEnum)6;
            talkScene.resultInfo.isFirst = false;
            talkScene.endADVNo = 0;
            StartTalkEnd(talkScene);
            return true;
        }

        /// <summary>
        /// 一緒に運動アクション。
        /// Accepts exercising together.
        /// </summary>
        private bool ExecuteAcceptRecreateTogether(TalkScene talkScene)
        {
            var game = Singleton<Game>.Instance;
            if (game == null || game.Player == null)
                return false;

            game.Player.physical = game.Player.physical + 3;
            talkScene.resultInfo.result = (ResultEnum)7;
            talkScene.resultInfo.isFirst = false;
            talkScene.endADVNo = 0;
            StartTalkEnd(talkScene);
            return true;
        }

        /// <summary>
        /// プレイヤーに同行アクション。
        /// Accepts accompanying the player.
        /// </summary>
        private bool ExecuteAcceptAccompanyPlayer(TalkScene talkScene)
        {
            talkScene.resultInfo.result = (ResultEnum)1;
            talkScene.resultInfo.isFirst = false;
            talkScene.endADVNo = 0;
            StartTalkEnd(talkScene);
            return true;
        }

        // =====================================================================
        // Helper
        // =====================================================================

        /// <summary>
        /// TalkEnd コルーチンを AICharacterBridgePlugin 経由で開始します。
        ///
        /// Starts the TalkEnd coroutine via AICharacterBridgePlugin.
        /// </summary>
        /// <param name="talkScene">TalkEnd を呼び出す対象の TalkScene / Target TalkScene to call TalkEnd on</param>
        private void StartTalkEnd(TalkScene talkScene)
        {
            var plugin = AICharacterBridgePlugin.Instance;
            if (plugin == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[TalkSceneActionExecutor] Cannot start TalkEnd: Plugin instance is null.");
                return;
            }

            plugin.StartCoroutine(talkScene.TalkEnd());
        }
    }
}
