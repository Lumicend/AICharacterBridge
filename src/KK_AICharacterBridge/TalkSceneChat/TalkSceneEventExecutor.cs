using ActionGame;
using ADV;
using AICharacterBridge.Data;
using AICharacterBridge.TalkSceneChat.Response;
using KKAPI.MainGame;
using Manager;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// ADVイベントの構築と実行を担当するクラス。
    /// AIの応答からゲーム内イベントを生成して実行します。
    /// Handles construction and execution of ADV events.
    /// Generates and executes in-game events from AI responses.
    /// </summary>
    public class TalkSceneEventExecutor
    {
        /// <summary>ゲーム画面に表示可能な1メッセージあたりの最大文字数</summary>
        private const int MAX_MESSAGE_LENGTH = 87;

        /// <summary>
        /// AIの応答からADVイベントを構築して実行します。
        /// Constructs and executes an ADV event from AI response.
        /// </summary>
        /// <param name="talkScene">現在のTalkScene</param>
        /// <param name="heroine">対象のHeroine</param>
        /// <param name="response">AIの応答</param>
        /// <param name="expressions">利用可能な表情リスト</param>
        /// <param name="charaMotions">利用可能なモーションリスト</param>
        /// <returns>イベント実行のコルーチン</returns>
        public IEnumerator ExecuteAIEvent(
            TalkScene talkScene,
            TalkSceneResponse response,
            List<ExpressionData> expressions,
            List<CharaMotionData> charaMotions)
        {
            var heroine = talkScene.targetHeroine;
            if (talkScene == null || heroine == null || response == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[TalkSceneEventExecutor] Cannot execute event: Invalid parameters");
                yield break;
            }

            // イベントコマンドリストを作成
            var commandList = BuildEventCommandList(response, expressions, charaMotions);

            if (commandList == null || commandList.Count == 0)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    "[TalkSceneEventExecutor] Event command list is empty");
                yield break;
            }

            // イベントを実行
            yield return EventUtils.StartTextSceneEvent(
                talkScene,
                commandList,
                endTalkScene: false,
                decreaseTalkTime: false
            );

            AICharacterBridgePlugin.Instance?.Logger.LogInfo(
                "[TalkSceneEventExecutor] AI event execution completed");
        }

        /// <summary>
        /// AIの応答からイベントコマンドリストを構築します。
        /// Builds an event command list from AI response.
        /// </summary>
        /// <param name="response">AIの応答</param>
        /// <param name="expressions">利用可能な表情リスト</param>
        /// <param name="charaMotions">利用可能なモーションリスト</param>
        /// <returns>イベントコマンドのリスト</returns>
        private List<Program.Transfer> BuildEventCommandList(
            TalkSceneResponse response,
            List<ExpressionData> expressions,
            List<CharaMotionData> charaMotions)
        {
            // イベントの初期化
            var list = EventUtils.CreateNewEvent(waitForSceneFade: false, setPlayerParam: true);

            // Heroineのパラメータを設定（HeroineのIDは -2）
            list.Insert(0, Program.Transfer.Create(true, Command.CharaChange, "-2", "true"));

            // 各セグメントをコマンドに変換
            foreach (var segment in response.ConversationSegments)
            {
                // 表情の変更
                if (!string.IsNullOrEmpty(segment.Expression))
                {
                    AddExpressionCommand(list, segment.Expression, expressions);
                }

                // モーションの変更
                if (!string.IsNullOrEmpty(segment.CharaMotion))
                {
                    AddMotionCommand(list, segment.CharaMotion, charaMotions);
                }

                // セリフまたは描写の追加（長文の場合は分割）
                if (segment.Type == "dialogue")
                {
                    var messageSegments = SplitMessage(segment.Content, MAX_MESSAGE_LENGTH);
                    foreach (var messageSegment in messageSegments)
                    {
                        list.Add(Program.Transfer.Text(EventUtils.HeroineName, messageSegment));
                    }
                }
                else if (segment.Type == "observation")
                {
                    var messageSegments = SplitMessage(segment.Content, MAX_MESSAGE_LENGTH);
                    foreach (var messageSegment in messageSegments)
                    {
                        list.Add(Program.Transfer.Text(EventUtils.Narrator, messageSegment));
                    }
                }
            }

            // イベント終了コマンド
            list.Add(Program.Transfer.Close());

            return list;
        }

        /// <summary>
        /// 表情変更コマンドをリストに追加します。
        /// Adds an expression change command to the list.
        /// </summary>
        /// <param name="list">コマンドリスト</param>
        /// <param name="expressionName">表情名</param>
        /// <param name="expressions">利用可能な表情リスト</param>
        private void AddExpressionCommand(
            List<Program.Transfer> list,
            string expressionName,
            List<ExpressionData> expressions)
        {
            ExpressionData expressionData = FindExpression(expressionName, expressions);

            if (expressionData == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[TalkSceneEventExecutor] Unknown expression: {expressionName}");
                return;
            }

            var args = expressionData.ToCommandArgs();
            list.Add(Program.Transfer.Create(true, Command.CharaExpression, "-2",
                args[0], args[1], args[2], args[3], args[4], args[5],
                args[6], args[7], args[8], args[9], args[10]));
        }

        /// <summary>
        /// モーション変更コマンドをリストに追加します。
        /// Adds a motion change command to the list.
        /// </summary>
        /// <param name="list">コマンドリスト</param>
        /// <param name="motionName">モーション名</param>
        /// <param name="charaMotions">利用可能なモーションリスト</param>
        private void AddMotionCommand(
            List<Program.Transfer> list,
            string motionName,
            List<CharaMotionData> charaMotions)
        {
            CharaMotionData motionData = FindMotion(motionName, charaMotions);

            if (motionData == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[TalkSceneEventExecutor] Unknown motion: {motionName}");
                return;
            }

            var args = motionData.ToCommandArgs();
            list.Add(Program.Transfer.Create(true, Command.CharaMotion, "-2",
                args[0], args[1], args[2], args[3], args[4],
                args[5], args[6], args[7], args[8], args[9]));
        }

        /// <summary>
        /// 長いメッセージを指定された最大長に分割します。
        /// Splits long messages into segments of specified maximum length.
        /// </summary>
        /// <param name="message">分割するメッセージ</param>
        /// <param name="maxLength">1セグメントあたりの最大文字数</param>
        /// <returns>分割されたメッセージのリスト</returns>
        private List<string> SplitMessage(string message, int maxLength)
        {
            var segments = new List<string>();

            if (string.IsNullOrEmpty(message))
                return segments;

            if (message.Length <= maxLength)
            {
                segments.Add(message);
                return segments;
            }

            int startIndex = 0;
            while (startIndex < message.Length)
            {
                int length = Math.Min(maxLength, message.Length - startIndex);
                segments.Add(message.Substring(startIndex, length));
                startIndex += length;
            }

            return segments;
        }

        /// <summary>
        /// 表情名から表情データを検索します。
        /// Finds expression data by name.
        /// </summary>
        private ExpressionData FindExpression(string name, List<ExpressionData> expressions)
        {
            if (expressions == null || string.IsNullOrEmpty(name))
                return null;

            foreach (var expr in expressions)
            {
                if (expr.Name == name)
                    return expr;
            }
            return null;
        }

        /// <summary>
        /// モーション名からモーションデータを検索します。
        /// Finds motion data by name.
        /// </summary>
        private CharaMotionData FindMotion(string name, List<CharaMotionData> charaMotions)
        {
            if (charaMotions == null || string.IsNullOrEmpty(name))
                return null;

            foreach (var motion in charaMotions)
            {
                if (motion.Name == name)
                    return motion;
            }
            return null;
        }
    }
}