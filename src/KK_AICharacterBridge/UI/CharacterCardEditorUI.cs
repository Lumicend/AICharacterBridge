using ActionGame;
using AICharacterBridge.Core.Data;
using AICharacterBridge.Data;
using AICharacterBridge;
using KKAPI.Utilities;
using Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AICharacterBridge.UI
{
    /// <summary>
    /// WorldSetting、Player、HeroineのCharacterCardデータを編集するためのUI
    /// UI for editing WorldSetting and CharacterCard data for Player and Heroines
    /// </summary>
    public sealed class CharacterCardEditorUI : ImguiWindow<CharacterCardEditorUI>
    {
        // ウィンドウの構造定数
        // Window layout constants
        private const float SELECTOR_PANEL_WIDTH = 200f;
        private const float FIXED_SELECTOR_HEIGHT = 60f;
        private const float FILE_IO_PANEL_HEIGHT = 30f;
        private const float ACTION_BUTTON_PANEL_HEIGHT = 30f;
        private const float TAB_HEIGHT = 25f;

        // Coordinateプリセット名（ボタン表示用）
        // Coordinate preset names for button labels
        private static readonly string[] PRESET_BUTTON_LABELS = {
            "0: School",
            "1: School (After School)",
            "2: Gym Clothes",
            "3: Swimsuit",
            "4: Club Activity",
            "5: Casual Wear",
            "6: Sleepwear"
        };

        // Coordinateプリセット名（ラベル表示用・フルネーム）
        // Coordinate preset names for label display (full names)
        private static readonly string[] PRESET_FULL_NAMES = {
            "School",
            "School (After School)",
            "Gym Clothes",
            "Swimsuit",
            "Club Activity",
            "Casual Wear",
            "Sleepwear"
        };

        // スクロール位置
        // Scroll positions
        private Vector2 _heroineScrollPosition = Vector2.zero;
        private Vector2 _descriptionTextScroll = Vector2.zero;
        private Vector2 _nameTextScroll = Vector2.zero;
        private Vector2 _personalityTextScroll = Vector2.zero;
        private Vector2 _coordinateTextScroll = Vector2.zero;

        // データ選択状態
        // Data type selection state:
        //   World     : WorldSetting を編集
        //   Character : CharacterCard を編集（_selectedHeroine == null の場合は Player、非 null の場合は Heroine）
        private enum DataType { World, Character }
        private DataType _selectedDataType = DataType.World;

        /// <summary>
        /// 現在選択中のヒロイン。
        /// null の場合は Player の CharacterCard を対象とする。
        /// Currently selected heroine.
        /// When null, targets the Player's CharacterCard.
        /// </summary>
        private SaveData.Heroine _selectedHeroine = null;

        // 優先表示ヒロインリスト
        // Priority heroine display list
        private List<SaveData.Heroine> _priorityHeroines = null;
        private List<SaveData.Heroine> _displayedHeroines = null;

        // タブ選択状態（順番: Description, Name, Personality, Coordinate）
        // Tab selection state (order: Description, Name, Personality, Coordinate)
        private enum EditorTab { Description, Name, Personality, Coordinate }
        private EditorTab _currentTab = EditorTab.Description;
        private readonly string[] _tabNames = { "Description", "Name", "Personality", "Coordinate" };

        // 編集中のデータ（重要: データ欠落を防ぐため、CharacterCardインスタンスを直接保持）
        // Editing data (important: hold CharacterCard instance directly to prevent data loss)
        private CharacterCard _editingCard = null;
        private WorldSetting _editingWorldSetting = null;

        // UI表示用の一時データ（CharacterCard 基本フィールド）
        // Temporary UI display data (CharacterCard basic fields)
        private string _nameText = "";
        private string _descriptionText = "";
        private string _personalityText = "";

        // Coordinateタブ用の状態
        // State for Coordinate tab
        private int _selectedPresetIndex = 0;

        private enum CoordinateField { Clothes, Appearance }
        private CoordinateField _selectedCoordinateField = CoordinateField.Clothes;

        /// <summary>
        /// 編集中のCoordinateデータ。
        /// CharacterCardのextensionsから読み込み、Applyで書き戻す。
        /// Editing coordinate data.
        /// Loaded from CharacterCard extensions and written back on Apply.
        /// </summary>
        private CoordinateData _editingCoordinateData = null;

        /// <summary>
        /// 現在表示中のCoordinateテキスト（プリセット × フィールドの1組）。
        /// Currently displayed coordinate text (one combination of preset × field).
        /// </summary>
        private string _coordinateText = "";

        // データ変更フラグ
        // Data dirty flag
        private bool _isDirty = false;

        // GUIスタイル
        // GUI styles
        private GUIStyle _selectedButtonStyle;
        private GUIStyle _normalButtonStyle;
        private GUIStyle _tabButtonStyle;
        private GUIStyle _selectedTabButtonStyle;

        // =====================================================================
        // Static methods
        // =====================================================================

        /// <summary>
        /// UIを有効化（優先表示ヒロインなし）
        /// Enables the UI without a priority heroine.
        /// </summary>
        public static void Enable()
        {
            Enable(null);
        }

        /// <summary>
        /// UIを有効化（優先表示ヒロイン指定可能）
        /// Enables the UI with an optional priority heroine.
        /// </summary>
        public static void Enable(List<SaveData.Heroine> priorityHeroines)
        {
            // メインゲーム中かチェック
            // Check if in main game
            var cycle = Singleton<Cycle>.Instance;
            if (cycle == null)
            {
                AICharacterBridgePlugin.Instance?.Logger.LogWarning("[CharacterCardEditorUI] This UI can only be opened during main game.");
                return;
            }

            if (Instance == null)
            {
                var go = new GameObject("CharacterCardEditorUI");
                DontDestroyOnLoad(go);
                go.AddComponent<CharacterCardEditorUI>();
            }

            Instance._priorityHeroines = priorityHeroines;
            Instance._displayedHeroines = null; // リセット / Reset
            Instance.enabled = true;

            // 優先ヒロインがいる場合は自動選択、いない場合はWorldを選択
            // Auto-select priority heroine if present, otherwise select World
            if (priorityHeroines != null && priorityHeroines.Count > 0)
            {
                Instance.SelectHeroine(priorityHeroines[0]);
            }
            else
            {
                Instance.SelectWorld();
            }
        }

        /// <summary>
        /// UIを無効化
        /// Disables the UI.
        /// </summary>
        public static void Disable()
        {
            if (Instance != null)
            {
                Instance.enabled = false;
                Instance._priorityHeroines = null;
                Instance._displayedHeroines = null;
                Instance._editingCard = null;
                Instance._editingWorldSetting = null;
                Instance._editingCoordinateData = null;
            }
        }

        // =====================================================================
        // ImguiWindow overrides
        // =====================================================================

        protected override Rect GetDefaultWindowRect(Rect screenRect)
        {
            const int width = 800;
            const int height = 600;
            return new Rect(
                (screenRect.width - width) / 2,
                (screenRect.height - height) / 2,
                width,
                height
            );
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Title = "Character Card & World Editor";
            MinimumSize = new Vector2(600, 400);

            // 初期データを読み込み（Worldを選択）
            // Load initial data (select World)
            SelectWorld();
        }

        protected override void DrawContents()
        {
            Title = _isDirty ? "Character Card & World Editor *" : "Character Card & World Editor";

            InitializeStyles();

            GUILayout.BeginHorizontal();
            {
                // 左側: データ選択パネル
                // Left: data selector panel
                DrawSelectorPanel();

                // 右側: エディタパネル
                // Right: editor panel
                DrawEditorPanel();
            }
            GUILayout.EndHorizontal();
        }

        // =====================================================================
        // Style initialization
        // =====================================================================

        private void InitializeStyles()
        {
            if (_normalButtonStyle == null)
            {
                _normalButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 5, 5)
                };

                _selectedButtonStyle = new GUIStyle(_normalButtonStyle)
                {
                    normal = { textColor = Color.cyan },
                    hover = { textColor = Color.cyan }
                };

                _tabButtonStyle = new GUIStyle(GUI.skin.button);

                _selectedTabButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.yellow }
                };
            }
        }

        // =====================================================================
        // Selector panel
        // =====================================================================

        /// <summary>
        /// 左側のデータ選択パネルを描画
        /// Draws the left-side data selector panel.
        /// </summary>
        private void DrawSelectorPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(SELECTOR_PANEL_WIDTH));
            {
                // 固定セレクタパネル（World, Player）
                // Fixed selector panel (World, Player)
                DrawFixedSelectorPanel();

                // ヒロインリストパネル
                // Heroine list panel
                DrawHeroineScrollPanel();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// WorldとPlayerのボタンを描画
        /// Draws World and Player selector buttons.
        /// </summary>
        private void DrawFixedSelectorPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(FIXED_SELECTOR_HEIGHT));
            {
                // Worldボタン
                // World button
                var worldStyle = _selectedDataType == DataType.World
                    ? _selectedButtonStyle : _normalButtonStyle;
                if (GUILayout.Button("World", worldStyle, GUILayout.Height(25)))
                {
                    SelectWorld();
                }

                // Playerボタン（DataType.Character かつ _selectedHeroine == null のとき選択中）
                // Player button (selected when DataType.Character and _selectedHeroine == null)
                var playerStyle = _selectedDataType == DataType.Character && _selectedHeroine == null
                    ? _selectedButtonStyle : _normalButtonStyle;
                if (GUILayout.Button("Player", playerStyle, GUILayout.Height(25)))
                {
                    SelectPlayer();
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// ヒロインリストをスクロール可能な形で描画
        /// Draws the heroine list in a scrollable area.
        /// </summary>
        private void DrawHeroineScrollPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
            {
                _heroineScrollPosition = GUILayout.BeginScrollView(_heroineScrollPosition, true, true);
                {
                    var heroines = GetOrderedHeroineList();

                    if (heroines.Count == 0)
                    {
                        GUILayout.Label("No heroines available.");
                    }
                    else
                    {
                        // 元のHeroineListを取得（インデックス番号取得用）
                        // Get the original HeroineList to resolve display indices
                        var game = Singleton<Game>.Instance;
                        var originalList = game?.HeroineList;

                        if (originalList == null)
                        {
                            GUILayout.Label("Error: Cannot get heroine list.");
                        }
                        else
                        {
                            // 桁数を計算（元のリストの総数に基づく）
                            // Determine digit count based on total original list count
                            int digitCount = originalList.Count.ToString().Length;
                            string formatString = $"D{digitCount}";

                            for (int i = 0; i < heroines.Count; i++)
                            {
                                var heroine = heroines[i];
                                var heroineName = heroine?.charFile?.parameter?.fullname ?? "Unknown";

                                // 元のリストでのインデックスを取得
                                // Get index in original list
                                int originalIndex = originalList.IndexOf(heroine);

                                // DataType.Character かつ対象ヒロインが一致するとき選択中
                                // Selected when DataType.Character and the heroine matches
                                var isSelected = _selectedDataType == DataType.Character && _selectedHeroine == heroine;
                                var style = isSelected ? _selectedButtonStyle : _normalButtonStyle;

                                if (GUILayout.Button($"#{originalIndex.ToString(formatString)} - {heroineName}", style, GUILayout.Height(25)))
                                {
                                    SelectHeroine(heroine);
                                }
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        // =====================================================================
        // Editor panel
        // =====================================================================

        /// <summary>
        /// 右側のエディタパネルを描画
        /// Draws the right-side editor panel.
        /// </summary>
        private void DrawEditorPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
            {
                // ファイルI/Oパネル
                // File I/O panel
                DrawFileIOPanel();

                // コンテンツエディタパネル
                // Content editor panel
                DrawContentEditorPanel();

                // アクションボタンパネル
                // Action button panel
                DrawActionButtonPanel();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// ファイルI/Oパネルを描画
        /// Draws the file I/O panel.
        /// </summary>
        private void DrawFileIOPanel()
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(FILE_IO_PANEL_HEIGHT));
            {
                if (GUILayout.Button("Load JSON", GUILayout.Width(120)))
                {
                    LoadFromJson();
                }

                if (GUILayout.Button("Save JSON", GUILayout.Width(120)))
                {
                    SaveToJson();
                }

                GUILayout.FlexibleSpace();

                // 現在の選択対象を表示
                // Display current selection target name
                string targetName = GetCurrentTargetName();
                GUILayout.Label($"Editing: {targetName}", GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// コンテンツエディタパネルを描画（タブ付き）
        /// Draws the content editor panel with tab bar.
        /// </summary>
        private void DrawContentEditorPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
            {
                // タブバー
                // Tab bar
                DrawTabBar();

                // タブの内容
                // Tab content
                DrawTabContent();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// タブバーを描画
        /// Draws the tab bar.
        /// </summary>
        private void DrawTabBar()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(TAB_HEIGHT));
            {
                if (_selectedDataType == DataType.World)
                {
                    // WorldSettingの場合はDescriptionタブのみ表示
                    // World only shows the Description tab
                    var style = _currentTab == EditorTab.Description ? _selectedTabButtonStyle : _tabButtonStyle;
                    if (GUILayout.Button("Description", style))
                    {
                        _currentTab = EditorTab.Description;
                    }
                }
                else
                {
                    // CharacterCard用の全タブを表示（Description / Name / Personality / Coordinate）
                    // Show all tabs for CharacterCard (Description / Name / Personality / Coordinate)
                    for (int i = 0; i < _tabNames.Length; i++)
                    {
                        var tab = (EditorTab)i;
                        var style = _currentTab == tab ? _selectedTabButtonStyle : _tabButtonStyle;

                        if (GUILayout.Button(_tabNames[i], style))
                        {
                            _currentTab = tab;
                        }
                    }
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 現在選択中のタブの内容を描画
        /// Draws the content area for the currently selected tab.
        /// </summary>
        private void DrawTabContent()
        {
            GUI.changed = false;

            switch (_currentTab)
            {
                case EditorTab.Description:
                    if (_selectedDataType == DataType.World)
                        DrawTextEditor("World Description:", ref _descriptionText, ref _descriptionTextScroll, multiline: true);
                    else
                        DrawTextEditor("Description:", ref _descriptionText, ref _descriptionTextScroll, multiline: true);
                    break;

                case EditorTab.Name:
                    DrawTextEditor("Name:", ref _nameText, ref _nameTextScroll, multiline: false);
                    break;

                case EditorTab.Personality:
                    DrawTextEditor("Personality:", ref _personalityText, ref _personalityTextScroll, multiline: true);
                    break;

                case EditorTab.Coordinate:
                    DrawCoordinateTab();
                    return; // _isDirty の設定を DrawCoordinateTab に委譲するため、ここで return する
                            // Return here to delegate _isDirty assignment to DrawCoordinateTab
            }

            if (GUI.changed)
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// テキストエディタを描画
        /// Draws a text editor with label and scrollable text area.
        /// </summary>
        private void DrawTextEditor(string label, ref string text, ref Vector2 scrollPosition, bool multiline)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(label);

                // 水平・垂直スクロールバー付きテキストエディタ
                // Text editor with horizontal and vertical scrollbars
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, multiline, GUILayout.ExpandHeight(true));
                {
                    if (multiline)
                    {
                        // 複数行テキストエリア（水平・垂直スクロール）
                        // Multi-line text area (horizontal and vertical scroll)
                        var style = new GUIStyle(GUI.skin.textArea) { wordWrap = false };
                        text = GUILayout.TextArea(text, style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        // 1行テキストフィールド（水平スクロールのみ）
                        // Single-line text field (horizontal scroll only)
                        text = GUILayout.TextField(text, GUILayout.Height(25), GUILayout.ExpandWidth(true));
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        // =====================================================================
        // Coordinate tab drawing
        // =====================================================================

        /// <summary>
        /// Coordinateタブの内容を描画する。
        /// Draws the Coordinate tab content.
        ///
        /// レイアウト:
        ///   [行1] プリセット選択ボタン 0〜3
        ///   [行2] プリセット選択ボタン 4〜6
        ///   [行3] フィールド選択ボタン [Clothes] [Appearance]
        ///         + 現在の選択を示すラベル
        ///   [テキストエリア] 選択中プリセット × フィールドの説明文
        ///
        /// Layout:
        ///   [Row 1] Preset selector buttons 0-3
        ///   [Row 2] Preset selector buttons 4-6
        ///   [Row 3] Field selector buttons [Clothes] [Appearance]
        ///           + Label showing current selection
        ///   [Text area] Description for selected preset × field
        /// </summary>
        private void DrawCoordinateTab()
        {
            if (_editingCoordinateData == null)
                _editingCoordinateData = new CoordinateData();

            GUILayout.BeginVertical();
            {
                // ---- プリセット選択ボタン（2行に分割: 0〜3 / 4〜6）
                // ---- Preset selector buttons (split into 2 rows: 0-3 / 4-6)
                DrawPresetSelectorRow(0, 4);
                DrawPresetSelectorRow(4, CoordinateData.PresetCount);

                GUILayout.Space(4f);

                // ---- フィールド選択ボタン + 現在の選択ラベル
                // ---- Field selector buttons + current selection label
                GUILayout.BeginHorizontal();
                {
                    DrawCoordinateFieldButton(CoordinateField.Clothes, "Clothes");
                    DrawCoordinateFieldButton(CoordinateField.Appearance, "Appearance");

                    GUILayout.Space(10f);

                    // 現在の選択: "5: Casual Wear / Clothes" のような形式
                    // Current selection label in the form "5: Casual Wear / Clothes"
                    string presetFullName = PRESET_FULL_NAMES[_selectedPresetIndex];
                    string fieldName = _selectedCoordinateField == CoordinateField.Clothes ? "Clothes" : "Appearance";
                    GUILayout.Label($"{_selectedPresetIndex}: {presetFullName} / {fieldName}", GUILayout.ExpandWidth(false));

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(4f);

                // ---- テキストエリア ----
                // ---- Text area ----

                GUI.changed = false;

                _coordinateTextScroll = GUILayout.BeginScrollView(_coordinateTextScroll, true, true, GUILayout.ExpandHeight(true));
                {
                    var style = new GUIStyle(GUI.skin.textArea) { wordWrap = false };
                    _coordinateText = GUILayout.TextArea(_coordinateText, style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            // テキストエリアへの実際の入力があった場合のみ、データ更新と _isDirty のセットを行う。
            // Update data and set _isDirty only when actual text input occurred in the text area.
            if (GUI.changed)
            {
                WriteCoordinateTextToData();
                _isDirty = true;
            }
        }

        /// <summary>
        /// プリセット選択ボタンを1行描画する（startIndex以上endIndex未満）。
        /// Draws one row of preset selector buttons (from startIndex to endIndex exclusive).
        /// </summary>
        private void DrawPresetSelectorRow(int startIndex, int endIndex)
        {
            GUILayout.BeginHorizontal();
            {
                for (int i = startIndex; i < endIndex && i < CoordinateData.PresetCount; i++)
                {
                    var style = _selectedPresetIndex == i ? _selectedTabButtonStyle : _tabButtonStyle;
                    if (GUILayout.Button(PRESET_BUTTON_LABELS[i], style))
                    {
                        // 切り替え前に現在のテキストを保存する
                        // Save current text before switching preset
                        WriteCoordinateTextToData();
                        _selectedPresetIndex = i;
                        ReadCoordinateTextFromData();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Coordinateフィールド（Clothes / Appearance）の切り替えボタンを描画する。
        /// Draws a toggle button for a coordinate field (Clothes or Appearance).
        /// </summary>
        private void DrawCoordinateFieldButton(CoordinateField field, string label)
        {
            var style = _selectedCoordinateField == field ? _selectedTabButtonStyle : _tabButtonStyle;
            if (GUILayout.Button(label, style, GUILayout.Width(100)))
            {
                if (_selectedCoordinateField != field)
                {
                    // 切り替え前に現在のテキストを保存する
                    // Save current text before switching field
                    WriteCoordinateTextToData();
                    _selectedCoordinateField = field;
                    ReadCoordinateTextFromData();
                }
            }
        }

        /// <summary>
        /// _coordinateText を _editingCoordinateData の現在選択位置に書き込む。
        /// Writes _coordinateText into _editingCoordinateData at the current selection.
        /// </summary>
        private void WriteCoordinateTextToData()
        {
            if (_editingCoordinateData == null) return;

            if (_selectedCoordinateField == CoordinateField.Clothes)
                _editingCoordinateData.SetClothes(_selectedPresetIndex, _coordinateText ?? "");
            else
                _editingCoordinateData.SetAppearance(_selectedPresetIndex, _coordinateText ?? "");
        }

        /// <summary>
        /// _editingCoordinateData の現在選択位置から _coordinateText を読み込む。
        /// Reads _coordinateText from _editingCoordinateData at the current selection.
        /// </summary>
        private void ReadCoordinateTextFromData()
        {
            if (_editingCoordinateData == null)
            {
                _coordinateText = "";
                return;
            }

            if (_selectedCoordinateField == CoordinateField.Clothes)
                _coordinateText = _editingCoordinateData.GetClothes(_selectedPresetIndex);
            else
                _coordinateText = _editingCoordinateData.GetAppearance(_selectedPresetIndex);
        }

        // =====================================================================
        // Action button panel
        // =====================================================================

        /// <summary>
        /// アクションボタンパネルを描画
        /// Draws the action button panel.
        /// </summary>
        private void DrawActionButtonPanel()
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(ACTION_BUTTON_PANEL_HEIGHT));
            {
                if (GUILayout.Button("Apply", GUILayout.Width(100)))
                {
                    ApplyChanges();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear", GUILayout.Width(100)))
                {
                    ClearCurrentData();
                }
            }
            GUILayout.EndHorizontal();
        }

        // =====================================================================
        // Data selection
        // =====================================================================

        private void SelectWorld()
        {
            _selectedDataType = DataType.World;
            _selectedHeroine = null;
            _currentTab = EditorTab.Description;
            LoadWorldData();
        }

        /// <summary>
        /// PlayerのCharacterCardを選択します。
        /// _selectedHeroine を null にして DataType.Character に設定します。
        /// Selects the Player's CharacterCard.
        /// Sets _selectedHeroine to null and DataType to Character.
        /// </summary>
        private void SelectPlayer()
        {
            _selectedDataType = DataType.Character;
            _selectedHeroine = null;
            _currentTab = EditorTab.Description;
            LoadCharacterCardData();
        }

        /// <summary>
        /// 指定したHeroineのCharacterCardを選択します。
        /// _selectedHeroine を設定して DataType.Character に設定します。
        /// Selects the specified Heroine's CharacterCard.
        /// Sets _selectedHeroine and DataType to Character.
        /// </summary>
        private void SelectHeroine(SaveData.Heroine heroine)
        {
            if (heroine == null)
            {
                LogWarning("Cannot select null heroine.");
                return;
            }

            _selectedDataType = DataType.Character;
            _selectedHeroine = heroine;
            _currentTab = EditorTab.Description;
            LoadCharacterCardData();
        }

        // =====================================================================
        // Data loading
        // =====================================================================

        private void LoadWorldData()
        {
            try
            {
                var saveData = GameController.CurrentSaveData;
                if (saveData == null)
                {
                    LogWarning("No save data available. Using default values.");
                    _editingWorldSetting = WorldSetting.CreateNew();
                    LoadWorldSettingToUI();
                    return;
                }

                var worldSetting = saveData.GetWorldSetting();
                _editingWorldSetting = worldSetting?.Clone() ?? WorldSetting.CreateNew();

                LoadWorldSettingToUI();
                _isDirty = false;
            }
            catch (Exception e)
            {
                LogError($"Failed to load WorldSetting: {e.Message}");
                _editingWorldSetting = WorldSetting.CreateNew();
                LoadWorldSettingToUI();
            }
        }

        /// <summary>
        /// CharacterCardを読み込みます。
        /// _selectedHeroine が null の場合は Player のカードを、非 null の場合は対象 Heroine のカードを取得します。
        /// Loads a CharacterCard.
        /// Loads the Player's card when _selectedHeroine is null, or the target Heroine's card otherwise.
        /// </summary>
        private void LoadCharacterCardData()
        {
            try
            {
                var saveData = GameController.CurrentSaveData;
                if (saveData == null)
                {
                    LogWarning("No save data available. Using default values.");
                    _editingCard = CharacterCard.CreateNew("v2");
                    LoadCharacterCardToUI();
                    return;
                }

                CharacterCard card;
                if (_selectedHeroine == null)
                {
                    card = saveData.GetPlayerCharacterCard();
                }
                else
                {
                    card = saveData.GetCharacterCardForHeroine(_selectedHeroine);
                }

                _editingCard = card?.Clone() ?? CharacterCard.CreateNew("v2");

                LoadCharacterCardToUI();
                _isDirty = false;
            }
            catch (Exception e)
            {
                LogError($"Failed to load character card: {e.Message}");
                _editingCard = CharacterCard.CreateNew("v2");
                LoadCharacterCardToUI();
            }
        }

        /// <summary>
        /// WorldSettingの内容をUIに反映します。
        /// Applies WorldSetting data to the UI fields.
        /// </summary>
        private void LoadWorldSettingToUI()
        {
            _nameText = "";
            // [変更] .Description プロパティ廃止に伴い GetDescription() を使用する
            // [Changed] Use GetDescription() since the .Description property has been removed
            _descriptionText = _editingWorldSetting?.GetDescription() ?? "";
            _personalityText = "";
            ResetScrollPositions();
        }

        /// <summary>
        /// CharacterCardの内容をUIに反映する。
        /// CoordinateDataもextensionsから読み込む。
        /// Applies CharacterCard data to the UI fields.
        /// Also loads CoordinateData from extensions.
        /// </summary>
        private void LoadCharacterCardToUI()
        {
            _nameText = _editingCard?.GetName() ?? "";
            _descriptionText = _editingCard?.GetDescription() ?? "";
            _personalityText = _editingCard?.GetPersonality() ?? "";

            // CoordinateDataをextensionsから読み込む
            // Load CoordinateData from CharacterCard extensions
            _editingCoordinateData = LoadCoordinateDataFromCard(_editingCard);

            // Coordinateタブの表示状態をリセットし、初期選択位置のテキストを読み込む
            // Reset Coordinate tab display state and load text for initial selection
            _selectedPresetIndex = 0;
            _selectedCoordinateField = CoordinateField.Clothes;
            ReadCoordinateTextFromData();

            ResetScrollPositions();
        }

        /// <summary>
        /// CharacterCardのextensionsからCoordinateDataを読み込む。
        /// Loads CoordinateData from CharacterCard extensions.
        /// </summary>
        /// <param name="card">対象のCharacterCard / Target CharacterCard</param>
        /// <returns>読み込んだCoordinateData。存在しない場合は新規インスタンス。
        /// Loaded CoordinateData, or a new instance if not found.</returns>
        private CoordinateData LoadCoordinateDataFromCard(CharacterCard card)
        {
            if (card == null)
                return new CoordinateData();

            try
            {
                var token = card.GetExtensionValue(
                    AICharacterBridgePlugin.ExtensionNamespace,
                    AICharacterBridgePlugin.CoordinateDataKey);

                if (token != null)
                {
                    var data = token.ToObject<CoordinateData>() ?? new CoordinateData();
                    data.EnsurePresetCount();
                    return data;
                }
            }
            catch (Exception e)
            {
                LogWarning($"Failed to load CoordinateData from extensions: {e.Message}");
            }

            return new CoordinateData();
        }

        // =====================================================================
        // Data saving
        // =====================================================================

        private void ApplyChanges()
        {
            try
            {
                var saveData = GameController.CurrentSaveData;
                if (saveData == null)
                {
                    LogError("No save data available. Cannot save changes.");
                    return;
                }

                switch (_selectedDataType)
                {
                    case DataType.World:
                        SaveWorldData(saveData);
                        break;

                    case DataType.Character:
                        SaveCharacterCardData(saveData);
                        break;
                }

                _isDirty = false;
                LogInfo("Changes saved successfully.");
            }
            catch (Exception e)
            {
                LogError($"Failed to save changes: {e.Message}");
            }
        }

        private void SaveWorldData(AICharacterBridgeSaveData saveData)
        {
            // UIの内容を _editingWorldSetting に反映する。
            // [変更] .Description プロパティ廃止に伴い SetDescription() を使用する。
            // Apply UI content to _editingWorldSetting.
            // [Changed] Use SetDescription() since the .Description property has been removed.
            _editingWorldSetting.SetDescription(_descriptionText ?? "");

            saveData.SetWorldSetting(_editingWorldSetting);
        }

        /// <summary>
        /// CharacterCardの編集内容をセーブデータに書き込みます。
        /// _selectedHeroine が null の場合は Player のカードとして、非 null の場合は対象 Heroine のカードとして保存します。
        /// Writes CharacterCard edits to save data.
        /// Saves as Player's card when _selectedHeroine is null, or as the target Heroine's card otherwise.
        /// </summary>
        private void SaveCharacterCardData(AICharacterBridgeSaveData saveData)
        {
            // UIの基本フィールドを_editingCardに反映
            // Apply basic UI fields to _editingCard
            _editingCard.SetName(_nameText ?? "");
            _editingCard.SetDescription(_descriptionText ?? "");
            _editingCard.SetPersonality(_personalityText ?? "");

            // 現在表示中のCoordinateテキストを確実に保存してからextensionsに書き込む
            // Flush current coordinate text, then write CoordinateData to extensions
            WriteCoordinateTextToData();
            SaveCoordinateDataToCard(_editingCard, _editingCoordinateData);

            if (_selectedHeroine == null)
                saveData.SetPlayerCharacterCard(_editingCard);
            else
                saveData.SetCharacterCardForHeroine(_selectedHeroine, _editingCard);
        }

        /// <summary>
        /// CoordinateDataをCharacterCardのextensionsに書き込む。
        ///
        /// CoordinateDataが空（nullまたは全フィールドが空文字列）の場合は書き込みを行わず、
        /// 代わりに以下のクリーンアップ処理を行う：
        ///   1. extensions から coordinate_data キーを削除する（存在しない場合は何もしない）
        ///   2. 名前空間 ai_character_bridge_kk にキーが残っていなければ名前空間ごと削除する
        ///
        /// Writes CoordinateData into CharacterCard extensions.
        ///
        /// If CoordinateData is empty (null or all fields are empty strings),
        /// no data is written. Instead, the following cleanup is performed:
        ///   1. Removes the coordinate_data key from extensions (does nothing if absent)
        ///   2. Removes the ai_character_bridge_kk namespace if no keys remain within it
        /// </summary>
        /// <param name="card">対象のCharacterCard / Target CharacterCard</param>
        /// <param name="coordinateData">書き込むCoordinateData / CoordinateData to write</param>
        private void SaveCoordinateDataToCard(CharacterCard card, CoordinateData coordinateData)
        {
            if (card == null) return;

            try
            {
                // CoordinateDataが空（nullまたは全フィールドが空文字列）の場合はクリーンアップを行う
                // Perform cleanup when CoordinateData is empty (null or all fields are empty strings)
                if (coordinateData == null || coordinateData.IsEmpty())
                {
                    // coordinate_data キーを削除する（キーが存在しない場合は何もしない）
                    // Remove the coordinate_data key (does nothing if the key does not exist)
                    card.RemoveExtensionValue(
                        AICharacterBridgePlugin.ExtensionNamespace,
                        AICharacterBridgePlugin.CoordinateDataKey);

                    // 名前空間にキーが残っていない場合は名前空間ごと削除する
                    // Remove the namespace itself if no keys remain within it
                    if (card.GetExtensionNamespaceKeyCount(AICharacterBridgePlugin.ExtensionNamespace) == 0)
                    {
                        card.RemoveExtensionNamespace(AICharacterBridgePlugin.ExtensionNamespace);
                        LogDebug("Removed empty extension namespace: " + AICharacterBridgePlugin.ExtensionNamespace);
                    }

                    return;
                }

                // CoordinateDataが空でない場合は通常どおり書き込む
                // Write CoordinateData to extensions as usual when it is not empty
                var token = JToken.FromObject(coordinateData);
                card.SetExtensionValue(
                    AICharacterBridgePlugin.ExtensionNamespace,
                    AICharacterBridgePlugin.CoordinateDataKey,
                    token);
            }
            catch (Exception e)
            {
                LogError($"Failed to save CoordinateData to extensions: {e.Message}");
            }
        }

        // =====================================================================
        // File I/O
        // =====================================================================

        private void LoadFromJson()
        {
            try
            {
                // データ種別に応じた初期ディレクトリを使用
                // Use the output directory that matches the current data type as the initial directory
                string initialDirectory = GetOutputDirectory(_selectedDataType);

                OpenFileDialog.Show(
                    paths => OnJsonFileSelected(paths, isLoad: true),
                    "Load CharacterCard/WorldSetting File",
                    initialDirectory,
                    "JSON Files (*.json)|*.json|All files|*.*",
                    ".json"
                );
            }
            catch (Exception e)
            {
                LogError($"Failed to open file dialog: {e.Message}");
            }
        }

        private void SaveToJson()
        {
            try
            {
                // 出力ディレクトリの決定（データ種別ごとに異なるサブディレクトリ）
                // Determine output directory (differs per data type)
                string directory = GetOutputDirectory(_selectedDataType);
                Directory.CreateDirectory(directory);

                // ファイル名のベース部分を決定
                // Determine the base name for the file
                string baseName = GetOutputBaseName();

                // タイムスタンプ付きファイル名を生成: {name}_{yyyyMMddHHmmssfff}.json
                // Generate timestamped filename: {name}_{yyyyMMddHHmmssfff}.json
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string fileName = $"{baseName}_{timestamp}.json";
                string fullPath = Path.Combine(directory, fileName);

                string jsonString = "";

                switch (_selectedDataType)
                {
                    case DataType.World:
                        // WorldSetting.ToJson() を使用（spec/data 構造を含む正規の形式で出力）
                        // Use WorldSetting.ToJson() to output in canonical format including spec/data structure
                        jsonString = _editingWorldSetting.ToJson();
                        break;

                    case DataType.Character:
                        // _editingCardのRawJsonをそのまま使用（データ欠落防止）
                        // Use _editingCard's RawJson directly to prevent data loss
                        if (_editingCard != null)
                        {
                            jsonString = JObject.Parse(_editingCard.RawJson).ToString(Formatting.Indented);
                        }
                        break;
                }

                File.WriteAllText(fullPath, jsonString, Encoding.UTF8);
                LogInfo($"Saved to: {fullPath}");

                // エクスプローラーで開く
                // Open in Explorer
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{fullPath}\"");
            }
            catch (Exception e)
            {
                LogError($"Failed to save JSON file: {e.Message}");
            }
        }

        /// <summary>
        /// ファイル選択後のロード処理。
        /// ロードする前に JSON の spec フィールドを検証し、
        /// 現在の編集種別（World / Character）と一致しない場合はロードを中止します。
        ///
        /// Handles loading after a file is selected.
        /// Validates the spec field in the JSON before loading.
        /// If the spec does not match the current editing type (World / Character),
        /// the load is aborted.
        /// </summary>
        private void OnJsonFileSelected(string[] paths, bool isLoad)
        {
            if (paths == null || paths.Length == 0) return;

            try
            {
                string path = paths[0];
                string jsonString = File.ReadAllText(path, Encoding.UTF8);

                if (_selectedDataType == DataType.World)
                {
                    // WorldSetting.FromJson() が spec フィールドを検証する。
                    // "world_setting" 以外の場合は NotSupportedException をスローするため、
                    // ロードを中止してエラーをログに記録する。
                    // WorldSetting.FromJson() validates the spec field.
                    // If spec is not "world_setting", it throws NotSupportedException;
                    // in that case we abort the load and log the error.
                    WorldSetting worldSetting;
                    try
                    {
                        worldSetting = WorldSetting.FromJson(jsonString);
                    }
                    catch (NotSupportedException ex)
                    {
                        LogError($"Cannot load file: {ex.Message}");
                        return;
                    }

                    _editingWorldSetting = worldSetting;
                    LoadWorldSettingToUI();
                    LogDebug($"Loaded WorldSetting: Description length = {_descriptionText.Length}");
                }
                else
                {
                    // CharacterCard.FromJson() が spec フィールドを検証する。
                    // "chara_card_v2" / "chara_card_v3" 以外の場合は NotSupportedException をスローするため、
                    // ロードを中止してエラーをログに記録する。
                    // CharacterCard.FromJson() validates the spec field.
                    // If spec is not "chara_card_v2" or "chara_card_v3", it throws NotSupportedException;
                    // in that case we abort the load and log the error.
                    CharacterCard card;
                    try
                    {
                        card = CharacterCard.FromJson(jsonString);
                    }
                    catch (NotSupportedException ex)
                    {
                        LogError($"Cannot load file: {ex.Message}");
                        return;
                    }

                    _editingCard = card;
                    LoadCharacterCardToUI();
                    LogDebug($"Loaded CharacterCard: Name={_nameText}, Version={_editingCard.GetSpec()} {_editingCard.GetSpecVersion()}");
                }

                _isDirty = true;
                LogInfo($"Loaded from: {Path.GetFileName(path)}");
            }
            catch (Exception e)
            {
                LogError($"Failed to load JSON file: {e.Message}");
                LogError($"Stack trace: {e.StackTrace}");
            }
        }

        // =====================================================================
        // Data reset
        // =====================================================================

        private void ClearCurrentData()
        {
            if (_selectedDataType == DataType.World)
            {
                _editingWorldSetting = WorldSetting.CreateNew();
                LoadWorldSettingToUI();
            }
            else
            {
                _editingCard = CharacterCard.CreateNew("v2");
                LoadCharacterCardToUI();
            }

            _isDirty = true;
        }

        private void ResetScrollPositions()
        {
            _nameTextScroll = Vector2.zero;
            _descriptionTextScroll = Vector2.zero;
            _personalityTextScroll = Vector2.zero;
            _coordinateTextScroll = Vector2.zero;
        }

        // =====================================================================
        // Utilities
        // =====================================================================

        /// <summary>
        /// 優先表示を考慮した並び替え済みヒロインリストを取得
        /// Returns a sorted heroine list considering priority display order.
        /// </summary>
        private List<SaveData.Heroine> GetOrderedHeroineList()
        {
            if (_displayedHeroines != null)
                return _displayedHeroines;

            var game = Singleton<Game>.Instance;
            if (game == null || game.HeroineList == null)
            {
                _displayedHeroines = new List<SaveData.Heroine>();
                return _displayedHeroines;
            }

            var allHeroines = game.HeroineList;

            // 優先リストがない場合は通常の順番
            // If no priority list, use default order
            if (_priorityHeroines == null || _priorityHeroines.Count == 0)
            {
                _displayedHeroines = new List<SaveData.Heroine>(allHeroines);
                return _displayedHeroines;
            }

            // 優先リスト + (全体リスト - 優先リスト)
            // Priority list + (all heroines - priority heroines)
            var result = new List<SaveData.Heroine>();

            foreach (var priority in _priorityHeroines)
            {
                if (allHeroines.Contains(priority))
                    result.Add(priority);
            }

            foreach (var heroine in allHeroines)
            {
                if (!result.Contains(heroine))
                    result.Add(heroine);
            }

            _displayedHeroines = result;
            return _displayedHeroines;
        }

        private string GetCurrentTargetName()
        {
            switch (_selectedDataType)
            {
                case DataType.World:
                    return "World";

                case DataType.Character:
                    if (_selectedHeroine == null)
                        return "Player";
                    return _selectedHeroine.charFile?.parameter?.fullname ?? "Unknown Heroine";

                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// このプラグインのDLLが存在するディレクトリを取得します。
        /// Gets the directory where this plugin's DLL resides.
        /// </summary>
        private string GetDllBaseDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// データ種別に応じた出力ディレクトリを取得します。
        /// Gets the output directory for the specified data type.
        /// CharacterCard は "CharacterCards"、WorldSetting は "WorldSettings" サブディレクトリを使用します。
        /// CharacterCard uses the "CharacterCards" subdirectory; WorldSetting uses "WorldSettings".
        /// </summary>
        /// <param name="dataType">データ種別 / Data type</param>
        private string GetOutputDirectory(DataType dataType)
        {
            string baseDir = GetDllBaseDirectory();
            string subDir = (dataType == DataType.World) ? "WorldSettings" : "CharacterCards";
            return Path.Combine(baseDir, subDir);
        }

        /// <summary>
        /// 現在の選択対象に応じた出力ファイル名のベース部分（拡張子・タイムスタンプなし）を取得します。
        /// Gets the base name (without extension or timestamp) for the output file based on the current selection.
        ///
        /// 取得元の優先順位 / Name resolution priority:
        ///   World              → "World"（固定）
        ///   Character (Player) → PlayerのCharacterCardの Name フィールド → 取得失敗時は "Player"
        ///   Character (Heroine)→ 編集中の CharacterCard の Name フィールド → 取得失敗時は "Heroine"
        /// いずれも <see cref="SanitizeFileName"/> でファイル名として無効な文字を除去します。
        /// All names are sanitized by <see cref="SanitizeFileName"/> to remove characters invalid in file names.
        /// </summary>
        private string GetOutputBaseName()
        {
            switch (_selectedDataType)
            {
                case DataType.World:
                    return "World";

                case DataType.Character:
                    if (_selectedHeroine == null)
                    {
                        // Player: セーブデータから Name を取得
                        // Player: get Name from save data
                        var saveData = GameController.CurrentSaveData;
                        string playerName = saveData?.GetPlayerCharacterCard()?.GetName();
                        if (string.IsNullOrEmpty(playerName))
                        {
                            LogDebug("[GetOutputBaseName] Player CharacterCard Name is empty. Falling back to 'Player'.");
                            playerName = "Player";
                        }
                        return SanitizeFileName(playerName, "Player");
                    }
                    else
                    {
                        // Heroine: 編集中のカードから Name を取得
                        // Heroine: get Name from the currently editing card
                        string heroineName = _editingCard?.GetName();
                        if (string.IsNullOrEmpty(heroineName))
                        {
                            LogDebug("[GetOutputBaseName] Heroine CharacterCard Name is empty. Falling back to 'Heroine'.");
                            heroineName = "Heroine";
                        }
                        return SanitizeFileName(heroineName, "Heroine");
                    }

                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// 文字列をファイル名として使用できる形にサニタイズします。
        /// Sanitizes a string to make it usable as a file name.
        ///
        /// 処理内容 / Processing:
        ///   1. <see cref="Path.GetInvalidFileNameChars"/> が返す無効文字をすべて除去します。
        ///      Removes all characters returned by <see cref="Path.GetInvalidFileNameChars"/>.
        ///   2. 前後の空白文字をトリムします。
        ///      Trims leading and trailing whitespace.
        ///   3. 結果が空文字になった場合は <paramref name="fallback"/> を返します。
        ///      Returns <paramref name="fallback"/> if the result is empty.
        /// </summary>
        /// <param name="name">サニタイズ対象の文字列 / String to sanitize</param>
        /// <param name="fallback">サニタイズ結果が空の場合に使用するフォールバック文字列 / Fallback string used when sanitized result is empty</param>
        private string SanitizeFileName(string name, string fallback = "Unknown")
        {
            if (string.IsNullOrEmpty(name))
                return fallback;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);

            foreach (char c in name)
            {
                // 無効文字はスキップ
                // Skip invalid characters
                bool isInvalid = false;
                foreach (char inv in invalidChars)
                {
                    if (c == inv)
                    {
                        isInvalid = true;
                        break;
                    }
                }
                if (!isInvalid)
                    sb.Append(c);
            }

            string sanitized = sb.ToString().Trim();

            if (string.IsNullOrEmpty(sanitized))
            {
                LogDebug($"[SanitizeFileName] Name '{name}' became empty after sanitization. Using fallback '{fallback}'.");
                return fallback;
            }

            return sanitized;
        }

        // =====================================================================
        // Logging
        // =====================================================================

        private void LogInfo(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogInfo($"[CharacterCardEditorUI] {message}");
        }

        private void LogDebug(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogDebug($"[CharacterCardEditorUI] {message}");
        }

        private void LogWarning(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogWarning($"[CharacterCardEditorUI] {message}");
        }

        private void LogError(string message)
        {
            AICharacterBridgePlugin.Instance?.Logger.LogError($"[CharacterCardEditorUI] {message}");
        }
    }
}
