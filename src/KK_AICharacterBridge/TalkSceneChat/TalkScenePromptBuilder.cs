using ActionGame;
using AICharacterBridge.Core.Data;
using AICharacterBridge.Core.Prompt;
using AICharacterBridge.Data;
using AICharacterBridge.TalkSceneChat.Data;
using Manager;
using System;
using System.Collections.Generic;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// TalkSceneチャット用のプロンプト構築を担当するクラス。
    /// Handles prompt construction for TalkScene chat.
    /// </summary>
    public class TalkScenePromptBuilder
    {
        private readonly TalkSceneSessionManager _sessionManager;
        private readonly TalkSceneActionFilter _actionFilter;

        /// <summary>
        /// コンストラクタ
        /// Constructor
        /// </summary>
        /// <param name="sessionManager">セッションマネージャー / Session manager</param>
        /// <param name="actionFilter">アクションフィルター / Action filter</param>
        public TalkScenePromptBuilder(
            TalkSceneSessionManager sessionManager,
            TalkSceneActionFilter actionFilter)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _actionFilter = actionFilter ?? throw new ArgumentNullException(nameof(actionFilter));
        }

        /// <summary>
        /// プロンプトを構築します。
        /// Builds a prompt.
        /// </summary>
        /// <param name="template">プロンプトテンプレート / Prompt template</param>
        /// <param name="userMessage">ユーザーメッセージ / User message</param>
        /// <param name="worldSetting">世界設定情報 / World setting</param>
        /// <param name="heroine">対象のヒロイン / Target heroine</param>
        /// <param name="expressions">利用可能な表情リスト / Available expressions</param>
        /// <param name="charaMotions">利用可能なモーションリスト / Available motions</param>
        /// <returns>構築されたプロンプト文字列、またはエラー時 null / Built prompt string, or null on error</returns>
        public string BuildPrompt(
            string template,
            string userMessage,
            WorldSetting worldSetting,
            SaveData.Heroine heroine,
            List<ExpressionData> expressions,
            List<CharaMotionData> charaMotions)
        {
            if (string.IsNullOrEmpty(template))
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[TalkScenePromptBuilder] Template is null or empty");
                return null;
            }

            if (heroine == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[TalkScenePromptBuilder] Heroine is null");
                return null;
            }

            try
            {
                // 1. CharacterCard データの取得
                // Get CharacterCard data
                var userCard = CharacterCardResolver.GetResolvedPlayerCard();
                var characterCard = CharacterCardResolver.GetResolvedHeroineCard(heroine);

                if (userCard == null || characterCard == null)
                    return null;

                // 2. 会話ログのフォーマット（TalkSceneLogFormatter を使用）
                // Format chat log (using TalkSceneLogFormatter)
                string chatLog = TalkSceneLogFormatter.FormatLogs(heroine, _sessionManager);

                // 3. 利用可能なアクションの取得
                // Get available actions
                var availableActions = _actionFilter.GetAvailableActions(heroine);

                // 4. context_note の取得
                // TalkSceneChat 専用セーブデータからヒロインの context_note を取得する。
                // Retrieve context_note for the heroine from TalkSceneChat save data.
                // タグ付き置換を使用し、空の場合はプレースホルダーを行ごと削除する。
                // Uses tagged replacement; removes the placeholder line if empty.
                string contextNote = TalkSceneChatGameController.CurrentSaveData?.GetContextNote(heroine) ?? "";

                // 5. 置換エントリーの構築
                // Build replacement entries
                var entries = BuildReplaceEntries(
                    userMessage,
                    worldSetting,
                    userCard,
                    characterCard,
                    chatLog,
                    contextNote,
                    expressions,
                    charaMotions,
                    availableActions);

                // 6. プロンプトの生成
                // Generate prompt
                return PromptReplacer.ReplaceAll(template, entries);
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    $"[TalkScenePromptBuilder] Failed to build prompt: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// プロンプト置換エントリーのリストを構築します。
        /// テンプレート内のプレースホルダーを置換するためのエントリーリストを、
        /// 置換順序を保証した状態で生成します。
        ///
        /// 置換方式の方針 / Replacement method policy:
        ///   Plain:
        ///     プレースホルダーがテキスト中にインラインで使用される場合、または
        ///     テンプレートに囲みタグを直書きしているもの（例: name タグ内の user_name）。
        ///     Used for inline placeholders; also for fields whose enclosing tags
        ///     are written directly in the template (e.g., user_name inside a name tag).
        ///
        ///   Tagged block:
        ///     テンプレートでは {{key}} のみ記述し、タグを Builder 側で付与する場合。
        ///     値が空のとき行ごと削除される（world_setting, context_note 等と同様）。
        ///     Used when the template has only {{key}} and the Builder adds surrounding tags.
        ///     An empty value removes the placeholder line entirely.
        ///
        ///   Tagged block + note:
        ///     タグに note 属性が必要な場合。
        ///     Used when the tag requires a note attribute.
        ///
        /// Builds an ordered list of replacement entries for prompt placeholders.
        /// The list order determines replacement priority.
        /// </summary>
        private List<ReplaceEntry> BuildReplaceEntries(
            string userMessage,
            WorldSetting worldSetting,
            CharacterCard userCard,
            CharacterCard characterCard,
            string chatLog,
            string contextNote,
            List<ExpressionData> expressions,
            List<CharaMotionData> charaMotions,
            List<string> availableActions)
        {
            // WorldSetting の処理
            // world_setting はタグ付き置換。値が null または空の場合はプレースホルダーを行ごと削除する。
            // WorldSetting processing.
            // world_setting uses tagged replacement. If null or empty, the placeholder line is removed.
            string worldSettingText = (worldSetting != null && !string.IsNullOrEmpty(worldSetting.GetDescription()))
                ? worldSetting.GetDescription()
                : null;

            return new List<ReplaceEntry>
            {
                // =====================================================================
                // 基本情報 / Basic information
                // =====================================================================

                // Plain: 出力フォーマット指示文中にインラインで埋め込まれる
                // Plain: embedded inline within the output format instruction
                ReplaceEntry.Plain("language", AICharacterBridgePlugin.Language.Value ?? "English"),

                // =====================================================================
                // 世界設定 / World setting
                // =====================================================================

                // Tagged block: 記述がない場合はプレースホルダー行ごと削除
                // Tagged block: removed entirely if not set
                ReplaceEntry.Tagged("world_setting", worldSettingText, "world_setting"),

                // =====================================================================
                // ユーザー情報 / User information
                // =====================================================================

                // Plain: テンプレートの name タグ内にインラインで埋め込まれる
                // Plain: embedded inline inside the name tag written directly in the template
                ReplaceEntry.Plain("user_name", userCard.GetName() ?? "User"),

                // Tagged block -> <description>
                ReplaceEntry.Tagged("user_description", userCard.GetDescription() ?? "", "description"),

                // Tagged block -> <personality>
                ReplaceEntry.Tagged("user_personality", userCard.GetPersonality() ?? "", "personality"),

                // =====================================================================
                // キャラクター情報 / Character information
                // =====================================================================

                // Plain: テンプレートの name タグ内にインラインで埋め込まれる、また文中にも複数登場
                // Plain: embedded inline inside the name tag, also appears in response_rules body text
                ReplaceEntry.Plain("char_name", characterCard.GetName() ?? "Character"),

                // Tagged block -> <description>
                ReplaceEntry.Tagged("char_description", characterCard.GetDescription() ?? "", "description"),

                // Tagged block -> <personality>
                ReplaceEntry.Tagged("char_personality", characterCard.GetPersonality() ?? "", "personality"),

                // =====================================================================
                // 現在の状況 / Current situation
                // =====================================================================

                // Plain: time タグ内にインラインで埋め込まれる
                // Plain: embedded inline inside the time tag
                ReplaceEntry.Plain("time_period", GameDataFormatter.FormatTimePeriod(GameStateProvider.GetCurrentTimePeriod())),

                // Plain: time タグ内にインラインで埋め込まれる
                // Plain: embedded inline inside the time tag
                ReplaceEntry.Plain("week", GameDataFormatter.FormatWeek(GameStateProvider.GetCurrentWeek())),

                // Plain: location タグ内にインラインで埋め込まれる
                // Plain: embedded inline inside the location tag
                ReplaceEntry.Plain("location", GameDataFormatter.FormatLocation(GameStateProvider.GetCurrentLocation())),

                // Plain: location タグ内にインラインで埋め込まれる
                // Plain: embedded inline inside the location tag
                ReplaceEntry.Plain("school_name", GameStateProvider.GetSchoolName()),

                // Tagged block: 記述がない場合はプレースホルダー行ごと削除
                // Tagged block: removed entirely if not set
                ReplaceEntry.Tagged("context_note", contextNote, "context_note"),

                // =====================================================================
                // 会話履歴 / Conversation history
                // =====================================================================

                // Tagged block -> <conversation_history>
                ReplaceEntry.Tagged("chat_log", chatLog ?? "None", "conversation_history"),

                // =====================================================================
                // ユーザー発言 / User message
                // =====================================================================

                // Tagged block + note -> <user_turn note="...">
                ReplaceEntry.Tagged(
                    "user_message",
                    userMessage ?? "",
                    "user_turn",
                    "This input contains the user's spoken words and/or physical actions."),

                // =====================================================================
                // 利用可能なオプション（JSON 配列形式）/ Available options (JSON array format)
                // =====================================================================

                // Tagged block -> <available_expressions>
                ReplaceEntry.Tagged(
                    "available_expressions",
                    ExpressionData.ToJsonList(expressions),
                    "available_expressions"),

                // Tagged block -> <available_poses>
                // キー名（available_chara_motions）とタグ名（available_poses）が異なる点に注意
                // Note: key name (available_chara_motions) intentionally differs from tag name (available_poses)
                ReplaceEntry.Tagged(
                    "available_chara_motions",
                    CharaMotionData.ToJsonList(charaMotions),
                    "available_poses"),

                // Tagged block -> <available_impressions_on_user>
                // キー名（available_impressions）とタグ名（available_impressions_on_user）が異なる点に注意
                // Note: key name intentionally differs from tag name
                ReplaceEntry.Tagged(
                    "available_impressions",
                    ReplaceEntry.FormatStringListAsJson(new List<string>
                    {
                        "very_bad", "bad", "neutral", "good", "very_good"
                    }),
                    "available_impressions_on_user"),

                // Tagged block -> <available_post_conversation_actions>
                // キー名（available_post_actions）とタグ名（available_post_conversation_actions）が異なる点に注意
                // Note: key name intentionally differs from tag name
                ReplaceEntry.Tagged(
                    "available_post_actions",
                    ReplaceEntry.FormatStringListAsJson(availableActions),
                    "available_post_conversation_actions"),

                // =====================================================================
                // 互換性のための変数（{{user}}, {{char}} 形式）
                // Compatibility variables ({{user}}, {{char}} format)
                // =====================================================================

                ReplaceEntry.Plain("user", userCard.GetName()      ?? "User"),
                ReplaceEntry.Plain("char", characterCard.GetName() ?? "Character"),
            };
        }
    }
}
