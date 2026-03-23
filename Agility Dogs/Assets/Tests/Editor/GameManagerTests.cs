using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AgilityDogs.Services;
using AgilityDogs.Core;
using AgilityDogs.Events;
using System.Collections;

namespace AgilityDogs.Tests.Editor
{
    public class GameManagerTests
    {
        private GameManager gameManager;
        private GameObject gameManagerObject;

        [SetUp]
        public void SetUp()
        {
            // Create a new GameManager for each test
            gameManagerObject = new GameObject("TestGameManager");
            gameManager = gameManagerObject.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            if (gameManagerObject != null)
            {
                Object.DestroyImmediate(gameManagerObject);
            }
        }

        [Test]
        public void GameManager_Singleton_WorksCorrectly()
        {
            // Arrange
            var secondGameManagerObject = new GameObject("SecondGameManager");
            var secondGameManager = secondGameManagerObject.AddComponent<GameManager>();

            // Act & Assert - First instance should be set as singleton
            Assert.IsNotNull(GameManager.Instance);
            Assert.AreEqual(gameManager, GameManager.Instance);

            // Second instance should be destroyed
            Assert.IsNull(secondGameManager);

            // Cleanup
            Object.DestroyImmediate(secondGameManagerObject);
        }

        [Test]
        public void InitialState_IsMainMenu()
        {
            // Assert
            Assert.AreEqual(GameState.MainMenu, gameManager.CurrentState);
        }

        [Test]
        public void SetState_ChangesStateCorrectly()
        {
            // Arrange
            bool stateChanged = false;
            GameState fromState = GameState.MainMenu;
            GameState toState = GameState.MainMenu;
            
            GameEvents.OnGameStateChanged += (from, to) =>
            {
                stateChanged = true;
                fromState = from;
                toState = to;
            };

            // Act
            gameManager.SetState(GameState.Countdown);

            // Assert
            Assert.AreEqual(GameState.Countdown, gameManager.CurrentState);
            Assert.IsTrue(stateChanged);
            Assert.AreEqual(GameState.MainMenu, fromState);
            Assert.AreEqual(GameState.Countdown, toState);
        }

        [Test]
        public void SetState_SameState_DoesNotTriggerEvent()
        {
            // Arrange
            bool stateChanged = false;
            GameEvents.OnGameStateChanged += (from, to) => stateChanged = true;

            // Act
            gameManager.SetState(GameState.MainMenu); // Same as initial state

            // Assert
            Assert.IsFalse(stateChanged);
        }

        [Test]
        public void StartCountdown_SetsStateToCountdown()
        {
            // Act
            gameManager.StartCountdown();

            // Assert
            Assert.AreEqual(GameState.Countdown, gameManager.CurrentState);
        }

        [Test]
        public void BeginRun_SetsStateToGameplayAndRaisesEvent()
        {
            // Arrange
            bool runStarted = false;
            GameEvents.OnRunStarted += () => runStarted = true;

            // Act
            gameManager.BeginRun();

            // Assert
            Assert.AreEqual(GameState.Gameplay, gameManager.CurrentState);
            Assert.IsTrue(runStarted);
        }

        [Test]
        public void CompleteRun_SetsStateAndRaisesEvent()
        {
            // Arrange
            RunResult result = RunResult.Qualified;
            float time = 45.5f;
            int faults = 0;
            bool runCompleted = false;
            RunResult receivedResult = RunResult.NonQualified;
            float receivedTime = 0f;
            int receivedFaults = -1;

            GameEvents.OnRunCompleted += (r, t, f) =>
            {
                runCompleted = true;
                receivedResult = r;
                receivedTime = t;
                receivedFaults = f;
            };

            // Act
            gameManager.CompleteRun(result, time, faults);

            // Assert
            Assert.AreEqual(GameState.RunComplete, gameManager.CurrentState);
            Assert.IsTrue(runCompleted);
            Assert.AreEqual(result, receivedResult);
            Assert.AreEqual(time, receivedTime);
            Assert.AreEqual(faults, receivedFaults);
        }

        [Test]
        public void ShowResults_SetsStateToResults()
        {
            // Act
            gameManager.ShowResults();

            // Assert
            Assert.AreEqual(GameState.Results, gameManager.CurrentState);
        }

        [Test]
        public void ReturnToMenu_SetsStateToMainMenu()
        {
            // Arrange - First set to a different state
            gameManager.SetState(GameState.Gameplay);

            // Act
            gameManager.ReturnToMenu();

            // Assert
            Assert.AreEqual(GameState.MainMenu, gameManager.CurrentState);
        }

        [Test]
        public void StartGame_CallsStartCountdown()
        {
            // Act
            gameManager.StartGame();

            // Assert
            Assert.AreEqual(GameState.Countdown, gameManager.CurrentState);
        }

        [Test]
        public void StartReplay_SetsStateToReplay()
        {
            // Act
            gameManager.StartReplay();

            // Assert
            Assert.AreEqual(GameState.Replay, gameManager.CurrentState);
        }

        [Test]
        public void RestartGame_CallsBeginRun()
        {
            // Arrange
            bool runStarted = false;
            GameEvents.OnRunStarted += () => runStarted = true;

            // Act
            gameManager.RestartGame();

            // Assert
            Assert.AreEqual(GameState.Gameplay, gameManager.CurrentState);
            Assert.IsTrue(runStarted);
        }

        [Test]
        public void ResumeGame_SetsStateToGameplay()
        {
            // Arrange - First set to a pause state
            gameManager.SetState(GameState.Countdown);

            // Act
            gameManager.ResumeGame();

            // Assert
            Assert.AreEqual(GameState.Gameplay, gameManager.CurrentState);
        }

        [Test]
        public void QuitToMenu_SetsStateToMainMenu()
        {
            // Arrange - First set to a different state
            gameManager.SetState(GameState.Gameplay);

            // Act
            gameManager.QuitToMenu();

            // Assert
            Assert.AreEqual(GameState.MainMenu, gameManager.CurrentState);
        }
    }
}