using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using AgilityDogs.Core;
using AgilityDogs.Data;
using AgilityDogs.Services;
using AgilityDogs.Events;
using AgilityDogs.Gameplay;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Gameplay.Obstacles;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Gameplay.Scoring;

namespace AgilityDogs.Demo
{
    public class DemoSceneManager : MonoBehaviour
    {
        public static DemoSceneManager Instance { get; private set; }

        [Header("Demo Sections")]
        [SerializeField] private Transform hubCenter;
        [SerializeField] private Transform selectionArea;
        [SerializeField] private Transform competitionArea;
        [SerializeField] private Transform trainingArea;
        
        [Header("Portals")]
        [SerializeField] private GameObject selectionPortal;
        [SerializeField] private GameObject competitionPortal;
        [SerializeField] private GameObject trainingPortal;
        [SerializeField] private GameObject hubReturnPortal;
        
        [Header("Dog Selection")]
        [SerializeField] private BreedData[] availableBreeds;
        [SerializeField] private Transform[] dogPedestals;
        [SerializeField] private GameObject dogSelectionUI;
        [SerializeField] private TextMeshProUGUI breedNameText;
        [SerializeField] private TextMeshProUGUI breedDescriptionText;
        [SerializeField] private TextMeshProUGUI speedStatText;
        [SerializeField] private TextMeshProUGUI agilityStatText;
        [SerializeField] private TextMeshProUGUI responsivenessStatText;
        [SerializeField] private Button selectDogButton;
        [SerializeField] private Button prevDogButton;
        [SerializeField] private Button nextDogButton;
        
        [Header("Competition")]
        [SerializeField] private GameObject competitionUI;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI faultsText;
        [SerializeField] private TextMeshProUGUI obstacleCountText;
        [SerializeField] private Button startRunButton;
        [SerializeField] private Button restartRunButton;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TextMeshProUGUI resultTimeText;
        [SerializeField] private TextMeshProUGUI resultFaultsText;
        [SerializeField] private TextMeshProUGUI resultQualificationText;
        
        [Header("Training")]
        [SerializeField] private GameObject trainingUI;
        [SerializeField] private TextMeshProUGUI trainingInstructionsText;
        [SerializeField] private TextMeshProUGUI repetitionText;
        [SerializeField] private Button startTrainingButton;
        [SerializeField] private Toggle slowMotionToggle;
        
        [Header("Spawn Points")]
        [SerializeField] private Transform handlerSpawnHub;
        [SerializeField] private Transform handlerSpawnSelection;
        [SerializeField] private Transform handlerSpawnCompetition;
        [SerializeField] private Transform handlerSpawnTraining;
        [SerializeField] private Transform dogSpawnCompetition;
        [SerializeField] private Transform dogSpawnTraining;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject handlerPrefab;
        [SerializeField] private GameObject[] dogPrefabs;
        
        [Header("Demo Obstacles")]
        [SerializeField] private ObstacleBase[] competitionObstacles;
        [SerializeField] private ObstacleBase[] trainingObstacles;
        
        [Header("Audio")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip hubMusic;
        [SerializeField] private AudioClip competitionMusic;
        [SerializeField] private AudioClip trainingMusic;
        [SerializeField] private AudioClip portalEnterSound;
        [SerializeField] private AudioClip countdownBeep;
        [SerializeField] private AudioClip startWhistle;
        [SerializeField] private AudioClip finishWhistle;
        
        [Header("Visual")]
        [SerializeField] private ParticleSystem portalParticles;
        [SerializeField] private Light hubLight;
        [SerializeField] private Light competitionLight;
        [SerializeField] private Light trainingLight;
        
        public enum DemoZone
        {
            Hub,
            Selection,
            Competition,
            Training
        }
        
        private DemoZone currentZone = DemoZone.Hub;
        private int selectedBreedIndex = 0;
        private GameObject currentHandler;
        private GameObject currentDog;
        private HandlerController handlerController;
        private DogAgentController dogController;
        private CourseRunner courseRunner;
        private AgilityScoringService scoringService;
        
        private bool isRunActive = false;
        private float runTimer = 0f;
        private int currentFaults = 0;
        private int obstaclesCompleted = 0;
        
        private const float PORTAL_TRIGGER_DISTANCE = 2f;
        
        public DemoZone CurrentZone => currentZone;
        public BreedData SelectedBreed => availableBreeds != null && selectedBreedIndex < availableBreeds.Length 
            ? availableBreeds[selectedBreedIndex] : null;
        
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
            InitializeDemo();
            SetupButtons();
            SetupEventListeners();
            SpawnHandlerInHub();
            ShowHubUI();
            PlayMusic(hubMusic);
        }
        
        private void OnDestroy()
        {
            GameEvents.OnFaultCommitted -= HandleFault;
            GameEvents.OnObstacleCompletedWithReference -= HandleObstacleCompleted;
        }
        
        private void InitializeDemo()
        {
            scoringService = FindObjectOfType<AgilityScoringService>();
            courseRunner = FindObjectOfType<CourseRunner>();
            
            if (scoringService == null)
            {
                var go = new GameObject("ScoringService");
                scoringService = go.AddComponent<AgilityScoringService>();
            }
            
            ActivateZone(DemoZone.Hub);
        }
        
        private void SetupButtons()
        {
            if (selectDogButton != null)
                selectDogButton.onClick.AddListener(ConfirmDogSelection);
            if (prevDogButton != null)
                prevDogButton.onClick.AddListener(() => ChangeDogSelection(-1));
            if (nextDogButton != null)
                nextDogButton.onClick.AddListener(() => ChangeDogSelection(1));
            if (startRunButton != null)
                startRunButton.onClick.AddListener(StartCompetitionRun);
            if (restartRunButton != null)
                restartRunButton.onClick.AddListener(RestartCompetitionRun);
            if (startTrainingButton != null)
                startTrainingButton.onClick.AddListener(StartTrainingSession);
        }
        
        private void SetupEventListeners()
        {
            GameEvents.OnFaultCommitted += HandleFault;
            GameEvents.OnObstacleCompletedWithReference += HandleObstacleCompleted;
        }
        
        private void Update()
        {
            CheckPortalInteractions();
            UpdateRunTimer();
        }
        
        #region Zone Management
        
        private void ActivateZone(DemoZone zone)
        {
            if (hubCenter != null) hubCenter.gameObject.SetActive(zone == DemoZone.Hub);
            if (selectionArea != null) selectionArea.gameObject.SetActive(zone == DemoZone.Selection);
            if (competitionArea != null) competitionArea.gameObject.SetActive(zone == DemoZone.Competition);
            if (trainingArea != null) trainingArea.gameObject.SetActive(zone == DemoZone.Training);
            
            currentZone = zone;
            
            switch (zone)
            {
                case DemoZone.Hub:
                    ShowHubUI();
                    PlayMusic(hubMusic);
                    if (hubLight != null) hubLight.enabled = true;
                    if (competitionLight != null) competitionLight.enabled = false;
                    if (trainingLight != null) trainingLight.enabled = false;
                    break;
                case DemoZone.Selection:
                    ShowSelectionUI();
                    SpawnDogsOnPedestals();
                    break;
                case DemoZone.Competition:
                    ShowCompetitionUI();
                    PlayMusic(competitionMusic);
                    if (hubLight != null) hubLight.enabled = false;
                    if (competitionLight != null) competitionLight.enabled = true;
                    SpawnCompetitionEntities();
                    break;
                case DemoZone.Training:
                    ShowTrainingUI();
                    PlayMusic(trainingMusic);
                    if (hubLight != null) hubLight.enabled = false;
                    if (trainingLight != null) trainingLight.enabled = true;
                    SpawnTrainingEntities();
                    break;
            }
        }
        
        private void CheckPortalInteractions()
        {
            if (currentHandler == null) return;
            
            Vector3 handlerPos = currentHandler.transform.position;
            
            if (currentZone == DemoZone.Hub)
            {
                if (selectionPortal != null && Vector3.Distance(handlerPos, selectionPortal.transform.position) < PORTAL_TRIGGER_DISTANCE)
                {
                    EnterZone(DemoZone.Selection);
                }
                else if (competitionPortal != null && Vector3.Distance(handlerPos, competitionPortal.transform.position) < PORTAL_TRIGGER_DISTANCE)
                {
                    if (SelectedBreed != null)
                        EnterZone(DemoZone.Competition);
                }
                else if (trainingPortal != null && Vector3.Distance(handlerPos, trainingPortal.transform.position) < PORTAL_TRIGGER_DISTANCE)
                {
                    if (SelectedBreed != null)
                        EnterZone(DemoZone.Training);
                }
            }
            else
            {
                if (hubReturnPortal != null && Vector3.Distance(handlerPos, hubReturnPortal.transform.position) < PORTAL_TRIGGER_DISTANCE)
                {
                    ReturnToHub();
                }
            }
        }
        
        private void EnterZone(DemoZone zone)
        {
            PlaySound(portalEnterSound);
            if (portalParticles != null) portalParticles.Play();
            
            MoveHandlerToZone(zone);
            ActivateZone(zone);
        }
        
        private void ReturnToHub()
        {
            PlaySound(portalEnterSound);
            if (portalParticles != null) portalParticles.Play();
            
            isRunActive = false;
            MoveHandlerToZone(DemoZone.Hub);
            ActivateZone(DemoZone.Hub);
        }
        
        private void MoveHandlerToZone(DemoZone zone)
        {
            if (currentHandler == null) return;
            
            Transform spawnPoint = zone switch
            {
                DemoZone.Hub => handlerSpawnHub,
                DemoZone.Selection => handlerSpawnSelection,
                DemoZone.Competition => handlerSpawnCompetition,
                DemoZone.Training => handlerSpawnTraining,
                _ => handlerSpawnHub
            };
            
            if (spawnPoint != null)
            {
                currentHandler.transform.position = spawnPoint.position;
                currentHandler.transform.rotation = spawnPoint.rotation;
            }
        }
        
        #endregion
        
        #region Spawning
        
        private void SpawnHandlerInHub()
        {
            if (handlerPrefab != null && handlerSpawnHub != null)
            {
                if (currentHandler != null) Destroy(currentHandler);
                currentHandler = Instantiate(handlerPrefab, handlerSpawnHub.position, handlerSpawnHub.rotation);
                handlerController = currentHandler.GetComponent<HandlerController>();
            }
        }
        
        private void SpawnDogsOnPedestals()
        {
            if (availableBreeds == null || dogPedestals == null) return;
            
            int numToSpawn = Mathf.Min(availableBreeds.Length, dogPedestals.Length);
            
            for (int i = 0; i < numToSpawn; i++)
            {
                if (dogPedestals[i] != null && availableBreeds[i] != null && dogPrefabs != null && i < dogPrefabs.Length)
                {
                    GameObject dog = Instantiate(dogPrefabs[i], dogPedestals[i].position, dogPedestals[i].rotation);
                    dog.transform.localScale *= availableBreeds[i].modelScale;
                    
                    var animator = dog.GetComponent<Animator>();
                    if (animator != null) animator.SetBool("Idle", true);
                }
            }
            
            UpdateDogSelectionUI();
        }
        
        private void SpawnCompetitionEntities()
        {
            SpawnDogForRun(dogSpawnCompetition);
        }
        
        private void SpawnTrainingEntities()
        {
            SpawnDogForRun(dogSpawnTraining);
        }
        
        private void SpawnDogForRun(Transform spawnPoint)
        {
            if (currentDog != null) Destroy(currentDog);
            
            if (SelectedBreed != null && dogPrefabs != null && selectedBreedIndex < dogPrefabs.Length && spawnPoint != null)
            {
                currentDog = Instantiate(dogPrefabs[selectedBreedIndex], spawnPoint.position, spawnPoint.rotation);
                currentDog.transform.localScale *= SelectedBreed.modelScale;
                
                dogController = currentDog.GetComponent<DogAgentController>();
                if (dogController != null && currentHandler != null)
                {
                    dogController.SetHandler(currentHandler.transform);
                }
            }
        }
        
        #endregion
        
        #region Dog Selection
        
        private void ChangeDogSelection(int direction)
        {
            if (availableBreeds == null) return;
            
            selectedBreedIndex = (selectedBreedIndex + direction + availableBreeds.Length) % availableBreeds.Length;
            UpdateDogSelectionUI();
        }
        
        private void UpdateDogSelectionUI()
        {
            if (SelectedBreed == null) return;
            
            if (breedNameText != null)
                breedNameText.text = SelectedBreed.displayName;
            if (breedDescriptionText != null)
                breedDescriptionText.text = SelectedBreed.description;
            if (speedStatText != null)
                speedStatText.text = $"Speed: {Mathf.RoundToInt(SelectedBreed.maxSpeed * 10)}";
            if (agilityStatText != null)
                agilityStatText.text = $"Agility: {Mathf.RoundToInt(SelectedBreed.turnRate / 5)}";
            if (responsivenessStatText != null)
                responsivenessStatText.text = $"Focus: {Mathf.RoundToInt(SelectedBreed.responsiveness * 100)}";
        }
        
        private void ConfirmDogSelection()
        {
            PlaySound(startWhistle);
            ReturnToHub();
        }
        
        #endregion
        
        #region Competition
        
        private void StartCompetitionRun()
        {
            StartCoroutine(CompetitionCountdown());
        }
        
        private IEnumerator CompetitionCountdown()
        {
            if (startRunButton != null) startRunButton.interactable = false;
            
            for (int i = 3; i > 0; i--)
            {
                if (timerText != null) timerText.text = i.ToString();
                PlaySound(countdownBeep);
                yield return new WaitForSeconds(1f);
            }
            
            PlaySound(startWhistle);
            if (timerText != null) timerText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            
            BeginRun();
        }
        
        private void BeginRun()
        {
            isRunActive = true;
            runTimer = 0f;
            currentFaults = 0;
            obstaclesCompleted = 0;
            
            if (courseRunner != null)
            {
                courseRunner.RestartCourse();
                courseRunner.StartCountdown();
            }
            
            UpdateCompetitionUI();
        }
        
        private void UpdateRunTimer()
        {
            if (!isRunActive) return;
            
            runTimer += Time.deltaTime;
            UpdateCompetitionUI();
        }
        
        private void HandleFault(FaultType faultType, string obstacleName)
        {
            if (!isRunActive) return;
            currentFaults++;
            UpdateCompetitionUI();
        }
        
        private void HandleObstacleCompleted(ObstacleBase obstacle, bool clean)
        {
            if (!isRunActive) return;
            obstaclesCompleted++;
            UpdateCompetitionUI();
            
            if (competitionObstacles != null && obstaclesCompleted >= competitionObstacles.Length)
            {
                CompleteRun();
            }
        }
        
        private void CompleteRun()
        {
            isRunActive = false;
            PlaySound(finishWhistle);
            
            bool qualified = currentFaults == 0 && runTimer <= 45f;
            
            if (resultsPanel != null) resultsPanel.SetActive(true);
            if (resultTimeText != null) resultTimeText.text = FormatTime(runTimer);
            if (resultFaultsText != null) resultFaultsText.text = $"Faults: {currentFaults}";
            if (resultQualificationText != null)
            {
                resultQualificationText.text = qualified ? "QUALIFIED!" : "NOT QUALIFIED";
                resultQualificationText.color = qualified ? Color.green : Color.red;
            }
        }
        
        private void RestartCompetitionRun()
        {
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (startRunButton != null) startRunButton.interactable = true;
            
            SpawnCompetitionEntities();
            isRunActive = false;
            runTimer = 0f;
            currentFaults = 0;
            obstaclesCompleted = 0;
            UpdateCompetitionUI();
        }
        
        private void UpdateCompetitionUI()
        {
            if (timerText != null) timerText.text = FormatTime(runTimer);
            if (faultsText != null) faultsText.text = $"Faults: {currentFaults}";
            if (obstacleCountText != null && competitionObstacles != null)
                obstacleCountText.text = $"{obstaclesCompleted}/{competitionObstacles.Length}";
        }
        
        #endregion
        
        #region Training
        
        private void StartTrainingSession()
        {
            StartCoroutine(TrainingCountdown());
        }
        
        private IEnumerator TrainingCountdown()
        {
            if (startTrainingButton != null) startTrainingButton.interactable = false;
            
            for (int i = 3; i > 0; i--)
            {
                if (trainingInstructionsText != null)
                    trainingInstructionsText.text = $"Starting in {i}...";
                PlaySound(countdownBeep);
                yield return new WaitForSeconds(1f);
            }
            
            PlaySound(startWhistle);
            BeginTrainingRun();
        }
        
        private void BeginTrainingRun()
        {
            isRunActive = true;
            
            if (slowMotionToggle != null && slowMotionToggle.isOn)
            {
                Time.timeScale = 0.5f;
            }
            
            if (trainingInstructionsText != null)
            {
                trainingInstructionsText.text = "Guide your dog through the weave poles using directional commands!";
            }
        }
        
        #endregion
        
        #region UI Management
        
        private void ShowHubUI()
        {
            if (dogSelectionUI != null) dogSelectionUI.SetActive(false);
            if (competitionUI != null) competitionUI.SetActive(false);
            if (trainingUI != null) trainingUI.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
        }
        
        private void ShowSelectionUI()
        {
            if (dogSelectionUI != null) dogSelectionUI.SetActive(true);
            if (competitionUI != null) competitionUI.SetActive(false);
            if (trainingUI != null) trainingUI.SetActive(false);
        }
        
        private void ShowCompetitionUI()
        {
            if (dogSelectionUI != null) dogSelectionUI.SetActive(false);
            if (competitionUI != null) competitionUI.SetActive(true);
            if (trainingUI != null) trainingUI.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (startRunButton != null) startRunButton.interactable = true;
        }
        
        private void ShowTrainingUI()
        {
            if (dogSelectionUI != null) dogSelectionUI.SetActive(false);
            if (competitionUI != null) competitionUI.SetActive(false);
            if (trainingUI != null) trainingUI.SetActive(true);
            if (startTrainingButton != null) startTrainingButton.interactable = true;
            if (trainingInstructionsText != null)
            {
                trainingInstructionsText.text = "Welcome to Training Mode!\n\nPractice weave poles with optional slow-motion.\nUse WASD to move and your mouse to point directions.";
            }
        }
        
        #endregion
        
        #region Helpers
        
        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int decimals = Mathf.FloorToInt((time % 1f) * 100f);
            return $"{minutes:00}:{seconds:00}.{decimals:00}";
        }
        
        private void PlayMusic(AudioClip clip)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main?.transform.position ?? Vector3.zero);
            }
        }
        
        #endregion
    }
}
