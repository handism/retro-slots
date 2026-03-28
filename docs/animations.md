# アニメーション設計指針

本プロジェクトにおける、シンボルやUIの演出（アニメーション）に関する設計指針をまとめます。

## 1. シンボルの当選演出 (Winning Animation)

シンボルの当選演出は、`SymbolView.PlayWinPresentation` によって一括管理されます。

### 1.1 演出の優先順位

1.  **専用アニメーション (Animator)**
    - `SymbolData.winAnim` に `AnimationClip` が設定されている場合に再生されます。
    - **用途**: Wild, Scatter, Bonus シンボルや、高配当の特定のシンボルに使用します。
    - **実装**: `Animator.Play(clip.name)` により再生され、クリップの長さ分だけ完了を待機します。

2.  **共通パルス演出 (DOTween)**
    - `SymbolData.winAnim` が未設定（null）の場合のデフォルト演出です。
    - **用途**: 低配当シンボルの共通的な強調に使用します。
    - **実装**: `transform.DOScale` による拡縮のループ（Yoyo）です。

### 1.2 実装上の注意点

- **ステート管理**: 演出の終了時やリール回転開始時には、必ず `StopPulseAnimation()` または `PlayIdleAnimation()` を呼び出して状態をクリアしてください。
- **キャンセル対応**: アニメーションの完了待機には `CancellationToken` を使用し、スピンのスキップや中断に適切に対応してください。

## 2. 実装の役割分担

- **SymbolData**: どのシンボルがどの専用アニメーションを持つかを定義します。
- **SymbolView**: 演出の具体的な再生ロジック（Animator か Pulse かの判定を含む）を秘匿します。
- **ReelView / UIManager**: 「どの位置のシンボルを演出するか」を指定するのみで、具体的な演出手法には依存しません。
