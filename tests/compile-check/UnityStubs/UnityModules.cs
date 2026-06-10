// Stubs for Unity module namespaces: AI, UI, SceneManagement, Networking,
// InputSystem, EventSystems, Video, TextMeshPro.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.AI
{
    public class NavMeshAgent : Behaviour
    {
        public float speed { get; set; }
        public float angularSpeed { get; set; }
        public float acceleration { get; set; }
        public float stoppingDistance { get; set; }
        public bool autoBraking { get; set; }
        public float radius { get; set; }
        public float height { get; set; }
        public bool updateRotation { get; set; }
        public bool updatePosition { get; set; }
        public Vector3 velocity { get; set; }
        public Vector3 destination { get; set; }
        public float remainingDistance => 0f;
        public bool pathPending => false;
        public bool hasPath => false;
        public bool isStopped { get; set; }
        public bool isOnNavMesh => true;
        public bool isOnOffMeshLink => false;
        public NavMeshPathStatus pathStatus => NavMeshPathStatus.PathComplete;
        public Vector3 nextPosition { get; set; }
        public bool SetDestination(Vector3 target) => true;
        public void ResetPath() { }
        public bool Warp(Vector3 newPosition) => true;
        public void Move(Vector3 offset) { }
        public bool CalculatePath(Vector3 targetPosition, NavMeshPath path) => true;
        public bool SamplePathPosition(int areaMask, float maxDistance, out NavMeshHit hit) { hit = default; return false; }
    }

    public enum NavMeshPathStatus { PathComplete, PathPartial, PathInvalid }

    public class NavMeshPath
    {
        public Vector3[] corners => new Vector3[0];
        public NavMeshPathStatus status => NavMeshPathStatus.PathComplete;
    }

    public struct NavMeshHit
    {
        public Vector3 position => default;
        public float distance => 0f;
        public bool hit => false;
    }

    public static class NavMesh
    {
        public const int AllAreas = -1;
        public static bool SamplePosition(Vector3 sourcePosition, out NavMeshHit hit, float maxDistance, int areaMask) { hit = default; return false; }
        public static bool Raycast(Vector3 source, Vector3 target, out NavMeshHit hit, int areaMask) { hit = default; return false; }
        public static bool CalculatePath(Vector3 source, Vector3 target, int areaMask, NavMeshPath path) => true;
    }

    public class NavMeshObstacle : Behaviour
    {
        public bool carving { get; set; }
        public Vector3 size { get; set; }
        public Vector3 center { get; set; }
    }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene
    {
        public string name => "";
        public int buildIndex => 0;
        public bool isLoaded => true;
        public bool IsValid() => true;
        public GameObject[] GetRootGameObjects() => new GameObject[0];
    }
    public enum LoadSceneMode { Single, Additive }
    public static class SceneManager
    {
        public static Scene GetActiveScene() => default;
        public static Scene GetSceneByName(string name) => default;
        public static Scene GetSceneAt(int index) => default;
        public static int sceneCount => 0;
        public static int sceneCountInBuildSettings => 0;
        public static void LoadScene(string sceneName) { }
        public static void LoadScene(string sceneName, LoadSceneMode mode) { }
        public static void LoadScene(int sceneBuildIndex) { }
        public static AsyncOperation LoadSceneAsync(string sceneName) => new AsyncOperation();
        public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode) => new AsyncOperation();
        public static AsyncOperation LoadSceneAsync(int sceneBuildIndex) => new AsyncOperation();
        public static AsyncOperation UnloadSceneAsync(string sceneName) => new AsyncOperation();
        public static event Action<Scene, LoadSceneMode> sceneLoaded { add { } remove { } }
        public static event Action<Scene> sceneUnloaded { add { } remove { } }
        public static bool SetActiveScene(Scene scene) => true;
    }
    public static class SceneUtility
    {
        public static string GetScenePathByBuildIndex(int buildIndex) => "";
    }
}

namespace UnityEngine.UI
{
    public class Graphic : Behaviour
    {
        public Color color { get; set; }
        public RectTransform rectTransform => null;
        public bool raycastTarget { get; set; }
        public Material material { get; set; }
        public void SetAllDirty() { }
        public CanvasRenderer canvasRenderer => null;
        public void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale) { }
    }
    public class MaskableGraphic : Graphic { }
    public class Image : MaskableGraphic
    {
        public Sprite sprite { get; set; }
        public Type type { get; set; }
        public float fillAmount { get; set; }
        public FillMethod fillMethod { get; set; }
        public bool preserveAspect { get; set; }
        public new enum Type { Simple, Sliced, Tiled, Filled }
        public enum FillMethod { Horizontal, Vertical, Radial90, Radial180, Radial360 }
    }
    public class RawImage : MaskableGraphic { public Texture texture { get; set; } }
    public class Text : MaskableGraphic
    {
        public string text { get; set; }
        public Font font { get; set; }
        public int fontSize { get; set; }
        public FontStyle fontStyle { get; set; }
        public TextAnchor alignment { get; set; }
        public bool resizeTextForBestFit { get; set; }
        public int resizeTextMaxSize { get; set; }
        public int resizeTextMinSize { get; set; }
        public HorizontalWrapMode horizontalOverflow { get; set; }
        public VerticalWrapMode verticalOverflow { get; set; }
        public float lineSpacing { get; set; }
        public bool supportRichText { get; set; }
    }
    public class Selectable : Behaviour
    {
        public bool interactable { get; set; }
        public Image image { get; set; }
        public Graphic targetGraphic { get; set; }
        public ColorBlock colors { get; set; }
        public Navigation navigation { get; set; }
        public void Select() { }
        public Selectable FindSelectableOnUp() => null;
        public Selectable FindSelectableOnDown() => null;
        public Selectable FindSelectableOnLeft() => null;
        public Selectable FindSelectableOnRight() => null;
    }
    public struct Navigation { public Mode mode { get; set; } public enum Mode { None, Horizontal, Vertical, Automatic, Explicit } }
    public struct ColorBlock
    {
        public Color normalColor { get; set; }
        public Color highlightedColor { get; set; }
        public Color pressedColor { get; set; }
        public Color selectedColor { get; set; }
        public Color disabledColor { get; set; }
        public float colorMultiplier { get; set; }
        public float fadeDuration { get; set; }
        public static ColorBlock defaultColorBlock => default;
    }
    public class Button : Selectable
    {
        public ButtonClickedEvent onClick { get; } = new ButtonClickedEvent();
        public class ButtonClickedEvent : Events.UnityEvent { }
    }
    public class Toggle : Selectable
    {
        public bool isOn { get; set; }
        public ToggleEvent onValueChanged { get; } = new ToggleEvent();
        public class ToggleEvent : Events.UnityEvent<bool> { }
    }
    public class Slider : Selectable
    {
        public float value { get; set; }
        public float minValue { get; set; }
        public float maxValue { get; set; }
        public bool wholeNumbers { get; set; }
        public SliderEvent onValueChanged { get; } = new SliderEvent();
        public class SliderEvent : Events.UnityEvent<float> { }
    }
    public class Scrollbar : Selectable { public float value { get; set; } }
    public class ScrollRect : Behaviour
    {
        public RectTransform content { get; set; }
        public bool horizontal { get; set; }
        public bool vertical { get; set; }
        public Vector2 normalizedPosition { get; set; }
        public float verticalNormalizedPosition { get; set; }
        public ScrollbarVisibility verticalScrollbarVisibility { get; set; }
        public Scrollbar verticalScrollbar { get; set; }
        public enum ScrollbarVisibility { Permanent, AutoHide, AutoHideAndExpandViewport }
        public RectTransform viewport { get; set; }
        public MovementType movementType { get; set; }
        public enum MovementType { Unrestricted, Elastic, Clamped }
        public float scrollSensitivity { get; set; }
    }
    public class Dropdown : Selectable
    {
        public int value { get; set; }
        public List<OptionData> options { get; set; } = new List<OptionData>();
        public DropdownEvent onValueChanged { get; } = new DropdownEvent();
        public void ClearOptions() { }
        public void AddOptions(List<string> options) { }
        public void RefreshShownValue() { }
        public class OptionData { public string text { get; set; } public OptionData() { } public OptionData(string text) { this.text = text; } }
        public class DropdownEvent : Events.UnityEvent<int> { }
    }
    public class InputField : Selectable
    {
        public string text { get; set; }
        public Text textComponent { get; set; }
        public Text placeholder { get; set; }
    }
    public class LayoutGroup : Behaviour { public RectOffset padding { get; set; } }
    public class HorizontalOrVerticalLayoutGroup : LayoutGroup
    {
        public float spacing { get; set; }
        public bool childForceExpandWidth { get; set; }
        public bool childForceExpandHeight { get; set; }
        public bool childControlWidth { get; set; }
        public bool childControlHeight { get; set; }
        public TextAnchor childAlignment { get; set; }
    }
    public class VerticalLayoutGroup : HorizontalOrVerticalLayoutGroup { }
    public class HorizontalLayoutGroup : HorizontalOrVerticalLayoutGroup { }
    public class GridLayoutGroup : LayoutGroup
    {
        public Vector2 cellSize { get; set; }
        public Vector2 spacing { get; set; }
        public Constraint constraint { get; set; }
        public int constraintCount { get; set; }
        public enum Constraint { Flexible, FixedColumnCount, FixedRowCount }
    }
    public class ContentSizeFitter : Behaviour
    {
        public FitMode horizontalFit { get; set; }
        public FitMode verticalFit { get; set; }
        public enum FitMode { Unconstrained, MinSize, PreferredSize }
    }
    public class LayoutElement : Behaviour
    {
        public float minHeight { get; set; }
        public float minWidth { get; set; }
        public float preferredHeight { get; set; }
        public float preferredWidth { get; set; }
        public float flexibleHeight { get; set; }
        public float flexibleWidth { get; set; }
    }
    public class CanvasScaler : Behaviour
    {
        public ScaleMode uiScaleMode { get; set; }
        public Vector2 referenceResolution { get; set; }
        public float matchWidthOrHeight { get; set; }
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
    }
    public class GraphicRaycaster : Behaviour { }
    public class Mask : Behaviour { public bool showMaskGraphic { get; set; } }
    public class RectMask2D : Behaviour { }
    public class Outline : Behaviour { public Color effectColor { get; set; } public Vector2 effectDistance { get; set; } }
    public class Shadow : Behaviour { public Color effectColor { get; set; } public Vector2 effectDistance { get; set; } }
}

namespace UnityEngine
{
    public class Canvas : Behaviour
    {
        public RenderMode renderMode { get; set; }
        public int sortingOrder { get; set; }
        public Camera worldCamera { get; set; }
        public float planeDistance { get; set; }
        public bool overrideSorting { get; set; }
        public static void ForceUpdateCanvases() { }
    }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public class CanvasGroup : Behaviour
    {
        public float alpha { get; set; }
        public bool interactable { get; set; }
        public bool blocksRaycasts { get; set; }
        public bool ignoreParentGroups { get; set; }
    }
    public class CanvasRenderer : Component { public void SetAlpha(float alpha) { } }
    public class Font : Object
    {
        public static Font CreateDynamicFontFromOSFont(string fontname, int size) => null;
    }
    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }
    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }
    public class RectOffset
    {
        public RectOffset() { }
        public RectOffset(int left, int right, int top, int bottom) { }
        public int left { get; set; }
        public int right { get; set; }
        public int top { get; set; }
        public int bottom { get; set; }
    }
}

namespace UnityEngine.EventSystems
{
    public class EventSystem : Behaviour
    {
        public static EventSystem current { get; set; }
        public GameObject currentSelectedGameObject => null;
        public void SetSelectedGameObject(GameObject selected) { }
    }
    public class StandaloneInputModule : Behaviour { }
    public class BaseInputModule : Behaviour { }
    public class PointerEventData
    {
        public PointerEventData(EventSystem eventSystem) { }
        public Vector2 position { get; set; }
    }
    public interface IPointerEnterHandler { void OnPointerEnter(PointerEventData eventData); }
    public interface IPointerExitHandler { void OnPointerExit(PointerEventData eventData); }
    public interface IPointerClickHandler { void OnPointerClick(PointerEventData eventData); }
    public interface IPointerDownHandler { void OnPointerDown(PointerEventData eventData); }
    public interface IPointerUpHandler { void OnPointerUp(PointerEventData eventData); }
}

namespace UnityEngine.Networking
{
    public class UnityWebRequest : IDisposable
    {
        public UnityWebRequest() { }
        public UnityWebRequest(string url) { }
        public UnityWebRequest(string url, string method) { }
        public string url { get; set; }
        public string method { get; set; }
        public bool isDone => true;
        public Result result => Result.Success;
        public long responseCode => 200;
        public string error => null;
        public DownloadHandler downloadHandler { get; set; }
        public UploadHandler uploadHandler { get; set; }
        public int timeout { get; set; }
        public void SetRequestHeader(string name, string value) { }
        public string GetResponseHeader(string name) => null;
        public UnityWebRequestAsyncOperation SendWebRequest() => new UnityWebRequestAsyncOperation();
        public void Abort() { }
        public void Dispose() { }
        public static UnityWebRequest Get(string uri) => new UnityWebRequest();
        public static UnityWebRequest Post(string uri, string postData) => new UnityWebRequest();
        public static UnityWebRequest Post(string uri, WWWForm formData) => new UnityWebRequest();
        public static UnityWebRequest Put(string uri, byte[] bodyData) => new UnityWebRequest();
        public static UnityWebRequest Delete(string uri) => new UnityWebRequest();
        public enum Result { InProgress, Success, ConnectionError, ProtocolError, DataProcessingError }
        public const string kHttpVerbGET = "GET";
        public const string kHttpVerbPOST = "POST";
    }
    public class UnityWebRequestAsyncOperation : AsyncOperation { public UnityWebRequest webRequest => null; }
    public class DownloadHandler : IDisposable
    {
        public string text => "";
        public byte[] data => new byte[0];
        public void Dispose() { }
    }
    public class DownloadHandlerBuffer : DownloadHandler { }
    public class DownloadHandlerAudioClip : DownloadHandler
    {
        public AudioClip audioClip => null;
        public static AudioClip GetContent(UnityWebRequest www) => null;
    }
    public class UploadHandler : IDisposable { public string contentType { get; set; } public void Dispose() { } }
    public class UploadHandlerRaw : UploadHandler { public UploadHandlerRaw(byte[] data) { } }
    public static class UnityWebRequestMultimedia
    {
        public static UnityWebRequest GetAudioClip(string uri, AudioType audioType) => new UnityWebRequest();
    }
    public class WWWForm
    {
        public void AddField(string fieldName, string value) { }
        public void AddBinaryData(string fieldName, byte[] contents) { }
    }
}

namespace UnityEngine
{
    public enum AudioType { UNKNOWN, MPEG, OGGVORBIS, WAV }
}

namespace UnityEngine.InputSystem
{
    public class PlayerInput : MonoBehaviour
    {
        public InputActionAsset actions { get; set; }
        public string currentActionMap { get; set; }
        public InputActionMap currentActionMapObject => null;
        public bool inputIsActive => true;
        public void SwitchCurrentActionMap(string mapName) { }
        public event Action<InputAction.CallbackContext> onActionTriggered { add { } remove { } }
    }
    public class InputActionAsset : ScriptableObject
    {
        public InputAction this[string actionName] => null;
        public InputActionMap FindActionMap(string name) => null;
        public InputAction FindAction(string name) => null;
        public void Enable() { }
        public void Disable() { }
    }
    public class InputActionMap
    {
        public InputAction this[string actionName] => null;
        public InputAction FindAction(string name) => null;
        public void Enable() { }
        public void Disable() { }
    }
    public class InputAction
    {
        public string name => "";
        public bool enabled => true;
        public void Enable() { }
        public void Disable() { }
        public T ReadValue<T>() where T : struct => default;
        public bool WasPressedThisFrame() => false;
        public bool WasPerformedThisFrame() => false;
        public bool IsPressed() => false;
        public event Action<CallbackContext> performed { add { } remove { } }
        public event Action<CallbackContext> started { add { } remove { } }
        public event Action<CallbackContext> canceled { add { } remove { } }
        public struct CallbackContext
        {
            public InputAction action => null;
            public T ReadValue<T>() where T : struct => default;
            public bool performed => false;
            public bool started => false;
            public bool canceled => false;
        }
    }
    public class InputValue
    {
        public T Get<T>() where T : struct => default;
        public bool isPressed => false;
    }
    public class Keyboard
    {
        public static Keyboard current => null;
        public KeyControl spaceKey => null;
        public KeyControl wKey => null;
        public KeyControl aKey => null;
        public KeyControl sKey => null;
        public KeyControl dKey => null;
        public KeyControl qKey => null;
        public KeyControl eKey => null;
        public KeyControl rKey => null;
        public KeyControl tKey => null;
        public KeyControl gKey => null;
        public KeyControl hKey => null;
        public KeyControl jKey => null;
        public KeyControl leftShiftKey => null;
        public KeyControl enterKey => null;
        public KeyControl escapeKey => null;
        public KeyControl digit1Key => null;
        public KeyControl digit2Key => null;
        public KeyControl digit3Key => null;
        public KeyControl digit4Key => null;
        public KeyControl digit5Key => null;
        public KeyControl upArrowKey => null;
        public KeyControl downArrowKey => null;
        public KeyControl leftArrowKey => null;
        public KeyControl rightArrowKey => null;
    }
    public class KeyControl
    {
        public bool isPressed => false;
        public bool wasPressedThisFrame => false;
        public bool wasReleasedThisFrame => false;
    }
    public class Mouse
    {
        public static Mouse current => null;
        public ButtonControl leftButton => null;
        public ButtonControl rightButton => null;
        public Vector2Control position => null;
    }
    public class ButtonControl
    {
        public bool isPressed => false;
        public bool wasPressedThisFrame => false;
    }
    public class Vector2Control { public Vector2 ReadValue() => default; }
}

namespace UnityEngine.Video
{
    public class VideoPlayer : Behaviour
    {
        public VideoClip clip { get; set; }
        public string url { get; set; }
        public bool isLooping { get; set; }
        public bool playOnAwake { get; set; }
        public bool isPlaying => false;
        public RenderMode renderMode { get; set; }
        public RenderTexture targetTexture { get; set; }
        public void Play() { }
        public void Stop() { }
        public void Pause() { }
        public event EventHandler loopPointReached { add { } remove { } }
        public delegate void EventHandler(VideoPlayer source);
        public new enum RenderMode { CameraFarPlane, CameraNearPlane, RenderTexture, MaterialOverride, APIOnly }
    }
    public class VideoClip : Object { }
}

namespace TMPro
{
    public class TMP_Text : UnityEngine.UI.Graphic
    {
        public string text { get; set; }
        public float fontSize { get; set; }
        public FontStyles fontStyle { get; set; }
        public TextAlignmentOptions alignment { get; set; }
        public bool enableWordWrapping { get; set; }
        public TextOverflowModes overflowMode { get; set; }
        public bool richText { get; set; }
        public float characterSpacing { get; set; }
        public float lineSpacing { get; set; }
        public TMP_FontAsset font { get; set; }
        public bool enableAutoSizing { get; set; }
        public float fontSizeMin { get; set; }
        public float fontSizeMax { get; set; }
        public UnityEngine.Vector4 margin { get; set; }
        public void SetText(string text) { }
        public void ForceMeshUpdate() { }
    }
    public class TextMeshProUGUI : TMP_Text { }
    public class TextMeshPro : TMP_Text { }
    public class TMP_FontAsset : UnityEngine.ScriptableObject { }
    public enum FontStyles { Normal = 0, Bold = 1, Italic = 2, Underline = 4 }
    public enum TextOverflowModes { Overflow, Ellipsis, Masking, Truncate, ScrollRect, Page }
    public enum TextAlignmentOptions
    {
        TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight,
        MidlineLeft, Midline, MidlineRight
    }
    public class TMP_InputField : UnityEngine.UI.Selectable
    {
        public string text { get; set; }
        public TMP_Text textComponent { get; set; }
        public UnityEngine.UI.Graphic placeholder { get; set; }
        public ContentType contentType { get; set; }
        public SubmitEvent onSubmit { get; } = new SubmitEvent();
        public OnChangeEvent onValueChanged { get; } = new OnChangeEvent();
        public enum ContentType { Standard, IntegerNumber, DecimalNumber, Alphanumeric, Name, EmailAddress, Password, Pin }
        public class SubmitEvent : UnityEngine.Events.UnityEvent<string> { }
        public class OnChangeEvent : UnityEngine.Events.UnityEvent<string> { }
    }
    public class TMP_Dropdown : UnityEngine.UI.Selectable
    {
        public int value { get; set; }
        public System.Collections.Generic.List<OptionData> options { get; set; } = new System.Collections.Generic.List<OptionData>();
        public DropdownEvent onValueChanged { get; } = new DropdownEvent();
        public void ClearOptions() { }
        public void AddOptions(System.Collections.Generic.List<string> options) { }
        public void RefreshShownValue() { }
        public class OptionData { public string text { get; set; } public OptionData() { } public OptionData(string text) { this.text = text; } }
        public class DropdownEvent : UnityEngine.Events.UnityEvent<int> { }
    }
}

namespace UnityEngine
{
    public struct Vector4
    {
        public float x, y, z, w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Vector4 zero => default;
    }
}
