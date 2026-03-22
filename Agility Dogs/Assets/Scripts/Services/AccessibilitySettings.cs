using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AgilityDogs.Services
{
    public class AccessibilitySettings : MonoBehaviour
    {
        public static AccessibilitySettings Instance { get; private set; }

        [Header("Visual Accessibility")]
        [SerializeField] private bool highContrastMode = false;
        [SerializeField] private Color highContrastForeground = Color.white;
        [SerializeField] private Color highContrastBackground = Color.black;
        [SerializeField] private Color highContrastAccent = Color.yellow;
        [SerializeField] private float uiScaleMultiplier = 1f;
        [SerializeField] private bool largerText = false;

        [Header("Input Accessibility")]
        [SerializeField] private bool remappableControlsEnabled = true;
        [SerializeField] private float inputRepeatDelay = 0.5f;
        [SerializeField] private float inputRepeatRate = 0.1f;
        [SerializeField] private bool holdToConfirm = false;
        [SerializeField] private float holdToConfirmDuration = 0.5f;

        [Header("Visual Effects")]
        [SerializeField] private bool reduceMotion = false;
        [SerializeField] private bool screenShakeEnabled = true;
        [SerializeField] private float screenShakeIntensity = 1f;
        [SerializeField] private bool flashingEffectsEnabled = true;

        [Header("Audio")]
        [SerializeField] private bool monoAudio = false;
        [SerializeField] private bool closedCaptions = false;
        [SerializeField] private float captionSize = 1f;

        // Events
        public event Action<bool> OnHighContrastModeChanged;
        public event Action<bool> OnReduceMotionChanged;
        public event Action<bool> OnScreenShakeChanged;
        public event Action<bool> OnFlashingEffectsChanged;
        public event Action<bool> OnMonoAudioChanged;
        public event Action<bool> OnClosedCaptionsChanged;
        public event Action<float> OnUIScaleChanged;
        public event Action OnControlBindingsChanged;

        // Custom binding storage
        private Dictionary<string, string> customBindings = new Dictionary<string, string>();

        // Properties
        public bool HighContrastMode => highContrastMode;
        public bool ReduceMotion => reduceMotion;
        public bool ScreenShakeEnabled => screenShakeEnabled;
        public bool FlashingEffectsEnabled => flashingEffectsEnabled;
        public bool MonoAudio => monoAudio;
        public bool ClosedCaptions => closedCaptions;
        public float UIScaleMultiplier => uiScaleMultiplier;
        public bool LargerText => largerText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
            ApplySettings();
        }

        #region Visual Accessibility

        public void SetHighContrastMode(bool enabled)
        {
            highContrastMode = enabled;
            PlayerPrefs.SetInt("HighContrastMode", enabled ? 1 : 0);
            OnHighContrastModeChanged?.Invoke(enabled);
            ApplyHighContrastMode();
        }

        public void SetUIScale(float scale)
        {
            uiScaleMultiplier = Mathf.Clamp(scale, 0.5f, 2f);
            PlayerPrefs.SetFloat("UIScale", uiScaleMultiplier);
            OnUIScaleChanged?.Invoke(uiScaleMultiplier);
        }

        public void SetLargerText(bool enabled)
        {
            largerText = enabled;
            PlayerPrefs.SetInt("LargerText", enabled ? 1 : 0);
            OnUIScaleChanged?.Invoke(largerText ? uiScaleMultiplier * 1.2f : uiScaleMultiplier);
        }

        public void SetReduceMotion(bool enabled)
        {
            reduceMotion = enabled;
            PlayerPrefs.SetInt("ReduceMotion", enabled ? 1 : 0);
            OnReduceMotionChanged?.Invoke(enabled);
        }

        public void SetScreenShakeEnabled(bool enabled)
        {
            screenShakeEnabled = enabled;
            PlayerPrefs.SetInt("ScreenShake", enabled ? 1 : 0);
            OnScreenShakeChanged?.Invoke(enabled);
        }

        public void SetScreenShakeIntensity(float intensity)
        {
            screenShakeIntensity = Mathf.Clamp(intensity, 0f, 2f);
            PlayerPrefs.SetFloat("ScreenShakeIntensity", screenShakeIntensity);
        }

        public void SetFlashingEffectsEnabled(bool enabled)
        {
            flashingEffectsEnabled = enabled;
            PlayerPrefs.SetInt("FlashingEffects", enabled ? 1 : 0);
            OnFlashingEffectsChanged?.Invoke(enabled);
        }

        private void ApplyHighContrastMode()
        {
            // Apply high contrast colors to UI elements
            // This would integrate with the UI system
            Debug.Log($"[Accessibility] High contrast mode: {(highContrastMode ? "Enabled" : "Disabled")}");
        }

        #endregion

        #region Input Accessibility

        public void SetRemappableControlsEnabled(bool enabled)
        {
            remappableControlsEnabled = enabled;
            PlayerPrefs.SetInt("RemappableControls", enabled ? 1 : 0);
        }

        public void SetInputRepeatDelay(float delay)
        {
            inputRepeatDelay = Mathf.Clamp(delay, 0.1f, 1f);
            PlayerPrefs.SetFloat("InputRepeatDelay", inputRepeatDelay);
        }

        public void SetInputRepeatRate(float rate)
        {
            inputRepeatRate = Mathf.Clamp(rate, 0.01f, 0.5f);
            PlayerPrefs.SetFloat("InputRepeatRate", inputRepeatRate);
        }

        public void SetHoldToConfirm(bool enabled)
        {
            holdToConfirm = enabled;
            PlayerPrefs.SetInt("HoldToConfirm", enabled ? 1 : 0);
        }

        public void SetHoldToConfirmDuration(float duration)
        {
            holdToConfirmDuration = Mathf.Clamp(duration, 0.2f, 2f);
            PlayerPrefs.SetFloat("HoldToConfirmDuration", holdToConfirmDuration);
        }

        public bool SaveBinding(string actionName, string bindingPath)
        {
            if (!remappableControlsEnabled) return false;

            customBindings[actionName] = bindingPath;
            PlayerPrefs.SetString($"Binding_{actionName}", bindingPath);
            OnControlBindingsChanged?.Invoke();

            Debug.Log($"[Accessibility] Saved binding for {actionName}: {bindingPath}");
            return true;
        }

        public string GetBinding(string actionName)
        {
            if (customBindings.TryGetValue(actionName, out string binding))
            {
                return binding;
            }

            // Try loading from PlayerPrefs
            return PlayerPrefs.GetString($"Binding_{actionName}", "");
        }

        public void ResetAllBindings()
        {
            customBindings.Clear();

            // Clear all binding PlayerPrefs
            foreach (var key in PlayerPrefs.GetString("CustomBindings", "").Split(','))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    PlayerPrefs.DeleteKey($"Binding_{key}");
                }
            }
            PlayerPrefs.SetString("CustomBindings", "");

            OnControlBindingsChanged?.Invoke();
            Debug.Log("[Accessibility] All control bindings reset to defaults");
        }

        public void ApplyBindingsToInputAction(PlayerInput playerInput)
        {
            if (playerInput == null || !remappableControlsEnabled) return;

            foreach (var binding in customBindings)
            {
                var action = playerInput.actions.FindAction(binding.Key);
                if (action != null && !string.IsNullOrEmpty(binding.Value))
                {
                    // Apply custom binding
                    // Note: This is simplified - real implementation would properly apply bindings
                    Debug.Log($"[Accessibility] Applied binding for {action.name}: {binding.Value}");
                }
            }
        }

        #endregion

        #region Audio Accessibility

        public void SetMonoAudio(bool enabled)
        {
            monoAudio = enabled;
            PlayerPrefs.SetInt("MonoAudio", enabled ? 1 : 0);
            OnMonoAudioChanged?.Invoke(enabled);

            // Apply mono audio setting
            AudioSettings.speakerMode = enabled ? AudioSpeakerMode.Mono : AudioSpeakerMode.Stereo;
        }

        public void SetClosedCaptions(bool enabled)
        {
            closedCaptions = enabled;
            PlayerPrefs.SetInt("ClosedCaptions", enabled ? 1 : 0);
            OnClosedCaptionsChanged?.Invoke(enabled);
        }

        public void SetCaptionSize(float size)
        {
            captionSize = Mathf.Clamp(size, 0.5f, 2f);
            PlayerPrefs.SetFloat("CaptionSize", captionSize);
        }

        #endregion

        #region Utility Methods

        public float GetAdjustedScreenShakeIntensity()
        {
            return screenShakeEnabled ? screenShakeIntensity : 0f;
        }

        public bool ShouldShowFlashingEffects()
        {
            return flashingEffectsEnabled && !reduceMotion;
        }

        public float GetAdjustedAnimationSpeed()
        {
            return reduceMotion ? 0.5f : 1f;
        }

        public float GetAdjustedParticleIntensity()
        {
            return reduceMotion ? 0.3f : 1f;
        }

        #endregion

        #region Save/Load

        private void LoadSettings()
        {
            highContrastMode = PlayerPrefs.GetInt("HighContrastMode", 0) == 1;
            uiScaleMultiplier = PlayerPrefs.GetFloat("UIScale", 1f);
            largerText = PlayerPrefs.GetInt("LargerText", 0) == 1;
            reduceMotion = PlayerPrefs.GetInt("ReduceMotion", 0) == 1;
            screenShakeEnabled = PlayerPrefs.GetInt("ScreenShake", 1) == 1;
            screenShakeIntensity = PlayerPrefs.GetFloat("ScreenShakeIntensity", 1f);
            flashingEffectsEnabled = PlayerPrefs.GetInt("FlashingEffects", 1) == 1;
            remappableControlsEnabled = PlayerPrefs.GetInt("RemappableControls", 1) == 1;
            inputRepeatDelay = PlayerPrefs.GetFloat("InputRepeatDelay", 0.5f);
            inputRepeatRate = PlayerPrefs.GetFloat("InputRepeatRate", 0.1f);
            holdToConfirm = PlayerPrefs.GetInt("HoldToConfirm", 0) == 1;
            holdToConfirmDuration = PlayerPrefs.GetFloat("HoldToConfirmDuration", 0.5f);
            monoAudio = PlayerPrefs.GetInt("MonoAudio", 0) == 1;
            closedCaptions = PlayerPrefs.GetInt("ClosedCaptions", 0) == 1;
            captionSize = PlayerPrefs.GetFloat("CaptionSize", 1f);

            // Load custom bindings
            LoadCustomBindings();
        }

        private void LoadCustomBindings()
        {
            customBindings.Clear();
            string savedBindings = PlayerPrefs.GetString("CustomBindings", "");
            if (!string.IsNullOrEmpty(savedBindings))
            {
                var bindings = savedBindings.Split(',');
                foreach (var binding in bindings)
                {
                    var parts = binding.Split(':');
                    if (parts.Length == 2)
                    {
                        customBindings[parts[0]] = parts[1];
                    }
                }
            }
        }

        private void ApplySettings()
        {
            ApplyHighContrastMode();

            if (monoAudio)
            {
                AudioSettings.speakerMode = AudioSpeakerMode.Mono;
            }
        }

        #endregion
    }

    public class AccessibilityProfile : ScriptableObject
    {
        public string profileName;

        [Header("Visual")]
        public bool highContrastMode;
        public float uiScale = 1f;
        public bool largerText;
        public bool reduceMotion;
        public bool reduceScreenShake;
        public float screenShakeIntensity = 1f;
        public bool disableFlashingEffects;

        [Header("Audio")]
        public bool monoAudio;
        public bool enableClosedCaptions;
        public float captionSize = 1f;

        [Header("Input")]
        public bool holdToConfirm;
        public float holdToConfirmDuration = 0.5f;
        public float inputRepeatDelay = 0.5f;
        public float inputRepeatRate = 0.1f;

        public void ApplyToSettings(AccessibilitySettings settings)
        {
            if (settings == null) return;

            settings.SetHighContrastMode(highContrastMode);
            settings.SetUIScale(uiScale);
            settings.SetLargerText(largerText);
            settings.SetReduceMotion(reduceMotion);
            settings.SetScreenShakeEnabled(!reduceScreenShake);
            settings.SetScreenShakeIntensity(screenShakeIntensity);
            settings.SetFlashingEffectsEnabled(!disableFlashingEffects);
            settings.SetMonoAudio(monoAudio);
            settings.SetClosedCaptions(enableClosedCaptions);
            settings.SetCaptionSize(captionSize);
            settings.SetHoldToConfirm(holdToConfirm);
            settings.SetHoldToConfirmDuration(holdToConfirmDuration);
            settings.SetInputRepeatDelay(inputRepeatDelay);
            settings.SetInputRepeatRate(inputRepeatRate);
        }

        public static AccessibilityProfile CreateDefault()
        {
            var profile = CreateInstance<AccessibilityProfile>();
            profile.profileName = "Default";
            return profile;
        }

        public static AccessibilityProfile CreateMinimalMotion()
        {
            var profile = CreateInstance<AccessibilityProfile>();
            profile.profileName = "Minimal Motion";
            profile.reduceMotion = true;
            profile.reduceScreenShake = true;
            profile.disableFlashingEffects = true;
            return profile;
        }

        public static AccessibilityProfile CreateHighVisibility()
        {
            var profile = CreateInstance<AccessibilityProfile>();
            profile.profileName = "High Visibility";
            profile.highContrastMode = true;
            profile.largerText = true;
            profile.uiScale = 1.25f;
            return profile;
        }
    }
}
