using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgilityDogs.Services;

namespace AgilityDogs.UI
{
    /// <summary>
    /// TournamentUI - Handles tournament bracket display and match management
    /// </summary>
    public class TournamentUI : MonoBehaviour
    {
        public static TournamentUI Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject tournamentCanvas;
        [SerializeField] private GameObject bracketPanel;
        [SerializeField] private GameObject matchPanel;
        [SerializeField] private GameObject resultsPanel;

        [Header("Bracket Display")]
        [SerializeField] private Transform bracketContainer;
        [SerializeField] private GameObject roundPrefab;
        [SerializeField] private GameObject matchCardPrefab;

        [Header("Current Match")]
        [SerializeField] private TextMeshProUGUI matchTitleText;
        [SerializeField] private TextMeshProUGUI competitor1NameText;
        [SerializeField] private TextMeshProUGUI competitor2NameText;
        [SerializeField] private TextMeshProUGUI vsText;
        [SerializeField] private Button startMatchButton;
        [SerializeField] private Image competitor1Portrait;
        [SerializeField] private Image competitor2Portrait;

        [Header("Results Display")]
        [SerializeField] private TextMeshProUGUI tournamentResultTitle;
        [SerializeField] private TextMeshProUGUI championNameText;
        [SerializeField] private TextMeshProUGUI championDogText;
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField] private Button returnToMenuButton;

        [Header("Pool Standings")]
        [SerializeField] private Transform poolStandingsContainer;
        [SerializeField] private GameObject poolStandingPrefab;

        // State
        private TournamentService tournamentService;
        private Match currentMatch;
        private bool isMatchInProgress = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            tournamentService = TournamentService.Instance;
            SubscribeToEvents();
            HideTournament();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            if (this == Instance) Instance = null;
        }

        private void SubscribeToEvents()
        {
            if (tournamentService != null)
            {
                tournamentService.OnTournamentStarted += HandleTournamentStarted;
                tournamentService.OnTournamentFormatGenerated += HandleFormatGenerated;
                tournamentService.OnMatchStarted += HandleMatchStarted;
                tournamentService.OnMatchCompleted += HandleMatchCompleted;
                tournamentService.OnTournamentCompleted += HandleTournamentCompleted;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (tournamentService != null)
            {
                tournamentService.OnTournamentStarted -= HandleTournamentStarted;
                tournamentService.OnTournamentFormatGenerated -= HandleFormatGenerated;
                tournamentService.OnMatchStarted -= HandleMatchStarted;
                tournamentService.OnMatchCompleted -= HandleMatchCompleted;
                tournamentService.OnTournamentCompleted -= HandleTournamentCompleted;
            }
        }

        #region Public Methods

        public void ShowTournament()
        {
            if (tournamentCanvas != null)
            {
                tournamentCanvas.SetActive(true);
            }
            UpdateBracketDisplay();
        }

        public void HideTournament()
        {
            if (tournamentCanvas != null)
            {
                tournamentCanvas.SetActive(false);
            }
        }

        public void StartCurrentMatch()
        {
            if (currentMatch == null || isMatchInProgress) return;

            isMatchInProgress = true;
            Debug.Log($"[TournamentUI] Starting match: {currentMatch.competitor1?.competitorName} vs {currentMatch.competitor2?.competitorName}");

            // This would trigger the actual gameplay scene
            // For now, we'll simulate the match result
            SimulateMatchResult();
        }

        #endregion

        #region Event Handlers

        private void HandleTournamentStarted(TournamentData tournament)
        {
            ShowTournament();
            UpdateBracketDisplay();
        }

        private void HandleFormatGenerated(TournamentFormat format)
        {
            Debug.Log($"[TournamentUI] Tournament format: {format}");
        }

        private void HandleMatchStarted(Match match)
        {
            currentMatch = match;
            ShowMatchPanel(match);
        }

        private void HandleMatchCompleted(Match match, CompetitorData winner)
        {
            isMatchInProgress = false;
            UpdateBracketDisplay();
            HideMatchPanel();
        }

        private void HandleTournamentCompleted(TournamentData tournament, CompetitorData champion)
        {
            ShowTournamentResults(champion);
        }

        #endregion

        #region Bracket Display

        private void UpdateBracketDisplay()
        {
            if (bracketContainer == null) return;

            // Clear existing
            foreach (Transform child in bracketContainer)
            {
                Destroy(child.gameObject);
            }

            var bracket = tournamentService?.GetTournamentBracket();
            if (bracket == null) return;

            // Create round columns
            foreach (var round in bracket.rounds)
            {
                GameObject roundGo = Instantiate(roundPrefab, bracketContainer);
                var roundTitle = roundGo.transform.Find("RoundTitle")?.GetComponent<TextMeshProUGUI>();
                if (roundTitle != null)
                {
                    roundTitle.text = round.roundName;
                }

                var matchesContainer = roundGo.transform.Find("MatchesContainer");
                if (matchesContainer != null)
                {
                    foreach (var match in round.matches)
                    {
                        CreateMatchCard(match, matchesContainer);
                    }
                }
            }
        }

        private void CreateMatchCard(Match match, Transform container)
        {
            GameObject card = Instantiate(matchCardPrefab, container);

            // Set competitor names
            var comp1Text = card.transform.Find("Competitor1Name")?.GetComponent<TextMeshProUGUI>();
            if (comp1Text != null)
            {
                comp1Text.text = match.competitor1?.competitorName ?? "TBD";
                if (match.winner == match.competitor1)
                {
                    comp1Text.fontStyle = FontStyles.Bold;
                }
            }

            var comp2Text = card.transform.Find("Competitor2Name")?.GetComponent<TextMeshProUGUI>();
            if (comp2Text != null)
            {
                comp2Text.text = match.competitor2?.competitorName ?? "TBD";
                if (match.winner == match.competitor2)
                {
                    comp2Text.fontStyle = FontStyles.Bold;
                }
            }

            // Set result if complete
            var resultText = card.transform.Find("ResultText")?.GetComponent<TextMeshProUGUI>();
            if (resultText != null)
            {
                if (match.isComplete && match.winner != null)
                {
                    resultText.text = $"W: {match.winner.competitorName}";
                    resultText.gameObject.SetActive(true);
                }
                else
                {
                    resultText.gameObject.SetActive(false);
                }
            }

            // Highlight current match
            if (currentMatch != null && currentMatch.matchId == match.matchId)
            {
                var outline = card.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = Color.yellow;
                    outline.effectDistance = new Vector2(3, 3);
                }
            }
        }

        #endregion

        #region Match Panel

        private void ShowMatchPanel(Match match)
        {
            if (matchPanel != null)
            {
                matchPanel.SetActive(true);
            }

            if (matchTitleText != null)
            {
                matchTitleText.text = $"Match: {match.competitor1?.competitorName ?? "???"} vs {match.competitor2?.competitorName ?? "???"}";
            }

            if (competitor1NameText != null)
            {
                competitor1NameText.text = match.competitor1?.competitorName ?? "???";
            }

            if (competitor2NameText != null)
            {
                competitor2NameText.text = match.competitor2?.competitorName ?? "???";
            }

            if (vsText != null)
            {
                vsText.text = "VS";
            }

            if (startMatchButton != null)
            {
                startMatchButton.interactable = true;
            }
        }

        private void HideMatchPanel()
        {
            if (matchPanel != null)
            {
                matchPanel.SetActive(false);
            }
        }

        private void SimulateMatchResult()
        {
            if (currentMatch == null) return;

            // Simple simulation based on skill
            float skill1 = currentMatch.competitor1?.skill ?? 0.5f;
            float skill2 = currentMatch.competitor2?.skill ?? 0.5f;

            // Add randomness
            skill1 += UnityEngine.Random.Range(-0.1f, 0.1f);
            skill2 += UnityEngine.Random.Range(-0.1f, 0.1f);

            CompetitorData winner = skill1 > skill2 ? currentMatch.competitor1 : currentMatch.competitor2;
            CompetitorData loser = skill1 > skill2 ? currentMatch.competitor2 : currentMatch.competitor1;

            float time = UnityEngine.Random.Range(35f, 55f);
            int faults = UnityEngine.Random.Range(0, 5);

            tournamentService?.CompleteMatch(currentMatch.matchId, winner, time, faults, loser);
        }

        #endregion

        #region Results Display

        private void ShowTournamentResults(CompetitorData champion)
        {
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(true);
            }

            if (championNameText != null)
            {
                championNameText.text = champion?.competitorName ?? "Unknown";
            }

            if (championDogText != null)
            {
                championDogText.text = $"with {champion?.dogName ?? "???"}";
            }

            if (champion?.isPlayer == true && celebrationParticles != null)
            {
                celebrationParticles.Play();
            }
        }

        #endregion

        #region Pool Standings

        private void UpdatePoolStandingsDisplay(string poolId)
        {
            if (poolStandingsContainer == null) return;

            // Clear existing
            foreach (Transform child in poolStandingsContainer)
            {
                Destroy(child.gameObject);
            }

            var standings = tournamentService?.GetPoolStandings(poolId);
            if (standings == null) return;

            int position = 1;
            foreach (var standing in standings)
            {
                GameObject standingGo = Instantiate(poolStandingPrefab, poolStandingsContainer);
                var posText = standingGo.transform.Find("Position")?.GetComponent<TextMeshProUGUI>();
                if (posText != null) posText.text = $"#{position}";

                var nameText = standingGo.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null) nameText.text = standing.competitor?.competitorName ?? "???";

                var winsText = standingGo.transform.Find("Wins")?.GetComponent<TextMeshProUGUI>();
                if (winsText != null) winsText.text = $"{standing.wins}W-{standing.losses}L";

                position++;
            }
        }

        #endregion
    }
}