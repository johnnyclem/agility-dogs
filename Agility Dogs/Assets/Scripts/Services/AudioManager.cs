using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.Services
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource commentarySource;
        [SerializeField] private AudioSource crowdSource;
        [SerializeField] private AudioSource dogMovementSource;

        [Header("Music")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip tensionMusic;
        [SerializeField] private float musicFadeTime = 1f;

        [Header("Ambience")]
        [SerializeField] private AudioClip outdoorAmbience;
        [SerializeField] private AudioClip indoorAmbience;
        [SerializeField] private float ambienceVolume = 0.3f;

        [Header("Dog Sounds")]
        [SerializeField] private AudioClip[] dogFootsteps;
        [SerializeField] private AudioClip[] dogPants;
        [SerializeField] private AudioClip dogBark;
        [SerializeField] private float footstepInterval = 0.3f;
        [SerializeField] private float minPitch = 0.9f;
        [SerializeField] private float maxPitch = 1.1f;

        [Header("Obstacle Sounds")]
        [SerializeField] private AudioClip jumpWhoosh;
        [SerializeField] private AudioClip barKnock;
        [SerializeField] private AudioClip tunnelEnter;
        [SerializeField] private AudioClip tunnelExit;
        [SerializeField] private AudioClip weavePoles;
        [SerializeField] private AudioClip contactZone;
        [SerializeField] private AudioClip tableImpact;
        [SerializeField] private AudioClip teeterBang;

        [Header("Handler Sounds")]
        [SerializeField] private AudioClip[] handlerFootsteps;
        [SerializeField] private AudioClip handlerBreathing;
        [SerializeField] private AudioClip gestureWhoosh;

        [Header("Crowd Sounds")]
        [SerializeField] private AudioClip crowdIdle;
        [SerializeField] private AudioClip crowdCheering;
        [SerializeField] private AudioClip crowdOvation;
        [SerializeField] private AudioClip crowdGasp;
        [SerializeField] private AudioClip crowdApplause;
        [SerializeField] private float crowdFadeTime = 0.5f;

        [Header("UI Sounds")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip countdownBeep;
        [SerializeField] private AudioClip countdownGo;
        [SerializeField] private AudioClip splitTimeAlert;
        [SerializeField] private AudioClip faultAlert;
        [SerializeField] private AudioClip personalBestAlert;

        [Header("Sound Priority")]
        [SerializeField] private float commentaryPriorityVolume = 0.8f;
        [SerializeField] private float replayDuckingAmount = 0.3f;

        // Volume settings
        private float musicVolume = 1f;
        private float sfxVolume = 1f;
        private float voiceVolume = 1f;
        private float crowdVolume = 1f;

        // State
        private bool isReplayActive;
        private float currentFootstepTimer;
        private DogState lastDogState;
        private float crowdIntensity;
        private float targetCrowdIntensity;
        private Coroutine musicFadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadVolumeSettings();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnObstacleCompleted += HandleObstacleCompleted;
            GameEvents.OnFaultCommitted += HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded += HandleSplitTime;
            GameEvents.OnRunStarted += HandleRunStarted;
            GameEvents.OnRunCompleted += HandleRunCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnObstacleCompleted -= HandleObstacleCompleted;
            GameEvents.OnFaultCommitted -= HandleFaultCommitted;
            GameEvents.OnSplitTimeRecorded -= HandleSplitTime;
            GameEvents.OnRunStarted -= HandleRunStarted;
            GameEvents.OnRunCompleted -= HandleRunCompleted;
        }

        private void Update()
        {
            UpdateCrowdIntensity();
            UpdateDynamicMix();
        }

        #region Volume Control

        private void LoadVolumeSettings()
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
            crowdVolume = PlayerPrefs.GetFloat("CrowdVolume", 1f);

            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume);
            if (musicSource != null) musicSource.volume = musicVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = volume;
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = volume;
            PlayerPrefs.SetFloat("VoiceVolume", volume);
            if (commentarySource != null) commentarySource.volume = voiceVolume;
        }

        public void SetCrowdVolume(float volume)
        {
            crowdVolume = volume;
            PlayerPrefs.SetFloat("CrowdVolume", volume);
        }

        private void UpdateAllVolumes()
        {
            if (musicSource != null) musicSource.volume = musicVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            if (commentarySource != null) commentarySource.volume = voiceVolume;
        }

        #endregion

        #region Music

        public void PlayMainMenuMusic()
        {
            PlayMusic(mainMenuMusic, true);
        }

        public void PlayGameplayMusic()
        {
            PlayMusic(gameplayMusic, true);
        }

        public void PlayVictoryMusic()
        {
            PlayMusic(victoryMusic, false);
        }

        public void PlayTensionMusic()
        {
            PlayMusic(tensionMusic, true);
        }

        private void PlayMusic(AudioClip clip, bool loop)
        {
            if (musicSource == null || clip == null) return;

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }

            musicFadeCoroutine = StartCoroutine(FadeMusic(clip, loop));
        }

        private System.Collections.IEnumerator FadeMusic(AudioClip newClip, bool loop)
        {
            float startVolume = musicSource.volume;

            // Fade out
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / musicFadeTime;
                yield return null;
            }

            // Change clip
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            // Fade in
            while (musicSource.volume < musicVolume)
            {
                musicSource.volume += musicVolume * Time.deltaTime / musicFadeTime;
                yield return null;
            }

            musicSource.volume = musicVolume;
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void SetMusicPitch(float pitch)
        {
            if (musicSource != null)
            {
                musicSource.pitch = pitch;
            }
        }

        #endregion

        #region Dog Sounds

        public void PlayDogFootstep(Vector3 position, float speed)
        {
            if (dogFootsteps == null || dogFootsteps.Length == 0) return;

            currentFootstepTimer -= Time.deltaTime;
            if (currentFootstepTimer <= 0)
            {
                AudioClip clip = dogFootsteps[Random.Range(0, dogFootsteps.Length)];
                PlaySFXAtPosition(clip, position, 0.5f, 1f + speed * 0.1f);

                currentFootstepTimer = footstepInterval / Mathf.Max(0.5f, speed);
            }
        }

        public void PlayDogBark(Vector3 position)
        {
            if (dogBark != null)
            {
                PlaySFXAtPosition(dogBark, position, 0.8f);
            }
        }

        public void PlayDogPant(Vector3 position)
        {
            if (dogPants == null || dogPants.Length == 0) return;

            AudioClip clip = dogPants[Random.Range(0, dogPants.Length)];
            PlaySFXAtPosition(clip, position, 0.3f);
        }

        #endregion

        #region Obstacle Sounds

        public void PlayJumpWhoosh(Vector3 position)
        {
            PlaySFXAtPosition(jumpWhoosh, position, 0.7f);
        }

        public void PlayBarKnock(Vector3 position)
        {
            PlaySFXAtPosition(barKnock, position, 1f);
        }

        public void PlayTunnelEnter(Vector3 position)
        {
            PlaySFXAtPosition(tunnelEnter, position, 0.6f);
        }

        public void PlayTunnelExit(Vector3 position)
        {
            PlaySFXAtPosition(tunnelExit, position, 0.6f);
        }

        public void PlayWeavePoles(Vector3 position)
        {
            PlaySFXAtPosition(weavePoles, position, 0.5f);
        }

        public void PlayContactZone(Vector3 position)
        {
            PlaySFXAtPosition(contactZone, position, 0.8f);
        }

        public void PlayTableImpact(Vector3 position)
        {
            PlaySFXAtPosition(tableImpact, position, 0.9f);
        }

        public void PlayTeeterBang(Vector3 position)
        {
            PlaySFXAtPosition(teeterBang, position, 1f);
        }

        #endregion

        #region Handler Sounds

        public void PlayHandlerFootstep(Vector3 position)
        {
            if (handlerFootsteps == null || handlerFootsteps.Length == 0) return;

            AudioClip clip = handlerFootsteps[Random.Range(0, handlerFootsteps.Length)];
            PlaySFXAtPosition(clip, position, 0.4f);
        }

        public void PlayGestureWhoosh(Vector3 position)
        {
            PlaySFXAtPosition(gestureWhoosh, position, 0.5f);
        }

        #endregion

        #region Crowd Sounds

        public void SetCrowdIntensity(float intensity)
        {
            targetCrowdIntensity = Mathf.Clamp01(intensity);
        }

        public void TriggerCrowdReaction(CrowdReaction reaction)
        {
            AudioClip clip = null;

            switch (reaction)
            {
                case CrowdReaction.Applause:
                    clip = crowdApplause;
                    SetCrowdIntensity(0.8f);
                    break;
                case CrowdReaction.Cheering:
                    clip = crowdCheering;
                    SetCrowdIntensity(0.9f);
                    break;
                case CrowdReaction.Ovation:
                    clip = crowdOvation;
                    SetCrowdIntensity(1f);
                    break;
                case CrowdReaction.Gasp:
                    clip = crowdGasp;
                    break;
            }

            if (clip != null && crowdSource != null)
            {
                crowdSource.PlayOneShot(clip, crowdVolume);
            }
        }

        private void UpdateCrowdIntensity()
        {
            crowdIntensity = Mathf.Lerp(crowdIntensity, targetCrowdIntensity, Time.deltaTime * 2f);

            // Adjust crowd audio based on intensity
            if (crowdSource != null && crowdSource.clip != null)
            {
                crowdSource.volume = crowdIntensity * crowdVolume;
            }

            // Decay intensity over time
            targetCrowdIntensity = Mathf.Lerp(targetCrowdIntensity, 0.2f, Time.deltaTime * 0.5f);
        }

        #endregion

        #region UI Sounds

        public void PlayButtonClick()
        {
            PlaySFX(buttonClick);
        }

        public void PlayButtonHover()
        {
            PlaySFX(buttonHover);
        }

        public void PlayCountdownBeep()
        {
            PlaySFX(countdownBeep);
        }

        public void PlayCountdownGo()
        {
            PlaySFX(countdownGo);
        }

        public void PlaySplitTimeAlert()
        {
            PlaySFX(splitTimeAlert);
        }

        public void PlayFaultAlert()
        {
            PlaySFX(faultAlert);
        }

        public void PlayPersonalBestAlert()
        {
            PlaySFX(personalBestAlert);
        }

        #endregion

        #region Ambience

        public void PlayOutdoorAmbience()
        {
            PlayAmbience(outdoorAmbience);
        }

        public void PlayIndoorAmbience()
        {
            PlayAmbience(indoorAmbience);
        }

        private void PlayAmbience(AudioClip clip)
        {
            if (ambienceSource == null || clip == null) return;

            ambienceSource.clip = clip;
            ambienceSource.loop = true;
            ambienceSource.volume = ambienceVolume;
            ambienceSource.Play();
        }

        public void StopAmbience()
        {
            if (ambienceSource != null)
            {
                ambienceSource.Stop();
            }
        }

        #endregion

        #region Dynamic Mix

        public void SetReplayMode(bool isReplay)
        {
            isReplayActive = isReplay;
        }

        private void UpdateDynamicMix()
        {
            // Duck other audio when commentary is playing
            if (commentarySource != null && commentarySource.isPlaying)
            {
                if (sfxSource != null) sfxSource.volume = sfxVolume * (1f - commentaryPriorityVolume);
                if (musicSource != null) musicSource.volume = musicVolume * (1f - commentaryPriorityVolume * 0.5f);
            }
            else
            {
                if (sfxSource != null) sfxSource.volume = sfxVolume;
                if (musicSource != null) musicSource.volume = musicVolume;
            }

            // Duck during replay
            if (isReplayActive)
            {
                if (musicSource != null) musicSource.volume = musicVolume * replayDuckingAmount;
            }
        }

        #endregion

        #region Helper Methods

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        private void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volumeScale);

            // For pitch variation, we'd need to use an AudioSource at the position
            // This is a simplified version
        }

        #endregion

        #region Event Handlers

        private void HandleGameStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.MainMenu:
                    PlayMainMenuMusic();
                    PlayIndoorAmbience();
                    break;
                case GameState.Countdown:
                    PlayCountdownBeep();
                    break;
                case GameState.Gameplay:
                    PlayGameplayMusic();
                    PlayOutdoorAmbience();
                    SetCrowdIntensity(0.3f);
                    break;
                case GameState.RunComplete:
                    // Handled by OnRunCompleted
                    break;
            }
        }

        private void HandleObstacleCompleted(ObstacleType type, bool clean)
        {
            if (clean)
            {
                SetCrowdIntensity(crowdIntensity + 0.1f);
            }
        }

        private void HandleFaultCommitted(FaultType fault, string obstacleName)
        {
            PlayFaultAlert();
            TriggerCrowdReaction(CrowdReaction.Gasp);
        }

        private void HandleSplitTime(float time)
        {
            PlaySplitTimeAlert();
            TriggerCrowdReaction(CrowdReaction.Applause);
        }

        private void HandleRunStarted()
        {
            SetCrowdIntensity(0.3f);
        }

        private void HandleRunCompleted(RunResult result, float time, int faults)
        {
            switch (result)
            {
                case RunResult.Qualified:
                    PlayVictoryMusic();
                    TriggerCrowdReaction(CrowdReaction.Ovation);
                    PlayPersonalBestAlert();
                    break;
                case RunResult.NonQualified:
                    TriggerCrowdReaction(CrowdReaction.Applause);
                    break;
                case RunResult.Elimination:
                    // No celebration sounds
                    break;
            }
        }

        #endregion
    }

    public enum CrowdReaction
    {
        Applause,
        Cheering,
        Ovation,
        Gasp
    }
}
