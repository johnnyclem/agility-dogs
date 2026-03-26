#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using AgilityDogs.UI;
using AgilityDogs.Gameplay;
using AgilityDogs.Gameplay.Scoring;
using AgilityDogs.Data;
using AgilityDogs.Services;
using System.Collections.Generic;

namespace AgilityDogs.Editor
{
    public class SceneSetupUtility : EditorWindow
    {
        [MenuItem("Agility Dogs/Setup/Complete Scene Setup")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupUtility>("Agility Dogs Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Agility Dogs - Scene Setup Utility", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Setup StartMenu Scene", GUILayout.Height(40)))
            {
                SetupStartMenuScene();
            }

            if (GUILayout.Button("Create All BreedData Assets", GUILayout.Height(40)))
            {
                CreateAllBreedDataAssets();
            }

            if (GUILayout.Button("Create UI Prefabs", GUILayout.Height(40)))
            {
                CreateUIPrefabs();
            }

            if (GUILayout.Button("Setup SampleScene", GUILayout.Height(40)))
            {
                SetupSampleScene();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Run these in order for a fresh project.", EditorStyles.helpBox);
        }

        private static void SetupStartMenuScene()
        {
            Debug.Log("[SceneSetup] Setting up StartMenu scene...");

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != "StartMenu")
            {
                Debug.LogError("[SceneSetup] Please open StartMenu scene first!");
                return;
            }

            // Find or create MenuManager
            var menuManagerGo = GameObject.Find("MenuManager");
            MenuManager menuManager = null;

            if (menuManagerGo == null)
            {
                menuManagerGo = new GameObject("MenuManager");
                menuManager = menuManagerGo.AddComponent<MenuManager>();
            }
            else
            {
                menuManager = menuManagerGo.GetComponent<MenuManager>();
            }

            // Find Canvas
            var canvasGo = GameObject.Find("Canvas");
            Canvas canvas = null;
            if (canvasGo != null)
            {
                canvas = canvasGo.GetComponent<Canvas>();
            }

            if (canvasGo == null)
            {
                Debug.LogError("[SceneSetup] Canvas not found! Please ensure there's a Canvas in the scene.");
                return;
            }

            // Create Main Menu Panel
            var mainMenuPanel = CreatePanel(canvasGo.transform, "MainMenuPanel", new Vector2(1920, 1080));
            SetupMainMenuPanel(mainMenuPanel);

            // Create Quick Play Panel
            var quickPlayPanel = CreatePanel(canvasGo.transform, "QuickPlayPanel", new Vector2(1920, 1080));
            SetupQuickPlayPanel(quickPlayPanel);

            // Create Training Panel
            var trainingPanel = CreatePanel(canvasGo.transform, "TrainingPanel", new Vector2(1920, 1080));
            SetupTrainingPanel(trainingPanel);

            // Create Team Select Panel
            var teamSelectPanel = CreatePanel(canvasGo.transform, "TeamSelectPanel", new Vector2(1920, 1080));
            SetupTeamSelectPanel(teamSelectPanel);

            // Create Settings Panel
            var settingsPanel = CreatePanel(canvasGo.transform, "SettingsPanel", new Vector2(1920, 1080));
            SetupSettingsPanel(settingsPanel);

            // Create Results Panel
            var resultsPanel = CreatePanel(canvasGo.transform, "ResultsPanel", new Vector2(1920, 1080));
            SetupResultsPanel(resultsPanel);

            // Create Mode Select Panel
            var modeSelectPanel = CreatePanel(canvasGo.transform, "ModeSelectPanel", new Vector2(1920, 1080));
            SetupModeSelectPanel(modeSelectPanel);

            // Create Pause Panel
            var pausePanel = CreatePanel(canvasGo.transform, "PausePanel", new Vector2(1920, 1080));
            SetupPausePanel(pausePanel);

            // Setup MenuManager references
            SetupMenuManagerReferences(menuManager, mainMenuPanel, modeSelectPanel, quickPlayPanel,
                trainingPanel, teamSelectPanel, settingsPanel, resultsPanel, pausePanel);

            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("[SceneSetup] StartMenu scene setup complete!");
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
        {
            var panelGo = GameObject.Find(name);
            if (panelGo == null)
            {
                panelGo = new GameObject(name);
                panelGo.transform.SetParent(parent);
                var rect = panelGo.AddComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = size;
                var canvasGroup = panelGo.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;
            }
            return panelGo;
        }

        private static void SetupMainMenuPanel(GameObject panel)
        {
            foreach (Transform child in panel.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            // Create title
            var titleGo = CreateText(panel.transform, "Title", "AGILITY DOGS", new Vector2(0, 200), new Vector2(400, 80));
            var titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;

            // Create buttons container
            var buttonsContainer = new GameObject("ButtonsContainer");
            buttonsContainer.transform.SetParent(panel.transform);
            var btnRect = buttonsContainer.AddComponent<RectTransform>();
            btnRect.anchoredPosition = Vector2.zero;

            // Create menu buttons
            var buttons = new string[] { "Quick Play", "Training", "Career", "Settings", "Quit" };
            float yPos = 50;
            foreach (var buttonName in buttons)
            {
                var btnGo = CreateButton(btnRect, buttonName.Replace(" ", "") + "Button", buttonName, new Vector2(0, yPos), new Vector2(300, 60));
                yPos -= 80;
            }

            // Create version text
            CreateText(panel.transform, "VersionText", "v1.0.0", new Vector2(0, -450), new Vector2(200, 30));
        }

        private static void SetupQuickPlayPanel(GameObject panel)
        {
            foreach (Transform child in panel.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            CreateText(panel.transform, "Title", "Quick Play", new Vector2(0, 200), new Vector2(400, 60));
            CreateText(panel.transform, "Description", "Jump straight into competition!", new Vector2(0, 100), new Vector2(600, 40));

            var btnContainer = new GameObject("Buttons");
            btnContainer.transform.SetParent(panel.transform);
            btnContainer.AddComponent<RectTransform>();

            CreateButton(btnContainer.transform, "StartButton", "Start Competition", new Vector2(0, 0), new Vector2(300, 60));
            CreateButton(btnContainer.transform, "BackButton", "Back", new Vector2(0, -80), new Vector2(200, 50));

            panel.SetActive(false);
        }

        private static void SetupTrainingPanel(GameObject panel)
        {
            foreach (Transform child in panel.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            CreateText(panel.transform, "Title", "Training Mode", new Vector2(0, 200), new Vector2(400, 60));
            CreateText(panel.transform, "Description", "Practice at your own pace", new Vector2(0, 100), new Vector2(600, 40));

            var courseListContainer = new GameObject("CourseListContainer");
            courseListContainer.transform.SetParent(panel.transform);
            var rect = courseListContainer.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-200, 0);
            rect.sizeDelta = new Vector2(400, 400);

            var btnContainer = new GameObject("Buttons");
            btnContainer.transform.SetParent(panel.transform);
            btnContainer.AddComponent<RectTransform>();

            CreateButton(btnContainer.transform, "StartButton", "Start Training", new Vector2(0, -100), new Vector2(300, 60));
            CreateButton(btnContainer.transform, "BackButton", "Back", new Vector2(0, -180), new Vector2(200, 50));

            panel.SetActive(false);
        }

        private static void SetupTeamSelectPanel(GameObject panel)
        {
            foreach (Transform child in panel.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            CreateText(panel.transform, "Title", "Select Your Team", new Vector2(0, 300), new Vector2(400, 60));

            // Handler section
            var handlerSection = new GameObject("HandlerSection");
            handlerSection.transform.SetParent(panel.transform);
            var handlerRect = handlerSection.AddComponent<RectTransform>();
            handlerRect.anchoredPosition = new Vector2(-300, 100);
            handlerRect.sizeDelta = new Vector2(300, 400);
            CreateText(handlerSection.transform, "Label", "Handler", new Vector2(0, 150), new Vector2(200, 30));

            var handlerListContainer = new GameObject("HandlerListContainer");
            handlerListContainer.transform.SetParent(handlerSection.transform);
            var hlRect = handlerListContainer.AddComponent<RectTransform>();
            hlRect.anchoredPosition = new Vector2(0, 0);
            hlRect.sizeDelta = new Vector2(280, 300);

            // Dog section
            var dogSection = new GameObject("DogSection");
            dogSection.transform.SetParent(panel.transform);
            var dogRect = dogSection.AddComponent<RectTransform>();
            dogRect.anchoredPosition = new Vector2(300, 100);
            dogRect.sizeDelta = new Vector2(300, 400);
            CreateText(dogSection.transform, "Label", "Dog", new Vector2(0, 150), new Vector2(200, 30));

            var dogListContainer = new GameObject("DogListContainer");
            dogListContainer.transform.SetParent(dogSection.transform);
            var dlRect = dogListContainer.AddComponent<RectTransform>();
            dlRect.anchoredPosition = new Vector2(0, 0);
            dlRect.sizeDelta = new Vector2(280, 300);

            // Buttons
            var btnContainer = new GameObject("Buttons");
            btnContainer.transform.SetParent(panel.transform);
            btnContainer.AddComponent<RectTransform>();

            CreateButton(btnContainer.transform, "StartRunButton", "Start Run", new Vector2(0, -200), new Vector2(300, 60));
            CreateButton(btnContainer.transform, "BackButton", "Back", new Vector2(0, -280), new Vector2(200, 50));

            panel.SetActive(false);
        }

        private static void SetupSettingsPanel(GameObject panel)
        {
            foreach (Transform child in panel.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            CreateText(panel.transform, "Title", "Settings", new Vector2(0, 300), new Vector2(400, 60));

            var settingsContainer = new GameObject("SettingsContainer");
            settingsContainer.transform.SetParent(panel.transform);
            var rect = settingsContainer.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 50);
            rect.sizeDelta = new Vector2(400, 400);

            float yPos = 150;
            CreateSliderSetting(settingsContainer.transform, "MusicVolume", "Music Volume", new Vector2(0, yPos));
            yPos -= 50;
            CreateSliderSetting(settingsContainer.transform, "SFXVolume", "SFX Volume", new Vector2(0, yPos));
            yPos -= 50;
            CreateSliderSetting(settingsContainer.transform, "VoiceVolume", "Voice Volume", new Vector2(0, yPos));
            yPos -= 50;
            CreateSliderSetting(settingsContainer.transform, "CrowdVolume", "Crowd Volume", new Vector2(0, yPos));

            var btnContainer = new GameObject("Buttons");
            btnContainer.transform.SetParent(panel.transform);
            btnContainer.AddComponent<RectTransform>();

            CreateButton(btnContainer.transform, "ApplyButton", "Apply", new Vector2(0, -250), new Vector2(200, 50));
            CreateButton(btnContainer.transform, "CloseButton", "Close", new Vector2(0, -320), new Vector2(200, 50));

            panel.SetActive(false);
        }

        private static void SetupResultsPanel(GameObject panel)
        {
            foreach (Transform child in panel.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            CreateText(panel.transform, "ResultTitle", "QUALIFIED!", new Vector2(0, 250), new Vector2(400, 60));
            CreateText(panel.transform, "TimeText", "Time: 00:00.00", new Vector2(0, 150), new Vector2(300, 40));
            CreateText(panel.transform, "FaultsText", "Faults: 0", new Vector2(0, 100), new Vector2(300, 40));
            CreateText(panel.transform, "ScoreText", "Score: 100.0", new Vector2(0, 50), new Vector2(300, 40));
            CreateText(panel.transform, "PositionText", "1st Place", new Vector2(0, 0), new Vector2(300, 40));
            CreateText(panel.transform, "PersonalBest", "", new Vector2(0, -50), new Vector2(300, 30));

            var btnContainer = new GameObject("Buttons");
            btnContainer.transform.SetParent(panel.transform);
            btnContainer.AddComponent<RectTransform>();

            CreateButton(btnContainer.transform, "ReplayButton", "Replay", new Vector2(-120, -150), new Vector2(200, 50));
            CreateButton(btnContainer.transform, "RetryButton", "Retry", new Vector2(120, -150), new Vector2(200, 50));
            CreateButton(btnContainer.transform, "NextButton", "Next Course", new Vector2(-120, -220), new Vector2(200, 50));
            CreateButton(btnContainer.transform, "MenuButton", "Main Menu", new Vector2(120, -220), new Vector2(200, 50));

            panel.SetActive(false);
        }

        private static void SetupModeSelectPanel(GameObject panel)
        {
            foreach (Transform child in panel.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            CreateText(panel.transform, "Title", "Select Mode", new Vector2(0, 250), new Vector2(400, 60));

            var modeContainer = new GameObject("ModeContainer");
            modeContainer.transform.SetParent(panel.transform);
            modeContainer.AddComponent<RectTransform>();

            float yPos = 100;
            CreateModeButton(modeContainer.transform, "TrainingButton", "Training Mode", "Practice courses", new Vector2(0, yPos));
            yPos -= 100;
            CreateModeButton(modeContainer.transform, "QuickPlayButton", "Quick Play", "Random competition", new Vector2(0, yPos));
            yPos -= 100;
            CreateModeButton(modeContainer.transform, "CareerButton", "Career Mode", "Build your legacy", new Vector2(0, yPos));

            CreateButton(panel.transform, "BackButton", "Back", new Vector2(0, -250), new Vector2(200, 50));

            panel.SetActive(false);
        }

        private static void SetupPausePanel(GameObject panel)
        {
            foreach (Transform child in panel.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            CreateText(panel.transform, "Title", "PAUSED", new Vector2(0, 150), new Vector2(400, 60));

            var btnContainer = new GameObject("Buttons");
            btnContainer.transform.SetParent(panel.transform);
            btnContainer.AddComponent<RectTransform>();

            CreateButton(btnContainer.transform, "ResumeButton", "Resume", new Vector2(0, 50), new Vector2(300, 60));
            CreateButton(btnContainer.transform, "RestartButton", "Restart", new Vector2(0, -30), new Vector2(300, 60));
            CreateButton(btnContainer.transform, "SettingsButton", "Settings", new Vector2(0, -110), new Vector2(300, 60));
            CreateButton(btnContainer.transform, "QuitButton", "Quit to Menu", new Vector2(0, -190), new Vector2(300, 60));

            panel.SetActive(false);
        }

        private static void SetupMenuManagerReferences(MenuManager menuManager, GameObject mainMenu, GameObject modeSelect,
            GameObject quickPlay, GameObject training, GameObject teamSelect, GameObject settings, GameObject results, GameObject pause)
        {
            SerializedObject so = new SerializedObject(menuManager);

            // Set panel references
            SetProperty(so, "mainMenuPanel", mainMenu);
            SetProperty(so, "modeSelectPanel", modeSelect);
            SetProperty(so, "quickPlayPanel", quickPlay);
            SetProperty(so, "trainingPanel", training);
            SetProperty(so, "teamSelectPanel", teamSelect);
            SetProperty(so, "settingsPanel", settings);
            SetProperty(so, "resultsPanel", results);
            SetProperty(so, "pausePanel", pause);

            // Find buttons in main menu
            var btnParent = mainMenu.transform.Find("ButtonsContainer");
            if (btnParent != null)
            {
                SetProperty(so, "quickPlayButton", btnParent.Find("QuickPlayButton")?.GetComponent<Button>());
                SetProperty(so, "trainingButton", btnParent.Find("TrainingButton")?.GetComponent<Button>());
                SetProperty(so, "careerButton", btnParent.Find("CareerButton")?.GetComponent<Button>());
                SetProperty(so, "settingsButton", btnParent.Find("SettingsButton")?.GetComponent<Button>());
                SetProperty(so, "quitButton", btnParent.Find("QuitButton")?.GetComponent<Button>());
            }

            // Find version text
            SetProperty(so, "versionText", mainMenu.transform.Find("VersionText")?.GetComponent<TextMeshProUGUI>());

            // Quick Play panel buttons
            var qpBtnContainer = quickPlay.transform.Find("Buttons");
            if (qpBtnContainer != null)
            {
                SetProperty(so, "startQuickPlayButton", qpBtnContainer.Find("StartButton")?.GetComponent<Button>());
                SetProperty(so, "backFromQuickPlayButton", qpBtnContainer.Find("BackButton")?.GetComponent<Button>());
            }

            // Training panel
            var trainBtnContainer = training.transform.Find("Buttons");
            if (trainBtnContainer != null)
            {
                SetProperty(so, "startTrainingButton", trainBtnContainer.Find("StartButton")?.GetComponent<Button>());
                SetProperty(so, "backFromTrainingButton", trainBtnContainer.Find("BackButton")?.GetComponent<Button>());
            }

            // Training course list container
            Transform courseList = training.transform.Find("CourseListContainer");
            if (courseList != null) SetProperty(so, "courseListContainer", courseList);

            // Team select
            Transform handlerList = teamSelect.transform.Find("HandlerSection/HandlerListContainer");
            if (handlerList != null) SetProperty(so, "handlerListContainer", handlerList);

            Transform dogList = teamSelect.transform.Find("DogSection/DogListContainer");
            if (dogList != null) SetProperty(so, "dogListContainer", dogList);

            var tsBtnContainer = teamSelect.transform.Find("Buttons");
            if (tsBtnContainer != null)
            {
                SetProperty(so, "startRunButton", tsBtnContainer.Find("StartRunButton")?.GetComponent<Button>());
                SetProperty(so, "backToModeSelectButton", tsBtnContainer.Find("BackButton")?.GetComponent<Button>());
            }

            // Settings
            var settingsBtnContainer = settings.transform.Find("Buttons");
            if (settingsBtnContainer != null)
            {
                SetProperty(so, "applySettingsButton", settingsBtnContainer.Find("ApplyButton")?.GetComponent<Button>());
                SetProperty(so, "closeSettingsButton", settingsBtnContainer.Find("CloseButton")?.GetComponent<Button>());
            }

            // Results
            var resultsBtnContainer = results.transform.Find("Buttons");
            if (resultsBtnContainer != null)
            {
                SetProperty(so, "replayButton", resultsBtnContainer.Find("ReplayButton")?.GetComponent<Button>());
                SetProperty(so, "retryButton", resultsBtnContainer.Find("RetryButton")?.GetComponent<Button>());
                SetProperty(so, "nextCourseButton", resultsBtnContainer.Find("NextButton")?.GetComponent<Button>());
                SetProperty(so, "returnToMenuButton", resultsBtnContainer.Find("MenuButton")?.GetComponent<Button>());
            }

            // Results texts
            SetProperty(so, "resultTitleText", results.transform.Find("ResultTitle")?.GetComponent<TextMeshProUGUI>());
            SetProperty(so, "resultTimeText", results.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>());
            SetProperty(so, "resultFaultsText", results.transform.Find("FaultsText")?.GetComponent<TextMeshProUGUI>());
            SetProperty(so, "resultScoreText", results.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>());
            SetProperty(so, "resultPositionText", results.transform.Find("PositionText")?.GetComponent<TextMeshProUGUI>());
            SetProperty(so, "personalBestText", results.transform.Find("PersonalBest")?.GetComponent<TextMeshProUGUI>());

            // Pause
            var pauseBtnContainer = pause.transform.Find("Buttons");
            if (pauseBtnContainer != null)
            {
                SetProperty(so, "resumeButton", pauseBtnContainer.Find("ResumeButton")?.GetComponent<Button>());
                SetProperty(so, "restartButton", pauseBtnContainer.Find("RestartButton")?.GetComponent<Button>());
                SetProperty(so, "pauseSettingsButton", pauseBtnContainer.Find("SettingsButton")?.GetComponent<Button>());
                SetProperty(so, "quitToMenuButton", pauseBtnContainer.Find("QuitButton")?.GetComponent<Button>());
            }

            // Load available data
            var handlers = Resources.LoadAll<HandlerData>("Data/Handlers");
            var dogs = Resources.LoadAll<BreedData>("Data/Breeds");
            var courses = Resources.LoadAll<CourseDefinition>("Data/Courses");

            SetProperty(so, "availableHandlers", handlers);
            SetProperty(so, "availableDogs", dogs);
            SetProperty(so, "availableCourses", courses);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(menuManager);

            Debug.Log("[SceneSetup] MenuManager references updated!");
        }

        private static void SetProperty(SerializedObject so, string name, object value)
        {
            var prop = so.FindProperty(name);
            if (prop != null)
            {
                if (value is UnityEngine.Object obj)
                    prop.objectReferenceValue = obj;
                // Note: arrayReferenceValue doesn't exist in Unity API - arrays need different handling
            }
        }

        private static GameObject CreateText(Transform parent, string name, string text, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var textComp = go.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.fontSize = 36;
            textComp.alignment = TextAlignmentOptions.Center;
            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;
            var textComp = textGo.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.fontSize = 24;
            textComp.alignment = TextAlignmentOptions.Center;

            return go;
        }

        private static void CreateModeButton(Transform parent, string name, string title, string desc, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(400, 80);
            go.AddComponent<Button>();

            var titleGo = CreateText(go.transform, "Title", title, new Vector2(0, 10), new Vector2(380, 40));
            titleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            var descGo = CreateText(go.transform, "Desc", desc, new Vector2(0, -20), new Vector2(380, 30));
            descGo.GetComponent<TextMeshProUGUI>().fontSize = 20;
            descGo.GetComponent<TextMeshProUGUI>().color = Color.gray;
            descGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        }

        private static void CreateSliderSetting(Transform parent, string name, string label, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(400, 40);

            var labelGo = CreateText(go.transform, "Label", label, new Vector2(-150, 0), new Vector2(150, 30));
            labelGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            labelGo.GetComponent<TextMeshProUGUI>().fontSize = 20;

            var sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(go.transform);
            var sliderRect = sliderGo.AddComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(80, 0);
            sliderRect.sizeDelta = new Vector2(200, 30);
            sliderGo.AddComponent<Slider>();
        }

        private static void CreateAllBreedDataAssets()
        {
            CreateBreedDataIfNotExists("BorderCollie", "Assets/Red_Deer/Dogs/Border_Collie/Dog/Prefabs/BorderCollie_anim_IP.prefab");
            CreateBreedDataIfNotExists("Corgi", "Assets/Red_Deer/Dogs/Corgi/Dog/Prefabs/Corgi_anim_IP.prefab");
            CreateBreedDataIfNotExists("JackRussellTerrier", "Assets/Red_Deer/Dogs/JackRussellTerrier/Dog/Prefabs/JRTerrier_anim_IP.prefab");
            CreateBreedDataIfNotExists("GoldenRetriever", "Assets/Red_Deer/Dogs/GoldenRetriever/Dog/Prefabs/Retriever_anim_IP.prefab");
            CreateBreedDataIfNotExists("ShibaInu", "Assets/Red_Deer/Dogs/Spitz/Dog/Prefabs/Spitz_anim_IP.prefab");
            CreateBreedDataIfNotExists("Beagle", "Assets/Red_Deer/Dogs/Beagle/Dog/Prefabs/Beagle_anim_IP.prefab");
            CreateBreedDataIfNotExists("Boxer", "Assets/Red_Deer/Dogs/Boxer/Dog/Prefabs/Dog_Boxer_anim_IP.prefab");
            CreateBreedDataIfNotExists("Husky", "Assets/Red_Deer/Dogs/Husky/Dog/Prefabs/Husky_anim_IP.prefab");
            CreateBreedDataIfNotExists("Labrador", "Assets/Red_Deer/Dogs/Labrador/Dog/Prefabs/Labrador_anim_IP.prefab");
            CreateBreedDataIfNotExists("Rottweiler", "Assets/Red_Deer/Dogs/Rottweiler/Dog/Prefabs/Rottweiler_anim_IP.prefab");
            CreateBreedDataIfNotExists("Pitbull", "Assets/Red_Deer/Dogs/Pitbull/Dog/Prefabs/Pitbull_anim_IP.prefab");
            CreateBreedDataIfNotExists("Dalmatian", "Assets/Red_Deer/Dogs/Dalmatian/Dog/Prefabs/Dalmatian_anim_IP.prefab");
            CreateBreedDataIfNotExists("Doberman", "Assets/Red_Deer/Dogs/Doberman/Dog/Prefabs/Doberman_anim_IP.prefab");
            CreateBreedDataIfNotExists("FrenchBulldog", "Assets/Red_Deer/Dogs/FrenchBulldog/Dog/Prefabs/FrenchBulldog_anim_IP.prefab");
            CreateBreedDataIfNotExists("Pug", "Assets/Red_Deer/Dogs/Pug/Dog/Prefabs/Pug_anim_IP.prefab");
            CreateBreedDataIfNotExists("Shepherd", "Assets/Red_Deer/Dogs/Shepherd/Dog/Prefabs/Shepherd_anim_IP.prefab");
            CreateBreedDataIfNotExists("BullTerrier", "Assets/Red_Deer/Dogs/BullTerrier/Dog/Prefabs/BullTerrier_anim_IP.prefab");
            CreateBreedDataIfNotExists("ToyTerrier", "Assets/Red_Deer/Dogs/ToyTerrier/Dog/Prefabs/ToyTerrier_anim_IP.prefab");
            CreateBreedDataIfNotExists("Spitz", "Assets/Red_Deer/Dogs/Spitz/Dog/Prefabs/Spitz_anim_IP.prefab");

            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetup] All BreedData assets created!");
        }

        private static void CreateBreedDataIfNotExists(string breedName, string prefabPath)
        {
            string assetPath = $"Assets/Data/Breeds/{breedName}.asset";

            if (AssetDatabase.LoadAssetAtPath<BreedData>(assetPath) != null)
            {
                Debug.Log($"[SceneSetup] BreedData already exists: {breedName}");
                return;
            }

            if (!System.IO.File.Exists(prefabPath))
            {
                Debug.LogWarning($"[SceneSetup] Prefab not found: {prefabPath}");
                return;
            }

            var breedData = ScriptableObject.CreateInstance<BreedData>();
            breedData.breedName = breedName;
            breedData.displayName = breedName;
            breedData.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            AssetDatabase.CreateAsset(breedData, assetPath);
            Debug.Log($"[SceneSetup] Created BreedData: {breedName}");
        }

        private static void CreateUIPrefabs()
        {
            if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
            {
                System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
            }

            // Create Handler Entry Prefab
            var handlerEntry = new GameObject("HandlerEntryPrefab");
            handlerEntry.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 80);
            handlerEntry.AddComponent<Button>();
            handlerEntry.AddComponent<Image>();

            var hPortrait = new GameObject("Portrait");
            hPortrait.transform.SetParent(handlerEntry.transform);
            hPortrait.AddComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
            hPortrait.AddComponent<Image>();

            var hNameText = new GameObject("NameText");
            hNameText.transform.SetParent(handlerEntry.transform);
            var hNameRect = hNameText.AddComponent<RectTransform>();
            hNameRect.anchoredPosition = new Vector2(40, 0);
            hNameRect.sizeDelta = new Vector2(140, 30);
            var hNameTextComp = hNameText.AddComponent<TextMeshProUGUI>();
            hNameTextComp.fontSize = 18;

            PrefabUtility.SaveAsPrefabAsset(handlerEntry, "Assets/Prefabs/UI/HandlerEntryPrefab.prefab");
            Debug.Log("[SceneSetup] Created HandlerEntryPrefab");
            DestroyImmediate(handlerEntry);

            // Create Dog Entry Prefab
            var dogEntry = new GameObject("DogEntryPrefab");
            dogEntry.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 100);
            dogEntry.AddComponent<Button>();
            dogEntry.AddComponent<Image>();

            var dPortrait = new GameObject("Portrait");
            dPortrait.transform.SetParent(dogEntry.transform);
            dPortrait.AddComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            dPortrait.AddComponent<Image>();

            var dNameText = new GameObject("NameText");
            dNameText.transform.SetParent(dogEntry.transform);
            var dNameRect = dNameText.AddComponent<RectTransform>();
            dNameRect.anchoredPosition = new Vector2(50, 15);
            dNameRect.sizeDelta = new Vector2(140, 30);
            var dNameTextComp = dNameText.AddComponent<TextMeshProUGUI>();
            dNameTextComp.fontSize = 18;

            var dBreedText = new GameObject("BreedText");
            dBreedText.transform.SetParent(dogEntry.transform);
            var dBreedRect = dBreedText.AddComponent<RectTransform>();
            dBreedRect.anchoredPosition = new Vector2(50, -15);
            dBreedRect.sizeDelta = new Vector2(140, 25);
            var dBreedTextComp = dBreedText.AddComponent<TextMeshProUGUI>();
            dBreedTextComp.fontSize = 14;
            dBreedTextComp.color = Color.gray;

            PrefabUtility.SaveAsPrefabAsset(dogEntry, "Assets/Prefabs/UI/DogEntryPrefab.prefab");
            Debug.Log("[SceneSetup] Created DogEntryPrefab");
            DestroyImmediate(dogEntry);

            // Create Course Entry Prefab
            var courseEntry = new GameObject("CourseEntryPrefab");
            courseEntry.AddComponent<RectTransform>().sizeDelta = new Vector2(300, 60);
            courseEntry.AddComponent<Button>();
            courseEntry.AddComponent<Image>();

            var cNameText = new GameObject("CourseNameText");
            cNameText.transform.SetParent(courseEntry.transform);
            cNameText.AddComponent<RectTransform>().sizeDelta = new Vector2(280, 40);
            var cNameTextComp = cNameText.AddComponent<TextMeshProUGUI>();
            cNameTextComp.fontSize = 20;
            cNameTextComp.alignment = TextAlignmentOptions.Left;

            PrefabUtility.SaveAsPrefabAsset(courseEntry, "Assets/Prefabs/UI/CourseEntryPrefab.prefab");
            Debug.Log("[SceneSetup] Created CourseEntryPrefab");
            DestroyImmediate(courseEntry);

            AssetDatabase.SaveAssets();
        }

        private static void SetupSampleScene()
        {
            Debug.Log("[SceneSetup] Setting up SampleScene...");

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != "SampleScene")
            {
                Debug.LogError("[SceneSetup] Please open SampleScene first!");
                return;
            }

            // Find or create GameManager
            var gameManager = GameObject.Find("GameManager");
            if (gameManager == null)
            {
                gameManager = new GameObject("GameManager");
                gameManager.AddComponent<GameManager>();
            }

            // Find or create CourseRunner
            var courseRunner = GameObject.Find("CourseRunner");
            if (courseRunner == null)
            {
                courseRunner = new GameObject("CourseRunner");
                courseRunner.AddComponent<CourseRunner>();
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("[SceneSetup] SampleScene setup complete!");
        }
    }
}
#endif
