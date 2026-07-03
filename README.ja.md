# AI Character Bridge

[![English](https://img.shields.io/badge/Language-English-red.svg)](https://github.com/Lumicend/AICharacterBridge/blob/master/README.md)
[![日本語](https://img.shields.io/badge/Language-日本語-blue.svg)](https://github.com/Lumicend/AICharacterBridge/blob/master/README.ja.md)

> **概念実証プラグイン** | Koikatsu (コイカツ！) 専用


ローカル LLM（大規模言語モデル）とコイカツのゲームキャラクターを接続し、文脈に応じた自然な会話を実現する BepInEx プラグインです。「AI とゲームを繋ぐことはできるのか」を個人的に検証した概念実証です。

「コイカツ！」以外のゲームタイトルには対応していません。

---

## ⚠️ ご利用にあたっての注意

### 生成 AI の利用について

このプラグインのプログラムコードの多くは、AI（**Claude** および **Gemini**）によって生成されたものです。このプラグインを利用する際、またはコードを参照する際はこの点をご留意ください。

### 不具合等に関する免責

本プラグインは試作品（概念実証）であり、十分な動作確認やテストは行っていません。  
**自己責任でご利用ください**。本プラグインの利用によって生じたいかなる損害についても、作者は責任を負いません。



---

## 主な機能

- **Character Card 対応**: 広く普及している [Character Card V2](https://github.com/malfoyslastname/character-card-spec-v2) / [V3](https://github.com/kwaroran/character-card-spec-v3) 形式の JSON ファイルでキャラクターの性格を定義。外部ツールで作成したカードをそのまま利用できます。
- **文脈を持つ会話**: 過去の TalkScene の会話ログが自動保存され、次回以降の会話に引き継がれます。
- **ゲーム内アクション連動**: AI の返答内容に応じて「一緒に昼食」「帰宅」「デート予約」などのゲーム内アクションを実行できます。
- **ヒロインパラメータ反映**: 会話の評価結果（AI が判定した印象）が好感度・親密度などに自動反映されます。

---

## インストール・使い方

初めての方は**スタートガイド**をお読みください。導入から最初の AI 会話を成功させるまでの手順を説明しています。

- 📖 [スタートガイド](https://github.com/Lumicend/AICharacterBridge/blob/master/docs/ja/StartGuide.md)
- 📖 [ユーザーマニュアル](https://github.com/Lumicend/AICharacterBridge/blob/master/docs/ja/UserManual.md)（各種機能の詳しい説明）

---

## ドキュメント（開発者向け）

参考資料として、AI にこのプラグインの概要を説明する際に使用した[プロジェクトドキュメント](https://github.com/Lumicend/AICharacterBridge/blob/master/docs/ja/plugin_overview.md)を公開しています。プラグインのアーキテクチャや各モジュールの設計を詳しく説明しています。

---

## 今後のアップデートについて

あくまで個人用に作成したプラグインのため、基本的にアップデートの予定はありません。  
ライセンスの範囲内であれば自由にご利用ください。今後のプラグイン開発の一助になれば幸いです。
