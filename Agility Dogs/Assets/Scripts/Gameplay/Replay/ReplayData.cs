using System;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;

namespace AgilityDogs.Gameplay.Replay
{
    [Serializable]
    public class ReplayFrame
    {
        public float timestamp;
        public Vector3 dogPosition;
        public Quaternion dogRotation;
        public Vector3 handlerPosition;
        public Quaternion handlerRotation;
        public DogState dogState;
        public bool isHandlerSprinting;
    }

    [Serializable]
    public class ReplayEvent
    {
        public float timestamp;
        public ReplayEventType eventType;
        public string data;
    }

    public enum ReplayEventType
    {
        CommandIssued,
        FaultCommitted,
        ObstacleCompleted,
        SplitTimeRecorded,
        RunStarted,
        RunCompleted,
        CountdownTick,
        CourseLoaded,
        GameStateChanged
    }

    [Serializable]
    public class ReplayHighlight
    {
        public float timestamp;
        public float startTime;
        public float endTime;
        public ReplayEventType eventType;
        public string description;
    }

    [CreateAssetMenu(fileName = "NewReplayData", menuName = "Agility Dogs/Replay Data")]
    public class ReplayData : ScriptableObject
    {
        public string replayName;
        public string courseName;
        public string breedName;
        public float runTime;
        public int faultCount;
        public RunResult runResult;
        
        public List<ReplayFrame> frames = new List<ReplayFrame>();
        public List<ReplayEvent> events = new List<ReplayEvent>();
        
        public void Clear()
        {
            frames.Clear();
            events.Clear();
        }
        
        public void AddFrame(ReplayFrame frame)
        {
            frames.Add(frame);
        }
        
        public void AddEvent(ReplayEvent replayEvent)
        {
            events.Add(replayEvent);
        }
        
        public float GetDuration()
        {
            if (frames.Count == 0) return 0f;
            return frames[frames.Count - 1].timestamp;
        }
    }
}