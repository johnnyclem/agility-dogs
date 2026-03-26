using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Data;

namespace AgilityDogs.Services
{
    /// <summary>
    /// TournamentService - Manages tournament brackets and competition formats
    /// Supports knockout, round-robin, and hybrid tournament formats
    /// </summary>
    public class TournamentService : MonoBehaviour
    {
        public static TournamentService Instance { get; private set; }

        [Header("Tournament Configuration")]
        [SerializeField] private int minCompetitorsForTournament = 4;
        [SerializeField] private int maxCompetitorsForTournament = 16;
        [SerializeField] private int poolsCount = 2;
        [SerializeField] private int qualifiersPerPool = 2;

        [Header("Tournament Types")]
        [SerializeField] private float knockoutChance = 0.4f;
        [SerializeField] private float roundRobinChance = 0.3f;
        [SerializeField] private float hybridChance = 0.3f;

        // Current tournament state
        private TournamentData currentTournament;
        private TournamentFormat currentFormat;
        private int currentRound = 0;
        private List<CompetitorData> currentCompetitors = new List<CompetitorData>();
        private List<Match> currentMatches = new List<Match>();
        private Dictionary<string, List<Match>> tournamentHistory = new Dictionary<string, List<Match>>();

        // Events
        public event Action<TournamentData> OnTournamentStarted;
        public event Action<TournamentFormat> OnTournamentFormatGenerated;
        public event Action<Match> OnMatchStarted;
        public event Action<Match, CompetitorData> OnMatchCompleted;
        public event Action<TournamentData, CompetitorData> OnTournamentCompleted;
        public event Action<CompetitorData> OnChampionDetermined;

        // Properties
        public TournamentData CurrentTournament => currentTournament;
        public TournamentFormat CurrentFormat => currentFormat;
        public int CurrentRound => currentRound;
        public List<CompetitorData> CurrentCompetitors => currentCompetitors;
        public List<Match> CurrentMatches => currentMatches;
        public bool IsTournamentActive => currentTournament != null && !currentTournament.isComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region Tournament Creation

        /// <summary>
        /// Start a new tournament with the given competitors
        /// </summary>
        public void StartTournament(List<CompetitorData> competitors, TournamentFormat format = TournamentFormat.Auto)
        {
            if (competitors == null || competitors.Count < minCompetitorsForTournament)
            {
                Debug.LogWarning($"[TournamentService] Not enough competitors for tournament: {competitors?.Count ?? 0}");
                return;
            }

            if (format == TournamentFormat.Auto)
            {
                format = DetermineTournamentFormat(competitors.Count);
            }

            currentFormat = format;
            currentRound = 0;
            currentCompetitors = new List<CompetitorData>(competitors);
            currentMatches = new List<Match>();

            currentTournament = new TournamentData
            {
                tournamentId = Guid.NewGuid().ToString(),
                tournamentName = GenerateTournamentName(format),
                format = format,
                startTime = DateTime.Now,
                competitors = new List<CompetitorData>(competitors),
                rounds = new List<TournamentRound>(),
                isComplete = false,
                champion = null
            };

            GenerateBracket();

            OnTournamentStarted?.Invoke(currentTournament);
            OnTournamentFormatGenerated?.Invoke(format);

            Debug.Log($"[TournamentService] Started {format} tournament with {competitors.Count} competitors");
        }

        private TournamentFormat DetermineTournamentFormat(int competitorCount)
        {
            float roll = UnityEngine.Random.value;

            if (competitorCount <= 8 && roll < knockoutChance)
            {
                return TournamentFormat.Knockout;
            }
            else if (competitorCount >= 8 && roll < knockoutChance + roundRobinChance)
            {
                return TournamentFormat.RoundRobin;
            }
            else
            {
                return TournamentFormat.Hybrid;
            }
        }

        private string GenerateTournamentName(TournamentFormat format)
        {
            string[] prefixes = { "Grand Prix", "Championship Cup", "Invitational", "Classic", "Masters" };
            string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];

            string suffix = format switch
            {
                TournamentFormat.Knockout => "Knockout",
                TournamentFormat.RoundRobin => "Round Robin",
                TournamentFormat.Hybrid => "Championship",
                _ => "Tournament"
            };

            return $"{prefix} {suffix}";
        }

        #endregion

        #region Bracket Generation

        private void GenerateBracket()
        {
            switch (currentFormat)
            {
                case TournamentFormat.Knockout:
                    GenerateKnockoutBracket();
                    break;
                case TournamentFormat.RoundRobin:
                    GenerateRoundRobinBracket();
                    break;
                case TournamentFormat.Hybrid:
                    GenerateHybridBracket();
                    break;
            }
        }

        private void GenerateKnockoutBracket()
        {
            currentTournament.bracketType = BracketType.SingleElimination;
            
            // Shuffle competitors for random seeding
            var seededCompetitors = currentCompetitors.OrderBy(_ => UnityEngine.Random.value).ToList();

            // Ensure power of 2 by adding byes
            int bracketSize = GetNextPowerOfTwo(seededCompetitors.Count);
            while (seededCompetitors.Count < bracketSize)
            {
                seededCompetitors.Add(CreateByeCompetitor());
            }

            // Create first round matches
            TournamentRound firstRound = new TournamentRound
            {
                roundNumber = 1,
                roundName = "First Round",
                matches = new List<Match>()
            };

            for (int i = 0; i < seededCompetitors.Count; i += 2)
            {
                Match match = new Match
                {
                    matchId = $"{currentTournament.tournamentId}_r1_m{i / 2 + 1}",
                    roundNumber = 1,
                    competitor1 = seededCompetitors[i],
                    competitor2 = seededCompetitors[i + 1],
                    isBye = seededCompetitors[i + 1].isBye || seededCompetitors[i].isBye,
                    winner = seededCompetitors[i + 1].isBye ? seededCompetitors[i] : 
                             seededCompetitors[i].isBye ? seededCompetitors[i + 1] : null,
                    isComplete = seededCompetitors[i + 1].isBye || seededCompetitors[i].isBye
                };

                firstRound.matches.Add(match);
                currentMatches.Add(match);
            }

            currentTournament.rounds.Add(firstRound);
            currentRound = 1;

            Debug.Log($"[TournamentService] Generated knockout bracket with {firstRound.matches.Count} first round matches");
        }

        private void GenerateRoundRobinBracket()
        {
            currentTournament.bracketType = BracketType.RoundRobin;
            currentTournament.pools = new List<TournamentPool>();

            // Divide competitors into pools
            var shuffledCompetitors = currentCompetitors.OrderBy(_ => UnityEngine.Random.value).ToList();
            int poolSize = Mathf.CeilToInt((float)shuffledCompetitors.Count / poolsCount);

            for (int p = 0; p < poolsCount; p++)
            {
                var poolCompetitors = shuffledCompetitors
                    .Skip(p * poolSize)
                    .Take(poolSize)
                    .ToList();

                if (poolCompetitors.Count < 2) continue;

                TournamentPool pool = new TournamentPool
                {
                    poolId = $"pool_{p + 1}",
                    poolName = $"Pool {GetPoolLetter(p)}",
                    competitors = poolCompetitors,
                    standings = new List<PoolStanding>(),
                    matches = new List<Match>()
                };

                // Generate round-robin matches within pool
                int matchNumber = 1;
                for (int i = 0; i < poolCompetitors.Count; i++)
                {
                    for (int j = i + 1; j < poolCompetitors.Count; j++)
                    {
                        Match match = new Match
                        {
                            matchId = $"{currentTournament.tournamentId}_pool{p + 1}_m{matchNumber++}",
                            roundNumber = 1,
                            competitor1 = poolCompetitors[i],
                            competitor2 = poolCompetitors[j],
                            poolId = pool.poolId,
                            isBye = false,
                            winner = null,
                            isComplete = false
                        };

                        pool.matches.Add(match);
                        currentMatches.Add(match);
                    }
                }

                currentTournament.pools.Add(pool);
            }

            // Add round-robin as first phase
            TournamentRound rrRound = new TournamentRound
            {
                roundNumber = 1,
                roundName = "Round Robin Phase",
                matches = currentMatches.Where(m => m.poolId != null).ToList()
            };
            currentTournament.rounds.Add(rrRound);
            currentRound = 1;

            Debug.Log($"[TournamentService] Generated round-robin with {currentTournament.pools.Count} pools");
        }

        private void GenerateHybridBracket()
        {
            currentTournament.bracketType = BracketType.Hybrid;
            currentTournament.pools = new List<TournamentPool>();

            // Hybrid: Round robin pools → knockout bracket
            var shuffledCompetitors = currentCompetitors.OrderBy(_ => UnityEngine.Random.value).ToList();
            int poolSize = Mathf.CeilToInt((float)shuffledCompetitors.Count / poolsCount);

            // Phase 1: Round Robin in pools
            for (int p = 0; p < poolsCount; p++)
            {
                var poolCompetitors = shuffledCompetitors
                    .Skip(p * poolSize)
                    .Take(poolSize)
                    .ToList();

                if (poolCompetitors.Count < 2) continue;

                TournamentPool pool = new TournamentPool
                {
                    poolId = $"pool_{p + 1}",
                    poolName = $"Pool {GetPoolLetter(p)}",
                    competitors = poolCompetitors,
                    standings = new List<PoolStanding>(),
                    matches = new List<Match>()
                };

                int matchNumber = 1;
                for (int i = 0; i < poolCompetitors.Count; i++)
                {
                    for (int j = i + 1; j < poolCompetitors.Count; j++)
                    {
                        Match match = new Match
                        {
                            matchId = $"{currentTournament.tournamentId}_pool{p + 1}_m{matchNumber++}",
                            roundNumber = 1,
                            competitor1 = poolCompetitors[i],
                            competitor2 = poolCompetitors[j],
                            poolId = pool.poolId,
                            isBye = false,
                            winner = null,
                            isComplete = false
                        };

                        pool.matches.Add(match);
                        currentMatches.Add(match);
                    }
                }

                currentTournament.pools.Add(pool);
            }

            // Add round-robin round
            TournamentRound rrRound = new TournamentRound
            {
                roundNumber = 1,
                roundName = "Pool Play",
                matches = currentMatches.Where(m => m.poolId != null).ToList()
            };
            currentTournament.rounds.Add(rrRound);

            // Phase 2: Knockout bracket will be generated after pool play
            TournamentRound koRound = new TournamentRound
            {
                roundNumber = 2,
                roundName = "Knockout Phase",
                matches = new List<Match>()
            };
            currentTournament.rounds.Add(koRound);

            currentRound = 1;

            Debug.Log($"[TournamentService] Generated hybrid bracket with {poolsCount} pools + knockout");
        }

        private int GetNextPowerOfTwo(int n)
        {
            int power = 1;
            while (power < n)
            {
                power *= 2;
            }
            return power;
        }

        private char GetPoolLetter(int index)
        {
            return (char)('A' + index);
        }

        private CompetitorData CreateByeCompetitor()
        {
            return new CompetitorData
            {
                competitorId = "BYE",
                competitorName = "BYE",
                dogName = "-",
                isBye = true,
                skill = 0f,
                tier = ShowTier.Local
            };
        }

        #endregion

        #region Match Management

        /// <summary>
        /// Get the current matches for display
        /// </summary>
        public List<Match> GetCurrentMatches()
        {
            if (currentTournament == null) return new List<Match>();

            return currentMatches.Where(m => m.roundNumber == currentRound && !m.isComplete).ToList();
        }

        /// <summary>
        /// Get all matches for a specific round
        /// </summary>
        public List<Match> GetMatchesForRound(int round)
        {
            return currentMatches.Where(m => m.roundNumber == round).ToList();
        }

        /// <summary>
        /// Get the current bracket for display
        /// </summary>
        public TournamentBracket GetTournamentBracket()
        {
            if (currentTournament == null) return null;

            return new TournamentBracket
            {
                tournamentId = currentTournament.tournamentId,
                tournamentName = currentTournament.tournamentName,
                format = currentFormat,
                bracketType = currentTournament.bracketType,
                rounds = currentTournament.rounds,
                pools = currentTournament.pools,
                champion = currentTournament.champion
            };
        }

        /// <summary>
        /// Complete a match with the winner
        /// </summary>
        public void CompleteMatch(string matchId, CompetitorData winner, float winnerTime, int winnerFaults, CompetitorData loser = null)
        {
            Match match = currentMatches.FirstOrDefault(m => m.matchId == matchId);
            if (match == null || match.isComplete)
            {
                Debug.LogWarning($"[TournamentService] Match not found or already complete: {matchId}");
                return;
            }

            match.winner = winner;
            match.winnerTime = winnerTime;
            match.winnerFaults = winnerFaults;
            match.loser = loser;
            match.isComplete = true;

            if (!string.IsNullOrEmpty(match.poolId))
            {
                UpdatePoolStandings(match.poolId);
            }

            OnMatchCompleted?.Invoke(match, winner);

            Debug.Log($"[TournamentService] Match {matchId} completed. Winner: {winner?.competitorName}");

            // Check if tournament is complete or if we should advance
            CheckTournamentProgress();
        }

        private void UpdatePoolStandings(string poolId)
        {
            TournamentPool pool = currentTournament.pools?.FirstOrDefault(p => p.poolId == poolId);
            if (pool == null) return;

            // Calculate standings based on match results
            var poolMatches = currentMatches.Where(m => m.poolId == poolId && m.isComplete).ToList();

            pool.standings = new List<PoolStanding>();
            foreach (var competitor in pool.competitors)
            {
                if (competitor.isBye) continue;

                PoolStanding standing = new PoolStanding
                {
                    competitor = competitor,
                    wins = poolMatches.Count(m => m.winner == competitor),
                    losses = poolMatches.Count(m => m.loser == competitor),
                    pointsFor = poolMatches.Where(m => m.winner == competitor).Sum(m => m.winnerTime > 0 ? Mathf.RoundToInt(1000 / m.winnerTime) : 0),
                    pointsAgainst = 0
                };

                pool.standings.Add(standing);
            }

            pool.standings = pool.standings.OrderByDescending(s => s.wins)
                                            .ThenByDescending(s => s.pointsFor)
                                            .ToList();

            Debug.Log($"[TournamentService] Updated {poolId} standings with {pool.standings.Count} competitors");
        }

        private void CheckTournamentProgress()
        {
            if (currentTournament.isComplete) return;

            switch (currentFormat)
            {
                case TournamentFormat.Knockout:
                    AdvanceKnockout();
                    break;
                case TournamentFormat.RoundRobin:
                    CheckRoundRobinComplete();
                    break;
                case TournamentFormat.Hybrid:
                    CheckHybridProgress();
                    break;
            }
        }

        private void AdvanceKnockout()
        {
            var currentRoundMatches = currentMatches.Where(m => m.roundNumber == currentRound && m.isComplete).ToList();
            var upcomingMatches = currentMatches.Where(m => m.roundNumber == currentRound && !m.isComplete).ToList();

            // Check if current round is complete
            if (upcomingMatches.Count > 0) return;

            // Check if tournament is complete (champion determined)
            if (currentRoundMatches.Count == 1)
            {
                CompleteTournament(currentRoundMatches[0].winner);
                return;
            }

            // Generate next round
            int nextRound = currentRound + 1;
            TournamentRound round = currentTournament.rounds.FirstOrDefault(r => r.roundNumber == nextRound);
            if (round == null)
            {
                round = new TournamentRound
                {
                    roundNumber = nextRound,
                    roundName = GetRoundName(nextRound),
                    matches = new List<Match>()
                };
                currentTournament.rounds.Add(round);
            }

            // Create next round matches from winners
            var winners = currentRoundMatches.Select(m => m.winner).ToList();
            for (int i = 0; i < winners.Count; i += 2)
            {
                if (i + 1 >= winners.Count) break;

                Match newMatch = new Match
                {
                    matchId = $"{currentTournament.tournamentId}_r{nextRound}_m{i / 2 + 1}",
                    roundNumber = nextRound,
                    competitor1 = winners[i],
                    competitor2 = winners[i + 1],
                    isBye = false,
                    isComplete = false
                };

                round.matches.Add(newMatch);
                currentMatches.Add(newMatch);

                OnMatchStarted?.Invoke(newMatch);
            }

            currentRound = nextRound;

            Debug.Log($"[TournamentService] Advanced to round {currentRound} with {round.matches.Count} matches");
        }

        private void CheckRoundRobinComplete()
        {
            var incompleteMatches = currentMatches.Where(m => !m.isComplete && m.poolId != null).ToList();
            if (incompleteMatches.Count > 0) return;

            // Round robin complete - determine champion by standings
            if (currentTournament.pools != null && currentTournament.pools.Count > 0)
            {
                var allStandings = currentTournament.pools
                    .SelectMany(p => p.standings)
                    .OrderByDescending(s => s.wins)
                    .ThenByDescending(s => s.pointsFor)
                    .ToList();

                if (allStandings.Count > 0)
                {
                    CompleteTournament(allStandings[0].competitor);
                }
            }
        }

        private void CheckHybridProgress()
        {
            if (currentRound == 1)
            {
                // Pool play phase
                var incompleteMatches = currentMatches.Where(m => m.roundNumber == 1 && !m.isComplete).ToList();
                if (incompleteMatches.Count > 0) return;

                // Pool play complete - advance top qualifiers to knockout
                AdvanceToKnockoutPhase();
            }
            else
            {
                // Knockout phase
                AdvanceKnockout();
            }
        }

        private void AdvanceToKnockoutPhase()
        {
            currentRound = 2;

            // Get top qualifiers from each pool
            var qualifiers = new List<CompetitorData>();
            foreach (var pool in currentTournament.pools)
            {
                var topQualifiers = pool.standings.Take(qualifiersPerPool).Select(s => s.competitor).ToList();
                qualifiers.AddRange(topQualifiers);
            }

            // Ensure power of 2
            while (qualifiers.Count > 1 && (qualifiers.Count & (qualifiers.Count - 1)) != 0)
            {
                qualifiers.RemoveAt(qualifiers.Count - 1);
            }

            // Generate knockout bracket
            qualifiers = qualifiers.OrderBy(_ => UnityEngine.Random.value).ToList();

            TournamentRound koRound = currentTournament.rounds.FirstOrDefault(r => r.roundNumber == 2);
            if (koRound == null)
            {
                koRound = new TournamentRound
                {
                    roundNumber = 2,
                    roundName = "Knockout Phase",
                    matches = new List<Match>()
                };
                currentTournament.rounds.Add(koRound);
            }

            for (int i = 0; i < qualifiers.Count; i += 2)
            {
                if (i + 1 >= qualifiers.Count) break;

                Match match = new Match
                {
                    matchId = $"{currentTournament.tournamentId}_r2_m{i / 2 + 1}",
                    roundNumber = 2,
                    competitor1 = qualifiers[i],
                    competitor2 = qualifiers[i + 1],
                    isBye = false,
                    isComplete = false
                };

                koRound.matches.Add(match);
                currentMatches.Add(match);

                OnMatchStarted?.Invoke(match);
            }

            Debug.Log($"[TournamentService] Advanced to knockout phase with {qualifiers.Count} qualifiers");
        }

        private string GetRoundName(int round)
        {
            return round switch
            {
                1 => "First Round",
                2 => "Quarterfinals",
                3 => "Semifinals",
                4 => "Finals",
                _ => $"Round {round}"
            };
        }

        private void CompleteTournament(CompetitorData champion)
        {
            currentTournament.isComplete = true;
            currentTournament.champion = champion;
            currentTournament.endTime = DateTime.Now;

            OnTournamentCompleted?.Invoke(currentTournament, champion);
            OnChampionDetermined?.Invoke(champion);

            Debug.Log($"[TournamentService] Tournament complete! Champion: {champion?.competitorName}");

            // Save to history
            tournamentHistory[currentTournament.tournamentId] = new List<Match>(currentMatches);
        }

        #endregion

        #region Standings & Queries

        /// <summary>
        /// Get current standings for a pool
        /// </summary>
        public List<PoolStanding> GetPoolStandings(string poolId)
        {
            return currentTournament?.pools?
                .FirstOrDefault(p => p.poolId == poolId)?
                .standings ?? new List<PoolStanding>();
        }

        /// <summary>
        /// Get the current tournament champion
        /// </summary>
        public CompetitorData GetChampion()
        {
            return currentTournament?.champion;
        }

        /// <summary>
        /// Check if a competitor is still in the tournament
        /// </summary>
        public bool IsCompetitorActive(CompetitorData competitor)
        {
            return currentMatches.Any(m => 
                !m.isComplete && 
                (m.competitor1 == competitor || m.competitor2 == competitor));
        }

        /// <summary>
        /// Get the next match for a competitor
        /// </summary>
        public Match GetNextMatchFor(CompetitorData competitor)
        {
            return currentMatches.FirstOrDefault(m =>
                !m.isComplete &&
                (m.competitor1 == competitor || m.competitor2 == competitor));
        }

        #endregion
    }

    #region Data Structures

    public enum TournamentFormat
    {
        Auto,
        Knockout,
        RoundRobin,
        Hybrid
    }

    public enum BracketType
    {
        SingleElimination,
        DoubleElimination,
        RoundRobin,
        Hybrid
    }

    [Serializable]
    public class TournamentData
    {
        public string tournamentId;
        public string tournamentName;
        public TournamentFormat format;
        public BracketType bracketType;
        public DateTime startTime;
        public DateTime endTime;
        public List<CompetitorData> competitors;
        public List<TournamentRound> rounds;
        public List<TournamentPool> pools;
        public bool isComplete;
        public CompetitorData champion;
    }

    [Serializable]
    public class TournamentRound
    {
        public int roundNumber;
        public string roundName;
        public List<Match> matches;
    }

    [Serializable]
    public class TournamentPool
    {
        public string poolId;
        public string poolName;
        public List<CompetitorData> competitors;
        public List<PoolStanding> standings;
        public List<Match> matches;
    }

    [Serializable]
    public class PoolStanding
    {
        public CompetitorData competitor;
        public int wins;
        public int losses;
        public int pointsFor;
        public int pointsAgainst;
    }

    [Serializable]
    public class Match
    {
        public string matchId;
        public int roundNumber;
        public CompetitorData competitor1;
        public CompetitorData competitor2;
        public string poolId;
        public bool isBye;
        public bool isComplete;
        public CompetitorData winner;
        public CompetitorData loser;
        public float winnerTime;
        public int winnerFaults;
    }

    [Serializable]
    public class TournamentBracket
    {
        public string tournamentId;
        public string tournamentName;
        public TournamentFormat format;
        public BracketType bracketType;
        public List<TournamentRound> rounds;
        public List<TournamentPool> pools;
        public CompetitorData champion;
    }

    #endregion
}