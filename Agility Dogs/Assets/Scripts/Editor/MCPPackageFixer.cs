using System.IO;
using UnityEngine;
using UnityEditor;

namespace AgilityDogs.Editor
{
    /// <summary>
    /// Automatically removes test-related files from the MCP for Unity package
    /// to prevent compilation errors when TestFramework is not properly loaded.
    /// </summary>
    [InitializeOnLoad]
    public class MCPPackageFixer
    {
        private const string MCP_PACKAGE_NAME = "com.coplaydev.unity-mcp";
        private const string MARKER_FILE = "Packages/" + MCP_PACKAGE_NAME + "/.testfiles_removed";

        static MCPPackageFixer()
        {
            // Run on domain reload (when scripts compile)
            EditorApplication.delayCall += RemoveMCPTestFiles;
        }

        [MenuItem("Agility Dogs/Fix MCP Package")]
        public static void RemoveMCPTestFiles()
        {
            string packageCachePath = Path.Combine(Application.dataPath, "../Library/PackageCache");
            string packagesPath = Path.Combine(Application.dataPath, "../Packages");

            // Check both cached and local package locations
            string[] possiblePaths = new string[]
            {
                Path.Combine(packagesPath, MCP_PACKAGE_NAME),
                GetCachedPackagePath(packageCachePath)
            };

            bool filesRemoved = false;

            foreach (string packagePath in possiblePaths)
            {
                if (string.IsNullOrEmpty(packagePath) || !Directory.Exists(packagePath))
                    continue;

                // Remove test-related Editor files
                string[] testFiles = new string[]
                {
                    Path.Combine(packagePath, "Editor/Services/TestRunnerService.cs"),
                    Path.Combine(packagePath, "Editor/Services/TestRunnerNoThrottle.cs"),
                    Path.Combine(packagePath, "Editor/Services/TestJobManager.cs"),
                    Path.Combine(packagePath, "Editor/Services/TestRunStatus.cs"),
                    Path.Combine(packagePath, "Editor/Services/ITestRunnerService.cs"),
                    Path.Combine(packagePath, "Editor/Tools/RunTests.cs")
                };

                foreach (string file in testFiles)
                {
                    if (File.Exists(file))
                    {
                        try
                        {
                            File.Delete(file);
                            // Also delete meta file if it exists
                            string metaFile = file + ".meta";
                            if (File.Exists(metaFile))
                                File.Delete(metaFile);
                            
                            filesRemoved = true;
                            Debug.Log($"[MCPPackageFixer] Removed: {Path.GetFileName(file)}");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[MCPPackageFixer] Could not remove {file}: {ex.Message}");
                        }
                    }
                }

                // Remove Tests folder
                string testsFolder = Path.Combine(packagePath, "Editor/Resources/Tests");
                if (Directory.Exists(testsFolder))
                {
                    try
                    {
                        Directory.Delete(testsFolder, true);
                        string testsMeta = testsFolder + ".meta";
                        if (Directory.Exists(testsMeta))
                            Directory.Delete(testsMeta, true);
                        
                        filesRemoved = true;
                        Debug.Log($"[MCPPackageFixer] Removed Tests folder");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[MCPPackageFixer] Could not remove Tests folder: {ex.Message}");
                    }
                }

                // Remove TestRunner folder
                string testRunnerFolder = Path.Combine(packagePath, "Editor/Services/TestRunner");
                if (Directory.Exists(testRunnerFolder))
                {
                    try
                    {
                        Directory.Delete(testRunnerFolder, true);
                        filesRemoved = true;
                        Debug.Log($"[MCPPackageFixer] Removed TestRunner folder");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[MCPPackageFixer] Could not remove TestRunner folder: {ex.Message}");
                    }
                }
            }

            if (filesRemoved)
            {
                Debug.Log("[MCPPackageFixer] MCP test files removed. Assembly reload may be needed.");
                AssetDatabase.Refresh();
            }
        }

        private static string GetCachedPackagePath(string packageCachePath)
        {
            if (!Directory.Exists(packageCachePath))
                return null;

            // Look for MCP package in cache (folder name includes version hash)
            string[] dirs = Directory.GetDirectories(packageCachePath, MCP_PACKAGE_NAME + "*");
            if (dirs.Length > 0)
                return dirs[0]; // Return first match

            return null;
        }

        [MenuItem("Agility Dogs/Fix MCP Package (Force Refresh)")]
        public static void ForceRefreshAndFix()
        {
            RemoveMCPTestFiles();
            
            // Force a script recompilation
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }
}