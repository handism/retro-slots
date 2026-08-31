using System.Diagnostics;
using NUnit.Framework;
using SlotGame.Data;
using SlotGame.View;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace SlotGame.Tests.EditMode
{
    public class PaytableViewBenchmark
    {
        [Test]
        public void BenchmarkPopulate()
        {
            var go = new GameObject();
            var paytableView = go.AddComponent<PaytableView>();

            var contentRoot = new GameObject("ContentRoot").AddComponent<RectTransform>();
            contentRoot.transform.SetParent(go.transform);

            var field = typeof(PaytableView).GetField("contentRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(paytableView, contentRoot.transform);

            var symbols = new SymbolData[10];
            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i] = new SymbolData
                {
                    type = SymbolType.Normal,
                    symbolName = $"Sym{i}",
                    payouts = new int[] { 10, 20, 30 },
                    sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,1,1), Vector2.zero)
                };
            }

            // warm up
            paytableView.Populate(symbols, null);

            int iterations = 1000;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                paytableView.Populate(symbols, null);
            }
            sw.Stop();

            Debug.Log($"Populate {iterations} times took: {sw.ElapsedMilliseconds} ms");

            Object.DestroyImmediate(go);
        }
    }
}
