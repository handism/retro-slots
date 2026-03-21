using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SlotGame.Audio;
using SlotGame.Core;
using SlotGame.Data;
using SlotGame.View;

namespace SlotGame.Editor
{
    /// <summary>
    /// SlotGame/Build All Scenes メニューから Boot / Main / BonusRound の
    /// 3 シーンを自動構築し、Build Settings に登録する。
    /// 既存シーンは上書き（冪等実行対応）。
    /// </summary>
    public static class SceneBuilder
    {
        private const string ScenesPath   = "Assets/Scenes";
        private const string PrefabPath   = "Assets/Art/Prefabs/SymbolView.prefab";
        private const string SOBasePath   = "Assets/ScriptableObjects";

        // ─── エントリーポイント ─────────────────────────────────────────

        [MenuItem("SlotGame/Build All Scenes")]
        public static void BuildAllScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[SceneBuilder] Build All Scenes cannot run during Play Mode. Stop Play Mode and run it again.");
                return;
            }

            EnsureFolder("Assets", "Scenes");
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Prefabs");

            CreateSymbolViewPrefab();
            BuildBootScene();
            BuildMainScene();
            BuildBonusRoundScene();
            AddScenesToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SceneBuilder] All scenes built successfully!");
        }

        [MenuItem("SlotGame/Build All Scenes", true)]
        private static bool ValidateBuildAllScenes()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        // ─── SymbolView Prefab ──────────────────────────────────────────

        private static void CreateSymbolViewPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                Debug.Log("[SceneBuilder] SymbolView.prefab already exists — skipped.");
                return;
            }

            var go = new GameObject("SymbolView");
            go.AddComponent<Image>();
            go.AddComponent<SymbolView>();
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 180);

            PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);

            Debug.Log("[SceneBuilder] SymbolView.prefab created.");
        }

        // ─── Boot.unity ─────────────────────────────────────────────────

        private static void BuildBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // EventSystem
            CreateEventSystem();

            // Canvas
            var canvas = CreateCanvas("Canvas", RenderMode.ScreenSpaceOverlay);

            // ProgressBar (Slider)
            var sliderGO = new GameObject("ProgressBar", typeof(Slider));
            SetParent(sliderGO, canvas);
            var rt = sliderGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.1f);
            rt.anchorMax = new Vector2(0.9f, 0.15f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var slider = sliderGO.GetComponent<Slider>();

            // BootManager
            var bootGO = new GameObject("BootManager");
            SceneManager.MoveGameObjectToScene(bootGO, scene);
            var boot = bootGO.AddComponent<BootManager>();
            WireField(boot, "progressBar", slider);

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Boot.unity");
            Debug.Log("[SceneBuilder] Boot.unity built.");
        }

        // ─── Main.unity ─────────────────────────────────────────────────

        private static void BuildMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // EventSystem
            CreateEventSystem();

            // Main Camera
            var camGO = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(camGO, scene);
            var cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.tag = "MainCamera";
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = Color.black;
            cam.orthographic     = true;
            cam.orthographicSize = 5f;

            // Main Canvas（Screen Space - Camera）
            var mainCanvasGO = CreateCanvas("Main Canvas", RenderMode.ScreenSpaceCamera);
            var mainCanvas   = mainCanvasGO.GetComponent<Canvas>();
            mainCanvas.worldCamera   = cam;
            mainCanvas.planeDistance = 100;
            SetupCanvasScaler(mainCanvasGO, 0.5f);

            // Background
            var bg = new GameObject("Background", typeof(Image));
            SetParent(bg, mainCanvasGO);
            StretchFull(bg);
            bg.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.15f);

            // ReelGrid + 5 reels
            var (reelGrid, reelControllers) = CreateReelGrid(mainCanvasGO);

            // WinPopup
            var (winPopupGO, winPopupView) = CreateWinPopup(mainCanvasGO);

            // SettingsPanel
            var (settingsPanelGO, settingsView) = CreateSettingsPanel(mainCanvasGO);
            settingsPanelGO.SetActive(false);

            // PaytablePanel
            var (paytablePanelGO, paytableView) = CreatePaytablePanel(mainCanvasGO);
            paytablePanelGO.SetActive(false);

            // HUD Canvas（Screen Space - Overlay）
            var hudCanvasGO = CreateCanvas("HUD Canvas", RenderMode.ScreenSpaceOverlay);
            hudCanvasGO.GetComponent<Canvas>().sortingOrder = 10;
            SetupCanvasScaler(hudCanvasGO, 0.5f);

            var (mainHUDGO, mainHUDView, spinButton, autoSpinButton, betButtons) = CreateMainHUD(hudCanvasGO);
            var (freeSpinGO, freeSpinView) = CreateFreeSpinHUD(hudCanvasGO);
            freeSpinGO.SetActive(false);

            // Managers
            var managerRoot = new GameObject("Managers");
            SceneManager.MoveGameObjectToScene(managerRoot, scene);

            var gameManagerGO  = CreateChild(managerRoot, "GameManagerGO");
            var spinManagerGO  = CreateChild(managerRoot, "SpinManagerGO");
            var bonusManagerGO = CreateChild(managerRoot, "BonusManagerGO");
            var uiManagerGO    = CreateChild(managerRoot, "UIManagerGO");
            var audioManagerGO = CreateChild(managerRoot, "AudioManagerGO");

            var gameManager  = gameManagerGO.AddComponent<GameManager>();
            var spinManager  = spinManagerGO.AddComponent<SpinManager>();
            var bonusManager = bonusManagerGO.AddComponent<BonusManager>();
            var uiManager    = uiManagerGO.AddComponent<UIManager>();
            var audioManager = audioManagerGO.AddComponent<AudioManager>();

            // AudioSources（AudioManager の子として追加）
            var bgmSourceGO = new GameObject("BGMSource");
            bgmSourceGO.transform.SetParent(audioManagerGO.transform, false);
            var bgmSource = bgmSourceGO.AddComponent<AudioSource>();
            bgmSource.loop        = true;
            bgmSource.playOnAwake = false;

            var seSourceGO = new GameObject("SESource");
            seSourceGO.transform.SetParent(audioManagerGO.transform, false);
            var seSource = seSourceGO.AddComponent<AudioSource>();
            seSource.playOnAwake = false;

            // ─── ワイヤリング ────────────────────────────────────────────

            // AudioManager
            WireField(audioManager, "bgmSource", bgmSource);
            WireField(audioManager, "seSource", seSource);

            // UIManager
            WireField(uiManager, "mainHUD",     mainHUDView);
            WireField(uiManager, "freeSpinHUD", freeSpinView);
            WireField(uiManager, "winPopup",    winPopupView);
            WireField(uiManager, "settingsView", settingsView);
            WireField(uiManager, "paytableView", paytableView);

            // BonusManager
            WireField(bonusManager, "spinManager", spinManager);

            // SpinManager → ReelController[]
            var so    = new SerializedObject(spinManager);
            var reels = so.FindProperty("reels");
            reels.arraySize = 5;
            for (int i = 0; i < 5; i++)
                reels.GetArrayElementAtIndex(i).objectReferenceValue = reelControllers[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            // GameManager
            var gso = new SerializedObject(gameManager);
            gso.FindProperty("spinManager").objectReferenceValue  = spinManager;
            gso.FindProperty("bonusManager").objectReferenceValue = bonusManager;
            gso.FindProperty("uiManager").objectReferenceValue    = uiManager;
            gso.FindProperty("audioManager").objectReferenceValue = audioManager;

            var strips = gso.FindProperty("reelStrips");
            strips.arraySize = 5;
            for (int i = 0; i < 5; i++)
                strips.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<ReelStripData>($"{SOBasePath}/Reels/Reel{i}.asset");

            gso.FindProperty("paylineData").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PaylineData>($"{SOBasePath}/Paylines/PaylineData.asset");
            gso.FindProperty("payoutData").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PayoutTableData>($"{SOBasePath}/PayoutTable/PayoutTableData.asset");
            gso.ApplyModifiedPropertiesWithoutUndo();

            // HUD button bindings
            UnityEventTools.AddPersistentListener(spinButton.onClick, gameManager.OnSpinButtonPressed);
            UnityEventTools.AddIntPersistentListener(autoSpinButton.onClick, gameManager.OnAutoSpinButtonPressed, 10);
            for (int i = 0; i < betButtons.Length; i++)
            {
                int bet = new[] { 10, 20, 50, 100 }[i];
                UnityEventTools.AddIntPersistentListener(betButtons[i].onClick, gameManager.OnBetChanged, bet);
            }

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Main.unity");
            Debug.Log("[SceneBuilder] Main.unity built.");
        }

        // ─── ReelGrid ──────────────────────────────────────────────────

        private static (GameObject, ReelController[]) CreateReelGrid(GameObject parent)
        {
            var gridGO = new GameObject("ReelGrid", typeof(RectTransform));
            SetParent(gridGO, parent);

            var rt = gridGO.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = new Vector2(920, 540);   // 5×180 + 4×5 余白

            var hlg = gridGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing             = 5f;
            hlg.childAlignment      = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth   = false;
            hlg.childControlHeight  = false;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var controllers = new ReelController[5];

            for (int i = 0; i < 5; i++)
            {
                var reelGO = new GameObject($"Reel{i}");
                SetParent(reelGO, gridGO);

                // LayoutElement でサイズ固定
                var le = reelGO.AddComponent<LayoutElement>();
                le.preferredWidth  = 180f;
                le.preferredHeight = 540f;

                // RectMask2D でクリップ
                reelGO.AddComponent<RectMask2D>();

                // ReelController（RequireComponent で ReelView も自動追加）
                var rc = reelGO.AddComponent<ReelController>();

                // ReelView の symbolViewPrefab をワイヤリング
                var rv = reelGO.GetComponent<ReelView>();
                WireField(rv, "symbolViewPrefab", prefab);

                // ReelController の reelStrip をワイヤリング
                var strip = AssetDatabase.LoadAssetAtPath<ReelStripData>(
                    $"{SOBasePath}/Reels/Reel{i}.asset");
                WireField(rc, "reelStrip", strip);

                controllers[i] = rc;
            }

            return (gridGO, controllers);
        }

        // ─── WinPopup ──────────────────────────────────────────────────

        private static (GameObject, WinPopupView) CreateWinPopup(GameObject parent)
        {
            var go = new GameObject("WinPopup", typeof(RectTransform), typeof(CanvasGroup));
            SetParent(go, parent);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = new Vector2(600, 300);

            go.GetComponent<CanvasGroup>().alpha = 0;
            var view = go.AddComponent<WinPopupView>();

            var amtText   = CreateTMPText(go, "WinAmountText", "0", 72);
            var levelText = CreateTMPText(go, "WinLevelText",  "WIN!", 48);

            var amtRT = amtText.GetComponent<RectTransform>();
            amtRT.anchorMin        = new Vector2(0, 0.5f);
            amtRT.anchorMax        = new Vector2(1, 1);
            amtRT.offsetMin        = amtRT.offsetMax = Vector2.zero;

            var lvlRT = levelText.GetComponent<RectTransform>();
            lvlRT.anchorMin        = new Vector2(0, 0);
            lvlRT.anchorMax        = new Vector2(1, 0.5f);
            lvlRT.offsetMin        = lvlRT.offsetMax = Vector2.zero;

            WireField(view, "winAmountText", amtText.GetComponent<TMP_Text>());
            WireField(view, "winLevelText",  levelText.GetComponent<TMP_Text>());

            return (go, view);
        }

        // ─── SettingsPanel ─────────────────────────────────────────────

        private static (GameObject, SettingsView) CreateSettingsPanel(GameObject parent)
        {
            var go = new GameObject("SettingsPanel", typeof(Image));
            SetParent(go, parent);
            StretchFull(go);
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            var view = go.AddComponent<SettingsView>();

            var bgmSlider  = CreateSlider(go, "BGMSlider");
            var seSlider   = CreateSlider(go, "SESlider");
            var bgmValText = CreateTMPText(go, "BGMValueText", "100%", 24);
            var seValText  = CreateTMPText(go, "SEValueText",  "100%", 24);
            var resetBtn   = CreateButton(go, "ResetCoinsButton", "RESET COINS");
            var closeBtn   = CreateButton(go, "CloseButton",      "CLOSE");

            LayoutVertical(bgmSlider, 0);
            LayoutVertical(seSlider, 1);

            WireField(view, "bgmSlider",       bgmSlider.GetComponent<Slider>());
            WireField(view, "seSlider",        seSlider.GetComponent<Slider>());
            WireField(view, "bgmValueText",    bgmValText.GetComponent<TMP_Text>());
            WireField(view, "seValueText",     seValText.GetComponent<TMP_Text>());
            WireField(view, "resetCoinsButton", resetBtn.GetComponent<Button>());
            WireField(view, "closeButton",     closeBtn.GetComponent<Button>());

            return (go, view);
        }

        // ─── PaytablePanel ─────────────────────────────────────────────

        private static (GameObject, PaytableView) CreatePaytablePanel(GameObject parent)
        {
            var go = new GameObject("PaytablePanel", typeof(Image));
            SetParent(go, parent);
            StretchFull(go);
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);

            var view = go.AddComponent<PaytableView>();

            var contentRoot = new GameObject("ContentRoot", typeof(RectTransform));
            SetParent(contentRoot, go);
            StretchFull(contentRoot);

            var closeBtn = CreateButton(go, "CloseButton", "CLOSE");
            var closeBtnRT = closeBtn.GetComponent<RectTransform>();
            closeBtnRT.anchorMin        = new Vector2(0.9f, 0.9f);
            closeBtnRT.anchorMax        = new Vector2(1f, 1f);
            closeBtnRT.offsetMin        = closeBtnRT.offsetMax = Vector2.zero;

            WireField(view, "contentRoot", contentRoot.GetComponent<Transform>());
            WireField(view, "closeButton", closeBtn.GetComponent<Button>());
            // rowPrefab は Art アセット整備後に設定するため null のまま

            return (go, view);
        }

        // ─── MainHUD ───────────────────────────────────────────────────

        private static (GameObject, MainHUDView, Button, Button, Button[]) CreateMainHUD(GameObject parent)
        {
            var go = new GameObject("MainHUD");
            SetParent(go, parent);
            StretchFull(go);

            var view = go.AddComponent<MainHUDView>();

            var coinText      = CreateTMPText(go, "CoinText",  "1000",  36);
            var winText       = CreateTMPText(go, "WinText",   "------", 28);
            var spinBtn       = CreateButton(go, "SpinButton",      "SPIN");
            var autoSpinBtn   = CreateButton(go, "AutoSpinButton",  "AUTO");

            PositionHUD(coinText,    new Vector2(40, -40));
            PositionHUD(winText,     new Vector2(40, -90));
            PositionHUD(spinBtn,     new Vector2(1500, -40));
            PositionHUD(autoSpinBtn, new Vector2(1500, -110));

            // Bet ボタン × 4
            var betValues  = new[] { 10, 20, 50, 100 };
            var betButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                var btn = CreateButton(go, $"BetButton{i}", $"BET {betValues[i]}");
                PositionHUD(btn, new Vector2(960 + i * 180, -180));
                betButtons[i] = btn.GetComponent<Button>();
            }

            WireField(view, "coinText",       coinText.GetComponent<TMP_Text>());
            WireField(view, "winText",        winText.GetComponent<TMP_Text>());
            WireField(view, "spinButton",     spinBtn.GetComponent<Button>());
            WireField(view, "autoSpinButton", autoSpinBtn.GetComponent<Button>());

            var so = new SerializedObject(view);
            var btnProp = so.FindProperty("betButtons");
            btnProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                btnProp.GetArrayElementAtIndex(i).objectReferenceValue = betButtons[i];

            var valProp = so.FindProperty("betValues");
            valProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                valProp.GetArrayElementAtIndex(i).intValue = betValues[i];

            so.ApplyModifiedPropertiesWithoutUndo();

            return (go, view, spinBtn.GetComponent<Button>(), autoSpinBtn.GetComponent<Button>(), betButtons);
        }

        // ─── FreeSpinHUD ───────────────────────────────────────────────

        private static (GameObject, FreeSpinHUDView) CreateFreeSpinHUD(GameObject parent)
        {
            var go = new GameObject("FreeSpinHUD", typeof(Image));
            SetParent(go, parent);
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 1f);
            rt.anchorMax        = new Vector2(0.5f, 1f);
            rt.pivot            = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, 0);
            rt.sizeDelta        = new Vector2(600, 100);

            var view = go.AddComponent<FreeSpinHUDView>();

            var remainText  = CreateTMPText(go, "RemainingText", "FREE SPINS: 0", 32);
            var totalWinTxt = CreateTMPText(go, "TotalWinText",  "TOTAL WIN: 0", 28);

            var remRT = remainText.GetComponent<RectTransform>();
            remRT.anchorMin        = new Vector2(0, 0.5f);
            remRT.anchorMax        = new Vector2(1, 1);
            remRT.offsetMin        = remRT.offsetMax = Vector2.zero;

            var totRT = totalWinTxt.GetComponent<RectTransform>();
            totRT.anchorMin        = new Vector2(0, 0);
            totRT.anchorMax        = new Vector2(1, 0.5f);
            totRT.offsetMin        = totRT.offsetMax = Vector2.zero;

            WireField(view, "remainingText", remainText.GetComponent<TMP_Text>());
            WireField(view, "totalWinText",  totalWinTxt.GetComponent<TMP_Text>());

            return (go, view);
        }

        // ─── BonusRound.unity ──────────────────────────────────────────

        private static void BuildBonusRoundScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);

            CreateEventSystem();

            var canvasGO = CreateCanvas("Canvas", RenderMode.ScreenSpaceOverlay);

            var panelGO = new GameObject("BonusRoundPanel", typeof(Image));
            SetParent(panelGO, canvasGO);
            StretchFull(panelGO);
            panelGO.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.15f, 0.95f);

            var view = panelGO.AddComponent<BonusRoundView>();

            // TotalWinText
            var totalWinText = CreateTMPText(panelGO, "TotalWinText", "0", 48);
            var twRT = totalWinText.GetComponent<RectTransform>();
            twRT.anchorMin        = new Vector2(0, 0.1f);
            twRT.anchorMax        = new Vector2(1, 0.25f);
            twRT.offsetMin        = twRT.offsetMax = Vector2.zero;

            // ChestGrid（3×3）
            var gridGO = new GameObject("ChestGrid", typeof(RectTransform));
            SetParent(gridGO, panelGO);
            var gridRT = gridGO.GetComponent<RectTransform>();
            gridRT.anchorMin        = new Vector2(0.5f, 0.5f);
            gridRT.anchorMax        = new Vector2(0.5f, 0.5f);
            gridRT.pivot            = new Vector2(0.5f, 0.5f);
            gridRT.anchoredPosition = Vector2.zero;
            gridRT.sizeDelta        = new Vector2(500, 500);

            var glg = gridGO.AddComponent<GridLayoutGroup>();
            glg.cellSize         = new Vector2(150, 150);
            glg.spacing          = new Vector2(10, 10);
            glg.constraint       = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount  = 3;
            glg.childAlignment   = TextAnchor.MiddleCenter;

            var chestButtons = new Button[9];
            var rewardTexts  = new TMP_Text[9];

            for (int i = 0; i < 9; i++)
            {
                var btnGO = new GameObject($"ChestButton{i}", typeof(Image));
                SetParent(btnGO, gridGO);
                btnGO.GetComponent<Image>().color = new Color(0.6f, 0.4f, 0.1f);

                var btn = btnGO.AddComponent<Button>();
                chestButtons[i] = btn;

                var rewardGO = CreateTMPText(btnGO, "RewardText", "?", 36);
                rewardTexts[i] = rewardGO.GetComponent<TMP_Text>();
                rewardTexts[i].alignment = TextAlignmentOptions.Center;
            }

            // BonusRoundView ワイヤリング
            var so = new SerializedObject(view);
            var chestProp = so.FindProperty("chestButtons");
            chestProp.arraySize = 9;
            for (int i = 0; i < 9; i++)
                chestProp.GetArrayElementAtIndex(i).objectReferenceValue = chestButtons[i];

            var rewardProp = so.FindProperty("rewardTexts");
            rewardProp.arraySize = 9;
            for (int i = 0; i < 9; i++)
                rewardProp.GetArrayElementAtIndex(i).objectReferenceValue = rewardTexts[i];

            so.FindProperty("totalWinText").objectReferenceValue = totalWinText.GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/BonusRound.unity");
            Debug.Log("[SceneBuilder] BonusRound.unity built.");
        }

        // ─── Build Settings ────────────────────────────────────────────

        private static void AddScenesToBuildSettings()
        {
            var paths = new[]
            {
                $"{ScenesPath}/Boot.unity",
                $"{ScenesPath}/Main.unity",
                $"{ScenesPath}/BonusRound.unity",
            };

            var buildScenes = new EditorBuildSettingsScene[paths.Length];
            for (int i = 0; i < paths.Length; i++)
                buildScenes[i] = new EditorBuildSettingsScene(paths[i], true);

            EditorBuildSettings.scenes = buildScenes;
            Debug.Log("[SceneBuilder] Build Settings updated.");
        }

        // ─── ユーティリティ ────────────────────────────────────────────

        private static void EnsureFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static GameObject CreateCanvas(string name, RenderMode renderMode)
        {
            var go     = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = renderMode;

            SetupCanvasScaler(go, 0.5f);
            return go;
        }

        private static void SetupCanvasScaler(GameObject canvasGO, float match)
        {
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            if (scaler == null) return;
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = match;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
        }

        private static void SetParent(GameObject child, GameObject parent)
        {
            child.transform.SetParent(parent.transform, false);
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static void StretchFull(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin  = Vector2.zero;
            rt.anchorMax  = Vector2.one;
            rt.offsetMin  = Vector2.zero;
            rt.offsetMax  = Vector2.zero;
        }

        private static GameObject CreateTMPText(GameObject parent, string name, string text, float size)
        {
            var go  = new GameObject(name, typeof(RectTransform));
            SetParent(go, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            return go;
        }

        private static GameObject CreateButton(GameObject parent, string name, string label)
        {
            var go  = new GameObject(name, typeof(Image));
            SetParent(go, parent);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.4f);
            go.AddComponent<Button>();

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(170, 56);

            var labelGO = CreateTMPText(go, "Label", label, 22);
            StretchFull(labelGO);

            return go;
        }

        private static GameObject CreateSlider(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(Slider));
            SetParent(go, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 30);
            var s = go.GetComponent<Slider>();
            s.minValue = 0;
            s.maxValue = 1;
            s.value    = 1;
            return go;
        }

        private static void PositionHUD(GameObject go, Vector2 anchoredPos)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0, 1f);
            rt.anchorMax        = new Vector2(0, 1f);
            rt.pivot            = new Vector2(0, 1f);
            rt.anchoredPosition = anchoredPos;
        }

        private static void LayoutVertical(GameObject go, int index)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.3f, 0.6f - index * 0.2f);
            rt.anchorMax        = new Vector2(0.7f, 0.75f - index * 0.2f);
            rt.offsetMin        = rt.offsetMax = Vector2.zero;
        }

        /// <summary>SerializedObject 経由で単一 SerializedField をワイヤリングする。</summary>
        private static void WireField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(fieldName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
