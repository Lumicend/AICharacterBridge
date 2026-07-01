using ActionGame;
using Manager;
using System.Collections.Generic;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// ゲーム状態に基づいて利用可能なアクションをフィルタリングするクラス。
    /// Filters available actions based on game state.
    /// </summary>
    public class TalkSceneActionFilter
    {
        // 全ての特殊アクション定義
        private static readonly List<string> ALL_ACTIONS = new List<string>
        {
            "continue_conversation",
            "consent_to_sex",
            "accept_lunch_together",
            "accept_club_activity_together",
            "accept_go_home_together",
            "accept_study_together",
            "accept_recreate_together",
            "accept_accompany_player",
            "accept_date_reservation"
            //"accept_club_recruitment",
            //"accept_confession_become_lovers",

        };

        /// <summary>
        /// 現在のゲーム状態に基づいて利用可能なアクションリストを取得します。
        /// Gets the list of available actions based on current game state.
        /// </summary>
        /// <param name="heroine">対象のHeroine</param>
        /// <returns>利用可能なアクションのリスト</returns>
        public List<string> GetAvailableActions(SaveData.Heroine heroine)
        {
            if (heroine == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneActionFilter] Heroine is null. Returning minimal actions.");
                return new List<string> { "continue_conversation" };
            }

            var game = Singleton<Game>.Instance;

            // 怒っている場合は会話継続のみ
            if (heroine.isAnger)
            {
                return new List<string> { "continue_conversation" };
            }

            // 全アクションから開始
            var availableActions = new List<string>(ALL_ACTIONS);

            // 各アクションの利用可能性をチェック
            FilterLunchAction(availableActions, game, heroine);
            FilterClubActivityAction(availableActions, game, heroine);
            FilterGoHomeAction(availableActions, game);
            FilterDateReservationAction(availableActions, heroine);

            return availableActions;
        }

        /// <summary>
        /// ランチアクションのフィルタリング
        /// </summary>
        private void FilterLunchAction(List<string> actions, Game game, SaveData.Heroine heroine)
        {
            if (game == null || game.actScene == null || game.actScene.Cycle == null)
                return;

            // 昼食時間でない、または既に昼食済みの場合は削除
            if (game.actScene.Cycle.nowType != Cycle.Type.LunchTime || heroine.isLunch)
            {
                actions.Remove("accept_lunch_together");
            }
        }

        /// <summary>
        /// 部活アクションのフィルタリング
        /// </summary>
        private void FilterClubActivityAction(List<string> actions, Game game, SaveData.Heroine heroine)
        {
            if (game == null || game.actScene == null || game.actScene.Cycle == null)
                return;

            // 部活時間でない、またはスタッフでない場合は削除
            if (game.actScene.Cycle.nowType != Cycle.Type.StaffTime || !heroine.isStaff)
            {
                actions.Remove("accept_club_activity_together");
            }
        }

        /// <summary>
        /// 帰宅アクションのフィルタリング
        /// </summary>
        private void FilterGoHomeAction(List<string> actions, Game game)
        {
            if (game == null || game.actScene == null || game.actScene.Cycle == null)
                return;

            // 放課後でない場合は削除
            if (game.actScene.Cycle.nowType != Cycle.Type.AfterSchool)
            {
                actions.Remove("accept_go_home_together");
            }
        }

        /// <summary>
        /// デート予約アクションのフィルタリング
        /// </summary>
        private void FilterDateReservationAction(List<string> actions, SaveData.Heroine heroine)
        {
            // 既にデート予約済みの場合は削除
            if (heroine.isDate)
            {
                actions.Remove("accept_date_reservation");
            }
        }

        /// <summary>
        /// アクション名から表示用のテキストを取得します。
        /// Gets display text for an action name.
        /// </summary>
        /// <param name="actionName">アクション名</param>
        /// <returns>表示用テキスト</returns>
        public string GetActionDisplayText(string actionName)
        {
            switch (actionName)
            {
                case "continue_conversation":
                    return "Continue Conversation";
                case "consent_to_sex":
                    return "Consent to H Scene";
                case "accept_lunch_together":
                    return "Have Lunch Together";
                case "accept_club_activity_together":
                    return "Join Club Activity Together";
                case "accept_go_home_together":
                    return "Go Home Together";
                case "accept_date_reservation":
                    return "Make Date Reservation";
                case "accept_study_together":
                    return "Study Together";
                case "accept_recreate_together":
                    return "Exercise Together";
                case "accept_club_recruitment":
                    return "Recruit to Club";
                case "accept_accompany_player":
                    return "Accompany Player";
                default:
                    return "Unknown Action";
            }
        }

        /// <summary>
        /// アクションが特殊アクション（ゲーム状態を変更するもの）かどうかを判定します。
        /// Determines if an action is a special action (one that modifies game state).
        /// </summary>
        /// <param name="actionName">アクション名</param>
        /// <returns>特殊アクションの場合true</returns>
        public bool IsSpecialAction(string actionName)
        {
            return !string.IsNullOrEmpty(actionName) && actionName != "continue_conversation";
        }
    }
}
