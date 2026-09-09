using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SlotGame.View;

namespace SlotGame.Tests.EditMode
{
    public class ReelViewTests
    {
        private GameObject _go;
        private ReelView _reelView;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("ReelView");
            _reelView = _go.AddComponent<ReelView>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                UnityEngine.Object.DestroyImmediate(_go);
            }
        }

        [Test]
        public void GetVisibleSymbolIds_ReturnsEmptyArray_WhenSymbolViewsIsNull()
        {
            // Verify GetVisibleSymbolIds handles uninitialized state gracefully
            var symbolIds = _reelView.GetVisibleSymbolIds();

            Assert.IsNotNull(symbolIds);
            Assert.AreEqual(0, symbolIds.Length);
        }

        [Test]
        public void GetVisibleSymbolIds_ReturnsCorrectIds_WhenSymbolViewsIsInitialized()
        {
            // Setup mock SymbolViews array to inject via reflection
            // BufferSize is 5, but we only care about indices 1, 2, 3
            var symbolViews = new SymbolView[5];
            var expectedIds = new[] { 101, 102, 103 };

            for (int i = 0; i < symbolViews.Length; i++)
            {
                var viewGo = new GameObject($"SymbolView_{i}");
                var view = viewGo.AddComponent<SymbolView>();
                view.SetSymbolId(i == 1 ? expectedIds[0] :
                                 i == 2 ? expectedIds[1] :
                                 i == 3 ? expectedIds[2] : 999);
                symbolViews[i] = view;
            }

            // Inject the array into ReelView using Reflection
            var fieldInfo = typeof(ReelView).GetField("_symbolViews", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(_reelView, symbolViews);

            // Act
            var symbolIds = _reelView.GetVisibleSymbolIds();

            // Assert
            Assert.IsNotNull(symbolIds);
            Assert.AreEqual(3, symbolIds.Length);
            Assert.AreEqual(expectedIds[0], symbolIds[0]);
            Assert.AreEqual(expectedIds[1], symbolIds[1]);
            Assert.AreEqual(expectedIds[2], symbolIds[2]);

            // Cleanup mock GameObjects
            foreach (var view in symbolViews)
            {
                if (view != null)
                {
                    UnityEngine.Object.DestroyImmediate(view.gameObject);
                }
            }
        }
    }
}
