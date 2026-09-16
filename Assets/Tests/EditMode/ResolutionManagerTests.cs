using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SlotGame.Utility;

namespace SlotGame.Tests.EditMode
{
    public class ResolutionManagerTests
    {
        private class TestableResolutionManager : ResolutionManager
        {
            public int MockScreenWidth = 1920;
            public int MockScreenHeight = 1080;

            protected override int GetScreenWidth() => MockScreenWidth;
            protected override int GetScreenHeight() => MockScreenHeight;

            public void ForceUpdateLayout()
            {
                // Invoke private UpdateLayout method via reflection
                var method = typeof(ResolutionManager).GetMethod("UpdateLayout", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(this, null);
            }

        }

        private GameObject _go;
        private Camera _camera;
        private TestableResolutionManager _manager;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _camera = _go.AddComponent<Camera>();
            _camera.rect = new Rect(0, 0, 1, 1);
            _manager = _go.AddComponent<TestableResolutionManager>();

            var field = typeof(ResolutionManager).GetField("_camera", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_manager, _camera);
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
        public void UpdateLayout_TargetAspect16x9_RectRemainsUnchanged()
        {
            _manager.MockScreenWidth = 1920;
            _manager.MockScreenHeight = 1080;

            _manager.ForceUpdateLayout();

            Assert.AreEqual(1.0f, _camera.rect.width, 0.01f);
            Assert.AreEqual(1.0f, _camera.rect.height, 0.01f);
            Assert.AreEqual(0.0f, _camera.rect.x, 0.01f);
            Assert.AreEqual(0.0f, _camera.rect.y, 0.01f);
        }

        [Test]
        public void UpdateLayout_TallerScreen4x3_LetterboxingApplied()
        {
            // 4:3 is taller than 16:9, scale will be less than 1 (0.75)
            _manager.MockScreenWidth = 1024;
            _manager.MockScreenHeight = 768;

            _manager.ForceUpdateLayout();

            float expectedScale = (1024f / 768f) / (16f / 9f);

            Assert.AreEqual(1.0f, _camera.rect.width, 0.01f);
            Assert.AreEqual(expectedScale, _camera.rect.height, 0.01f);
            Assert.AreEqual(0.0f, _camera.rect.x, 0.01f);
            Assert.AreEqual((1.0f - expectedScale) / 2.0f, _camera.rect.y, 0.01f);
        }

        [Test]
        public void UpdateLayout_WiderScreen21x9_PillarboxingApplied()
        {
            // 21:9 is wider than 16:9, scale will be > 1
            _manager.MockScreenWidth = 2560;
            _manager.MockScreenHeight = 1080;

            _manager.ForceUpdateLayout();

            float windowAspect = 2560f / 1080f;
            float scale = windowAspect / (16f / 9f);
            float invScale = 1.0f / scale;

            Assert.AreEqual(invScale, _camera.rect.width, 0.01f);
            Assert.AreEqual(1.0f, _camera.rect.height, 0.01f);
            Assert.AreEqual((1.0f - invScale) / 2.0f, _camera.rect.x, 0.01f);
            Assert.AreEqual(0.0f, _camera.rect.y, 0.01f);
        }
    }
}
