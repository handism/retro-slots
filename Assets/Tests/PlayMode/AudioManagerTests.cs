using System.Collections;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SlotGame.Audio.Tests
{
    public class AudioManagerTests
    {
        private GameObject _gameObject;
        private AudioManager _audioManager;
        private AudioSource _bgmSource;
        private AudioSource _seSource;
        private AudioClip _bgmNormalClip;
        private AudioClip _seSpinStartClip;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("AudioManagerTest");
            _audioManager = _gameObject.AddComponent<AudioManager>();

            _bgmSource = _gameObject.AddComponent<AudioSource>();
            _seSource = _gameObject.AddComponent<AudioSource>();

            // Create dummy audio clips
            _bgmNormalClip = AudioClip.Create("BGM_Normal", 1, 1, 44100, false);
            _seSpinStartClip = AudioClip.Create("SE_SpinStart", 1, 1, 44100, false);

            // Inject via reflection since fields are private serialized
            SetPrivateField(_audioManager, "bgmSource", _bgmSource);
            SetPrivateField(_audioManager, "seSource", _seSource);
            SetPrivateField(_audioManager, "bgmNormal", _bgmNormalClip);
            SetPrivateField(_audioManager, "seSpinStart", _seSpinStartClip);

            // We must also initialize dotween for tests that use it
            DOTween.Init();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameObject);
            Object.DestroyImmediate(_bgmNormalClip);
            Object.DestroyImmediate(_seSpinStartClip);
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field {fieldName} not found on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        [Test]
        public void PlayBGM_ValidType_PlaysCorrectClipAndLoops()
        {
            _audioManager.PlayBGM(BGMType.Normal);

            Assert.IsTrue(_bgmSource.isPlaying);
            Assert.AreEqual(_bgmNormalClip, _bgmSource.clip);
            Assert.IsTrue(_bgmSource.loop);
        }

        [Test]
        public void PlayBGM_InvalidType_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _audioManager.PlayBGM((BGMType)999));
        }

        [Test]
        public void PlaySE_ValidType_PlaysClip()
        {
            // Note: AudioSource.PlayOneShot doesn't set isPlaying or clip synchronously in tests the same way Play() does.
            // But we can verify it doesn't throw.
            Assert.DoesNotThrow(() => _audioManager.PlaySE(SEType.SpinStart));
        }

        [Test]
        public void ToggleMute_MutesAndUnmutesCorrectly()
        {
            _audioManager.SetBGMVolume(0.8f);
            _audioManager.SetSEVolume(1.0f);

            // Initially not muted
            Assert.IsFalse(_audioManager.IsMuted);

            // Toggle to mute
            _audioManager.ToggleMute();

            Assert.IsTrue(_audioManager.IsMuted);
            Assert.AreEqual(0f, _bgmSource.volume);
            Assert.AreEqual(0f, _seSource.volume);

            // Toggle to unmute
            _audioManager.ToggleMute();

            Assert.IsFalse(_audioManager.IsMuted);
            Assert.AreEqual(0.8f, _bgmSource.volume);
            Assert.AreEqual(1.0f, _seSource.volume);
        }

        [Test]
        public void SetBGMVolume_ClampsVolume()
        {
            _audioManager.SetBGMVolume(1.5f);
            Assert.AreEqual(1.0f, _bgmSource.volume);

            _audioManager.SetBGMVolume(-0.5f);
            Assert.AreEqual(0.0f, _bgmSource.volume);
        }

        [Test]
        public void SetSEVolume_ClampsVolume()
        {
            _audioManager.SetSEVolume(1.5f);
            Assert.AreEqual(1.0f, _seSource.volume);

            _audioManager.SetSEVolume(-0.5f);
            Assert.AreEqual(0.0f, _seSource.volume);
        }

        [UnityTest]
        public IEnumerator FadeOutBGM_FadesVolumeToZeroAndStops() => UniTask.ToCoroutine(async () =>
        {
            _audioManager.PlayBGM(BGMType.Normal);
            _bgmSource.volume = 1f;

            await _audioManager.FadeOutBGM(0.1f, CancellationToken.None);

            Assert.IsFalse(_bgmSource.isPlaying);
            // After fade out, it resets the volume back to original startVolume for the next play
            Assert.AreEqual(1f, _bgmSource.volume);
        });

        [UnityTest]
        public IEnumerator CrossFadeBGM_PlaysNewBGMAndFadesIn() => UniTask.ToCoroutine(async () =>
        {
            _audioManager.SetBGMVolume(0.8f);

            await _audioManager.CrossFadeBGM(BGMType.Normal, 0.2f, CancellationToken.None);

            Assert.IsTrue(_bgmSource.isPlaying);
            Assert.AreEqual(_bgmNormalClip, _bgmSource.clip);
            Assert.AreEqual(0.8f, _bgmSource.volume);
        });
    }
}
