#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AgilityDogs.Editor
{
    /// <summary>
    /// Editor utility for creating placeholder character portrait sprites
    /// These are simple colored circles that can be replaced with actual art later
    /// </summary>
    public class CharacterPortraitGenerator : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<CharacterPortraitInfo> characters = new List<CharacterPortraitInfo>
        {
            new CharacterPortraitInfo { id = "narrator", name = "Narrator", color = Color.gray },
            new CharacterPortraitInfo { id = "coach_sarah", name = "Coach Sarah Chen", color = new Color(0.8f, 0.6f, 0.4f) },
            new CharacterPortraitInfo { id = "marcus", name = "Marcus Chen", color = new Color(0.4f, 0.7f, 0.4f) },
            new CharacterPortraitInfo { id = "emily", name = "Emily Rodriguez", color = new Color(0.6f, 0.4f, 0.8f) },
            new CharacterPortraitInfo { id = "victoria", name = "Victoria Price", color = new Color(0.9f, 0.9f, 0.3f) },
            new CharacterPortraitInfo { id = "announcer", name = "Announcer", color = new Color(0.4f, 0.6f, 0.8f) }
        };

        [MenuItem("Agility Dogs/Generate Character Portraits")]
        public static void ShowWindow()
        {
            GetWindow<CharacterPortraitGenerator>("Portrait Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Character Portrait Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This generates placeholder portrait sprites for characters.\n" +
                "Replace these with actual character art in production.",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate All Placeholder Portraits", GUILayout.Height(30)))
            {
                GenerateAllPortraits();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Character List", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var character in characters)
            {
                EditorGUILayout.BeginHorizontal("box");
                character.foldout = EditorGUILayout.Foldout(character.foldout, character.name);

                if (character.foldout)
                {
                    EditorGUILayout.LabelField("ID:", GUILayout.Width(80));
                    character.id = EditorGUILayout.TextField(character.id, GUILayout.Width(150));

                    EditorGUILayout.LabelField("Color:", GUILayout.Width(50));
                    character.color = EditorGUILayout.ColorField(character.color, GUILayout.Width(80));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            GUILayout.Label($"Total Characters: {characters.Count}", EditorStyles.miniLabel);
        }

        private void GenerateAllPortraits()
        {
            string portraitsPath = "Assets/Data/Characters/Portraits";

            if (!AssetDatabase.IsValidFolder(portraitsPath))
            {
                string parentPath = "Assets/Data/Characters";
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    AssetDatabase.CreateFolder("Assets/Data", "Characters");
                }
                AssetDatabase.CreateFolder("Assets/Data/Characters", "Portraits");
            }

            foreach (var character in characters)
            {
                GeneratePortrait(character, portraitsPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PortraitGenerator] Generated {characters.Count} placeholder portraits");
        }

        private void GeneratePortrait(CharacterPortraitInfo info, string path)
        {
            int size = 256;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];

            Color fillColor = info.color;
            Color outlineColor = new Color(fillColor.r * 0.7f, fillColor.g * 0.7f, fillColor.b * 0.7f);

            float centerX = size / 2f;
            float centerY = size / 2f;
            float radius = size * 0.4f;
            float outlineWidth = size * 0.03f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < radius - outlineWidth)
                    {
                        // Inside circle - use fill color with slight gradient
                        float gradient = 1f - (dy / radius) * 0.2f;
                        colors[y * size + x] = new Color(
                            fillColor.r * gradient,
                            fillColor.g * gradient,
                            fillColor.b * gradient,
                            1f
                        );
                    }
                    else if (dist < radius)
                    {
                        // Outline
                        colors[y * size + x] = outlineColor;
                    }
                    else
                    {
                        // Outside - transparent
                        colors[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            DestroyImmediate(texture);

            string filePath = $"{path}/{info.id}_portrait.png";
            System.IO.File.WriteAllBytes(filePath, bytes);

            // Import as sprite
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                AssetDatabase.WriteImportSettingsIfDirty(filePath);
            }

            Debug.Log($"[PortraitGenerator] Generated portrait: {filePath}");
        }
    }

    [System.Serializable]
    public class CharacterPortraitInfo
    {
        public string id;
        public string name;
        public Color color;
        public bool foldout = true;
    }
}
#endif
