using System;

namespace AICharacterBridge.TalkSceneChat
{
    /// <summary>
    /// TalkSceneチャット用のデフォルトプロンプトテンプレートを提供します。
    /// XMLハイブリッド構造を採用し、データの境界を明確化しています。
    ///
    /// テンプレート内のプレースホルダーと、TalkScenePromptBuilder における置換方式の対応:
    ///   {{language}}               Plain
    ///   {{world_setting}}          Tagged block
    ///   {{user_name}}              Plain  (name タグはテンプレートに直書き)
    ///   {{user_description}}       Tagged block  -> description タグ
    ///   {{user_personality}}       Tagged block  -> personality タグ
    ///   {{char_name}}              Plain
    ///   {{char_description}}       Tagged block  -> description タグ
    ///   {{char_personality}}       Tagged block  -> personality タグ
    ///   {{chat_log}}               Tagged block  -> conversation_history タグ(過去セッションのみ)
    ///   {{conversation}}           Tagged block + note  -> conversation タグ(進行中セッション + 今回のユーザー発言)
    ///   {{available_expressions}}  Tagged block  -> available_expressions タグ
    ///   {{available_chara_motions}}Tagged block  -> available_poses タグ
    ///   {{available_impressions}}  Tagged block  -> available_impressions_on_user タグ
    ///   {{available_post_actions}} Tagged block  -> available_post_conversation_actions タグ
    ///   {{user}}                   Plain  (互換性エイリアス)
    ///   {{char}}                   Plain  (互換性エイリアス)
    ///
    /// Provides the default prompt template for TalkScene chat.
    /// Uses XML hybrid structure for clear data boundaries.
    ///
    /// Note on tag names:
    ///   user_name and char_name are replaced via Plain because
    ///   using Tagged with tagName="user_name" would produce user_name tags
    ///   which are misrendered in some display tools.
    ///   Instead, the enclosing name tag is written directly in the template.
    ///
    /// 会話ブロックの構成について / About the conversation block structure:
    ///   時間帯・場所などが同一のプレースホルダー({{time_period}} 等)として
    ///   個別に存在していた旧構成を廃止し、{{chat_log}}(過去セッションのログ)と
    ///   {{conversation}}(進行中セッションのログ + 今回のユーザー発言)の
    ///   2ブロックに統合した。時間帯・場所・曜日・context_note(ooc_note)は、
    ///   いずれも TalkSceneLogFormatter がこの2ブロックのヘッダー生成時に
    ///   埋め込むため、個別のプレースホルダーとしては存在しない。
    ///   {{conversation}} の末尾行が、まだAIに応答されていないユーザーの
    ///   今回の発言(セリフまたは動作描写)であり、AIはこの行に応答する。
    ///
    ///   The previous structure, where time period, location, etc. each existed
    ///   as separate placeholders (e.g. {{time_period}}), has been removed in
    ///   favor of two blocks: {{chat_log}} (past-session logs) and
    ///   {{conversation}} (the in-progress session's log plus the user's
    ///   current turn). Time period, location, day of week, and context_note
    ///   (ooc_note) are all embedded by TalkSceneLogFormatter when it builds
    ///   the header for these two blocks, so they no longer exist as
    ///   individual placeholders.
    ///   The final line of {{conversation}} is the user's current turn
    ///   (spoken words or narrated action) that has not yet been responded to;
    ///   the AI is expected to respond to that line.
    ///
    /// 記述フォーマット(統一記法)について / About the unified narration format:
    ///   {{chat_log}} の直前に配置された "narration_format" ブロックは、
    ///   プレースホルダーを含まない固定文であり、TalkScenePromptBuilder による
    ///   置換の対象ではない。この固定文は、{{chat_log}}(過去セッションのログ)と
    ///   {{conversation}}(進行中セッションのログ + ユーザーの今回の発言)の
    ///   両方に共通して適用される記法("..." / *...*)をAIに説明するためのものである。
    ///   ユーザー発言は UserMessageFormatter によって送信時点でこの記法へ
    ///   正規化済みであり、キャラクター発言(過去・進行中を問わず)も
    ///   TalkSceneLog.FormatEntry によって同じ記法で整形されるため、
    ///   双方を読み解く際の共通ルールとして1箇所にまとめて記載している。
    ///
    ///   The "narration_format" block placed immediately before {{chat_log}}
    ///   is a fixed block of text containing no placeholders, and is not a
    ///   target of replacement by TalkScenePromptBuilder. It explains to the
    ///   AI the notation ("..."/*...*) that applies to both {{chat_log}}
    ///   (past-session logs) and {{conversation}} (the in-progress session's
    ///   log plus the user's current turn). User utterances are normalized
    ///   into this notation at send time by UserMessageFormatter, and
    ///   character utterances (past or in-progress) are formatted into the
    ///   same notation by TalkSceneLog.FormatEntry, so the shared rule for
    ///   interpreting both is documented once, in a single place.
    /// </summary>
    public static class TalkSceneDefaultTemplate
    {
        /// <summary>
        /// デフォルトのTalkSceneチャットプロンプトテンプレートを取得します。
        /// Gets the default TalkScene chat prompt template.
        /// </summary>
        public static string GetTemplate()
        {
            return
@"You are {{char_name}}, a fictional character in a game. You are NOT an AI assistant. Stay in character at all times. Never break character or acknowledge being an AI.

Your task is to respond to {{user_name}}'s most recent turn — the final line of the ""conversation"" block below. Use the provided information to generate an authentic, in-character response.

{{world_setting}}

<user_information>
<name>{{user_name}}</name>

{{user_description}}

{{user_personality}}
</user_information>

<your_character note=""You ARE this character. Embody them fully."">
<name>{{char_name}}</name>

{{char_description}}

{{char_personality}}
</your_character>

<narration_format note=""Applies to the conversation_history and conversation blocks below."">
Spoken words are written as plain text, or wrapped in ""double quotes"".
Actions, scenery, and emotional descriptions are wrapped in *asterisks*.
A single message may freely mix both, in any order. For example:
*stands up straight* ""Let's go."" *smiles softly*
</narration_format>

{{chat_log}}

{{conversation}}

<available_options>
{{available_expressions}}

{{available_chara_motions}}

{{available_impressions}}

{{available_post_actions}}
</available_options>

<response_rules>
1. All your responses, evaluations, and choices must reflect {{char_name}}'s feelings, personality, and perspective.

2. Your response must contain one or more segments of type ""dialogue"" or ""observation"".
   - conversation_segments does not have to start with ""dialogue"".
   - The ""observation"" type should only describe things the user can see, hear, or otherwise perceive. Do NOT describe the character's internal thoughts or feelings.

3. For EVERY segment, you MUST include ""expression"" and ""pose"" keys.
   - Choose values from the Available Options above.
   - If the expression or pose has not changed from the previous segment, write the same value again.

4. You MUST choose exactly one value for ""impression_on_user"" from the Available Impressions on User.
   - impression_on_user: How {{char_name}} currently feels about the user based on this conversation.

5. Evaluate whether {{char_name}} (your character) was sexually aroused by this conversation.
   - Set ""is_aroused_by_conversation"" to exactly ""yes"" or ""no"".

6. You MUST choose exactly one value for ""post_conversation_action"" from the Available Post-Conversation Actions.
   - post_conversation_action: The action {{char_name}} will take after this conversation ends.

7. Do NOT wrap ""content"" in quotation marks or asterisks. The ""type"" key already indicates whether a segment is dialogue or an observation, so the notation described above is not needed inside ""content"".
</response_rules>

<output_format>
Your response MUST be in {{language}}.
Do not use any emoji in your response.

Respond ONLY with the following JSON object. Do not include any text or markdown before or after it.

{
  ""conversation_segments"": [
    {
      ""type"": ""dialogue"",
      ""content"": ""(Write {{char_name}}'s dialogue here)"",
      ""expression"": ""(An expression from Available Expressions)"",
      ""pose"": ""(A pose from Available Poses)""
    },
    {
      ""type"": ""observation"",
      ""content"": ""(Describe the character's observable action or the scene here)"",
      ""expression"": ""(An expression from Available Expressions)"",
      ""pose"": ""(A pose from Available Poses)""
    }
  ],
  ""impression_on_user"": ""(An impression from Available Impressions on User)"",
  ""is_aroused_by_conversation"": ""(yes or no)"",
  ""post_conversation_action"": ""(An action from Available Post-Conversation Actions)""
}
</output_format>";
        }
    }
}
