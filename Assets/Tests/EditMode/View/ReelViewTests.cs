using NUnit.Framework;
using UnityEngine;
using SlotGame.View;
using SlotGame.Data;
using System.Collections.Generic;
using System.Reflection;

namespace SlotGame.Tests.EditMode.View
{
    public class ReelViewTests
    {
        private GameObject _go;
        private ReelView _reelView;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("ReelViewTest", typeof(RectTransform));
            _reelView = _go.AddComponent<ReelView>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }
        }

        [Test]
        public void Initialize_InstantiatesSymbolViewsAndSetsUpState()
        {
            // 1. Setup SymbolView Prefab
            var prefabGo = new GameObject("SymbolViewPrefab", typeof(RectTransform));
            var symbolViewPrefab = prefabGo.AddComponent<SymbolView>();
            prefabGo.AddComponent<UnityEngine.UI.Image>(); // Required by SymbolView
            prefabGo.AddComponent<Animator>();

            // 2. Setup ReelStripData
            var stripData = ScriptableObject.CreateInstance<ReelStripData>();
            stripData.strip = new List<SymbolData>();
            for (int i = 0; i < 5; i++)
            {
                var symbol = ScriptableObject.CreateInstance<SymbolData>();
                symbol.symbolId = i;
                stripData.strip.Add(symbol);
            }

            // 3. Inject prefab using reflection
            var prefabField = typeof(ReelView).GetField("symbolViewPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(prefabField, "symbolViewPrefab field should exist.");
            prefabField.SetValue(_reelView, symbolViewPrefab);

            // 4. Call Initialize
            _reelView.Initialize(stripData);

            // 5. Verify _symbolViews and _symbolRects initialization
            var viewsField = typeof(ReelView).GetField("_symbolViews", BindingFlags.NonPublic | BindingFlags.Instance);
            var rectsField = typeof(ReelView).GetField("_symbolRects", BindingFlags.NonPublic | BindingFlags.Instance);

            var views = (SymbolView[])viewsField.GetValue(_reelView);
            var rects = (RectTransform[])rectsField.GetValue(_reelView);

            Assert.IsNotNull(views, "_symbolViews should be initialized.");
            Assert.IsNotNull(rects, "_symbolRects should be initialized.");
            Assert.AreEqual(5, views.Length, "_symbolViews length should be 5 (BufferSize).");
            Assert.AreEqual(5, rects.Length, "_symbolRects length should be 5.");

            for (int i = 0; i < views.Length; i++)
            {
                Assert.IsNotNull(views[i], $"_symbolViews[{i}] should not be null.");
                Assert.AreSame(_reelView.transform, views[i].transform.parent, $"_symbolViews[{i}] should be parented to ReelView.");
                Assert.IsNotNull(rects[i], $"_symbolRects[{i}] should not be null.");
            }

            // Verify the symbol positions are set correctly (based on GetSymbolYPosition logic)
            // BufferSize = 5, symbolHeight is default 180f
            // GetSymbolYPosition(i) = ( (5-1)*0.5 - i ) * 180f = (2 - i) * 180f
            // i=0: 360, i=1: 180, i=2: 0, i=3: -180, i=4: -360
            Assert.AreEqual(360f, rects[0].anchoredPosition.y);
            Assert.AreEqual(180f, rects[1].anchoredPosition.y);
            Assert.AreEqual(0f, rects[2].anchoredPosition.y);
            Assert.AreEqual(-180f, rects[3].anchoredPosition.y);
            Assert.AreEqual(-360f, rects[4].anchoredPosition.y);

            // Verify _stripIndex and initial symbols
            var stripIndexField = typeof(ReelView).GetField("_stripIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            int stripIndex = (int)stripIndexField.GetValue(_reelView);
            Assert.AreEqual(0, stripIndex, "_stripIndex should be 0 after initialization.");

            for (int i = 0; i < views.Length; i++)
            {
                Assert.AreEqual(i, views[i].SymbolId, $"_symbolViews[{i}] should have SymbolId {i} based on strip data.");
            }

            // Clean up instances
            Object.DestroyImmediate(prefabGo);
            Object.DestroyImmediate(stripData);
            foreach (var symbol in stripData.strip)
            {
                Object.DestroyImmediate(symbol);
            }
        }
    }
}
