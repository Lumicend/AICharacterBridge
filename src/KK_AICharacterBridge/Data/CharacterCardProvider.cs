using AICharacterBridge.Core.Data;
using Manager;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// CharacterCardデータの取得を担当する静的クラス
    /// Static class responsible for retrieving CharacterCard data
    /// </summary>
    public static class CharacterCardProvider
    {
        /// <summary>
        /// PlayerのCharacterCardを取得します。
        /// Gets the Player's CharacterCard.
        /// </summary>
        /// <returns>PlayerのCharacterCard、取得できない場合はnull</returns>
        public static CharacterCard GetPlayerCharacterCard()
        {
            var saveData = GameController.CurrentSaveData;
            if (saveData == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[CharacterCardProvider] Save data not found");
                return null;
            }

            var userCard = saveData.GetPlayerCharacterCard();
            if (userCard == null || userCard.IsDefault())
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[CharacterCardProvider] Player CharacterCard not found or is default");
                return null;
            }

            // Nameが空の場合はキャラクター名を使用
            // Use character name if Name is empty
            if (string.IsNullOrEmpty(userCard.GetName()))
            {
                var player = Singleton<Game>.Instance?.Player;
                if (player != null && player.charFile != null && player.charFile.parameter != null)
                    userCard.SetName(player.charFile.parameter.fullname);
            }

            return userCard;
        }

        /// <summary>
        /// HeroineのCharacterCardを取得します。
        /// Gets the Heroine's CharacterCard.
        /// </summary>
        /// <param name="heroine">対象のHeroine</param>
        /// <returns>HeroineのCharacterCard、取得できない場合はnull</returns>
        public static CharacterCard GetHeroineCharacterCard(SaveData.Heroine heroine)
        {
            if (heroine == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[CharacterCardProvider] Heroine is null");
                return null;
            }

            var saveData = GameController.CurrentSaveData;
            if (saveData == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[CharacterCardProvider] Save data not found");
                return null;
            }

            var characterCard = saveData.GetCharacterCardForHeroine(heroine);
            if (characterCard == null || characterCard.IsDefault())
            {
                AICharacterBridgePlugin.Instance?.Logger.LogError(
                    "[CharacterCardProvider] Heroine CharacterCard not found or is default");
                return null;
            }

            // Nameが空の場合はキャラクター名を使用
            // Use character name if Name is empty
            if (string.IsNullOrEmpty(characterCard.GetName()))
            {
                if (heroine.charFile != null && heroine.charFile.parameter != null)
                    characterCard.SetName(heroine.charFile.parameter.fullname);
            }

            return characterCard;
        }
    }
}
