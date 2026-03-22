using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.Gameplay.Commands
{
    public class CommandBuffer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxBufferSize = 8;
        [SerializeField] private float commandWindowSeconds = 1.5f;

        private Queue<CommandEntry> commandQueue = new Queue<CommandEntry>();
        private HandlerCommand lastCommand = HandlerCommand.None;
        private float lastCommandTime;

        public HandlerCommand LastCommand => lastCommand;
        public int Count => commandQueue.Count;

        public struct CommandEntry
        {
            public HandlerCommand command;
            public float timestamp;
            public Vector3 handlerPosition;
            public Vector3 handlerForward;
        }

        public void IssueCommand(HandlerCommand command)
        {
            if (command == HandlerCommand.None) return;

            CommandEntry entry = new CommandEntry
            {
                command = command,
                timestamp = Time.time,
                handlerPosition = GetHandlerPosition(),
                handlerForward = GetHandlerForward()
            };

            commandQueue.Enqueue(entry);

            while (commandQueue.Count > maxBufferSize)
            {
                commandQueue.Dequeue();
            }

            lastCommand = command;
            lastCommandTime = Time.time;

            GameEvents.RaiseCommandIssued(command);
        }

        public CommandEntry? GetLatestCommand()
        {
            if (commandQueue.Count == 0) return null;

            CommandEntry[] all = commandQueue.ToArray();
            return all[all.Length - 1];
        }

        public CommandEntry? GetRecentCommand()
        {
            if (commandQueue.Count == 0) return null;

            CommandEntry[] all = commandQueue.ToArray();
            for (int i = all.Length - 1; i >= 0; i--)
            {
                if (Time.time - all[i].timestamp <= commandWindowSeconds)
                {
                    return all[i];
                }
            }
            return null;
        }

        public List<CommandEntry> GetCommandsSince(float time)
        {
            List<CommandEntry> result = new List<CommandEntry>();
            foreach (var entry in commandQueue)
            {
                if (entry.timestamp >= time)
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        public void Clear()
        {
            commandQueue.Clear();
            lastCommand = HandlerCommand.None;
        }

        public bool HasCommandInWindow()
        {
            return Time.time - lastCommandTime <= commandWindowSeconds;
        }

        private Vector3 GetHandlerPosition()
        {
            var handler = GetComponentInParent<Gameplay.Handler.HandlerController>();
            return handler != null ? handler.transform.position : transform.position;
        }

        private Vector3 GetHandlerForward()
        {
            var handler = GetComponentInParent<Gameplay.Handler.HandlerController>();
            return handler != null ? handler.transform.forward : transform.forward;
        }
    }
}
