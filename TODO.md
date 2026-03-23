# Fantasy Slot 課題一覧（TODO.md）

ドキュメント（要件定義書・設計書）と現在の実装の乖離を元に、今後修正が必要な項目をまとめました。

## 1. データ・ロジック（HIGH）

- [ ] **配当値の修正**
  - `ScriptableObjectCreator.cs` 内の `CreateSymbolAssets` で定義されている配当倍率が `requirements.md` と一致していない。
    - 例: Dragon 3揃え 現在 10 → 要件 50
    - 全シンボルの倍率を要件通りに修正する必要がある。
- [ ] **セーブデータの整合性検証（HashPath対応）**
  - `design.md` では `savedata.json.hash` ファイルにハッシュを保存する仕様だが、現在は `SaveData` クラス内の `checksum` フィールドに保持している。
  - セキュリティと仕様準拠のため、外部ファイル方式への移行を検討。
## 2. アーキテクチャ・パフォーマンス（MEDIUM）

- [ ] **ReelView のオブジェクトプール化**
  - `design.md` では `UnityEngine.Pool.ObjectPool` の使用が指定されているが、現在は固定 5 シンボルの配列（循環バッファ）で実装されている。
  - 将来的な拡張性（リール行数の動的変更など）のため、公式プール機能への差し替えを検討。
- [ ] **UniTask のキャンセレーション伝播**
  - `SpinManager` や `BonusManager` の一部の非同期メソッドで、`CancellationToken` が末端の API（DOTween 等）まで完全に伝播しているか再点検が必要。

## 3. UI・演出（MEDIUM）

- [ ] **ベット選択スライダーの実装**
  - `requirements.md` および `MainHUDView` の設計に含まれている「ベット額変更用スライダー」が未実装。現在はボタン選択のみ。
- [ ] **ゲーム説明画面の追加**
  - `requirements.md` の「設定メニュー」に含まれるべきゲームルール・配当表（簡易版）の説明テキストが `SettingsView` に存在しない。
- [ ] **サウンドイベントの補完**
  - `AudioManager.cs` に定義はあるが、コードから呼び出されていない SE がある（ビッグウィン、ボーナスラウンド開始時の専用演出用など）。

## 4. その他（LOW）

- [ ] **TitleManager のテスト作成**
  - 新規追加した `TitleManager` に対する単体テストまたは結合テストが不足している。
- [ ] **RtpCalculator の精度向上**
  - 現在のシミュレーション回数（10万回）を増やし、より厳密な RTP 算出を行う。
