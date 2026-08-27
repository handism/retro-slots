using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SlotGame.Core;
using SlotGame.Utility;
using SlotGame.Data;
using Cysharp.Threading.Tasks;
using SlotGame.Model;
using System.Reflection;
using SlotGame.View;

namespace SlotGame.Tests.PlayMode
{
    public class SpinManagerTests
    {
        private class MockRandom : IRandomGenerator
        {
            public int[] Values;
            public int Index;
            public int Next(int min, int max)
            {
                if (Values != null && Values.Length > 0)
                {
                    return Values[Index++ % Values.Length];
                }
                return min;
            }
            public float NextFloat() => 0.5f;
        }

        private SpinManager _spinManager;
        private GameObject _spinManagerGo;
        private GameState _gameState;
        private SaveDataManager _saveDataManager;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameContextInitializer.Instance != null)
                UnityEngine.Object.Destroy(GameContextInitializer.Instance.gameObject);

            yield return null;

            var goInit = new GameObject("[GameContextInitializer]");
            goInit.AddComponent<GameContextInitializer>();

            _gameState = new GameState(1000, 9999999, new[] { 10, 20, 50, 100 }, 1000, 10);
            _saveDataManager = new SaveDataManager();

            GameContextInitializer.Instance.Provide(
                _gameState,
                _saveDataManager,
                new MockRandom(),
                new SaveData { coins = 1000, betAmount = 10 }
            );

            _spinManagerGo = new GameObject("SpinManager");
            _spinManager = _spinManagerGo.AddComponent<SpinManager>();

            var reelsField = typeof(SpinManager).GetField("reels", BindingFlags.NonPublic | BindingFlags.Instance);
            var reels = new ReelController[5];
            for (int i = 0; i < 5; i++)
            {
                var reelGo = new GameObject($"Reel_{i}");
                reelGo.transform.position = new Vector3(i, 0, 0);
                var reelController = reelGo.AddComponent<ReelController>();
                reels[i] = reelController;
            }
            reelsField.SetValue(_spinManager, reels);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_spinManagerGo != null)
                UnityEngine.Object.Destroy(_spinManagerGo);

            if (GameContextInitializer.Instance != null)
                UnityEngine.Object.Destroy(GameContextInitializer.Instance.gameObject);

            yield return null;
        }

        [Test]
        public void Initialize_SortsReelsBasedOnXPosition()
        {
            var random = new MockRandom();
            var strips = new ReelStripData[5];
            for (int i = 0; i < 5; i++)
            {
                strips[i] = ScriptableObject.CreateInstance<ReelStripData>();
                strips[i].strip = new List<SymbolData>();
            }

            var reels = _spinManager.Reels;
            reels[0].transform.position = new Vector3(4, 0, 0);
            reels[4].transform.position = new Vector3(0, 0, 0);

            _spinManager.Initialize(random, strips);

            Assert.AreEqual(0, _spinManager.Reels[0].transform.position.x);
            Assert.AreEqual(4, _spinManager.Reels[4].transform.position.x);
        }

        [UnityTest]
        public IEnumerator ExecuteSpin_NormalMode_DurationIsAtLeastNormalSpinDuration() => UniTask.ToCoroutine(async () =>
        {
            var random = new MockRandom { Values = new[] { 0 } };
            var strips = CreateDummyStrips();
            _spinManager.Initialize(random, strips);
            GameContext.GameState.SetTurbo(false);

            var paylines = CreateDummyPaylines();
            var payouts = CreateDummyPayouts();

            var startTime = Time.realtimeSinceStartup;
            AddDummyViewsToReels();

            var result = await _spinManager.ExecuteSpin(strips, paylines, payouts, 10, CancellationToken.None);

            var duration = Time.realtimeSinceStartup - startTime;
            Assert.GreaterOrEqual(duration, 2.0f);
            Assert.IsNotNull(result);
        });

        [UnityTest]
        public IEnumerator ExecuteSpin_TurboMode_DurationIsShorter() => UniTask.ToCoroutine(async () =>
        {
            var random = new MockRandom { Values = new[] { 0 } };
            var strips = CreateDummyStrips();
            _spinManager.Initialize(random, strips);
            GameContext.GameState.SetTurbo(true);

            var paylines = CreateDummyPaylines();
            var payouts = CreateDummyPayouts();

            var startTime = Time.realtimeSinceStartup;
            AddDummyViewsToReels();

            var result = await _spinManager.ExecuteSpin(strips, paylines, payouts, 10, CancellationToken.None);

            var duration = Time.realtimeSinceStartup - startTime;
            Assert.Less(duration, 2.0f);
            Assert.GreaterOrEqual(duration, 0.5f);
            Assert.IsNotNull(result);
        });

        [UnityTest]
        public IEnumerator ExecuteSpin_WithSkipRequested_StopsEarlyAndSnaps() => UniTask.ToCoroutine(async () =>
        {
            var random = new MockRandom { Values = new[] { 0 } };
            var strips = CreateDummyStrips();
            _spinManager.Initialize(random, strips);
            GameContext.GameState.SetTurbo(false);

            var paylines = CreateDummyPaylines();
            var payouts = CreateDummyPayouts();

            AddDummyViewsToReels();

            var startTime = Time.realtimeSinceStartup;

            var spinTask = _spinManager.ExecuteSpin(strips, paylines, payouts, 10, CancellationToken.None);

            // Request skip slightly after starting
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            _spinManager.RequestSkip();

            var result = await spinTask;
            var duration = Time.realtimeSinceStartup - startTime;

            // Duration should be roughly 0.5s instead of >2.0s
            Assert.Less(duration, 1.5f);
            Assert.IsNotNull(result);
        });

        [UnityTest]
        public IEnumerator ExecuteSpin_WhenCancelled_ThrowsOperationCanceledException() => UniTask.ToCoroutine(async () =>
        {
            var random = new MockRandom { Values = new[] { 0 } };
            var strips = CreateDummyStrips();
            _spinManager.Initialize(random, strips);

            var paylines = CreateDummyPaylines();
            var payouts = CreateDummyPayouts();

            AddDummyViewsToReels();

            var cts = new CancellationTokenSource();

            var spinTask = _spinManager.ExecuteSpin(strips, paylines, payouts, 10, cts.Token);

            // Cancel shortly after starting
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            cts.Cancel();

            bool threwException = false;
            try
            {
                await spinTask;
            }
            catch (OperationCanceledException)
            {
                threwException = true;
            }

            Assert.IsTrue(threwException, "Expected OperationCanceledException when CancellationToken is canceled.");
        });

        [UnityTest]
        public IEnumerator CollectSymbolDefs_CachesCorrectlyAndEvaluates() => UniTask.ToCoroutine(async () =>
        {
            var random = new MockRandom { Values = new[] { 0 } };
            var strips = CreateDummyStrips();
            // Modify one strip to ensure specific symbol is there
            strips[0].strip[0].symbolId = 99;
            _spinManager.Initialize(random, strips);
            GameContext.GameState.SetTurbo(true); // fast

            var paylines = CreateDummyPaylines();
            var payouts = CreateDummyPayouts();

            AddDummyViewsToReels();

            // First run, _cachedSymbolDefs is null, it should be populated
            await _spinManager.ExecuteSpin(strips, paylines, payouts, 10, CancellationToken.None);

            var cacheField = typeof(SpinManager).GetField("_cachedSymbolDefs", BindingFlags.NonPublic | BindingFlags.Instance);
            var cache = cacheField.GetValue(_spinManager) as IReadOnlyDictionary<int, SymbolData>;

            Assert.IsNotNull(cache);
            Assert.IsTrue(cache.ContainsKey(99), "The cache should contain the symbol ID collected from strips.");
        });

        private PaylineData CreateDummyPaylines()
        {
            var paylines = ScriptableObject.CreateInstance<PaylineData>();
            paylines.lines = Array.Empty<PaylineEntry>();
            return paylines;
        }

        private PayoutTableData CreateDummyPayouts()
        {
            var payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            payouts.scatterPayouts = Array.Empty<ScatterPayout>();
            payouts.freeSpinRewards = Array.Empty<FreeSpinReward>();
            payouts.bonusRewards = Array.Empty<BonusRewardEntry>();
            return payouts;
        }

        private ReelStripData[] CreateDummyStrips()
        {
            var strips = new ReelStripData[5];
            for (int i = 0; i < 5; i++)
            {
                strips[i] = ScriptableObject.CreateInstance<ReelStripData>();
                strips[i].strip = new List<SymbolData>();
                for (int j = 0; j < 5; j++)
                {
                    var sym = ScriptableObject.CreateInstance<SymbolData>();
                    sym.symbolId = j;
                    strips[i].strip.Add(sym);
                }
            }
            return strips;
        }

        private void AddDummyViewsToReels()
        {
            foreach (var reel in _spinManager.Reels)
            {
                var view = reel.gameObject.AddComponent<ReelView>();
                var symbolViewsField = typeof(ReelView).GetField("_symbolViews", BindingFlags.NonPublic | BindingFlags.Instance);
                var mockViews = new SymbolView[5];
                for (int j = 0; j < 5; j++)
                {
                    var svGo = new GameObject("SV");
                    var symView = svGo.AddComponent<SymbolView>();
                    // Needs to return a symbol id when queried
                    var symField = typeof(SymbolView).GetField("_currentSymbol", BindingFlags.NonPublic | BindingFlags.Instance);
                    var dummySym = ScriptableObject.CreateInstance<SymbolData>();
                    dummySym.symbolId = j;
                    symField.SetValue(symView, dummySym);
                    mockViews[j] = symView;
                }
                symbolViewsField.SetValue(view, mockViews);

                var rectsField = typeof(ReelView).GetField("_symbolRects", BindingFlags.NonPublic | BindingFlags.Instance);
                var mockRects = new RectTransform[5];
                for (int j = 0; j < 5; j++)
                {
                    mockRects[j] = mockViews[j].gameObject.AddComponent<RectTransform>();
                }
                rectsField.SetValue(view, mockRects);

                var stripField = typeof(ReelView).GetField("_strip", BindingFlags.NonPublic | BindingFlags.Instance);
                var dummyStrip = ScriptableObject.CreateInstance<ReelStripData>();
                dummyStrip.strip = new List<SymbolData>();
                for (int i = 0; i < 10; i++) dummyStrip.strip.Add(ScriptableObject.CreateInstance<SymbolData>());
                stripField.SetValue(view, dummyStrip);
            }
        }
    }
}
