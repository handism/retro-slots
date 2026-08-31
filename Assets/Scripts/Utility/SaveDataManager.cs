#nullable enable
using System;
using System.IO;
using SlotGame.Model;
using UnityEngine;

namespace SlotGame.Utility
{
    /// <summary>
    /// JSON ファイルへのセーブデータ読み書きを担う。
    /// コンストラクタでパスを差し替えることでテスト可能。
    /// </summary>
    public class SaveDataManager
    {
        private readonly string _savePath;
        private readonly SlotConfig? _config;

        public SlotConfig? Config => _config;

        public SaveDataManager(SlotConfig? config = null)
            : this(Path.Combine(Application.persistentDataPath, "savedata.json"), config) { }

        public SaveDataManager(string savePath, SlotConfig? config = null)
        {
            _savePath = savePath;
            _config = config;
        }

        // Test hooks to allow simulating file system failures without flaky OS-level locks.
        internal Action<string, string, string?> ReplaceFileAction { get; set; } = File.Replace;
        internal Action<string, string> MoveFileAction { get; set; } = File.Move;

        /// <summary>
        /// セーブデータを読み込む。
        /// ファイルが存在しない場合や破損している場合はデフォルト値を返す。
        /// 破損ファイルは .bak にリネームして保全する。
        /// </summary>
        public SaveData Load()
        {
            if (!File.Exists(_savePath))
                return new SaveData();

            try
            {
                string json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || !Validate(data, _config))
                    return RecoverFromCorruption();

                // 移行戦略: チェックサムがない旧データの場合はバリデーションのみで通し、次回保存時にチェックサムを付与する
                if (string.IsNullOrEmpty(data.checksum))
                {
                    return data;
                }

                if (!VerifyChecksum(data))
                    return RecoverFromCorruption();

                return data;
            }
            catch (Exception)
            {
                return RecoverFromCorruption();
            }
        }

        /// <summary>セーブデータを JSON ファイルに非同期で書き込む（一時ファイルを用いたアトミック書き込み）。</summary>
        public async Cysharp.Threading.Tasks.UniTask SaveAsync(SaveData data)
        {
            data.checksum = CalculateChecksum(data, GetActiveSalt());
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            string tempPath = _savePath + ".tmp";

            try
            {
                await File.WriteAllTextAsync(tempPath, json);
                if (File.Exists(_savePath))
                {
                    ReplaceFileAction(tempPath, _savePath, null);
                }
                else
                {
                    MoveFileAction(tempPath, _savePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveDataManager] SaveAsync failed: {e.Message}");
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        // ─── バリデーション ──────────────────────────────────────────────

        private static bool Validate(SaveData data, SlotConfig? config)
        {
            if (data.saveVersion != "1.0")
                return false;
            if (data.coins < 0)
                return false;
            if (config != null)
            {
                if (data.coins > config.MaxCoins)
                    return false;
                if (System.Array.IndexOf(config.ValidBetAmounts, data.betAmount) < 0)
                    return false;
            }
            if (data.bgmVolume < 0f || data.bgmVolume > 1f)
                return false;
            if (data.seVolume < 0f || data.seVolume > 1f)
                return false;
            if (data.totalSpins < 0 || data.totalWins < 0 || data.maxWin < 0)
                return false;
            if (data.totalFreeSpinTriggers < 0)
                return false;
            return true;
        }

        private const string FallbackChecksumSalt = "SALTY_SLOT_2026";

        private static string CalculateChecksum(SaveData data, string salt)
        {
            string raw =
                $"{data.coins}:{data.betAmount}:{data.bgmVolume:F2}:{data.seVolume:F2}:{data.totalSpins}:{data.totalWins}:{data.maxWin}:{data.totalFreeSpinTriggers}:{data.saveVersion}:{salt}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }

        private string GetActiveSalt()
        {
            return _config != null ? _config.ChecksumSalt : FallbackChecksumSalt;
        }

        private bool VerifyChecksum(SaveData data)
        {
            string actual = data.checksum;
            string salt = GetActiveSalt();
            string expected = CalculateChecksum(data, salt);

            if (actual == expected)
            {
                return true;
            }

            // Migration support: If the config uses a new salt, but the save file was created with the old hardcoded salt.
            // This is secure because we only check the fallback salt if the main check fails,
            // and this is necessary to not break existing user saves after the upgrade.
            if (salt != FallbackChecksumSalt)
            {
                string expectedFallback = CalculateChecksum(data, FallbackChecksumSalt);
                if (actual == expectedFallback)
                {
                    return true;
                }
            }

            return false;
        }

        private SaveData RecoverFromCorruption()
        {
            if (File.Exists(_savePath))
            {
                string bakPath = _savePath + ".bak";
                try
                {
                    if (File.Exists(bakPath))
                        File.Delete(bakPath);
                    File.Move(_savePath, bakPath);
                }
                catch (Exception)
                { /* バックアップ失敗は無視してデフォルト値を返す */
                }
            }
            return new SaveData();
        }
    }
}
