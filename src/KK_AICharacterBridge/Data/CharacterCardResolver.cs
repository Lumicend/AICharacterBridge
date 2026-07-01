using AICharacterBridge.Core.Data;
using AICharacterBridge.Core.Prompt;
using Manager;
using System;
using System.Collections.Generic;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// CharacterCard 内のプレースホルダーをゲームの現在状態に基づいて解決する静的クラス。
    /// Static class that resolves placeholders in CharacterCards based on the current game state.
    ///
    /// 対象プレースホルダー / Target placeholders:
    ///   {{clothes}}    → 現在の衣装プリセットに対応する服装説明文
    ///                    Clothes description for the current outfit preset
    ///   {{appearance}} → 現在の衣装プリセットに対応する容姿説明文
    ///                    Appearance description for the current outfit preset
    ///
    /// 元の CharacterCard は変更しない（クローンに対して置換を行う）。
    /// The original CharacterCard is never modified (replacement is performed on a clone).
    /// </summary>
    public static class CharacterCardResolver
    {
        // =====================================================================
        // Public API
        // =====================================================================

        /// <summary>
        /// プレイヤーの CharacterCard を取得し、衣装プレースホルダーを解決して返す。
        /// Gets the player's CharacterCard and returns it with outfit placeholders resolved.
        ///
        /// 衣装プリセットは Singleton&lt;Game&gt;.Instance.Player.changeClothesType から取得する。
        /// The outfit preset is obtained from Singleton&lt;Game&gt;.Instance.Player.changeClothesType.
        /// changeClothesType が -1（自動）の場合はインデックス 0 として扱う（暫定処理）。
        /// If changeClothesType is -1 (auto), it is treated as index 0 (temporary behavior).
        /// </summary>
        /// <returns>
        /// プレースホルダーが解決された CharacterCard のクローン。取得に失敗した場合は null。
        /// A clone of the CharacterCard with placeholders resolved. Returns null on failure.
        /// </returns>
        public static CharacterCard GetResolvedPlayerCard()
        {
            try
            {
                var card = CharacterCardProvider.GetPlayerCharacterCard();
                if (card == null)
                    return null;

                // changeClothesType を取得し、-1（自動）は 0 として扱う
                // Get changeClothesType; treat -1 (auto) as 0
                int rawIndex = Singleton<Game>.Instance?.Player?.changeClothesType ?? 0;
                int presetIndex = rawIndex < 0 ? 0 : rawIndex;

                return ResolveCoordinatePlaceholders(card, presetIndex);
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    $"[CharacterCardResolver] Failed to get resolved player card: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 指定したヒロインの CharacterCard を取得し、衣装プレースホルダーを解決して返す。
        /// Gets the specified heroine's CharacterCard and returns it with outfit placeholders resolved.
        ///
        /// 衣装プリセットは heroine.NowCoordinate から取得する。
        /// The outfit preset is obtained from heroine.NowCoordinate.
        /// </summary>
        /// <param name="heroine">対象のヒロイン / Target heroine</param>
        /// <returns>
        /// プレースホルダーが解決された CharacterCard のクローン。取得に失敗した場合は null。
        /// A clone of the CharacterCard with placeholders resolved. Returns null on failure.
        /// </returns>
        public static CharacterCard GetResolvedHeroineCard(SaveData.Heroine heroine)
        {
            if (heroine == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[CharacterCardResolver] Heroine is null");
                return null;
            }

            try
            {
                var card = CharacterCardProvider.GetHeroineCharacterCard(heroine);
                if (card == null)
                    return null;

                return ResolveCoordinatePlaceholders(card, heroine.NowCoordinate);
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    $"[CharacterCardResolver] Failed to get resolved heroine card: {ex.Message}");
                return null;
            }
        }

        // =====================================================================
        // Private implementation
        // =====================================================================

        /// <summary>
        /// CharacterCard をクローンし、全テキストフィールドの衣装プレースホルダーを解決して返す。
        /// Clones the CharacterCard and returns it with outfit placeholders resolved in all text fields.
        ///
        /// 置換対象フィールド / Fields subject to replacement:
        ///   Name, Description, Personality, MessageExample, FirstMessage, Scenario
        /// </summary>
        /// <param name="card">元の CharacterCard / Original CharacterCard</param>
        /// <param name="presetIndex">衣装プリセット番号（0〜6）/ Outfit preset number (0–6)</param>
        /// <returns>プレースホルダーが解決された CharacterCard のクローン / Clone with resolved placeholders</returns>
        private static CharacterCard ResolveCoordinatePlaceholders(CharacterCard card, int presetIndex)
        {
            // RawJson からクローンを生成し、元のカードを保護する
            // Generate a clone from RawJson to protect the original card
            var clone = CharacterCard.FromJson(card.RawJson);

            // CoordinateData を extensions から読み込む
            // Load CoordinateData from extensions
            var coordinateData = LoadCoordinateData(card);

            // プレースホルダー置換エントリーを構築する
            // Build placeholder replacement entries
            var entries = new List<ReplaceEntry>
            {
                ReplaceEntry.Plain("clothes",    coordinateData.GetClothes(presetIndex)),
                ReplaceEntry.Plain("appearance", coordinateData.GetAppearance(presetIndex)),
            };

            // 全テキストフィールドにプレースホルダー置換を適用する
            // Apply placeholder replacement to all text fields
            clone.SetName(PromptReplacer.ReplaceAll(clone.GetName(), entries));
            clone.SetDescription(PromptReplacer.ReplaceAll(clone.GetDescription(), entries));
            clone.SetPersonality(PromptReplacer.ReplaceAll(clone.GetPersonality(), entries));
            clone.SetMessageExample(PromptReplacer.ReplaceAll(clone.GetMessageExample(), entries));
            clone.SetFirstMessage(PromptReplacer.ReplaceAll(clone.GetFirstMessage(), entries));
            clone.SetScenario(PromptReplacer.ReplaceAll(clone.GetScenario(), entries));

            return clone;
        }

        /// <summary>
        /// CharacterCard の extensions から CoordinateData を読み込む。
        /// Loads CoordinateData from CharacterCard extensions.
        ///
        /// extensions に値が存在しない場合や読み込みに失敗した場合は、
        /// 空の CoordinateData（全プレースホルダーが空文字に置換される）を返す。
        /// Returns an empty CoordinateData (all placeholders replaced with empty strings)
        /// if the value does not exist in extensions or loading fails.
        /// </summary>
        /// <param name="card">対象の CharacterCard / Target CharacterCard</param>
        /// <returns>読み込まれた CoordinateData / Loaded CoordinateData</returns>
        private static CoordinateData LoadCoordinateData(CharacterCard card)
        {
            try
            {
                var token = card.GetExtensionValue(
                    AICharacterBridgePlugin.ExtensionNamespace,
                    AICharacterBridgePlugin.CoordinateDataKey);

                if (token != null)
                {
                    var data = token.ToObject<CoordinateData>();
                    if (data != null)
                    {
                        data.EnsurePresetCount();
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning(
                    $"[CharacterCardResolver] Failed to load CoordinateData from extensions: {ex.Message}");
            }

            // CoordinateData が未設定の場合は空インスタンスを返す（プレースホルダーは空文字に置換）
            // Return an empty instance if CoordinateData is not set (placeholders replaced with empty strings)
            return new CoordinateData();
        }
    }
}
