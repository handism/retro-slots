using NUnit.Framework;
using UnityEngine;
using SlotGame.Core;

namespace SlotGame.Tests.EditMode
{
    [TestFixture]
    public class BootManagerTests
    {
        [Test]
        public void ResolveStartupSceneName_TitleCanBeLoaded_ReturnsTitleSceneName()
        {
            // Arrange
            System.Func<string, bool> mockCanStreamedLevelBeLoaded = name => name == "Title" || name == "Main";

            // Act
            var result = BootManager.ResolveStartupSceneName(mockCanStreamedLevelBeLoaded);

            // Assert
            Assert.AreEqual("Title", result);
        }

        [Test]
        public void ResolveStartupSceneName_OnlyMainCanBeLoaded_ReturnsMainSceneName()
        {
            // Arrange
            System.Func<string, bool> mockCanStreamedLevelBeLoaded = name => name == "Main";

            // Act
            var result = BootManager.ResolveStartupSceneName(mockCanStreamedLevelBeLoaded);

            // Assert
            Assert.AreEqual("Main", result);
        }

        [Test]
        public void ResolveStartupSceneName_NeitherCanBeLoaded_ReturnsMainSceneName()
        {
            // Arrange
            System.Func<string, bool> mockCanStreamedLevelBeLoaded = name => false;

            // Act
            var result = BootManager.ResolveStartupSceneName(mockCanStreamedLevelBeLoaded);

            // Assert
            Assert.AreEqual("Main", result);
        }
    }
}
