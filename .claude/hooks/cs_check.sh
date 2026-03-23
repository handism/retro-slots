#!/bin/bash
# cs_check.sh
# PostToolUse hook: Write|Edit 後に .cs ファイルのフォーマット・ビルドチェックを行う
# stdin: {"tool_name": "...", "tool_input": {"file_path": "..."}, ...}

CSHARPIER="$HOME/.dotnet/tools/csharpier"
PROJECT_ROOT="/Users/mac/git/slot-unity"

# ファイルパスを stdin の JSON から取得
FILE_PATH=$(cat | jq -r '.tool_input.file_path // empty' 2>/dev/null)

# .cs ファイルでなければ終了
[[ "$FILE_PATH" == *.cs ]] || exit 0

echo ""
echo "── cs_check: $FILE_PATH ──"

# ──────────────────────────────────────────────
# 1. CSharpier フォーマットチェック
# ──────────────────────────────────────────────
if [ -x "$CSHARPIER" ]; then
  FORMAT_OUT=$("$CSHARPIER" check "$FILE_PATH" 2>&1)
  FORMAT_EXIT=$?
  if [ $FORMAT_EXIT -ne 0 ]; then
    echo "[FORMAT] ❌ スタイル違反あり:"
    echo "$FORMAT_OUT"
    echo "[FORMAT] 修正コマンド: $CSHARPIER format \"$FILE_PATH\""
  else
    echo "[FORMAT] ✅ OK"
  fi
else
  echo "[FORMAT] ⚠️  CSharpier が見つかりません ($CSHARPIER)"
fi

# ──────────────────────────────────────────────
# 2. ビルドチェック（ファイルパスから .csproj を決定）
# ──────────────────────────────────────────────
CSPROJ=""
case "$FILE_PATH" in
  */Assets/Scripts/Model/*)    CSPROJ="SlotGame.Model.csproj" ;;
  */Assets/Scripts/Core/*)     CSPROJ="SlotGame.Core.csproj" ;;
  */Assets/Scripts/View/*)     CSPROJ="SlotGame.View.csproj" ;;
  */Assets/Scripts/Data/*)     CSPROJ="SlotGame.Data.csproj" ;;
  */Assets/Scripts/Utility/*)  CSPROJ="SlotGame.Utility.csproj" ;;
  */Assets/Scripts/Audio/*)    CSPROJ="SlotGame.Audio.csproj" ;;
  */Assets/Editor/*)           CSPROJ="SlotGame.Editor.csproj" ;;
  */Assets/Tests/EditMode/*)   CSPROJ="SlotGame.Tests.EditMode.csproj" ;;
  */Assets/Tests/PlayMode/*)   CSPROJ="SlotGame.Tests.PlayMode.csproj" ;;
esac

if [ -n "$CSPROJ" ]; then
  CSPROJ_PATH="$PROJECT_ROOT/$CSPROJ"
  if [ -f "$CSPROJ_PATH" ]; then
    echo "[BUILD]  🔨 $CSPROJ ..."
    BUILD_OUT=$(dotnet build "$CSPROJ_PATH" -nologo -v:minimal --no-restore 2>&1)
    BUILD_EXIT=$?
    if [ $BUILD_EXIT -ne 0 ]; then
      echo "[BUILD]  ❌ ビルドエラー:"
      echo "$BUILD_OUT"
    else
      # エラー・警告行だけ抽出して表示
      WARNINGS=$(echo "$BUILD_OUT" | grep -E "warning|error" | grep -v "^Build succeeded")
      if [ -n "$WARNINGS" ]; then
        echo "[BUILD]  ⚠️  警告:"
        echo "$WARNINGS"
      else
        echo "[BUILD]  ✅ OK"
      fi
    fi
  else
    echo "[BUILD]  ⚠️  $CSPROJ が見つかりません（Unity でプロジェクトを一度開いてください）"
  fi
else
  echo "[BUILD]  ℹ️  対象外のパス（チェックをスキップ）"
fi

echo "──────────────────────────────────────────"
echo ""

exit 0
