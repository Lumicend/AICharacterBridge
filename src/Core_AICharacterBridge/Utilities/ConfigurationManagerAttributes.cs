using System;
using BepInEx.Configuration;

namespace AICharacterBridge.Core.Utilities
{
    /// <summary>
    /// ConfigurationManager用のカスタム属性クラス。
    /// ConfigurationManagerプラグインがリフレクションでこのクラスを検出し、
    /// CustomDrawerを使用してカスタムUIを描画します。
    /// 
    /// このクラスはプロジェクト全体で再利用可能な汎用ユーティリティです。
    /// 
    /// Custom attributes class for ConfigurationManager.
    /// The ConfigurationManager plugin detects this class via reflection
    /// and uses CustomDrawer to render custom UI.
    /// 
    /// This class is a reusable utility for the entire project.
    /// </summary>
    internal sealed class ConfigurationManagerAttributes
    {
        /// <summary>
        /// 設定項目のカスタム描画を行うデリゲート。
        /// Delegate for custom drawing of configuration entries.
        /// </summary>
        public Action<ConfigEntryBase> CustomDrawer;
    }
}
