using System.IO;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SlotGame.Model;
using SlotGame.Utility;
using UnityEngine.TestTools;

namespace SlotGame.Tests.EditMode
{
    public class SaveDataManagerTests
    {
        private string _tempPath;

        [SetUp]
        public void SetUp()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), $"savedata_test_{System.Guid.NewGuid()}.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempPath))
                File.Delete(_tempPath);
            if (File.Exists(_tempPath + ".bak"))
                File.Delete(_tempPath + ".bak");
            if (Directory.Exists(_tempPath))
                Directory.Delete(_tempPath, true);

            UnityEngine.PlayerPrefs.DeleteKey("SlotGame_DeviceSalt");
        }

        [Test]
        public void Load_FileNotExists_ReturnsDefault()
        {
            var mgr = new SaveDataManager(_tempPath, null);
            var data = mgr.Load();

            Assert.AreEqual(1000, data.coins);
            Assert.AreEqual(10, data.betAmount);
            Assert.AreEqual("1.0", data.saveVersion);
        }

        [Test]
        public void Save_ThenLoad_RoundTrip()
        {
            var mgr = new SaveDataManager(_tempPath, null);
            var save = new SaveData
            {
                coins = 5000,
                betAmount = 50,
                bgmVolume = 0.5f,
            };
            mgr.SaveAsync(save).AsTask().Wait();

            var loaded = mgr.Load();
            Assert.AreEqual(5000, loaded.coins);
            Assert.AreEqual(50, loaded.betAmount);
            Assert.AreEqual(0.5f, loaded.bgmVolume, 0.001f);
        }

        [Test]
        public void SaveAsync_ThenLoad_RoundTrip()
        {
            // Unity Test Runner in EditMode can handle async tests if they return IEnumerator or are run with Task.Run,
            // but for simplicity we can just block on the UniTask for the test.
            var mgr = new SaveDataManager(_tempPath, null);
            var save = new SaveData
            {
                coins = 5000,
                betAmount = 50,
                bgmVolume = 0.5f,
            };
            mgr.SaveAsync(save).AsTask().Wait();

            var loaded = mgr.Load();
            Assert.AreEqual(5000, loaded.coins);
            Assert.AreEqual(50, loaded.betAmount);
            Assert.AreEqual(0.5f, loaded.bgmVolume, 0.001f);
        }

        [Test]
        public void Load_CorruptedJson_ReturnsDefaultAndCreatesBak()
        {
            File.WriteAllText(_tempPath, "{ invalid json !!!");
            var mgr = new SaveDataManager(_tempPath, null);
            var data = mgr.Load();

            Assert.AreEqual(1000, data.coins);
            Assert.IsTrue(File.Exists(_tempPath + ".bak"));
        }

        [Test]
        public void Load_InvalidVersion_ReturnsDefault()
        {
            var bad = new SaveData { saveVersion = "9.9" };
            File.WriteAllText(_tempPath, UnityEngine.JsonUtility.ToJson(bad));
            var mgr = new SaveDataManager(_tempPath, null);
            var data = mgr.Load();

            Assert.AreEqual(1000, data.coins);
        }

        [Test]
        public void Load_CoinsOutOfRange_ReturnsDefault()
        {
            var bad = new SaveData { coins = -100 };
            File.WriteAllText(_tempPath, UnityEngine.JsonUtility.ToJson(bad));
            var mgr = new SaveDataManager(_tempPath, null);
            var data = mgr.Load();

            Assert.AreEqual(1000, data.coins);
        }

        [Test]
        public void Load_TamperedData_ReturnsDefault()
        {
            var mgr = new SaveDataManager(_tempPath, null);
            var save = new SaveData { coins = 5000 };
            mgr.SaveAsync(save).AsTask().Wait();

            // Manual tampering
            string json = File.ReadAllText(_tempPath);
            json = json.Replace("5000", "999999");
            File.WriteAllText(_tempPath, json);

            var loaded = mgr.Load();
            Assert.AreEqual(1000, loaded.coins); // Back to default due to checksum failure
        }

        [Test]
        public void VerifyChecksum_LegacyFallbackSalt_Accepted()
        {
            // Calculate what the checksum WOULD have been using the old hardcoded salt
            var data = new SaveData
            {
                coins = 5000,
                betAmount = 50,
                bgmVolume = 0.5f,
                saveVersion = "1.0",
            };

            string raw =
                $"{data.coins}:{data.betAmount}:{data.bgmVolume:F2}:{data.seVolume:F2}:{data.totalSpins}:{data.totalWins}:{data.maxWin}:{data.totalFreeSpinTriggers}:{data.saveVersion}:SALTY_SLOT_2026";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
            data.checksum = System.Convert.ToBase64String(bytes);

            File.WriteAllText(_tempPath, UnityEngine.JsonUtility.ToJson(data));

            // Load it with a manager that has NO config, which will generate a new device salt
            // but the migration logic should still accept the SALTY_SLOT_2026 checksum
            var mgr = new SaveDataManager(_tempPath, null);
            var loaded = mgr.Load();

            Assert.AreEqual(5000, loaded.coins);
            Assert.AreEqual(50, loaded.betAmount);
            Assert.AreEqual(0.5f, loaded.bgmVolume, 0.001f);
        }

        [Test]
        public void Load_InvalidBetAmount_ReturnsDefault()
        {
            var config = new SlotConfig(
                1000,
                999999,
                new[] { 10, 20, 50, 100 },
                5,
                3,
                3,
                new[] { 0, 2, 4 },
                2,
                20,
                10,
                0.8f,
                1.0f,
                0.8f,
                0.1f,
                1.5f,
                0.3f,
                "TEST_SALT"
            );
            var mgr = new SaveDataManager(_tempPath, config);

            var bad = new SaveData { betAmount = 999 };
            // Manually save valid json but with invalid betAmount (skipping mgr.Save which would add checksum)
            File.WriteAllText(_tempPath, UnityEngine.JsonUtility.ToJson(bad));

            var data = mgr.Load();
            Assert.AreEqual(1000, data.coins);
            Assert.AreEqual(10, data.betAmount);
        }

        [Test]
        public void Load_ExceptionDuringRead_ReturnsDefault()
        {
            File.WriteAllText(_tempPath, "{}");
            var mgr = new SaveDataManager(_tempPath, null);

            // Lock the file exclusively to force an IOException when Load() tries to read it
            using (var stream = new FileStream(_tempPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var data = mgr.Load();

                // The catch block should handle the IOException and return the default SaveData
                Assert.AreEqual(1000, data.coins);
                Assert.AreEqual(10, data.betAmount);
                Assert.AreEqual("1.0", data.saveVersion);
            }
        }

        [Test]
        public void SaveAsync_ExceptionThrown_HandlesErrorAndDeletesTempFile()
        {
            // Create a directory at _tempPath so File.Move throws IOException
            Directory.CreateDirectory(_tempPath);

            var mgr = new SaveDataManager(_tempPath, null);
            var save = new SaveData { coins = 5000 };

            LogAssert.Expect(
                UnityEngine.LogType.Error,
                new System.Text.RegularExpressions.Regex(".*SaveAsync failed.*")
            );
            mgr.SaveAsync(save).AsTask().Wait();

            // temp path is _tempPath + ".tmp". We need _tempPath to be the savePath.
            Assert.IsFalse(File.Exists(_tempPath + ".tmp"));
        }
    }
}
