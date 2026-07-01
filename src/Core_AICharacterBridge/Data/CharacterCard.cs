using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AICharacterBridge.Core.Data
{
    /// <summary>
    /// CharacterCardの基底クラス。
    /// RAW JSONを保持し、必要な値のみを動的に取得・更新する。
    /// Base class for CharacterCard.
    /// Holds RAW JSON and dynamically retrieves/updates only necessary values.
    /// </summary>
    [Serializable]
    public abstract class CharacterCard
    {
        /// <summary>完全なJSON文字列（全データ保持用）</summary>
        [JsonProperty("raw_json")]
        public string RawJson { get; set; }

        /// <summary>パース済みJObjectキャッシュ（内部使用、シリアライズしない）</summary>
        [JsonIgnore]
        protected JObject ParsedData { get; set; }

        protected CharacterCard(string rawJson)
        {
            if (string.IsNullOrEmpty(rawJson))
                throw new ArgumentException("Raw JSON cannot be null or empty", nameof(rawJson));

            RawJson = rawJson;
            ParsedData = JObject.Parse(rawJson);
        }

        // === バージョン情報 ===

        /// <summary>
        /// specフィールドの値を取得（"chara_card_v2" または "chara_card_v3"）
        /// Gets the spec field value ("chara_card_v2" or "chara_card_v3")
        /// </summary>
        public abstract string GetSpec();

        /// <summary>
        /// spec_versionフィールドの値を取得（"2.0" または "3.0"）
        /// Gets the spec_version field value ("2.0" or "3.0")
        /// </summary>
        public abstract string GetSpecVersion();

        // === 共通フィールドアクセス（V2/V3共通） ===

        public abstract string GetName();
        public abstract void SetName(string name);

        public abstract string GetDescription();
        public abstract void SetDescription(string description);

        public abstract string GetPersonality();
        public abstract void SetPersonality(string personality);

        public abstract string GetMessageExample();
        public abstract void SetMessageExample(string mesExample);

        public abstract string GetFirstMessage();
        public abstract void SetFirstMessage(string firstMes);

        public abstract string GetScenario();
        public abstract void SetScenario(string scenario);

        // === extensions アクセス（V2/V3共通） ===
        //
        // data.extensions 内のデータを名前空間キーで階層化して管理する。
        // Manages data within data.extensions using a namespace key hierarchy.
        //
        // JSON 構造例 / JSON structure example:
        //   "extensions": {
        //     "<namespaceKey>": {
        //       "<key>": <value>
        //     }
        //   }
        //
        // 設計方針 / Design policy:
        //   - 指定した名前空間・キーのみを読み書きし、それ以外のデータには一切触れない。
        //     Only the specified namespace and key are read/written; all other data is untouched.
        //   - extensions オブジェクト自体を上書きしないことで、未知のキーの欠落を防ぐ。
        //     The extensions object itself is never overwritten, preventing loss of unknown keys.

        /// <summary>
        /// 指定した名前空間・キーの extensions 値を取得する。
        /// Gets the extensions value for the specified namespace and key.
        /// 存在しない場合は null を返す。
        /// Returns null if the value does not exist.
        /// </summary>
        /// <param name="namespaceKey">名前空間キー（例: "ai_character_bridge_kk"）/ Namespace key</param>
        /// <param name="key">データキー（例: "coordinate_data"）/ Data key</param>
        public JToken GetExtensionValue(string namespaceKey, string key)
        {
            if (string.IsNullOrEmpty(namespaceKey) || string.IsNullOrEmpty(key))
                return null;

            return ParsedData["data"]?["extensions"]?[namespaceKey]?[key];
        }

        /// <summary>
        /// 指定した名前空間・キーに extensions 値を設定する。
        /// Sets an extensions value for the specified namespace and key.
        ///
        /// 既存の他のキーは一切変更しない。
        /// All other existing keys remain untouched.
        /// </summary>
        /// <param name="namespaceKey">名前空間キー（例: "ai_character_bridge_kk"）/ Namespace key</param>
        /// <param name="key">データキー（例: "coordinate_data"）/ Data key</param>
        /// <param name="value">設定する値 / Value to set</param>
        public void SetExtensionValue(string namespaceKey, string key, JToken value)
        {
            if (string.IsNullOrEmpty(namespaceKey) || string.IsNullOrEmpty(key))
                return;

            var data = ParsedData["data"];
            if (data == null) return;

            // extensions オブジェクトがなければ新規作成（既存データには触れない）
            // Create extensions object if absent (do not touch existing data)
            if (data["extensions"] == null)
                data["extensions"] = new JObject();

            var extensions = (JObject)data["extensions"];

            // 名前空間オブジェクトがなければ新規作成
            // Create namespace object if absent
            if (extensions[namespaceKey] == null)
                extensions[namespaceKey] = new JObject();

            var ns = (JObject)extensions[namespaceKey];

            // 指定キーのみ更新
            // Update only the specified key
            ns[key] = value;

            UpdateRawJson();
        }

        /// <summary>
        /// 指定した名前空間・キーの extensions 値を削除する。
        /// Removes the extensions value for the specified namespace and key.
        ///
        /// 指定キー以外のデータは一切変更しない。
        /// All other data remains untouched.
        /// 名前空間オブジェクトが空になっても、名前空間オブジェクト自体は残す。
        /// The namespace object is retained even if it becomes empty.
        /// 名前空間ごと削除したい場合は RemoveExtensionNamespace() を使用する。
        /// Use RemoveExtensionNamespace() to remove the entire namespace.
        /// </summary>
        /// <param name="namespaceKey">名前空間キー / Namespace key</param>
        /// <param name="key">データキー / Data key</param>
        public void RemoveExtensionValue(string namespaceKey, string key)
        {
            if (string.IsNullOrEmpty(namespaceKey) || string.IsNullOrEmpty(key))
                return;

            var ns = ParsedData["data"]?["extensions"]?[namespaceKey] as JObject;
            if (ns == null) return;

            if (ns[key] != null)
            {
                ns.Remove(key);
                UpdateRawJson();
            }
        }

        /// <summary>
        /// 指定した名前空間・キーの extensions 値が存在するかを確認する。
        /// Checks whether the extensions value for the specified namespace and key exists.
        /// </summary>
        /// <param name="namespaceKey">名前空間キー / Namespace key</param>
        /// <param name="key">データキー / Data key</param>
        public bool HasExtensionValue(string namespaceKey, string key)
        {
            if (string.IsNullOrEmpty(namespaceKey) || string.IsNullOrEmpty(key))
                return false;

            return ParsedData["data"]?["extensions"]?[namespaceKey]?[key] != null;
        }

        /// <summary>
        /// 指定した名前空間内のキー数を取得する。
        /// Gets the number of keys within the specified namespace.
        ///
        /// 名前空間が存在しない場合は 0 を返す。
        /// Returns 0 if the namespace does not exist.
        ///
        /// 用途: 名前空間内のすべてのキーを削除した後、
        ///       名前空間自体を削除すべきかどうかの判定に使用する。
        /// Usage: Used to determine whether the namespace itself should be removed
        ///        after all its keys have been deleted.
        /// </summary>
        /// <param name="namespaceKey">名前空間キー / Namespace key</param>
        /// <returns>名前空間内のキー数。名前空間が存在しない場合は 0。
        /// Number of keys in the namespace, or 0 if the namespace does not exist.</returns>
        public int GetExtensionNamespaceKeyCount(string namespaceKey)
        {
            if (string.IsNullOrEmpty(namespaceKey))
                return 0;

            var ns = ParsedData["data"]?["extensions"]?[namespaceKey] as JObject;
            return ns?.Count ?? 0;
        }

        /// <summary>
        /// 指定した名前空間全体を extensions から削除する。
        /// Removes the entire specified namespace from extensions.
        ///
        /// 名前空間が存在しない場合は何もしない。
        /// Does nothing if the namespace does not exist.
        /// 他の名前空間のデータは一切変更しない。
        /// All other namespace data remains untouched.
        ///
        /// 用途: 名前空間内の有効データがすべてなくなった場合に、
        ///       空の名前空間オブジェクトを残さないためのクリーンアップに使用する。
        /// Usage: Used for cleanup to avoid leaving an empty namespace object
        ///        when all valid data within the namespace has been removed.
        /// </summary>
        /// <param name="namespaceKey">削除する名前空間キー / Namespace key to remove</param>
        public void RemoveExtensionNamespace(string namespaceKey)
        {
            if (string.IsNullOrEmpty(namespaceKey))
                return;

            var extensions = ParsedData["data"]?["extensions"] as JObject;
            if (extensions == null) return;

            if (extensions[namespaceKey] != null)
            {
                extensions.Remove(namespaceKey);
                UpdateRawJson();
            }
        }

        // === 変更の永続化 ===

        /// <summary>
        /// ParsedDataの変更をRawJsonに反映する
        /// Reflects ParsedData changes to RawJson
        /// </summary>
        protected void UpdateRawJson()
        {
            RawJson = ParsedData.ToString(Formatting.None);
        }

        // === ファクトリメソッド ===

        /// <summary>
        /// JSON文字列からCharacterCardインスタンスを生成
        /// Creates a CharacterCard instance from JSON string
        /// </summary>
        public static CharacterCard FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("JSON cannot be null or empty", nameof(json));

            try
            {
                var parsed = JObject.Parse(json);
                var spec = parsed["spec"]?.ToString();

                if (spec == "chara_card_v2")
                    return new CharacterCardV2(json);
                else if (spec == "chara_card_v3")
                    return new CharacterCardV3(json);
                else
                    throw new NotSupportedException($"Unsupported character card spec: {spec ?? "null"}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse character card JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 新しいCharacterCardを作成（デフォルトはV2）
        /// Creates a new CharacterCard (default is V2)
        /// </summary>
        public static CharacterCard CreateNew(string version = "v2")
        {
            string template = version.ToLower() == "v3" ? GetV3Template() : GetV2Template();
            return FromJson(template);
        }

        /// <summary>
        /// V2用の空テンプレートを取得
        /// Gets empty template for V2
        /// </summary>
        private static string GetV2Template()
        {
            return @"{
  ""spec"": ""chara_card_v2"",
  ""spec_version"": ""2.0"",
  ""data"": {
    ""name"": """",
    ""description"": """",
    ""personality"": """",
    ""scenario"": """",
    ""first_mes"": """",
    ""mes_example"": """",
    ""creator_notes"": """",
    ""system_prompt"": """",
    ""post_history_instructions"": """",
    ""alternate_greetings"": [],
    ""tags"": [],
    ""creator"": """",
    ""character_version"": ""1.0"",
    ""extensions"": {}
  }
}";
        }

        /// <summary>
        /// V3用の空テンプレートを取得
        /// Gets empty template for V3
        /// </summary>
        private static string GetV3Template()
        {
            return @"{
  ""spec"": ""chara_card_v3"",
  ""spec_version"": ""3.0"",
  ""data"": {
    ""name"": """",
    ""description"": """",
    ""personality"": """",
    ""scenario"": """",
    ""first_mes"": """",
    ""mes_example"": """",
    ""creator_notes"": """",
    ""system_prompt"": """",
    ""post_history_instructions"": """",
    ""alternate_greetings"": [],
    ""tags"": [],
    ""creator"": """",
    ""character_version"": ""1.0"",
    ""extensions"": {},
    ""nickname"": """",
    ""group_only_greetings"": []
  }
}";
        }

        /// <summary>
        /// ディープコピーを作成
        /// Creates a deep copy
        /// </summary>
        public virtual CharacterCard Clone()
        {
            return FromJson(RawJson);
        }

        /// <summary>
        /// デフォルト値かどうかを判定
        /// Determines if this is a default value
        /// </summary>
        public virtual bool IsDefault()
        {
            return string.IsNullOrEmpty(GetDescription()) &&
                   string.IsNullOrEmpty(GetPersonality()) &&
                   string.IsNullOrEmpty(GetMessageExample());
        }

        public override string ToString()
        {
            return $"{GetSpec()} v{GetSpecVersion()}: {GetName()}";
        }
    }

    /// <summary>
    /// Character Card V2実装
    /// Character Card V2 implementation
    /// </summary>
    [Serializable]
    public class CharacterCardV2 : CharacterCard
    {
        public CharacterCardV2(string rawJson) : base(rawJson) { }

        public override string GetSpec() => "chara_card_v2";

        public override string GetSpecVersion()
            => ParsedData["spec_version"]?.ToString() ?? "2.0";

        public override string GetName()
            => ParsedData["data"]?["name"]?.ToString() ?? "";

        public override void SetName(string name)
        {
            ParsedData["data"]["name"] = name ?? "";
            UpdateRawJson();
        }

        public override string GetDescription()
            => ParsedData["data"]?["description"]?.ToString() ?? "";

        public override void SetDescription(string description)
        {
            ParsedData["data"]["description"] = description ?? "";
            UpdateRawJson();
        }

        public override string GetPersonality()
            => ParsedData["data"]?["personality"]?.ToString() ?? "";

        public override void SetPersonality(string personality)
        {
            ParsedData["data"]["personality"] = personality ?? "";
            UpdateRawJson();
        }

        public override string GetMessageExample()
            => ParsedData["data"]?["mes_example"]?.ToString() ?? "";

        public override void SetMessageExample(string mesExample)
        {
            ParsedData["data"]["mes_example"] = mesExample ?? "";
            UpdateRawJson();
        }

        public override string GetFirstMessage()
            => ParsedData["data"]?["first_mes"]?.ToString() ?? "";

        public override void SetFirstMessage(string firstMes)
        {
            ParsedData["data"]["first_mes"] = firstMes ?? "";
            UpdateRawJson();
        }

        public override string GetScenario()
            => ParsedData["data"]?["scenario"]?.ToString() ?? "";

        public override void SetScenario(string scenario)
        {
            ParsedData["data"]["scenario"] = scenario ?? "";
            UpdateRawJson();
        }

        // === V2固有フィールド ===

        public string GetCreatorNotes()
            => ParsedData["data"]?["creator_notes"]?.ToString() ?? "";

        public void SetCreatorNotes(string notes)
        {
            ParsedData["data"]["creator_notes"] = notes ?? "";
            UpdateRawJson();
        }

        public string GetSystemPrompt()
            => ParsedData["data"]?["system_prompt"]?.ToString() ?? "";

        public void SetSystemPrompt(string systemPrompt)
        {
            ParsedData["data"]["system_prompt"] = systemPrompt ?? "";
            UpdateRawJson();
        }

        public string GetPostHistoryInstructions()
            => ParsedData["data"]?["post_history_instructions"]?.ToString() ?? "";

        public void SetPostHistoryInstructions(string instructions)
        {
            ParsedData["data"]["post_history_instructions"] = instructions ?? "";
            UpdateRawJson();
        }

        public string GetCreator()
            => ParsedData["data"]?["creator"]?.ToString() ?? "";

        public void SetCreator(string creator)
        {
            ParsedData["data"]["creator"] = creator ?? "";
            UpdateRawJson();
        }

        public string GetCharacterVersion()
            => ParsedData["data"]?["character_version"]?.ToString() ?? "";

        public void SetCharacterVersion(string version)
        {
            ParsedData["data"]["character_version"] = version ?? "";
            UpdateRawJson();
        }
    }

    /// <summary>
    /// Character Card V3実装
    /// Character Card V3 implementation
    /// </summary>
    [Serializable]
    public class CharacterCardV3 : CharacterCard
    {
        public CharacterCardV3(string rawJson) : base(rawJson) { }

        public override string GetSpec() => "chara_card_v3";

        public override string GetSpecVersion()
            => ParsedData["spec_version"]?.ToString() ?? "3.0";

        public override string GetName()
            => ParsedData["data"]?["name"]?.ToString() ?? "";

        public override void SetName(string name)
        {
            ParsedData["data"]["name"] = name ?? "";
            UpdateRawJson();
        }

        public override string GetDescription()
            => ParsedData["data"]?["description"]?.ToString() ?? "";

        public override void SetDescription(string description)
        {
            ParsedData["data"]["description"] = description ?? "";
            UpdateRawJson();
        }

        public override string GetPersonality()
            => ParsedData["data"]?["personality"]?.ToString() ?? "";

        public override void SetPersonality(string personality)
        {
            ParsedData["data"]["personality"] = personality ?? "";
            UpdateRawJson();
        }

        public override string GetMessageExample()
            => ParsedData["data"]?["mes_example"]?.ToString() ?? "";

        public override void SetMessageExample(string mesExample)
        {
            ParsedData["data"]["mes_example"] = mesExample ?? "";
            UpdateRawJson();
        }

        public override string GetFirstMessage()
            => ParsedData["data"]?["first_mes"]?.ToString() ?? "";

        public override void SetFirstMessage(string firstMes)
        {
            ParsedData["data"]["first_mes"] = firstMes ?? "";
            UpdateRawJson();
        }

        public override string GetScenario()
            => ParsedData["data"]?["scenario"]?.ToString() ?? "";

        public override void SetScenario(string scenario)
        {
            ParsedData["data"]["scenario"] = scenario ?? "";
            UpdateRawJson();
        }

        // === V3固有フィールド ===

        /// <summary>
        /// nicknameフィールドを取得（空の場合はnameを返す）
        /// Gets nickname field (returns name if empty)
        /// </summary>
        public string GetNickname()
        {
            var nickname = ParsedData["data"]?["nickname"]?.ToString();
            return string.IsNullOrEmpty(nickname) ? GetName() : nickname;
        }

        public void SetNickname(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                ParsedData["data"]["nickname"] = null;
            else
                ParsedData["data"]["nickname"] = nickname;
            UpdateRawJson();
        }

        /// <summary>
        /// creator_notesを取得（creator_notes_multilingualも考慮）
        /// Gets creator_notes (considering creator_notes_multilingual)
        /// </summary>
        public string GetCreatorNotes(string language = "en")
        {
            var multilingual = ParsedData["data"]?["creator_notes_multilingual"] as JObject;
            if (multilingual != null && multilingual[language] != null)
                return multilingual[language].ToString();

            return ParsedData["data"]?["creator_notes"]?.ToString() ?? "";
        }

        public void SetCreatorNotes(string notes)
        {
            ParsedData["data"]["creator_notes"] = notes ?? "";
            UpdateRawJson();
        }

        public string GetSystemPrompt()
            => ParsedData["data"]?["system_prompt"]?.ToString() ?? "";

        public void SetSystemPrompt(string systemPrompt)
        {
            ParsedData["data"]["system_prompt"] = systemPrompt ?? "";
            UpdateRawJson();
        }

        public string GetPostHistoryInstructions()
            => ParsedData["data"]?["post_history_instructions"]?.ToString() ?? "";

        public void SetPostHistoryInstructions(string instructions)
        {
            ParsedData["data"]["post_history_instructions"] = instructions ?? "";
            UpdateRawJson();
        }

        public string GetCreator()
            => ParsedData["data"]?["creator"]?.ToString() ?? "";

        public void SetCreator(string creator)
        {
            ParsedData["data"]["creator"] = creator ?? "";
            UpdateRawJson();
        }

        public string GetCharacterVersion()
            => ParsedData["data"]?["character_version"]?.ToString() ?? "";

        public void SetCharacterVersion(string version)
        {
            ParsedData["data"]["character_version"] = version ?? "";
            UpdateRawJson();
        }
    }
}
