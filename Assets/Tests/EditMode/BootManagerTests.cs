using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;
using SlotGame.Core;

namespace SlotGame.Tests.EditMode
{
    public class BootManagerTests
    {
        [Test]
        public void ResolveStartupSceneName_TitleCanBeLoaded_ReturnsTitleScene()
        {
            // Arrange
            System.Func<string, bool> mockCanLoadLevel = (sceneName) => sceneName == "Title";

            // Act
            string result = BootManager.ResolveStartupSceneName(mockCanLoadLevel);

            // Assert
            Assert.AreEqual("Title", result);
        }

        [Test]
        public void ResolveStartupSceneName_TitleFailsMainCanBeLoaded_ReturnsMainSceneAndLogsWarning()
        {
            // Arrange
            System.Func<string, bool> mockCanLoadLevel = (sceneName) => sceneName == "Main";
            LogAssert.Expect(LogType.Warning, new Regex(".*Scene 'Title' is not in the active build profile.*Falling back to 'Main'.*"));

            // Act
            string result = BootManager.ResolveStartupSceneName(mockCanLoadLevel);

            // Assert
            Assert.AreEqual("Main", result);
        }

        [Test]
        public void ResolveStartupSceneName_NeitherCanBeLoaded_ReturnsMainSceneAndLogsError()
        {
            // Arrange
            System.Func<string, bool> mockCanLoadLevel = (sceneName) => false;
            LogAssert.Expect(LogType.Error, new Regex(".*Neither 'Title' nor 'Main' can be loaded.*"));

            // Act
            string result = BootManager.ResolveStartupSceneName(mockCanLoadLevel);

            // Assert
            Assert.AreEqual("Main", result);
        }
    }
}
