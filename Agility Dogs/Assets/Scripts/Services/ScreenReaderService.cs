using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Service for screen reader compatibility and accessibility announcements.
    /// Provides methods to announce UI elements, game state changes, and important events.
    /// </summary>
    public class ScreenReaderService : MonoBehaviour
    {
        public static ScreenReaderService Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool screenReaderEnabled = false;
        [SerializeField] private float announcementDelay = 0.1f;
        [SerializeField] private float interruptionGracePeriod = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip focusSound;
        [SerializeField] private AudioClip announcementSound;
        [SerializeField] private float focusSoundVolume = 0.3f;
        [SerializeField] private float announcementSoundVolume = 0.5f;

        // Queue system
        private Queue<Announcement> announcementQueue = new Queue<Announcement>();
        private Announcement currentAnnouncement;
        private float lastAnnouncementTime;
        private AudioSource audioSource;

        // Navigation state
        private GameObject lastFocusedElement;
        private bool isNavigating;

        // Events
        public event Action<bool> OnScreenReaderToggled;
        public event Action<string> OnAnnouncementMade;
        public event Action<GameObject> OnElementFocused;

        // Properties
        public bool ScreenReaderEnabled
        {
            get => screenReaderEnabled;
            set
            {
                if (screenReaderEnabled != value)
                {
                    screenReaderEnabled = value;
                    PlayerPrefs.SetInt("ScreenReaderEnabled", value ? 1 : 0);
                    OnScreenReaderToggled?.Invoke(value);
                    
                    if (value)
                    {
                        Announce("Screen reader enabled. Use arrow keys or tab to navigate.");
                    }
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Get or add audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D

            LoadSettings();
        }

        private void Update()
        {
            if (!screenReaderEnabled) return;

            HandleNavigationInput();
            ProcessAnnouncementQueue();
        }

        #region Public API

        /// <summary>
        /// Announce a message to screen readers.
        /// </summary>
        public void Announce(string message, AnnouncementPriority priority = AnnouncementPriority.Normal)
        {
            if (!screenReaderEnabled) return;

            var announcement = new Announcement
            {
                message = message,
                priority = priority,
                timestamp = Time.time
            };

            if (priority == AnnouncementPriority.Interrupt)
            {
                // Clear queue and interrupt current announcement
                announcementQueue.Clear();
                if (currentAnnouncement != null)
                {
                    StopCurrentAnnouncement();
                }
                PlayAnnouncementImmediate(announcement);
            }
            else
            {
                announcementQueue.Enqueue(announcement);
            }

            OnAnnouncementMade?.Invoke(message);
            Debug.Log($"[ScreenReader] Announce: {message}");
        }

        /// <summary>
        /// Announce a UI element's state or description.
        /// </summary>
        public void AnnounceElement(GameObject element, string customMessage = "")
        {
            if (!screenReaderEnabled || element == null) return;

            string message = customMessage;

            if (string.IsNullOrEmpty(message))
            {
                // Try to get description from accessibility component
                var accessibility = element.GetComponent<AccessibilityDescriptor>();
                if (accessibility != null)
                {
                    message = accessibility.GetScreenReaderDescription();
                }
                else
                {
                    // Fallback: use UI element properties
                    message = GetElementDescription(element);
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                Announce(message);
                FocusElement(element);
            }
        }

        /// <summary>
        /// Announce game state changes (timer, score, faults, etc.)
        /// </summary>
        public void AnnounceGameState(string stateType, string value, string context = "")
        {
            if (!screenReaderEnabled) return;

            string message = string.IsNullOrEmpty(context) 
                ? $"{stateType}: {value}" 
                : $"{stateType}: {value}. {context}";

            Announce(message, AnnouncementPriority.Normal);
        }

        /// <summary>
        /// Announce time-sensitive information (countdown, split times, etc.)
        /// </summary>
        public void AnnounceUrgent(string message)
        {
            Announce(message, AnnouncementPriority.High);
        }

        /// <summary>
        /// Announce critical events (faults, elimination, etc.)
        /// </summary>
        public void AnnounceCritical(string message)
        {
            Announce(message, AnnouncementPriority.Interrupt);
        }

        /// <summary>
        /// Provide audio feedback for UI interactions.
        /// </summary>
        public void PlayFocusSound()
        {
            if (!screenReaderEnabled || focusSound == null) return;
            audioSource.PlayOneShot(focusSound, focusSoundVolume);
        }

        #endregion

        #region Navigation

        private void HandleNavigationInput()
        {
            // Tab navigation
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                NavigateToNextElement(!shiftHeld);
            }

            // Arrow key navigation within groups
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateDirection(1);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateDirection(-1);
            }

            // Enter/Space to activate
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ActivateCurrentElement();
            }
        }

        private void NavigateToNextElement(bool forward)
        {
            var current = EventSystem.current?.currentSelectedGameObject;
            if (current == null) return;

            // Find next/previous selectable element
            Selectable next = forward 
                ? current.GetComponent<Selectable>()?.FindSelectableOnRight()
                : current.GetComponent<Selectable>()?.FindSelectableOnLeft();

            if (next != null)
            {
                next.Select();
                AnnounceElement(next.gameObject);
            }
        }

        private void NavigateDirection(int direction)
        {
            var current = EventSystem.current?.currentSelectedGameObject;
            if (current == null) return;

            Selectable target = null;
            var selectable = current.GetComponent<Selectable>();

            if (selectable != null)
            {
                target = direction > 0 
                    ? selectable.FindSelectableOnDown() 
                    : selectable.FindSelectableOnUp();
            }

            if (target != null)
            {
                target.Select();
                AnnounceElement(target.gameObject);
            }
        }

        private void ActivateCurrentElement()
        {
            var current = EventSystem.current?.currentSelectedGameObject;
            if (current == null) return;

            // Trigger click on button
            var button = current.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
                Announce("Activated");
            }

            // Toggle checkbox
            var toggle = current.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = !toggle.isOn;
                Announce(toggle.isOn ? "Checked" : "Unchecked");
            }

            // Adjust slider
            var slider = current.GetComponent<Slider>();
            if (slider != null)
            {
                // Increase or decrease based on shift key
                float step = slider.wholeNumbers ? 1f : 0.1f;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    slider.value -= step;
                }
                else
                {
                    slider.value += step;
                }
                Announce($"Slider: {slider.value:F1}");
            }
        }

        private void FocusElement(GameObject element)
        {
            if (element == lastFocusedElement) return;

            lastFocusedElement = element;
            PlayFocusSound();
            OnElementFocused?.Invoke(element);

            if (EventSystem.current != null)
            {
                var selectable = element.GetComponent<Selectable>();
                if (selectable != null)
                {
                    EventSystem.current.SetSelectedGameObject(element);
                }
            }
        }

        #endregion

        #region Announcement Queue

        private void ProcessAnnouncementQueue()
        {
            if (announcementQueue.Count == 0) return;
            if (currentAnnouncement != null)
            {
                // Wait for current announcement to complete
                if (Time.time - lastAnnouncementTime < interruptionGracePeriod)
                    return;
            }

            currentAnnouncement = announcementQueue.Dequeue();
            PlayAnnouncementImmediate(currentAnnouncement);
        }

        private void PlayAnnouncementImmediate(Announcement announcement)
        {
            lastAnnouncementTime = Time.time;

            // Play announcement sound
            if (announcementSound != null)
            {
                audioSource.PlayOneShot(announcementSound, announcementSoundVolume);
            }

            // Here you would integrate with the platform's TTS service
            // For Unity, you might use:
            // - Windows: System.Speech.Synthesis (via P/Invoke)
            // - macOS: NSSpeechSynthesizer (via P/Invoke)
            // - Cross-platform: Unity's Accessibility Plugin or third-party solutions
            
            // For now, log the announcement
            Debug.Log($"[ScreenReader] Speaking: {announcement.message}");

            // Simulate TTS duration based on message length
            float simulatedDuration = announcement.message.Length * 0.05f;
            StartCoroutine(ClearAnnouncementAfterDelay(simulatedDuration));
        }

        private IEnumerator ClearAnnouncementAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            currentAnnouncement = null;
        }

        private void StopCurrentAnnouncement()
        {
            StopAllCoroutines();
            currentAnnouncement = null;
            // Stop TTS if using platform TTS
        }

        #endregion

        #region Helper Methods

        private string GetElementDescription(GameObject element)
        {
            var description = element.GetComponent<AccessibilityDescriptor>();
            if (description != null)
            {
                return description.GetScreenReaderDescription();
            }

            // Generate description from UI components
            string desc = "";

            // Get text content
            var text = element.GetComponentInChildren<Text>();
            if (text != null && !string.IsNullOrEmpty(text.text))
            {
                desc = text.text;
            }

            var tmpText = element.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmpText != null && !string.IsNullOrEmpty(tmpText.text))
            {
                desc = tmpText.text;
            }

            // Add component type info
            if (element.GetComponent<Button>() != null)
            {
                desc = string.IsNullOrEmpty(desc) ? "Button" : $"{desc}, Button";
            }
            else if (element.GetComponent<Toggle>() != null)
            {
                var toggle = element.GetComponent<Toggle>();
                desc = string.IsNullOrEmpty(desc) 
                    ? $"Checkbox, {(toggle.isOn ? "checked" : "unchecked")}" 
                    : $"{desc}, Checkbox, {(toggle.isOn ? "checked" : "unchecked")}";
            }
            else if (element.GetComponent<Slider>() != null)
            {
                var slider = element.GetComponent<Slider>();
                desc = string.IsNullOrEmpty(desc)
                    ? $"Slider, {slider.value:F1}"
                    : $"{desc}, Slider, {slider.value:F1}";
            }

            return desc;
        }

        private void LoadSettings()
        {
            screenReaderEnabled = PlayerPrefs.GetInt("ScreenReaderEnabled", 0) == 1;
        }

        #endregion
    }

    [Serializable]
    public class Announcement
    {
        public string message;
        public AnnouncementPriority priority;
        public float timestamp;
    }

    public enum AnnouncementPriority
    {
        Normal,     // Regular announcements
        High,       // Time-sensitive, can interrupt Normal
        Interrupt   // Critical, interrupts everything
    }

    /// <summary>
    /// Component to provide screen reader descriptions for UI elements.
    /// Attach to any GameObject that needs custom screen reader text.
    /// </summary>
    public class AccessibilityDescriptor : MonoBehaviour
    {
        [TextArea(2, 4)]
        public string screenReaderDescription;

        [Tooltip("Announce when element gains focus")]
        public bool announceOnFocus = true;

        [Tooltip("Announce when element value changes")]
        public bool announceOnValueChange = true;

        public string GetScreenReaderDescription()
        {
            if (!string.IsNullOrEmpty(screenReaderDescription))
            {
                return screenReaderDescription;
            }

            // Generate from name
            return gameObject.name.Replace("_", " ").Replace("-", " ");
        }
    }
}
