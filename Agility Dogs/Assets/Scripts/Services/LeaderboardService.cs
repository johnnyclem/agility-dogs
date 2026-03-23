using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Manages leaderboard data, submissions, and ghost run storage.
    /// Supports both online and local leaderboard functionality.
    /// </summary>
    public class LeaderboardService : MonoBehaviour
    {
        public static LeaderboardService Instance { get; set; }

        [Header("Leaderboard Configuration")]
        [SerializeField] private int maxLocalEntries = 100;
        [SerializeField] private int maxGhostRunsStored = 10;
        [SerializeField] private bool onlineLeaderboardsEnabled = false;

        [Header("Leaderboard Categories")]
        [SerializeField] private LeaderboardCategory[] categories = new LeaderboardCategory[]
        {
            new LeaderboardCategory { id = "best_time", name = "Best Time", sortAscending = true },
            new LeaderboardCategory { id = "fewest_faults", name = "Fewest Faults", sortAscending = true },
            new LeaderboardCategory { id = "highest_score", name = "Highest Score", sortAscending = false },
            new LeaderboardCategory { id = "perfect_runs", name = "Perfect Runs", sortAscending = false }
        };

        // Local leaderboard data
        private Dictionary<string, List<LeaderboardEntry>> localLeaderboards = new Dictionary<string, List<LeaderboardEntry>>();
        private Dictionary<string, List<GhostRunData>> ghostRuns = new Dictionary<string, List<GhostRunData>>();

        // Events
        public event Action<string, List<LeaderboardEntry>> OnLeaderboardLoaded;
        public event Action<string, bool> OnScoreSubmitted;
        public event Action<string, GhostRunData> OnGhostRunSaved;
        public event Action<string> OnLeaderboardError;

        // Properties
        public bool IsOnlineEnabled => onlineLeaderboardsEnabled;
        public LeaderboardCategory[] Categories => categories;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadLocalLeaderboards();
        }

        #region Score Submission

        /// <summary>
        /// Submit a score to a leaderboard.
        /// </summary>
        public void SubmitScore(string leaderboardId, LeaderboardEntry entry, bool saveGhostRun = false)
        {
            if (entry == null || string.IsNullOrEmpty(leaderboardId)) return;

            entry.submittedAt = DateTime.Now;
            entry.playerName = GetPlayerName();
            entry.isLocalPlayer = true;

            // Add to local leaderboard
            if (!localLeaderboards.ContainsKey(leaderboardId))
            {
                localLeaderboards[leaderboardId] = new List<LeaderboardEntry>();
            }

            var leaderboard = localLeaderboards[leaderboardId];
            leaderboard.Add(entry);

            // Sort and trim
            SortLeaderboard(leaderboardId);
            TrimLeaderboard(leaderboardId);

            // Check if it's a personal best
            bool isPersonalBest = IsPersonalBest(leaderboardId, entry);

            // Save locally
            SaveLocalLeaderboards();

            OnScoreSubmitted?.Invoke(leaderboardId, isPersonalBest);
            Debug.Log($"[Leaderboard] Score submitted to {leaderboardId}: {entry.score} (PB: {isPersonalBest})");

            // Save ghost run if requested
            if (saveGhostRun && entry.hasGhostData)
            {
                SaveGhostRun(leaderboardId, entry.ghostData);
            }

            // Submit to online leaderboard if enabled
            if (onlineLeaderboardsEnabled)
            {
                SubmitToOnlineLeaderboard(leaderboardId, entry);
            }
        }

        /// <summary>
        /// Submit a run result to all relevant leaderboards.
        /// </summary>
        public void SubmitRunResult(string courseId, float time, int faults, int score, GhostRunData ghostData = null)
        {
            // Best Time
            SubmitScore($"time_{courseId}", new LeaderboardEntry
            {
                score = time,
                secondaryValue = faults,
                hasGhostData = ghostData != null,
                ghostData = ghostData
            }, ghostData != null);

            // Fewest Faults
            SubmitScore($"faults_{courseId}", new LeaderboardEntry
            {
                score = faults,
                secondaryValue = time,
                hasGhostData = false
            });

            // Highest Score
            SubmitScore($"score_{courseId}", new LeaderboardEntry
            {
                score = score,
                secondaryValue = time,
                hasGhostData = false
            });

            // Perfect Runs (if applicable)
            if (faults == 0)
            {
                SubmitScore($"perfect_{courseId}", new LeaderboardEntry
                {
                    score = time,
                    secondaryValue = 1, // Count of perfect runs
                    hasGhostData = ghostData != null,
                    ghostData = ghostData
                });
            }
        }

        #endregion

        #region Leaderboard Retrieval

        /// <summary>
        /// Get leaderboard entries for a specific leaderboard.
        /// </summary>
        public List<LeaderboardEntry> GetLeaderboard(string leaderboardId, int limit = 10)
        {
            if (!localLeaderboards.ContainsKey(leaderboardId))
            {
                return new List<LeaderboardEntry>();
            }

            return localLeaderboards[leaderboardId].Take(limit).ToList();
        }

        /// <summary>
        /// Get the player's rank on a leaderboard.
        /// </summary>
        public int GetPlayerRank(string leaderboardId)
        {
            if (!localLeaderboards.ContainsKey(leaderboardId)) return -1;

            var leaderboard = localLeaderboards[leaderboardId];
            var playerEntry = leaderboard.FirstOrDefault(e => e.isLocalPlayer);

            if (playerEntry == null) return -1;

            return leaderboard.IndexOf(playerEntry) + 1;
        }

        /// <summary>
        /// Get the player's best score on a leaderboard.
        /// </summary>
        public LeaderboardEntry GetPlayerBestScore(string leaderboardId)
        {
            if (!localLeaderboards.ContainsKey(leaderboardId)) return null;

            return localLeaderboards[leaderboardId]
                .Where(e => e.isLocalPlayer)
                .OrderBy(e => e.score)
                .FirstOrDefault();
        }

        /// <summary>
        /// Load leaderboard from online service.
        /// </summary>
        public void LoadOnlineLeaderboard(string leaderboardId, int limit = 100)
        {
            if (!onlineLeaderboardsEnabled)
            {
                OnLeaderboardError?.Invoke("Online leaderboards are disabled");
                return;
            }

            // This would make an API call to online service
            Debug.Log($"[Leaderboard] Loading online leaderboard: {leaderboardId}");

            // Simulate async load
            StartCoroutine(LoadOnlineLeaderboardCoroutine(leaderboardId, limit));
        }

        private System.Collections.IEnumerator LoadOnlineLeaderboardCoroutine(string leaderboardId, int limit)
        {
            yield return new WaitForSeconds(0.5f); // Simulate network delay

            // Would parse response and update local leaderboard
            OnLeaderboardLoaded?.Invoke(leaderboardId, GetLeaderboard(leaderboardId, limit));
        }

        #endregion

        #region Ghost Runs

        /// <summary>
        /// Save a ghost run for later playback.
        /// </summary>
        public void SaveGhostRun(string leaderboardId, GhostRunData ghostData)
        {
            if (ghostData == null) return;

            if (!ghostRuns.ContainsKey(leaderboardId))
            {
                ghostRuns[leaderboardId] = new List<GhostRunData>();
            }

            var ghosts = ghostRuns[leaderboardId];
            ghosts.Add(ghostData);

            // Sort by time (best first)
            ghosts.Sort((a, b) => a.finalTime.CompareTo(b.finalTime));

            // Keep only best N ghosts
            if (ghosts.Count > maxGhostRunsStored)
            {
                ghosts.RemoveRange(maxGhostRunsStored, ghosts.Count - maxGhostRunsStored);
            }

            SaveGhostRuns();
            OnGhostRunSaved?.Invoke(leaderboardId, ghostData);

            Debug.Log($"[Leaderboard] Ghost run saved: {ghostData.finalTime:F2}s");
        }

        /// <summary>
        /// Load the best ghost run for a leaderboard.
        /// </summary>
        public GhostRunData LoadBestGhostRun(string leaderboardId)
        {
            if (!ghostRuns.ContainsKey(leaderboardId) || ghostRuns[leaderboardId].Count == 0)
            {
                return null;
            }

            return ghostRuns[leaderboardId].First();
        }

        /// <summary>
        /// Load a specific ghost run.
        /// </summary>
        public GhostRunData LoadGhostRun(string leaderboardId, int index)
        {
            if (!ghostRuns.ContainsKey(leaderboardId)) return null;
            if (index < 0 || index >= ghostRuns[leaderboardId].Count) return null;

            return ghostRuns[leaderboardId][index];
        }

        /// <summary>
        /// Get all stored ghost runs for a leaderboard.
        /// </summary>
        public List<GhostRunData> GetAllGhostRuns(string leaderboardId)
        {
            if (!ghostRuns.ContainsKey(leaderboardId))
            {
                return new List<GhostRunData>();
            }

            return new List<GhostRunData>(ghostRuns[leaderboardId]);
        }

        /// <summary>
        /// Delete a ghost run.
        /// </summary>
        public bool DeleteGhostRun(string leaderboardId, int index)
        {
            if (!ghostRuns.ContainsKey(leaderboardId)) return false;
            if (index < 0 || index >= ghostRuns[leaderboardId].Count) return false;

            ghostRuns[leaderboardId].RemoveAt(index);
            SaveGhostRuns();

            Debug.Log($"[Leaderboard] Ghost run deleted from {leaderboardId}");
            return true;
        }

        #endregion

        #region Online Leaderboards

        private void SubmitToOnlineLeaderboard(string leaderboardId, LeaderboardEntry entry)
        {
            // This would integrate with platform-specific online services:
            // - Steam Leaderboards
            // - Xbox Live Leaderboards
            // - PlayStation Leaderboards
            // - Nintendo Switch Leaderboards
            // - Epic Games Leaderboards
            // - Custom REST API

            Debug.Log($"[Leaderboard] Submitting to online: {leaderboardId}");

            // Simulate API call
            StartCoroutine(SubmitToOnlineCoroutine(leaderboardId, entry));
        }

        private System.Collections.IEnumerator SubmitToOnlineCoroutine(string leaderboardId, LeaderboardEntry entry)
        {
            yield return new WaitForSeconds(0.3f); // Simulate network delay

            // Would handle response
            Debug.Log($"[Leaderboard] Online submission complete: {leaderboardId}");
        }

        #endregion

        #region Utility Methods

        private void SortLeaderboard(string leaderboardId)
        {
            var category = categories.FirstOrDefault(c => c.id == leaderboardId);
            bool ascending = category?.sortAscending ?? true;

            var leaderboard = localLeaderboards[leaderboardId];

            if (ascending)
            {
                leaderboard.Sort((a, b) => a.score.CompareTo(b.score));
            }
            else
            {
                leaderboard.Sort((a, b) => b.score.CompareTo(a.score));
            }
        }

        private void TrimLeaderboard(string leaderboardId)
        {
            var leaderboard = localLeaderboards[leaderboardId];
            if (leaderboard.Count > maxLocalEntries)
            {
                leaderboard.RemoveRange(maxLocalEntries, leaderboard.Count - maxLocalEntries);
            }
        }

        private bool IsPersonalBest(string leaderboardId, LeaderboardEntry entry)
        {
            var leaderboard = localLeaderboards[leaderboardId];
            var existingBest = leaderboard
                .Where(e => e.isLocalPlayer && e != entry)
                .OrderBy(e => e.score)
                .FirstOrDefault();

            if (existingBest == null) return true;

            var category = categories.FirstOrDefault(c => c.id == leaderboardId);
            bool ascending = category?.sortAscending ?? true;

            return ascending 
                ? entry.score < existingBest.score 
                : entry.score > existingBest.score;
        }

        private string GetPlayerName()
        {
            // Would get from player profile or platform
            return "Player";
        }

        #endregion

        #region Persistence

        private void LoadLocalLeaderboards()
        {
            string path = GetLeaderboardSavePath();
            if (!System.IO.File.Exists(path)) return;

            try
            {
                string json = System.IO.File.ReadAllText(path);
                var data = JsonUtility.FromJson<LeaderboardSaveData>(json);

                localLeaderboards = data.leaderboards ?? new Dictionary<string, List<LeaderboardEntry>>();
                ghostRuns = data.ghostRuns ?? new Dictionary<string, List<GhostRunData>>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Leaderboard] Failed to load: {e.Message}");
            }
        }

        private void SaveLocalLeaderboards()
        {
            var data = new LeaderboardSaveData
            {
                leaderboards = localLeaderboards,
                ghostRuns = ghostRuns
            };

            string json = JsonUtility.ToJson(data, true);
            string path = GetLeaderboardSavePath();
            System.IO.File.WriteAllText(path, json);
        }

        private void SaveGhostRuns()
        {
            // Same save operation
            SaveLocalLeaderboards();
        }

        private string GetLeaderboardSavePath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "leaderboards.json");
        }

        #endregion
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string id;
        public string playerName;
        public float score;
        public float secondaryValue; // Used for tie-breaking
        public bool isLocalPlayer;
        public bool hasGhostData;
        public GhostRunData ghostData;
        public DateTime submittedAt;
        public string playerAvatarId;
    }

    [Serializable]
    public class GhostRunData
    {
        public string id;
        public float finalTime;
        public int totalFaults;
        public DateTime recordedAt;

        // Position data (sampled at intervals)
        public List<GhostFrame> frames = new List<GhostFrame>();

        // Split times
        public Dictionary<string, float> splitTimes = new Dictionary<string, float>();
    }

    [Serializable]
    public class GhostFrame
    {
        public float time;
        public Vector3Data handlerPosition;
        public Vector3Data dogPosition;
        public QuaternionData dogRotation;
        public float dogSpeed;
    }

    [Serializable]
    public class Vector3Data
    {
        public float x, y, z;

        public Vector3Data(Vector3 v) { x = v.x; y = v.y; z = v.z; }
        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    public class QuaternionData
    {
        public float x, y, z, w;

        public QuaternionData(Quaternion q) { x = q.x; y = q.y; z = q.z; w = q.w; }
        public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
    }

    [Serializable]
    public class LeaderboardCategory
    {
        public string id;
        public string name;
        public bool sortAscending;
    }

    [Serializable]
    public class LeaderboardSaveData
    {
        public Dictionary<string, List<LeaderboardEntry>> leaderboards;
        public Dictionary<string, List<GhostRunData>> ghostRuns;
    }
}
