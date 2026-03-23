using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AgilityDogs.Core;
using AgilityDogs.Events;
using System.Collections;

namespace AgilityDogs.Tests.Editor
{
    public class CoreComponentTests
    {
        [Test]
        public void GameEvents_StateChange_WorksCorrectly()
        {
            // Arrange
            bool eventRaised = false;
            GameState fromState = GameState.MainMenu;
            GameState toState = GameState.MainMenu;

            GameEvents.OnGameStateChanged += (from, to) =>
            {
                eventRaised = true;
                fromState = from;
                toState = to;
            };

            // Act
            GameEvents.RaiseGameStateChanged(GameState.MainMenu, GameState.Countdown);

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.AreEqual(GameState.MainMenu, fromState);
            Assert.AreEqual(GameState.Countdown, toState);
        }

        [Test]
        public void GameEvents_RunStarted_WorksCorrectly()
        {
            // Arrange
            bool runStarted = false;
            GameEvents.OnRunStarted += () => runStarted = true;

            // Act
            GameEvents.RaiseRunStarted();

            // Assert
            Assert.IsTrue(runStarted);
        }

        [Test]
        public void GameEvents_RunCompleted_WorksCorrectly()
        {
            // Arrange
            bool runCompleted = false;
            RunResult receivedResult = RunResult.NonQualified;
            float receivedTime = 0f;
            int receivedFaults = -1;

            GameEvents.OnRunCompleted += (result, time, faults) =>
            {
                runCompleted = true;
                receivedResult = result;
                receivedTime = time;
                receivedFaults = faults;
            };

            // Act
            GameEvents.RaiseRunCompleted(RunResult.Qualified, 42.5f, 2);

            // Assert
            Assert.IsTrue(runCompleted);
            Assert.AreEqual(RunResult.Qualified, receivedResult);
            Assert.AreEqual(42.5f, receivedTime);
            Assert.AreEqual(2, receivedFaults);
        }

        [Test]
        public void GameEvents_CommandIssued_WorksCorrectly()
        {
            // Arrange
            bool commandIssued = false;
            HandlerCommand receivedCommand = HandlerCommand.None;

            GameEvents.OnCommandIssued += (command) =>
            {
                commandIssued = true;
                receivedCommand = command;
            };

            // Act
            GameEvents.RaiseCommandIssued(HandlerCommand.Jump);

            // Assert
            Assert.IsTrue(commandIssued);
            Assert.AreEqual(HandlerCommand.Jump, receivedCommand);
        }

        [Test]
        public void GameEvents_FaultCommitted_WorksCorrectly()
        {
            // Arrange
            bool faultCommitted = false;
            FaultType receivedFault = FaultType.None;
            string receivedObstacle = "";

            GameEvents.OnFaultCommitted += (fault, obstacle) =>
            {
                faultCommitted = true;
                receivedFault = fault;
                receivedObstacle = obstacle;
            };

            // Act
            GameEvents.RaiseFaultCommitted(FaultType.KnockedBar, "BarJump");

            // Assert
            Assert.IsTrue(faultCommitted);
            Assert.AreEqual(FaultType.KnockedBar, receivedFault);
            Assert.AreEqual("BarJump", receivedObstacle);
        }

        [Test]
        public void GameState_Enum_ContainsExpectedValues()
        {
            // Assert - Check that GameState enum has expected values
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.MainMenu));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.ModeSelect));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.TeamSelect));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.CourseLoad));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Countdown));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Gameplay));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.RunComplete));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Results));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Replay));
            Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), GameState.Career));
        }

        [Test]
        public void RunResult_Enum_ContainsExpectedValues()
        {
            // Assert - Check that RunResult enum has expected values
            Assert.IsTrue(System.Enum.IsDefined(typeof(RunResult), RunResult.Qualified));
            Assert.IsTrue(System.Enum.IsDefined(typeof(RunResult), RunResult.NonQualified));
            Assert.IsTrue(System.Enum.IsDefined(typeof(RunResult), RunResult.Elimination));
            Assert.IsTrue(System.Enum.IsDefined(typeof(RunResult), RunResult.TimeFaultOnly));
        }

        [Test]
        public void HandlerCommand_Enum_ContainsExpectedValues()
        {
            // Assert - Check that HandlerCommand enum has expected values
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.None));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.ComeBye));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Away));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Jump));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Tunnel));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Weave));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Table));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Here));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Out));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Go));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Left));
            Assert.IsTrue(System.Enum.IsDefined(typeof(HandlerCommand), HandlerCommand.Right));
        }
    }
}