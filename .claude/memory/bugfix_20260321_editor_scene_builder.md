---
name: Editor SceneBuilder の落とし穴（Unity 6.3）
description: EditorSceneManager でシーンをコード生成する際の複数のハマりポイントと対処法
type: feedback
---

## 背景

`SceneBuilder.cs`（Editor スクリプト）で Boot / Main / BonusRound の 3 シーンを自動構築した際に
4 つのエラーが連続して発生した。同様のスクリプトを書く際は最初からこれらを避けること。

---

## ハマり 1: `NewSceneMode.Additive` は未保存の Untitled シーンがあると失敗する

**エラー:**
```
InvalidOperationException: Cannot create a new scene additively with an untitled scene unsaved.
```

**原因:** Unity が起動直後や新規プロジェクト開始時などに Untitled（未保存・パスなし）のシーンが
アクティブになっている状態で `Additive` モードを使おうとすると拒否される。

**対処:** Editor でシーンを順次生成する場合は **`NewSceneMode.Single`** を使う。
Single は現在のシーンを置き換えて新しいシーンをアクティブにするため Untitled 問題が発生しない。
また `CloseScene` も不要になる。`SetActiveScene` も Single が自動でアクティブにするため不要。

```csharp
// NG
var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
SceneManager.SetActiveScene(scene);
// ...
EditorSceneManager.SaveScene(scene, path);
EditorSceneManager.CloseScene(scene, true);

// OK
var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
// ...
EditorSceneManager.SaveScene(scene, path);
// CloseScene 不要。次の NewScene(Single) が自動で置き換える
```

**注意:** Single を使うと実行時に「変更を保存しますか？」ダイアログが出ることがある（前のシーンが
dirty な場合）。Editor スクリプト内で連続して Save → 次の NewScene(Single) と流せば、保存済みの
ため dirty にならずダイアログは出ない。最初の呼び出しのみユーザーに出る可能性がある。

---

## ハマり 2: `const string` に補間文字列は C# 9 では使えない

**エラー:**
```
error CS8773: Feature 'constant interpolated strings' is not available in C# 9.0.
Please use language version 10.0 or greater.
```

**原因:** Unity（C# 9）では `const string foo = $"..."` が使えない。C# 10 以降の機能。

**対処:** `const` を外すか、文字列連結にする。

```csharp
// NG
const string tempScene = $"{ScenesPath}/_temp.unity";

// OK
string tempScene = ScenesPath + "/_temp.unity";
```

---

## ハマり 3: `CanvasGroup` は `RectTransform` を自動追加しない

**エラー:**
```
MissingComponentException: There is no 'RectTransform' attached to the "WinPopup" game object
```

**原因:** `new GameObject("WinPopup", typeof(CanvasGroup))` では `RectTransform` が付かない。
`Image` や `Text` など UI Graphic 系コンポーネントは自動で RectTransform を追加するが、
`CanvasGroup` はそうでない。

**対処:** `typeof(RectTransform)` を明示的に追加するか、`Image` も同時に指定する。

```csharp
// NG
var go = new GameObject("WinPopup", typeof(CanvasGroup));

// OK
var go = new GameObject("WinPopup", typeof(RectTransform), typeof(CanvasGroup));
```

---

## ハマり 4: `MoveGameObjectToScene` は Root GO にしか使えない

**エラー:**
```
ArgumentException: Gameobject is not a root in a scene
```

**原因:** `SetParent()` で親を設定した後のオブジェクトに `MoveGameObjectToScene` を呼ぶと失敗する。
親を持つ GO はシーンルートではないため。

**対処:** `SetParent` 後は `MoveGameObjectToScene` を呼ばない。子 GO は親と同じシーンに自動で属する。

```csharp
// NG
var child = new GameObject("Child");
child.transform.SetParent(parent.transform, false);
SceneManager.MoveGameObjectToScene(child, scene);   // エラー

// OK
var child = new GameObject("Child");
child.transform.SetParent(parent.transform, false);
// MoveGameObjectToScene 不要
```

---

## 総合指針：Editor でシーンをコード生成する場合の定石

1. `NewSceneMode.Single` を使い、1シーンずつ作成・保存→次のシーンへ
2. `CloseScene` / `SetActiveScene` は不要
3. `new GameObject()` は自動でアクティブシーンに入る
4. `MoveGameObjectToScene` は Root GO のみ（SetParent 後には使わない）
5. UI GO に RectTransform が必要な場合は `typeof(RectTransform)` を明示
6. `const string` に補間文字列は使わない（C# 9 制限）
