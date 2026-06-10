// Minimal UnityEngine API surface for compile-checking game scripts without
// the Unity Editor. Behavior is irrelevant; only signatures matter.
#pragma warning disable 0626
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    // ---- Math types ----
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => default;
        public static Vector2 one => new Vector2(1, 1);
        public float magnitude => (float)Math.Sqrt(x * x + y * y);
        public float sqrMagnitude => x * x + y * y;
        public Vector2 normalized => this;
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.x * d, a.y * d);
        public static float Distance(Vector2 a, Vector2 b) => (a - b).magnitude;
        public static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;
        public static float Angle(Vector2 a, Vector2 b) => 0f;
        public static float SignedAngle(Vector2 from, Vector2 to) => 0f;
        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, v.y, 0);
        public static implicit operator Vector2(Vector3 v) => new Vector2(v.x, v.y);
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => a + (b - a) * Mathf.Clamp01(t);
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public Vector3(float x, float y) { this.x = x; this.y = y; this.z = 0; }
        public static Vector3 zero => default;
        public static Vector3 one => new Vector3(1, 1, 1);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 down => new Vector3(0, -1, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public static Vector3 back => new Vector3(0, 0, -1);
        public static Vector3 left => new Vector3(-1, 0, 0);
        public static Vector3 right => new Vector3(1, 0, 0);
        public float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);
        public float sqrMagnitude => x * x + y * y + z * z;
        public Vector3 normalized { get { var m = magnitude; return m > 1e-5f ? this / m : zero; } }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator *(float d, Vector3 a) => a * d;
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);
        public static bool operator ==(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
        public override bool Equals(object o) => o is Vector3 v && v == this;
        public override int GetHashCode() => 0;
        public static float Distance(Vector3 a, Vector3 b) => (a - b).magnitude;
        public static float Dot(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * Mathf.Clamp01(t);
        public static Vector3 Slerp(Vector3 a, Vector3 b, float t) => Lerp(a, b, t);
        public static Vector3 MoveTowards(Vector3 c, Vector3 t, float d) => c;
        public static Vector3 SmoothDamp(Vector3 c, Vector3 t, ref Vector3 v, float s) => c;
        public static Vector3 ClampMagnitude(Vector3 v, float max) => v;
        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 Project(Vector3 v, Vector3 n) => n * Dot(v, n);
        public static Vector3 ProjectOnPlane(Vector3 v, Vector3 n) => v - Project(v, n);
        public static float Angle(Vector3 a, Vector3 b) => 0f;
        public static float SignedAngle(Vector3 a, Vector3 b, Vector3 axis) => 0f;
        public override string ToString() => $"({x}, {y}, {z})";
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Quaternion identity => default;
        public static Quaternion Euler(float x, float y, float z) => default;
        public static Quaternion Euler(Vector3 e) => default;
        public static Quaternion LookRotation(Vector3 forward) => default;
        public static Quaternion LookRotation(Vector3 forward, Vector3 up) => default;
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t) => a;
        public static Quaternion Lerp(Quaternion a, Quaternion b, float t) => a;
        public static Quaternion RotateTowards(Quaternion a, Quaternion b, float d) => a;
        public static Quaternion AngleAxis(float angle, Vector3 axis) => default;
        public static Quaternion Inverse(Quaternion q) => q;
        public static float Angle(Quaternion a, Quaternion b) => 0f;
        public Vector3 eulerAngles { get => default; set { } }
        public static Vector3 operator *(Quaternion q, Vector3 v) => v;
        public static bool operator ==(Quaternion a, Quaternion b) => true;
        public static bool operator !=(Quaternion a, Quaternion b) => false;
        public override bool Equals(object o) => o is Quaternion;
        public override int GetHashCode() => 0;
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => new Color(1, 1, 1);
        public static Color black => new Color(0, 0, 0);
        public static Color red => new Color(1, 0, 0);
        public static Color green => new Color(0, 1, 0);
        public static Color blue => new Color(0, 0, 1);
        public static Color yellow => new Color(1, 0.92f, 0.016f);
        public static Color cyan => new Color(0, 1, 1);
        public static Color magenta => new Color(1, 0, 1);
        public static Color gray => new Color(0.5f, 0.5f, 0.5f);
        public static Color grey => gray;
        public static Color clear => new Color(0, 0, 0, 0);
        public static Color Lerp(Color a, Color b, float t) => a;
        public static Color operator *(Color a, float m) => new Color(a.r * m, a.g * m, a.b * m, a.a * m);
        public static Color operator +(Color a, Color b) => a;
    }

    public struct Color32
    {
        public byte r, g, b, a;
        public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static implicit operator Color(Color32 c) => new Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
        public static implicit operator Color32(Color c) => new Color32(0, 0, 0, 255);
    }

    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; }
    }

    public struct Bounds
    {
        public Bounds(Vector3 center, Vector3 size) { this.center = center; this.size = size; extents = size * 0.5f; }
        public Vector3 center { get; set; }
        public Vector3 size { get; set; }
        public Vector3 extents { get; set; }
        public bool Contains(Vector3 p) => false;
    }

    public static class Mathf
    {
        public const float PI = (float)Math.PI;
        public const float Infinity = float.PositiveInfinity;
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 180f / PI;
        public static float Abs(float f) => Math.Abs(f);
        public static int Abs(int i) => Math.Abs(i);
        public static float Min(float a, float b) => Math.Min(a, b);
        public static int Min(int a, int b) => Math.Min(a, b);
        public static float Max(float a, float b) => Math.Max(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Clamp(float v, float min, float max) => Math.Min(Math.Max(v, min), max);
        public static int Clamp(int v, int min, int max) => Math.Min(Math.Max(v, min), max);
        public static float Clamp01(float v) => Clamp(v, 0f, 1f);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float LerpUnclamped(float a, float b, float t) => a + (b - a) * t;
        public static float InverseLerp(float a, float b, float v) => a != b ? Clamp01((v - a) / (b - a)) : 0f;
        public static float MoveTowards(float c, float t, float d) => Math.Abs(t - c) <= d ? t : c + Math.Sign(t - c) * d;
        public static float SmoothDamp(float c, float t, ref float v, float s) => c;
        public static float SmoothDampAngle(float c, float t, ref float v, float s) => c;
        public static float Sin(float f) => (float)Math.Sin(f);
        public static float Cos(float f) => (float)Math.Cos(f);
        public static float Tan(float f) => (float)Math.Tan(f);
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Sqrt(float f) => (float)Math.Sqrt(f);
        public static float Pow(float f, float p) => (float)Math.Pow(f, p);
        public static float Exp(float f) => (float)Math.Exp(f);
        public static float Log(float f) => (float)Math.Log(f);
        public static float Round(float f) => (float)Math.Round(f);
        public static int RoundToInt(float f) => (int)Math.Round(f);
        public static int FloorToInt(float f) => (int)Math.Floor(f);
        public static int CeilToInt(float f) => (int)Math.Ceiling(f);
        public static float Floor(float f) => (float)Math.Floor(f);
        public static float Ceil(float f) => (float)Math.Ceiling(f);
        public static float Sign(float f) => Math.Sign(f);
        public static float Repeat(float t, float length) => t - (float)Math.Floor(t / length) * length;
        public static float PingPong(float t, float length) => length - Math.Abs(Repeat(t, length * 2f) - length);
        public static float DeltaAngle(float a, float b) => 0f;
        public static bool Approximately(float a, float b) => Math.Abs(a - b) < 1e-5f;
        public static float PerlinNoise(float x, float y) => 0.5f;
        public static float SmoothStep(float from, float to, float t) => Lerp(from, to, Clamp01(t));
    }

    public static class Time
    {
        public static float deltaTime => 0.016f;
        public static float fixedDeltaTime { get; set; } = 0.02f;
        public static float time => 0f;
        public static float unscaledTime => 0f;
        public static float unscaledDeltaTime => 0.016f;
        public static float realtimeSinceStartup => 0f;
        public static float timeScale { get; set; }
        public static int frameCount => 0;
    }

    public static class Random
    {
        public static float value => 0.5f;
        public static int Range(int min, int max) => min;
        public static float Range(float min, float max) => min;
        public static Vector3 insideUnitSphere => default;
        public static Vector2 insideUnitCircle => default;
        public static void InitState(int seed) { }
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void Log(object message, Object context) { }
        public static void LogWarning(object message) { }
        public static void LogWarning(object message, Object context) { }
        public static void LogError(object message) { }
        public static void LogError(object message, Object context) { }
        public static void LogException(Exception e) { }
        public static void DrawLine(Vector3 a, Vector3 b, Color c) { }
        public static void DrawLine(Vector3 a, Vector3 b, Color c, float duration) { }
        public static void DrawRay(Vector3 o, Vector3 d, Color c) { }
    }

    // ---- Object model ----
    public class Object
    {
        public string name { get; set; }
        public HideFlags hideFlags { get; set; }
        public int GetInstanceID() => 0;
        public static void Destroy(Object obj) { }
        public static void Destroy(Object obj, float t) { }
        public static void DestroyImmediate(Object obj) { }
        public static void DontDestroyOnLoad(Object target) { }
        public static T Instantiate<T>(T original) where T : Object => original;
        public static T Instantiate<T>(T original, Transform parent) where T : Object => original;
        public static T Instantiate<T>(T original, Vector3 pos, Quaternion rot) where T : Object => original;
        public static T Instantiate<T>(T original, Vector3 pos, Quaternion rot, Transform parent) where T : Object => original;
        public static T FindObjectOfType<T>() where T : Object => null;
        public static T FindObjectOfType<T>(bool includeInactive) where T : Object => null;
        public static T[] FindObjectsOfType<T>() where T : Object => new T[0];
        public static T[] FindObjectsOfType<T>(bool includeInactive) where T : Object => new T[0];
        public static T FindFirstObjectByType<T>() where T : Object => null;
        public static T FindAnyObjectByType<T>() where T : Object => null;
        public static T[] FindObjectsByType<T>(FindObjectsSortMode sortMode) where T : Object => new T[0];
        public static implicit operator bool(Object o) => o != null;
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object o) => ReferenceEquals(this, o);
        public override int GetHashCode() => base.GetHashCode();
    }

    public enum HideFlags { None, HideAndDontSave, DontSave }
    public enum FindObjectsSortMode { None, InstanceID }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => null;
        public static ScriptableObject CreateInstance(Type type) => null;
    }

    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string name) { this.name = name; }
        public Transform transform { get; } = new Transform();
        public bool activeSelf => true;
        public bool activeInHierarchy => true;
        public string tag { get; set; }
        public int layer { get; set; }
        public GameObject gameObject => this;
        public void SetActive(bool value) { }
        public T AddComponent<T>() where T : Component, new() => new T();
        public Component AddComponent(Type type) => null;
        public T GetComponent<T>() => default;
        public Component GetComponent(Type t) => null;
        public T GetComponentInChildren<T>() => default;
        public T GetComponentInChildren<T>(bool includeInactive) => default;
        public T GetComponentInParent<T>() => default;
        public T[] GetComponents<T>() => new T[0];
        public T[] GetComponentsInChildren<T>() => new T[0];
        public T[] GetComponentsInChildren<T>(bool includeInactive) => new T[0];
        public bool TryGetComponent<T>(out T component) { component = default; return false; }
        public bool CompareTag(string t) => false;
        public static GameObject Find(string name) => null;
        public static GameObject FindWithTag(string tag) => null;
        public static GameObject FindGameObjectWithTag(string tag) => null;
        public static GameObject[] FindGameObjectsWithTag(string tag) => new GameObject[0];
        public static GameObject CreatePrimitive(PrimitiveType type) => new GameObject(type.ToString());
    }

    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }

    public class Component : Object
    {
        public GameObject gameObject { get; } = new GameObject();
        public Transform transform => gameObject.transform;
        public string tag { get => gameObject.tag; set => gameObject.tag = value; }
        public T GetComponent<T>() => gameObject.GetComponent<T>();
        public Component GetComponent(Type t) => null;
        public T GetComponentInChildren<T>() => default;
        public T GetComponentInChildren<T>(bool includeInactive) => default;
        public T GetComponentInParent<T>() => default;
        public T[] GetComponents<T>() => new T[0];
        public T[] GetComponentsInChildren<T>() => new T[0];
        public T[] GetComponentsInChildren<T>(bool includeInactive) => new T[0];
        public bool TryGetComponent<T>(out T component) { component = default; return false; }
        public bool CompareTag(string t) => false;
        public void SendMessage(string method) { }
        public void BroadcastMessage(string method) { }
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; } = true;
        public bool isActiveAndEnabled => enabled;
    }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine) => null;
        public Coroutine StartCoroutine(string method) => null;
        public void StopCoroutine(IEnumerator routine) { }
        public void StopCoroutine(Coroutine routine) { }
        public void StopCoroutine(string method) { }
        public void StopAllCoroutines() { }
        public void Invoke(string method, float time) { }
        public void InvokeRepeating(string method, float time, float rate) { }
        public void CancelInvoke() { }
        public void CancelInvoke(string method) { }
        public bool IsInvoking(string method) => false;
    }

    public class Coroutine : YieldInstruction { }
    public class YieldInstruction { }
    public class WaitForSeconds : YieldInstruction { public WaitForSeconds(float seconds) { } }
    public class WaitForSecondsRealtime : YieldInstruction { public WaitForSecondsRealtime(float seconds) { } }
    public class WaitForEndOfFrame : YieldInstruction { }
    public class WaitForFixedUpdate : YieldInstruction { }
    public class WaitUntil : YieldInstruction { public WaitUntil(Func<bool> predicate) { } }
    public class WaitWhile : YieldInstruction { public WaitWhile(Func<bool> predicate) { } }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        public Vector3 localScale { get; set; } = Vector3.one;
        public Vector3 lossyScale => localScale;
        public Vector3 forward { get; set; } = Vector3.forward;
        public Vector3 right { get; set; } = Vector3.right;
        public Vector3 up { get; set; } = Vector3.up;
        public Vector3 eulerAngles { get; set; }
        public Vector3 localEulerAngles { get; set; }
        public Transform parent { get; set; }
        public int childCount => 0;
        public Transform root => this;
        public void SetParent(Transform p) { parent = p; }
        public void SetParent(Transform p, bool worldPositionStays) { parent = p; }
        public Transform Find(string name) => null;
        public Transform GetChild(int index) => null;
        public void SetSiblingIndex(int i) { }
        public int GetSiblingIndex() => 0;
        public void SetAsFirstSibling() { }
        public void SetAsLastSibling() { }
        public void LookAt(Transform target) { }
        public void LookAt(Vector3 worldPosition) { }
        public void Rotate(Vector3 eulers) { }
        public void Rotate(float x, float y, float z) { }
        public void RotateAround(Vector3 point, Vector3 axis, float angle) { }
        public void Translate(Vector3 translation) { }
        public void Translate(Vector3 translation, Space relativeTo) { }
        public Vector3 TransformPoint(Vector3 p) => p;
        public Vector3 InverseTransformPoint(Vector3 p) => p;
        public Vector3 TransformDirection(Vector3 d) => d;
        public Vector3 InverseTransformDirection(Vector3 d) => d;
        public IEnumerator GetEnumerator() { yield break; }
    }

    public class RectTransform : Transform
    {
        public Vector2 anchoredPosition { get; set; }
        public Vector2 sizeDelta { get; set; }
        public Vector2 anchorMin { get; set; }
        public Vector2 anchorMax { get; set; }
        public Vector2 pivot { get; set; }
        public Vector2 offsetMin { get; set; }
        public Vector2 offsetMax { get; set; }
        public Rect rect => default;
        public void SetSizeWithCurrentAnchors(Axis axis, float size) { }
        public enum Axis { Horizontal, Vertical }
    }

    public enum Space { World, Self }

    // ---- Attributes ----
    public class SerializeField : Attribute { }
    public class HideInInspector : Attribute { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string header) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string tooltip) { } }
    public class SpaceAttribute : Attribute { public SpaceAttribute() { } public SpaceAttribute(float height) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(float min, float max) { } }
    public class MinAttribute : Attribute { public MinAttribute(float min) { } }
    public class TextAreaAttribute : Attribute { public TextAreaAttribute() { } public TextAreaAttribute(int min, int max) { } }
    public class MultilineAttribute : Attribute { public MultilineAttribute() { } public MultilineAttribute(int lines) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; public int order; }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponent : Attribute { public RequireComponent(Type t) { } public RequireComponent(Type t1, Type t2) { } }
    public class ContextMenu : Attribute { public ContextMenu(string name) { } }
    public class ExecuteInEditMode : Attribute { }
    public class ExecuteAlways : Attribute { }
    public class DisallowMultipleComponent : Attribute { }
    public class AddComponentMenu : Attribute { public AddComponentMenu(string menuName) { } }
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute() { }
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType) { }
    }
    public enum RuntimeInitializeLoadType { AfterSceneLoad, BeforeSceneLoad, AfterAssembliesLoaded, BeforeSplashScreen, SubsystemRegistration }

    // ---- Resources / PlayerPrefs / Application ----
    public static class Resources
    {
        public static T Load<T>(string path) where T : Object => null;
        public static Object Load(string path) => null;
        public static T[] LoadAll<T>(string path) where T : Object => new T[0];
        public static void UnloadAsset(Object asset) { }
        public static AsyncOperation UnloadUnusedAssets() => null;
    }

    public static class PlayerPrefs
    {
        public static int GetInt(string key, int def = 0) => def;
        public static void SetInt(string key, int value) { }
        public static float GetFloat(string key, float def = 0f) => def;
        public static void SetFloat(string key, float value) { }
        public static string GetString(string key, string def = "") => def;
        public static void SetString(string key, string value) { }
        public static bool HasKey(string key) => false;
        public static void DeleteKey(string key) { }
        public static void DeleteAll() { }
        public static void Save() { }
    }

    public static class Application
    {
        public static string persistentDataPath => "/tmp";
        public static string temporaryCachePath => "/tmp";
        public static string dataPath => "/tmp";
        public static string streamingAssetsPath => "/tmp";
        public static bool isPlaying => true;
        public static bool isEditor => false;
        public static RuntimePlatform platform => RuntimePlatform.LinuxPlayer;
        public static string version => "1.0";
        public static string productName => "";
        public static int targetFrameRate { get; set; }
        public static event Action<string, string, LogType> logMessageReceived { add { } remove { } }
        public static void Quit() { }
        public static void OpenURL(string url) { }
        public static string systemLanguage => "English";
        public static bool runInBackground { get; set; }
    }

    public enum RuntimePlatform { WindowsPlayer, OSXPlayer, LinuxPlayer, Android, IPhonePlayer, WebGLPlayer, WindowsEditor, OSXEditor, LinuxEditor }
    public enum LogType { Error, Assert, Warning, Log, Exception }
    public enum DeviceType { Unknown, Handheld, Console, Desktop }
    public static class SystemInfo
    {
        public static string deviceModel => "";
        public static string operatingSystem => "";
        public static int systemMemorySize => 0;
        public static string graphicsDeviceName => "";
        public static int processorCount => 1;
        public static bool supportsMotionVectors => false;
        public static bool supportsInstancing => false;
        public static int maxTextureSize => 4096;
        public static int graphicsMemorySize => 0;
        public static string deviceUniqueIdentifier => "";
        public static DeviceType deviceType => DeviceType.Desktop;
    }
    public static class QualitySettings
    {
        public static float shadowDistance { get; set; }
        public static int particleRaycastBudget { get; set; }
        public static int globalTextureMipmapLimit { get; set; }
        public static int antiAliasing { get; set; }
        public static ShadowQuality shadows { get; set; }
        public static ShadowResolution shadowResolution { get; set; }
        public static AnisotropicFiltering anisotropicFiltering { get; set; }
        public static int pixelLightCount { get; set; }
        public static float lodBias { get; set; }
        public static int GetQualityLevel() => 0;
        public static void SetQualityLevel(int index) { }
        public static void SetQualityLevel(int index, bool applyExpensiveChanges) { }
        public static string[] names => new string[0];
        public static int vSyncCount { get; set; }
    }
    public static class Screen
    {
        public static int width => 1920;
        public static int height => 1080;
        public static bool fullScreen { get; set; }
        public static Resolution currentResolution => default;
        public static Resolution[] resolutions => new Resolution[0];
        public static void SetResolution(int w, int h, bool fs) { }
        public static int sleepTimeout { get; set; }
    }
    public struct Resolution { public int width, height, refreshRate; }
    public enum ShadowQuality { Disable, HardOnly, All }
    public enum ShadowResolution { Low, Medium, High, VeryHigh }
    public enum AnisotropicFiltering { Disable, Enable, ForceEnable }
    public static class SleepTimeout { public const int NeverSleep = -1; public const int SystemSetting = -2; }
    public static class Cursor
    {
        public static bool visible { get; set; }
        public static CursorLockMode lockState { get; set; }
    }
    public enum CursorLockMode { None, Locked, Confined }

    public static class JsonUtility
    {
        public static string ToJson(object obj) => "{}";
        public static string ToJson(object obj, bool prettyPrint) => "{}";
        public static T FromJson<T>(string json) => default;
        public static void FromJsonOverwrite(string json, object objectToOverwrite) { }
    }

    public class AsyncOperation : YieldInstruction
    {
        public bool isDone => true;
        public float progress => 1f;
        public bool allowSceneActivation { get; set; }
        public event Action<AsyncOperation> completed { add { } remove { } }
    }

    // ---- Physics ----
    public class Collider : Component
    {
        public bool isTrigger { get; set; }
        public bool enabled { get; set; }
        public Bounds bounds => default;
        public Rigidbody attachedRigidbody => null;
        public Vector3 ClosestPoint(Vector3 p) => p;
    }
    public class BoxCollider : Collider { public Vector3 center { get; set; } public Vector3 size { get; set; } }
    public class SphereCollider : Collider { public Vector3 center { get; set; } public float radius { get; set; } }
    public class CapsuleCollider : Collider { public Vector3 center { get; set; } public float radius { get; set; } public float height { get; set; } public int direction { get; set; } }
    public class MeshCollider : Collider { public bool convex { get; set; } public Mesh sharedMesh { get; set; } }
    public class CharacterController : Collider
    {
        public bool isGrounded => true;
        public Vector3 velocity => default;
        public CollisionFlags Move(Vector3 motion) => default;
        public void SimpleMove(Vector3 speed) { }
        public float stepOffset { get; set; }
        public float slopeLimit { get; set; }
    }
    public enum CollisionFlags { None }

    public class Rigidbody : Component
    {
        public float mass { get; set; }
        public float drag { get; set; }
        public float angularDrag { get; set; }
        public bool useGravity { get; set; }
        public bool isKinematic { get; set; }
        public Vector3 velocity { get; set; }
        public Vector3 linearVelocity { get; set; }
        public Vector3 angularVelocity { get; set; }
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public RigidbodyConstraints constraints { get; set; }
        public RigidbodyInterpolation interpolation { get; set; }
        public CollisionDetectionMode collisionDetectionMode { get; set; }
        public void AddForce(Vector3 force) { }
        public void AddForce(Vector3 force, ForceMode mode) { }
        public void AddTorque(Vector3 torque) { }
        public void MovePosition(Vector3 position) { }
        public void MoveRotation(Quaternion rot) { }
    }
    public enum ForceMode { Force, Acceleration, Impulse, VelocityChange }
    public enum RigidbodyConstraints { None = 0, FreezeRotation = 112, FreezeRotationX = 16, FreezeRotationY = 32, FreezeRotationZ = 64, FreezePositionY = 4, FreezeAll = 126 }
    public enum RigidbodyInterpolation { None, Interpolate, Extrapolate }
    public enum CollisionDetectionMode { Discrete, Continuous, ContinuousDynamic, ContinuousSpeculative }

    public class Collision
    {
        public Collider collider => null;
        public GameObject gameObject => null;
        public Transform transform => null;
        public Vector3 relativeVelocity => default;
        public ContactPoint[] contacts => new ContactPoint[0];
        public int contactCount => 0;
        public ContactPoint GetContact(int index) => default;
    }
    public struct ContactPoint { public Vector3 point => default; public Vector3 normal => default; }

    public struct RaycastHit
    {
        public Collider collider => null;
        public Vector3 point => default;
        public Vector3 normal => default;
        public float distance => 0f;
        public Transform transform => null;
    }

    public static class Physics
    {
        public static bool Raycast(Vector3 origin, Vector3 dir) => false;
        public static bool Raycast(Vector3 origin, Vector3 dir, float maxDistance) => false;
        public static bool Raycast(Vector3 origin, Vector3 dir, float maxDistance, int layerMask) => false;
        public static bool Raycast(Vector3 origin, Vector3 dir, out RaycastHit hit) { hit = default; return false; }
        public static bool Raycast(Vector3 origin, Vector3 dir, out RaycastHit hit, float maxDistance) { hit = default; return false; }
        public static bool Raycast(Vector3 origin, Vector3 dir, out RaycastHit hit, float maxDistance, int layerMask) { hit = default; return false; }
        public static bool Raycast(Ray ray, out RaycastHit hit) { hit = default; return false; }
        public static bool Raycast(Ray ray, out RaycastHit hit, float maxDistance) { hit = default; return false; }
        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 dir, float maxDistance) => new RaycastHit[0];
        public static Collider[] OverlapSphere(Vector3 position, float radius) => new Collider[0];
        public static Collider[] OverlapSphere(Vector3 position, float radius, int layerMask) => new Collider[0];
        public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents) => new Collider[0];
        public static bool CheckSphere(Vector3 position, float radius) => false;
        public static bool CheckSphere(Vector3 position, float radius, int layerMask) => false;
        public static bool SphereCast(Vector3 origin, float radius, Vector3 dir, out RaycastHit hit, float maxDistance) { hit = default; return false; }
        public static bool SphereCast(Vector3 origin, float radius, Vector3 dir, out RaycastHit hit, float maxDistance, int layerMask) { hit = default; return false; }
        public const int DefaultRaycastLayers = -5;
    }

    public struct Ray
    {
        public Ray(Vector3 origin, Vector3 direction) { this.origin = origin; this.direction = direction; }
        public Vector3 origin { get; set; }
        public Vector3 direction { get; set; }
        public Vector3 GetPoint(float distance) => origin + direction * distance;
    }

    public struct LayerMask
    {
        public int value { get; set; }
        public static implicit operator int(LayerMask mask) => mask.value;
        public static implicit operator LayerMask(int v) => new LayerMask { value = v };
        public static int GetMask(params string[] layerNames) => 0;
        public static int NameToLayer(string layerName) => 0;
        public static string LayerToName(int layer) => "";
    }

    // ---- Rendering ----
    public class Renderer : Component
    {
        public Material material { get; set; } = new Material((Shader)null);
        public Material sharedMaterial { get; set; }
        public Material[] materials { get; set; } = new Material[0];
        public Material[] sharedMaterials { get; set; } = new Material[0];
        public bool enabled { get; set; }
        public Bounds bounds => default;
        public ShadowCastingMode shadowCastingMode { get; set; }
        public bool receiveShadows { get; set; }
    }
    public enum ShadowCastingMode { Off, On, TwoSided, ShadowsOnly }
    public class MeshRenderer : Renderer { }
    public class SkinnedMeshRenderer : Renderer
    {
        public Mesh sharedMesh { get; set; }
        public Transform[] bones { get; set; } = new Transform[0];
        public Transform rootBone { get; set; }
    }
    public class LineRenderer : Renderer
    {
        public int positionCount { get; set; }
        public float startWidth { get; set; }
        public float endWidth { get; set; }
        public Color startColor { get; set; }
        public Color endColor { get; set; }
        public bool useWorldSpace { get; set; }
        public void SetPosition(int index, Vector3 position) { }
        public void SetPositions(Vector3[] positions) { }
    }
    public class TrailRenderer : Renderer { public float time { get; set; } }

    public class MeshFilter : Component { public Mesh mesh { get; set; } public Mesh sharedMesh { get; set; } }
    public class Mesh : Object
    {
        public Vector3[] vertices { get; set; }
        public int[] triangles { get; set; }
        public Vector2[] uv { get; set; }
        public Vector3[] normals { get; set; }
        public Color[] colors { get; set; }
        public void RecalculateNormals() { }
        public void RecalculateBounds() { }
        public void Clear() { }
    }

    public class Material : Object
    {
        public Material(Shader shader) { }
        public Material(Material source) { }
        public Color color { get; set; }
        public Shader shader { get; set; }
        public Texture mainTexture { get; set; }
        public void SetColor(string name, Color value) { }
        public void SetFloat(string name, float value) { }
        public void SetInt(string name, int value) { }
        public void SetTexture(string name, Texture value) { }
        public Texture GetTexture(string name) => null;
        public Color GetColor(string name) => default;
        public float GetFloat(string name) => 0f;
        public bool HasProperty(string name) => false;
        public void EnableKeyword(string keyword) { }
        public void DisableKeyword(string keyword) { }
        public int renderQueue { get; set; }
    }
    public class Shader : Object { public static Shader Find(string name) => null; }
    public class MaterialPropertyBlock
    {
        public void SetColor(string name, Color value) { }
        public void SetFloat(string name, float value) { }
        public void Clear() { }
    }
    public class Texture : Object { public int width => 0; public int height => 0; }
    public class Texture2D : Texture
    {
        public Texture2D(int width, int height) { }
        public Texture2D(int width, int height, TextureFormat format, bool mipChain) { }
        public void SetPixel(int x, int y, Color color) { }
        public void SetPixels(Color[] colors) { }
        public Color GetPixel(int x, int y) => default;
        public void Apply() { }
        public byte[] EncodeToPNG() => new byte[0];
        public bool LoadImage(byte[] data) => false;
    }
    public enum TextureFormat { RGBA32, RGB24, ARGB32 }
    public class RenderTexture : Texture { public RenderTexture(int w, int h, int depth) { } }
    public class Sprite : Object
    {
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot) => null;
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit) => null;
        public Texture2D texture => null;
        public Rect rect => default;
    }

    public class Camera : Behaviour
    {
        public static Camera main => null;
        public static Camera[] allCameras => new Camera[0];
        public float fieldOfView { get; set; }
        public float nearClipPlane { get; set; }
        public float farClipPlane { get; set; }
        public float orthographicSize { get; set; }
        public bool orthographic { get; set; }
        public Color backgroundColor { get; set; }
        public CameraClearFlags clearFlags { get; set; }
        public int cullingMask { get; set; }
        public float depth { get; set; }
        public Rect rect { get; set; }
        public RenderTexture targetTexture { get; set; }
        public Ray ScreenPointToRay(Vector3 pos) => default;
        public Vector3 WorldToScreenPoint(Vector3 position) => position;
        public Vector3 ScreenToWorldPoint(Vector3 position) => position;
        public Vector3 ViewportToWorldPoint(Vector3 position) => position;
        public void Render() { }
    }
    public enum CameraClearFlags { Skybox, SolidColor, Depth, Nothing, Color }

    public class Light : Behaviour
    {
        public LightType type { get; set; }
        public Color color { get; set; }
        public float intensity { get; set; }
        public float range { get; set; }
        public float spotAngle { get; set; }
        public LightShadows shadows { get; set; }
    }
    public enum LightType { Spot, Directional, Point, Area }
    public enum LightShadows { None, Hard, Soft }
    public static class RenderSettings
    {
        public static Color ambientLight { get; set; }
        public static bool fog { get; set; }
        public static Color fogColor { get; set; }
        public static float fogDensity { get; set; }
        public static Material skybox { get; set; }
        public static AmbientMode ambientMode { get; set; }
        public static float ambientIntensity { get; set; }
    }
    public enum AmbientMode { Skybox, Trilight, Flat, Custom }

    public class ParticleSystem : Component
    {
        public void Play() { }
        public void Stop() { }
        public void Pause() { }
        public bool isPlaying => false;
        public MainModule main => default;
        public EmissionModule emission => default;
        public ShapeModule shape => default;
        public struct MainModule
        {
            public float duration { get; set; }
            public bool loop { get; set; }
            public MinMaxCurve startLifetime { get; set; }
            public MinMaxCurve startSpeed { get; set; }
            public MinMaxCurve startSize { get; set; }
            public MinMaxGradient startColor { get; set; }
            public int maxParticles { get; set; }
            public bool playOnAwake { get; set; }
        }
        public struct EmissionModule { public MinMaxCurve rateOverTime { get; set; } public bool enabled { get; set; } public void SetBursts(Burst[] bursts) { } }
        public struct ShapeModule { public ParticleSystemShapeType shapeType { get; set; } public float radius { get; set; } public float angle { get; set; } }
        public struct Burst
        {
            public Burst(float time, short count) { }
            public Burst(float time, int count) { }
        }
        public struct ColorOverLifetimeModule { public bool enabled { get; set; } public MinMaxGradient color { get; set; } }
        public struct SizeOverLifetimeModule { public bool enabled { get; set; } public MinMaxCurve size { get; set; } }
        public struct VelocityOverLifetimeModule { public bool enabled { get; set; } }
        public ColorOverLifetimeModule colorOverLifetime => default;
        public SizeOverLifetimeModule sizeOverLifetime => default;
        public VelocityOverLifetimeModule velocityOverLifetime => default;
        public struct MinMaxCurve
        {
            public MinMaxCurve(float constant) { }
            public MinMaxCurve(float min, float max) { }
            public static implicit operator MinMaxCurve(float constant) => new MinMaxCurve(constant);
        }
        public struct MinMaxGradient
        {
            public MinMaxGradient(Color color) { }
            public MinMaxGradient(Gradient gradient) { }
            public static implicit operator MinMaxGradient(Color color) => new MinMaxGradient(color);
            public static implicit operator MinMaxGradient(Gradient g) => new MinMaxGradient(g);
        }
    }
    public enum ParticleSystemShapeType { Sphere, Hemisphere, Cone, Box, Circle }

    // ---- Animation ----
    public class Animator : Behaviour
    {
        public RuntimeAnimatorController runtimeAnimatorController { get; set; }
        public bool applyRootMotion { get; set; }
        public float speed { get; set; }
        public bool enabled { get; set; }
        public void SetTrigger(string name) { }
        public void ResetTrigger(string name) { }
        public void SetBool(string name, bool value) { }
        public bool GetBool(string name) => false;
        public void SetFloat(string name, float value) { }
        public void SetFloat(string name, float value, float dampTime, float deltaTime) { }
        public float GetFloat(string name) => 0f;
        public void SetInteger(string name, int value) { }
        public int GetInteger(string name) => 0;
        public void Play(string stateName) { }
        public void Play(string stateName, int layer) { }
        public void CrossFade(string stateName, float duration) { }
        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layer) => default;
        public bool HasState(int layer, int stateId) => false;
        public static int StringToHash(string name) => 0;
        public AnimatorControllerParameter[] parameters => new AnimatorControllerParameter[0];
        public Avatar avatar { get; set; }
        public bool isHuman => false;
        public Transform GetBoneTransform(HumanBodyBones humanBoneId) => null;
        public void Rebind() { }
    }
    public struct AnimatorStateInfo
    {
        public bool IsName(string name) => false;
        public float normalizedTime => 0f;
        public int shortNameHash => 0;
        public float length => 0f;
    }
    public class AnimatorControllerParameter
    {
        public string name { get; set; }
        public AnimatorControllerParameterType type { get; set; }
    }
    public enum AnimatorControllerParameterType { Float = 1, Int = 3, Bool = 4, Trigger = 9 }
    public class RuntimeAnimatorController : Object { }
    public class Avatar : Object { public bool isValid => false; public bool isHuman => false; }
    public enum HumanBodyBones
    {
        Hips, LeftUpperLeg, RightUpperLeg, LeftLowerLeg, RightLowerLeg, LeftFoot, RightFoot,
        Spine, Chest, Neck, Head, LeftShoulder, RightShoulder, LeftUpperArm, RightUpperArm,
        LeftLowerArm, RightLowerArm, LeftHand, RightHand, LeftToes, RightToes, LastBone
    }
    public class Animation : Behaviour
    {
        public AnimationClip clip { get; set; }
        public bool playAutomatically { get; set; }
        public bool isPlaying => false;
        public void Play() { }
        public bool Play(string animation) => true;
        public void Stop() { }
        public void AddClip(AnimationClip clip, string newName) { }
        public AnimationState this[string name] => null;
        public void Rewind() { }
        public void Sample() { }
    }
    public class AnimationState
    {
        public float speed { get; set; }
        public float time { get; set; }
        public WrapMode wrapMode { get; set; }
    }
    public enum WrapMode { Once, Loop, PingPong, Default, ClampForever }
    public class AnimationClip : Object { public float length => 0f; }
    public class AnimationCurve
    {
        public AnimationCurve(params Keyframe[] keys) { }
        public AnimationCurve() { }
        public float Evaluate(float time) => 0f;
        public static AnimationCurve Linear(float t0, float v0, float t1, float v1) => new AnimationCurve();
        public static AnimationCurve EaseInOut(float t0, float v0, float t1, float v1) => new AnimationCurve();
        public Keyframe[] keys { get; set; }
        public int AddKey(float time, float value) => 0;
    }
    public class Gradient
    {
        public GradientColorKey[] colorKeys { get; set; }
        public GradientAlphaKey[] alphaKeys { get; set; }
        public void SetKeys(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys) { }
        public Color Evaluate(float time) => default;
    }
    public struct GradientColorKey
    {
        public GradientColorKey(Color color, float time) { this.color = color; this.time = time; }
        public Color color; public float time;
    }
    public struct GradientAlphaKey
    {
        public GradientAlphaKey(float alpha, float time) { this.alpha = alpha; this.time = time; }
        public float alpha; public float time;
    }

    public struct Keyframe
    {
        public Keyframe(float time, float value) { this.time = time; this.value = value; }
        public float time { get; set; }
        public float value { get; set; }
    }

    // ---- Audio ----
    public class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; }
        public bool loop { get; set; }
        public bool playOnAwake { get; set; }
        public float volume { get; set; }
        public float pitch { get; set; }
        public float spatialBlend { get; set; }
        public float minDistance { get; set; }
        public float maxDistance { get; set; }
        public bool isPlaying => false;
        public float time { get; set; }
        public bool mute { get; set; }
        public int priority { get; set; }
        public UnityEngine.Audio.AudioMixerGroup outputAudioMixerGroup { get; set; }
        public AudioRolloffMode rolloffMode { get; set; }
        public void Play() { }
        public void PlayDelayed(float delay) { }
        public void Stop() { }
        public void Pause() { }
        public void UnPause() { }
        public void PlayOneShot(AudioClip clip) { }
        public void PlayOneShot(AudioClip clip, float volumeScale) { }
        public static void PlayClipAtPoint(AudioClip clip, Vector3 position) { }
        public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume) { }
    }
    public enum AudioRolloffMode { Logarithmic, Linear, Custom }
    public enum AudioSpeakerMode { Mono, Stereo, Quad, Surround, Mode5point1, Mode7point1 }
    public static class AudioSettings
    {
        public static AudioConfiguration GetConfiguration() => default;
        public static bool Reset(AudioConfiguration config) => true;
        public static AudioSpeakerMode speakerMode { get; set; }
    }
    public struct AudioConfiguration
    {
        public AudioSpeakerMode speakerMode;
        public int dspBufferSize;
        public int sampleRate;
        public int numRealVoices;
        public int numVirtualVoices;
    }
    public class AudioClip : Object
    {
        public float length => 0f;
        public int samples => 0;
        public int channels => 0;
        public int frequency => 0;
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream) => null;
        public bool SetData(float[] data, int offsetSamples) => false;
        public bool GetData(float[] data, int offsetSamples) => false;
    }
    public class AudioListener : Behaviour
    {
        public static float volume { get; set; }
        public static bool pause { get; set; }
    }

    // ---- Misc ----
    public class GUIStyle
    {
        public GUIStyle() { }
        public GUIStyle(GUIStyle other) { }
        public GUIStyle(string name) { }
        public int fontSize { get; set; }
        public FontStyle fontStyle { get; set; }
        public TextAnchor alignment { get; set; }
        public GUIStyleState normal { get; set; } = new GUIStyleState();
        public GUIStyleState hover { get; set; } = new GUIStyleState();
        public GUIStyleState active { get; set; } = new GUIStyleState();
        public RectOffset padding { get; set; } = new RectOffset();
        public RectOffset margin { get; set; } = new RectOffset();
        public bool wordWrap { get; set; }
        public bool richText { get; set; }
        public Font font { get; set; }
        public float fixedWidth { get; set; }
        public float fixedHeight { get; set; }
        public static implicit operator GUIStyle(string name) => new GUIStyle(name);
    }
    public class GUIStyleState
    {
        public Color textColor { get; set; }
        public Texture2D background { get; set; }
    }
    public class GUIContent
    {
        public GUIContent(string text) { }
        public string text { get; set; }
    }
    public class GUISkin : ScriptableObject
    {
        public GUIStyle label { get; set; } = new GUIStyle();
        public GUIStyle button { get; set; } = new GUIStyle();
        public GUIStyle box { get; set; } = new GUIStyle();
    }
    public static class GUI
    {
        public static Color color { get; set; }
        public static Color backgroundColor { get; set; }
        public static Color contentColor { get; set; }
        public static GUISkin skin { get; set; } = new GUISkin();
        public static bool enabled { get; set; }
        public static void Label(Rect position, string text) { }
        public static void Label(Rect position, string text, GUIStyle style) { }
        public static bool Button(Rect position, string text) => false;
        public static bool Button(Rect position, string text, GUIStyle style) => false;
        public static void Box(Rect position, string text) { }
        public static void Box(Rect position, string text, GUIStyle style) { }
        public static void DrawTexture(Rect position, Texture image) { }
        public static string TextField(Rect position, string text) => text;
        public static bool Toggle(Rect position, bool value, string text) => value;
        public static float HorizontalSlider(Rect position, float value, float left, float right) => value;
        public static void BeginGroup(Rect position) { }
        public static void BeginGroup(Rect position, GUIStyle style) { }
        public static void EndGroup() { }
    }
    public static class GUILayout
    {
        public class Option { }
        public static void Label(string text, params Option[] options) { }
        public static void Label(string text, GUIStyle style, params Option[] options) { }
        public static bool Button(string text, params Option[] options) => false;
        public static bool Button(string text, GUIStyle style, params Option[] options) => false;
        public static void Box(string text, params Option[] options) { }
        public static void Box(string text, GUIStyle style, params Option[] options) { }
        public static void Space(float pixels) { }
        public static void FlexibleSpace() { }
        public static void BeginHorizontal(params Option[] options) { }
        public static void BeginHorizontal(GUIStyle style, params Option[] options) { }
        public static void EndHorizontal() { }
        public static void BeginVertical(params Option[] options) { }
        public static void BeginVertical(GUIStyle style, params Option[] options) { }
        public static void EndVertical() { }
        public static void BeginArea(Rect screenRect) { }
        public static void BeginArea(Rect screenRect, GUIStyle style) { }
        public static void EndArea() { }
        public static Vector2 BeginScrollView(Vector2 scrollPosition, params Option[] options) => scrollPosition;
        public static void EndScrollView() { }
        public static string TextField(string text, params Option[] options) => text;
        public static bool Toggle(bool value, string text, params Option[] options) => value;
        public static float HorizontalSlider(float value, float left, float right, params Option[] options) => value;
        public static Option Width(float width) => new Option();
        public static Option Height(float height) => new Option();
        public static Option ExpandWidth(bool expand) => new Option();
        public static Option ExpandHeight(bool expand) => new Option();
        public static Option MinWidth(float w) => new Option();
        public static Option MaxWidth(float w) => new Option();
    }

    public class Gizmos
    {
        public static Color color { get; set; }
        public static void DrawLine(Vector3 from, Vector3 to) { }
        public static void DrawSphere(Vector3 center, float radius) { }
        public static void DrawWireSphere(Vector3 center, float radius) { }
        public static void DrawCube(Vector3 center, Vector3 size) { }
        public static void DrawWireCube(Vector3 center, Vector3 size) { }
        public static void DrawRay(Vector3 from, Vector3 direction) { }
    }

    public static class Input
    {
        public static bool GetKey(KeyCode key) => false;
        public static bool GetKey(string name) => false;
        public static bool GetKeyDown(string name) => false;
        public static bool GetKeyUp(string name) => false;
        public static bool GetKeyDown(KeyCode key) => false;
        public static bool GetKeyUp(KeyCode key) => false;
        public static bool GetMouseButton(int button) => false;
        public static bool GetMouseButtonDown(int button) => false;
        public static bool GetMouseButtonUp(int button) => false;
        public static Vector3 mousePosition => default;
        public static float GetAxis(string axisName) => 0f;
        public static float GetAxisRaw(string axisName) => 0f;
        public static bool GetButton(string buttonName) => false;
        public static bool GetButtonDown(string buttonName) => false;
        public static bool anyKeyDown => false;
        public static int touchCount => 0;
        public static Touch[] touches => new Touch[0];
        public static Touch GetTouch(int index) => default;
    }

    public struct Touch
    {
        public Vector2 position => default;
        public Vector2 deltaPosition => default;
        public TouchPhase phase => TouchPhase.Began;
        public int fingerId => 0;
    }
    public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }

    public enum KeyCode
    {
        None, Space, Return, Escape, Tab, LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt, Backspace,
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,
        Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9,
        UpArrow, DownArrow, LeftArrow, RightArrow, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        Mouse0, Mouse1, Mouse2
    }

    namespace Events
    {
        public class UnityEvent
        {
            public void AddListener(UnityAction call) { }
            public void RemoveListener(UnityAction call) { }
            public void RemoveAllListeners() { }
            public void Invoke() { }
        }
        public class UnityEvent<T>
        {
            public void AddListener(UnityAction<T> call) { }
            public void RemoveListener(UnityAction<T> call) { }
            public void RemoveAllListeners() { }
            public void Invoke(T arg) { }
        }
        public delegate void UnityAction();
        public delegate void UnityAction<T>(T arg);
    }

    namespace Audio
    {
        public class AudioMixer : Object
        {
            public bool SetFloat(string name, float value) => true;
            public bool GetFloat(string name, out float value) { value = 0; return true; }
            public AudioMixerGroup[] FindMatchingGroups(string subPath) => new AudioMixerGroup[0];
        }
        public class AudioMixerGroup : Object { public AudioMixer audioMixer => null; }
        public class AudioMixerSnapshot : Object { public void TransitionTo(float timeToReach) { } }
    }
}

namespace UnityEngine.Profiling
{
    public static class Profiler
    {
        public static long GetTotalAllocatedMemoryLong() => 0;
        public static long GetTotalReservedMemoryLong() => 0;
        public static void BeginSample(string name) { }
        public static void EndSample() { }
    }
}
