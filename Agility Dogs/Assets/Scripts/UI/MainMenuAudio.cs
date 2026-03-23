using System.Collections;
using UnityEngine;

namespace AgilityDogs.UI
{
    /// <summary>
    /// Manages audio for the main menu, including background music and ambience.
    /// </summary>
    public class MainMenuAudio : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambienceSource;

        [Header("Music")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip[] musicTracks;
        [SerializeField] private bool shuffleMusic = false;
        [SerializeField] private float musicFadeInDuration = 2f;
        [SerializeField] private float musicFadeOutDuration = 1.5f;
        [SerializeField] private float crossfadeDuration = 1f;
        [SerializeField] private float pauseBetweenTracks = 5f;

        [Header("Ambience")]
        [SerializeField] private AudioClip menuAmbience;
        [SerializeField] private float ambienceVolume = 0.3f;

        [Header("Settings")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loopMusic = true;
        [SerializeField] private bool autoPlayNextTrack = true;
        [SerializeField] private float targetMusicVolume = 0.5f;
        [SerializeField] private float targetAmbienceVolume = 0.3f;

        // State
        private bool isPlaying = false;
        private int currentTrackIndex = 0;
        private Coroutine musicFadeCoroutine;
        private Coroutine ambienceFadeCoroutine;

        private void Start()
        {
            // Setup audio sources
            SetupAudioSources();

            // Start playing if enabled
            if (playOnStart)
            {
                StartMusic();
            }
        }

        private void SetupAudioSources()
        {
            // Music source
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.spatialBlend = 0f; // 2D
                musicSource.loop = loopMusic;
            }

            // Ambience source
            if (ambienceSource == null)
            {
                ambienceSource = gameObject.AddComponent<AudioSource>();
                ambienceSource.playOnAwake = false;
                ambienceSource.spatialBlend = 0f; // 2D
                ambienceSource.loop = true;
            }
        }

        #region Public API

        /// <summary>
        /// Start playing the main menu music.
        /// </summary>
        public void StartMusic()
        {
            if (isPlaying) return;

            isPlaying = true;

            // Start with main menu music or first track
            if (mainMenuMusic != null)
            {
                PlayTrack(mainMenuMusic);
            }
            else if (musicTracks != null && musicTracks.Length > 0)
            {
                PlayTrack(musicTracks[0]);
            }

            // Start ambience
            StartAmbience();

            Debug.Log("[MainMenuAudio] Music started");
        }

        /// <summary>
        /// Stop the main menu music.
        /// </summary>
        public void StopMusic(float fadeOutDuration = -1f)
        {
            if (!isPlaying) return;

            if (fadeOutDuration < 0) fadeOutDuration = this.musicFadeOutDuration;

            // Fade out music
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            musicFadeCoroutine = StartCoroutine(FadeOutMusic(fadeOutDuration));

            // Stop ambience
            StopAmbience();

            isPlaying = false;

            Debug.Log("[MainMenuAudio] Music stopping");
        }

        /// <summary>
        /// Pause the music.
        /// </summary>
        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
            }

            if (ambienceSource != null && ambienceSource.isPlaying)
            {
                ambienceSource.Pause();
            }
        }

        /// <summary>
        /// Resume the music.
        /// </summary>
        public void ResumeMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.UnPause();
            }

            if (ambienceSource != null && ambienceSource.isPlaying)
            {
                ambienceSource.UnPause();
            }
        }

        /// <summary>
        /// Play a specific track.
        /// </summary>
        public void PlayTrack(AudioClip track, bool fade = true)
        {
            if (track == null || musicSource == null) return;

            if (fade)
            {
                // Crossfade to new track
                if (musicFadeCoroutine != null)
                {
                    StopCoroutine(musicFadeCoroutine);
                }
                musicFadeCoroutine = StartCoroutine(CrossfadeToTrack(track));
            }
            else
            {
                // Immediate switch
                musicSource.clip = track;
                musicSource.volume = targetMusicVolume;
                musicSource.Play();

                // Check if we should play next track
                if (autoPlayNextTrack && !loopMusic)
                {
                    StartCoroutine(PlayNextTrackAfterDelay(pauseBetweenTracks));
                }
            }

            Debug.Log($"[MainMenuAudio] Playing track: {track.name}");
        }

        /// <summary>
        /// Play the next track in the playlist.
        /// </summary>
        public void PlayNextTrack()
        {
            if (musicTracks == null || musicTracks.Length == 0) return;

            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;

            // Shuffle if enabled
            if (shuffleMusic && musicTracks.Length > 1)
            {
                int newIndex;
                do
                {
                    newIndex = Random.Range(0, musicTracks.Length);
                } while (newIndex == currentTrackIndex);
                currentTrackIndex = newIndex;
            }

            PlayTrack(musicTracks[currentTrackIndex]);
        }

        /// <summary>
        /// Set the music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            targetMusicVolume = Mathf.Clamp01(volume);
            if (musicSource != null && !IsFading())
            {
                musicSource.volume = targetMusicVolume;
            }
        }

        /// <summary>
        /// Set the ambience volume.
        /// </summary>
        public void SetAmbienceVolume(float volume)
        {
            targetAmbienceVolume = Mathf.Clamp01(volume);
            if (ambienceSource != null && !IsFading())
            {
                ambienceSource.volume = targetAmbienceVolume;
            }
        }

        /// <summary>
        /// Check if music is currently playing.
        /// </summary>
        public bool IsPlaying => isPlaying && musicSource != null && musicSource.isPlaying;

        /// <summary>
        /// Get the current track name.
        /// </summary>
        public string GetCurrentTrackName()
        {
            if (musicSource != null && musicSource.clip != null)
            {
                return musicSource.clip.name;
            }
            return "None";
        }

        #endregion

        #region Ambience

        private void StartAmbience()
        {
            if (ambienceSource == null || menuAmbience == null) return;

            ambienceSource.clip = menuAmbience;
            ambienceSource.volume = 0f;
            ambienceSource.Play();

            // Fade in ambience
            if (ambienceFadeCoroutine != null)
            {
                StopCoroutine(ambienceFadeCoroutine);
            }
            ambienceFadeCoroutine = StartCoroutine(FadeInAmbience());
        }

        private void StopAmbience(float fadeOutDuration = 1f)
        {
            if (ambienceSource == null) return;

            if (ambienceFadeCoroutine != null)
            {
                StopCoroutine(ambienceFadeCoroutine);
            }
            ambienceFadeCoroutine = StartCoroutine(FadeOutAmbience(fadeOutDuration));
        }

        #endregion

        #region Coroutines

        private void PlayTrackImmediate(AudioClip track)
        {
            if (track == null || musicSource == null) return;

            musicSource.clip = track;
            musicSource.volume = targetMusicVolume;
            musicSource.Play();

            // Check if we should play next track
            if (autoPlayNextTrack && !loopMusic)
            {
                StartCoroutine(PlayNextTrackAfterDelay(pauseBetweenTracks));
            }
        }

        private IEnumerator CrossfadeToTrack(AudioClip newTrack)
        {
            if (musicSource == null || newTrack == null) yield break;

            // Fade out current track
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < crossfadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / (crossfadeDuration / 2f));
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            // Switch track
            musicSource.clip = newTrack;
            musicSource.Play();

            // Fade in new track
            elapsed = 0f;
            while (elapsed < crossfadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / (crossfadeDuration / 2f));
                musicSource.volume = Mathf.Lerp(0f, targetMusicVolume, t);
                yield return null;
            }

            musicSource.volume = targetMusicVolume;

            // Check if we should play next track
            if (autoPlayNextTrack && !loopMusic)
            {
                StartCoroutine(PlayNextTrackAfterDelay(pauseBetweenTracks));
            }
        }

        private IEnumerator FadeOutMusic(float duration)
        {
            if (musicSource == null) yield break;

            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }

        private IEnumerator FadeInAmbience()
        {
            if (ambienceSource == null) yield break;

            float elapsed = 0f;

            while (elapsed < musicFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / musicFadeInDuration);
                ambienceSource.volume = Mathf.Lerp(0f, targetAmbienceVolume, t);
                yield return null;
            }

            ambienceSource.volume = targetAmbienceVolume;
        }

        private IEnumerator FadeOutAmbience(float duration)
        {
            if (ambienceSource == null) yield break;

            float startVolume = ambienceSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                ambienceSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            ambienceSource.Stop();
            ambienceSource.volume = startVolume;
        }

        private IEnumerator PlayNextTrackAfterDelay(float delay)
        {
            // Wait for track to finish
            yield return new WaitForSeconds(delay);

            // Play next track
            if (isPlaying)
            {
                PlayNextTrack();
            }
        }

        private bool IsFading()
        {
            return musicFadeCoroutine != null || ambienceFadeCoroutine != null;
        }

        #endregion

        private void OnDestroy()
        {
            // Stop all coroutines
            StopAllCoroutines();
        }
    }
}