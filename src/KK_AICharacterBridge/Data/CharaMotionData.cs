using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// キャラクターのモーション（ポーズ）を定義するデータクラス
    /// Character motion (pose) definition data class
    /// </summary>
    [Serializable]
    public class CharaMotionData
    {
        /// <summary>AIが選択時に使用するモーション名（例: "standing_still", "sitting"）</summary>
        public string Name { get; set; }

        /// <summary>アニメーションステートの名前（例: "Stand_00_00"）</summary>
        public string State { get; set; }

        /// <summary>アニメーションバンドルのパス（オプション）</summary>
        public string Bundle { get; set; }

        /// <summary>アニメーションアセットの名前（オプション）</summary>
        public string Asset { get; set; }

        /// <summary>IKバンドルのパス（オプション）</summary>
        public string IKBundle { get; set; }

        /// <summary>IKアセットの名前（オプション）</summary>
        public string IKAsset { get; set; }

        /// <summary>シェイクバンドルのパス（オプション）</summary>
        public string ShakeBundle { get; set; }

        /// <summary>シェイクアセットの名前（オプション）</summary>
        public string ShakeAsset { get; set; }

        /// <summary>オーバーライドバンドルのパス（オプション）</summary>
        public string OverrideBundle { get; set; }

        /// <summary>オーバーライドアセットの名前（オプション）</summary>
        public string OverrideAsset { get; set; }

        /// <summary>レイヤー番号（オプション）</summary>
        public string LayerNo { get; set; }

        public CharaMotionData()
        {
            Name = "";
            State = "";
            Bundle = "";
            Asset = "";
            IKBundle = "";
            IKAsset = "";
            ShakeBundle = "";
            ShakeAsset = "";
            OverrideBundle = "";
            OverrideAsset = "";
            LayerNo = "";
        }

        /// <summary>
        /// CharaMotionコマンドの引数配列に変換
        /// Convert to CharaMotion command arguments
        /// </summary>
        public string[] ToCommandArgs()
        {
            return new[]
            {
                State,
                Bundle,
                Asset,
                IKBundle,
                IKAsset,
                ShakeBundle,
                ShakeAsset,
                OverrideBundle,
                OverrideAsset,
                LayerNo
            };
        }

        /// <summary>
        /// 妥当性を検証
        /// </summary>
        public bool IsValid()
        {
            // NameとStateは必須
            return !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(State);
        }

        /// <summary>
        /// ディープコピーを作成
        /// </summary>
        public CharaMotionData Clone()
        {
            return new CharaMotionData
            {
                Name = this.Name,
                State = this.State,
                Bundle = this.Bundle,
                Asset = this.Asset,
                IKBundle = this.IKBundle,
                IKAsset = this.IKAsset,
                ShakeBundle = this.ShakeBundle,
                ShakeAsset = this.ShakeAsset,
                OverrideBundle = this.OverrideBundle,
                OverrideAsset = this.OverrideAsset,
                LayerNo = this.LayerNo
            };
        }

        /// <summary>
        /// CharaMotionDataのリストをJSON配列形式（名前のみ）にシリアライズします。
        /// Serializes a list of CharaMotionData to JSON array format (names only).
        /// </summary>
        /// <param name="charaMotions">モーションデータのリスト</param>
        /// <returns>JSON配列形式の文字列（例: ["standing_normal", "standing_arms_crossed"]）</returns>
        public static string ToJsonList(List<CharaMotionData> charaMotions)
        {
            if (charaMotions == null || charaMotions.Count == 0)
                return "[]";

            var names = new List<string>();
            foreach (var motion in charaMotions)
            {
                if (!string.IsNullOrEmpty(motion.Name))
                    names.Add(motion.Name);
            }

            return JsonConvert.SerializeObject(names, Formatting.Indented);
        }
    }
}
