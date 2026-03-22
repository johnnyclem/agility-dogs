using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.Services
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        public GameState CurrentState => currentState;

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

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            GameState previous = currentState;
            currentState = newState;
            GameEvents.RaiseGameStateChanged(previous, newState);
        }

        public void StartCountdown()
        {
            SetState(GameState.Countdown);
        }

        public void BeginRun()
        {
            SetState(GameState.Gameplay);
            GameEvents.RaiseRunStarted();
        }

        public void CompleteRun(RunResult result, float time, int faults)
        {
            SetState(GameState.RunComplete);
            GameEvents.RaiseRunCompleted(result, time, faults);
        }

        public void ShowResults()
        {
            SetState(GameState.Results);
        }

        public void ReturnToMenu()
        {
            SetState(GameState.MainMenu);
        }
    }
}
