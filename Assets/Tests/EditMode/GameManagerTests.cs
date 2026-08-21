using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SlotGame.Core;
using SlotGame.View;

namespace SlotGame.Tests.EditMode
{
    public class GameManagerTests
    {
        private GameManager _gameManager;
        private GameObject _gameObject;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("GameManager");
            _gameManager = _gameObject.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        private object InvokeCalcWinLevel(long amount, int betAmount)
        {
            MethodInfo method = typeof(GameManager).GetMethod("CalcWinLevel", BindingFlags.NonPublic | BindingFlags.Static);
            return method.Invoke(null, new object[] { amount, betAmount });
        }

        private bool InvokeCanTransitionTo(GamePhase nextPhase)
        {
            MethodInfo method = typeof(GameManager).GetMethod("CanTransitionTo", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)method.Invoke(_gameManager, new object[] { nextPhase });
        }

        private void InvokeTransitionTo(GamePhase nextPhase)
        {
            MethodInfo method = typeof(GameManager).GetMethod("TransitionTo", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_gameManager, new object[] { nextPhase });
        }

        private GamePhase GetCurrentPhase()
        {
            FieldInfo field = typeof(GameManager).GetField("_currentPhase", BindingFlags.NonPublic | BindingFlags.Instance);
            return (GamePhase)field.GetValue(_gameManager);
        }

        private void SetCurrentPhase(GamePhase phase)
        {
            FieldInfo field = typeof(GameManager).GetField("_currentPhase", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_gameManager, phase);
        }

        // --- CalcWinLevel Tests ---

        [Test]
        public void CalcWinLevel_BetAmountZeroOrLess_ReturnsSmall()
        {
            Assert.AreEqual(WinLevel.Small, InvokeCalcWinLevel(100, 0));
            Assert.AreEqual(WinLevel.Small, InvokeCalcWinLevel(100, -10));
        }

        [Test]
        public void CalcWinLevel_MultiplierLessThan15_ReturnsSmall()
        {
            Assert.AreEqual(WinLevel.Small, InvokeCalcWinLevel(140, 10)); // Multiplier 14
        }

        [Test]
        public void CalcWinLevel_Multiplier15To29_ReturnsBig()
        {
            Assert.AreEqual(WinLevel.Big, InvokeCalcWinLevel(150, 10)); // Multiplier 15
            Assert.AreEqual(WinLevel.Big, InvokeCalcWinLevel(290, 10)); // Multiplier 29
        }

        [Test]
        public void CalcWinLevel_Multiplier30To49_ReturnsMega()
        {
            Assert.AreEqual(WinLevel.Mega, InvokeCalcWinLevel(300, 10)); // Multiplier 30
            Assert.AreEqual(WinLevel.Mega, InvokeCalcWinLevel(490, 10)); // Multiplier 49
        }

        [Test]
        public void CalcWinLevel_Multiplier50OrGreater_ReturnsEpic()
        {
            Assert.AreEqual(WinLevel.Epic, InvokeCalcWinLevel(500, 10)); // Multiplier 50
            Assert.AreEqual(WinLevel.Epic, InvokeCalcWinLevel(1000, 10)); // Multiplier 100
        }

        // --- State Machine Tests ---

        [Test]
        public void TransitionTo_ValidTransition_ChangesPhase()
        {
            SetCurrentPhase(GamePhase.Idle);
            InvokeTransitionTo(GamePhase.Spinning);
            Assert.AreEqual(GamePhase.Spinning, GetCurrentPhase());
        }

        [Test]
        public void TransitionTo_InvalidTransition_DoesNotChangePhase()
        {
            SetCurrentPhase(GamePhase.Idle);
            // Idle to Evaluating is invalid
            InvokeTransitionTo(GamePhase.Evaluating);
            Assert.AreEqual(GamePhase.Idle, GetCurrentPhase());
        }

        // --- CanTransitionTo Valid Cases ---

        [Test]
        public void CanTransitionTo_Idle_To_Spinning_Or_GameOver_IsTrue()
        {
            SetCurrentPhase(GamePhase.Idle);
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.Spinning));
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.GameOver));
        }

        [Test]
        public void CanTransitionTo_Spinning_To_Evaluating_Or_Idle_IsTrue()
        {
            SetCurrentPhase(GamePhase.Spinning);
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.Evaluating));
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.Idle));
        }

        [Test]
        public void CanTransitionTo_Evaluating_To_Win_Bonus_FreeSpin_Idle_IsTrue()
        {
            SetCurrentPhase(GamePhase.Evaluating);
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.WinPresentation));
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.BonusRound));
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.FreeSpin));
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.Idle));
        }

        [Test]
        public void CanTransitionTo_WinPresentation_To_Bonus_FreeSpin_Idle_IsTrue()
        {
            SetCurrentPhase(GamePhase.WinPresentation);
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.BonusRound));
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.FreeSpin));
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.Idle));
        }

        [Test]
        public void CanTransitionTo_BonusRound_To_FreeSpin_Idle_IsTrue()
        {
            SetCurrentPhase(GamePhase.BonusRound);
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.FreeSpin));
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.Idle));
        }

        [TestCase(GamePhase.FreeSpin)]
        [TestCase(GamePhase.GameOver)]
        public void CanTransitionTo_TerminalStates_To_Idle_IsTrue(GamePhase phase)
        {
            SetCurrentPhase(phase);
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.Idle));
        }

        // --- CanTransitionTo Invalid Cases ---

        [Test]
        public void CanTransitionTo_Idle_To_Invalid_IsFalse()
        {
            SetCurrentPhase(GamePhase.Idle);
            Assert.IsFalse(InvokeCanTransitionTo(GamePhase.WinPresentation));
        }

        [Test]
        public void CanTransitionTo_SamePhase_IsFalse_ExceptIdle()
        {
            // Transitioning to same state is generally false
            SetCurrentPhase(GamePhase.Spinning);
            Assert.IsFalse(InvokeCanTransitionTo(GamePhase.Spinning));

            SetCurrentPhase(GamePhase.Evaluating);
            Assert.IsFalse(InvokeCanTransitionTo(GamePhase.Evaluating));

            // Except Idle -> Idle is true
            SetCurrentPhase(GamePhase.Idle);
            Assert.IsTrue(InvokeCanTransitionTo(GamePhase.Idle));
        }
    }
}
