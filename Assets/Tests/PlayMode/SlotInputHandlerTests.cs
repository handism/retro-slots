using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using SlotGame.Core;
using SlotGame.View;
using SlotGame.Audio;
using SlotGame.Data;

namespace SlotGame.Tests.PlayMode
{
    public class SlotInputHandlerTests : InputTestFixture
    {
        private GameObject _go;
        private SlotInputHandler _inputHandler;
        private PlayerInput _playerInput;
        private GameManager _gameManager;
        private InputActionAsset _actionAsset;
        private Gamepad _gamepad;

        public override void Setup()
        {
            base.Setup();

            _gamepad = InputSystem.AddDevice<Gamepad>();

            _go = new GameObject("TestGameObject");
            _gameManager = _go.AddComponent<GameManager>();
            _playerInput = _go.AddComponent<PlayerInput>();

            _actionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = _actionAsset.AddActionMap("Slot");

            actionMap.AddAction("Spin", type: InputActionType.Button, binding: "<Gamepad>/buttonSouth");
            actionMap.AddAction("BetUp", type: InputActionType.Button, binding: "<Gamepad>/dpad/up");
            actionMap.AddAction("BetDown", type: InputActionType.Button, binding: "<Gamepad>/dpad/down");
            actionMap.AddAction("AutoSpin", type: InputActionType.Button, binding: "<Gamepad>/buttonNorth");
            actionMap.AddAction("Skip", type: InputActionType.Button, binding: "<Gamepad>/buttonEast");
            actionMap.AddAction("Mute", type: InputActionType.Button, binding: "<Gamepad>/select");
            actionMap.AddAction("Turbo", type: InputActionType.Button, binding: "<Gamepad>/buttonWest");
            actionMap.AddAction("Paytable", type: InputActionType.Button, binding: "<Gamepad>/start");

            _playerInput.actions = _actionAsset;
            _inputHandler = _go.AddComponent<SlotInputHandler>();

            typeof(SlotInputHandler).GetField("gameManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_inputHandler, _gameManager);

            // Mock minimal dependencies
            var uiGo = new GameObject("UIManager");
            var uiManager = uiGo.AddComponent<UIManager>();
            var audioGo = new GameObject("AudioManager");
            var audioManager = audioGo.AddComponent<AudioManager>();

            typeof(GameManager).GetField("uiManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, uiManager);
            typeof(GameManager).GetField("audioManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, audioManager);

            // Set GamePhase to Idle to allow state transitions
            typeof(GameManager).GetField("_currentPhase", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, GamePhase.Idle);

            var gameState = new GameState();
            typeof(GameManager).GetField("_gameState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_gameManager, gameState);
        }

        public override void TearDown()
        {
            if (_go != null) UnityEngine.Object.DestroyImmediate(_go);
            if (_actionAsset != null) UnityEngine.Object.DestroyImmediate(_actionAsset);
            base.TearDown();
        }

        [Test]
        public void Awake_ActionMapNotFound_LogsWarning()
        {
            var go2 = new GameObject();
            var pi2 = go2.AddComponent<PlayerInput>();
            var emptyAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            pi2.actions = emptyAsset;

            LogAssert.Expect(LogType.Warning, "[SlotInputHandler] 'Slot' action map not found.");
            go2.AddComponent<SlotInputHandler>();

            UnityEngine.Object.DestroyImmediate(go2);
            UnityEngine.Object.DestroyImmediate(emptyAsset);
        }

        [UnityTest]
        public IEnumerator InputActions_Paytable_InvokesTogglePaytable()
        {
            _go.SetActive(false);
            _go.SetActive(true);
            yield return null;

            _playerInput.actions.FindActionMap("Slot").Enable();

            bool initialPaytableState = (bool)typeof(GameManager).GetField("_isPaytableOpen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_gameManager);
            Assert.IsFalse(initialPaytableState, "Paytable should be initially closed");

            Press(_gamepad.startButton);
            yield return null;

            bool newPaytableState = (bool)typeof(GameManager).GetField("_isPaytableOpen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_gameManager);
            Assert.IsTrue(newPaytableState, "Paytable should be opened after pressing Paytable button");
        }

        [UnityTest]
        public IEnumerator InputActions_Turbo_InvokesToggleTurbo()
        {
            _go.SetActive(false);
            _go.SetActive(true);
            yield return null;

            _playerInput.actions.FindActionMap("Slot").Enable();

            var gameState = (GameState)typeof(GameManager).GetField("_gameState", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_gameManager);
            bool initialTurbo = gameState.IsTurbo;
            Assert.IsFalse(initialTurbo, "Turbo should be initially disabled");

            Press(_gamepad.buttonWest);
            yield return null;

            bool newTurbo = gameState.IsTurbo;
            Assert.IsTrue(newTurbo, "Turbo should be enabled after pressing Turbo button");
        }

        [UnityTest]
        public IEnumerator OnDisable_UnbindsActions()
        {
            _go.SetActive(false);
            _go.SetActive(true);
            yield return null;

            _playerInput.actions.FindActionMap("Slot").Enable();

            _go.SetActive(false);
            yield return null;

            // Pressing Paytable button should not change the state since it's disabled
            Press(_gamepad.startButton);
            yield return null;

            bool isPaytableOpen = (bool)typeof(GameManager).GetField("_isPaytableOpen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_gameManager);
            Assert.IsFalse(isPaytableOpen, "Paytable should remain closed because actions are unbound");
        }
    }
}
