#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace AgilityDogs.Editor
{
    public class URPMaterialConverter : EditorWindow
    {
        [MenuItem("Agility Dogs/Setup/Convert Dog Materials to URP")]
        public static void ShowWindow()
        {
            ConvertDogMaterials();
        }

        [MenuItem("Agility Dogs/Setup/Convert All Red_Deer Materials to URP")]
        public static void ConvertAllRedDeerMaterials()
        {
            string basePath = "Assets/Red_Deer/Dogs";
            string[] matFiles = Directory.GetFiles(basePath, "*.mat", SearchOption.AllDirectories);
            
            int converted = 0;
            int skipped = 0;
            
            foreach (string matPath in matFiles)
            {
                if (ConvertMaterialToURP(matPath))
                    converted++;
                else
                    skipped++;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[URP Converter] Converted {converted} materials, skipped {skipped} (already URP or error)");
            EditorUtility.DisplayDialog("URP Conversion Complete", 
                $"Converted {converted} materials to URP.\nSkipped {skipped} materials.", "OK");
        }

        public static void ConvertDogMaterials()
        {
            string[] dogFolders = {
                "Assets/Red_Deer/Dogs/Beagle/Dog/Materials",
                "Assets/Red_Deer/Dogs/Border_Collie/Dog/Materials",
                "Assets/Red_Deer/Dogs/Boxer/Dog/Materials",
                "Assets/Red_Deer/Dogs/BullTerrier/Dog/Materials",
                "Assets/Red_Deer/Dogs/Corgi/Dog/Materials",
                "Assets/Red_Deer/Dogs/Dalmatian/Dog/Materials",
                "Assets/Red_Deer/Dogs/Doberman/Dog/Materials",
                "Assets/Red_Deer/Dogs/FrenchBulldog/Dog/Materials",
                "Assets/Red_Deer/Dogs/GoldenRetriever/Dog/Materials",
                "Assets/Red_Deer/Dogs/Husky/Dog/Materials",
                "Assets/Red_Deer/Dogs/JackRussellTerrier/Dog/Materials",
                "Assets/Red_Deer/Dogs/Labrador/Dog/Materials",
                "Assets/Red_Deer/Dogs/Pitbull/Dog/Materials",
                "Assets/Red_Deer/Dogs/Pug/Dog/Materials",
                "Assets/Red_Deer/Dogs/Rottweiler/Dog/Materials",
                "Assets/Red_Deer/Dogs/Shepherd/Dog/Materials",
                "Assets/Red_Deer/Dogs/ShibaInu/Dog/Materials",
                "Assets/Red_Deer/Dogs/Spitz/Dog/Materials",
                "Assets/Red_Deer/Dogs/ToyTerrier/Dog/Materials"
            };
            
            int converted = 0;
            
            foreach (string folder in dogFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                
                string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { folder });
                foreach (string guid in matGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (ConvertMaterialToURP(path))
                        converted++;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[URP Converter] Converted {converted} dog materials to URP");
            EditorUtility.DisplayDialog("Dog Materials Converted", 
                $"Converted {converted} dog materials to URP.", "OK");
        }

        private static bool ConvertMaterialToURP(string materialPath)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat == null) return false;
            
            // Skip if already URP
            if (mat.shader != null && mat.shader.name.Contains("Universal Render Pipeline"))
                return false;
            
            // Store original properties
            Color mainColor = mat.HasProperty("_Color") ? mat.color : Color.white;
            Texture mainTex = mat.GetTexture("_MainTex");
            Texture normalMap = mat.GetTexture("_BumpMap");
            Texture metallicMap = mat.GetTexture("_MetallicGlossMap");
            float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
            float smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;
            
            // Check if cutout
            bool isCutout = mat.HasProperty("_Mode") && mat.GetFloat("_Mode") >= 1;
            
            // Find URP Lit shader
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                Debug.LogWarning($"[URP Converter] URP/Lit shader not found for {materialPath}");
                return false;
            }
            
            // Apply URP shader
            mat.shader = urpShader;
            
            // Restore properties
            mat.color = mainColor;
            if (mainTex != null) mat.SetTexture("_BaseMap", mainTex);
            if (normalMap != null)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (metallicMap != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallicMap);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            
            // Set up cutout if needed
            if (isCutout)
            {
                mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
                mat.SetFloat("_AlphaClip", 1);
                mat.SetFloat("_Cutoff", 0.5f);
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                mat.SetShaderPassEnabled("ShadowCaster", true);
            }
            
            EditorUtility.SetDirty(mat);
            return true;
        }
    }
}
#endif
