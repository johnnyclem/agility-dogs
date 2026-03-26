#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace AgilityDogs.Editor
{
    public class DogAnimationSetup : EditorWindow
    {
        [MenuItem("Agility Dogs/Setup/Setup Dog Animations")]
        public static void SetupDogAnimations()
        {
            // Find the animation FBX files
            string runFbxPath = "Assets/Red_Deer/Dogs/Corgi/Dog/FBX/Anim/Corgi_anim_RM.fbx";
            string idleFbxPath = "Assets/Red_Deer/Dogs/Corgi/Dog/FBX/Anim/Corgi_anim_IP.fbx";
            string controllerPath = "Assets/Scripts/Demo/DogAnimator.controller";

            Debug.Log("Setting up dog animations...");

            // First, configure the FBX import settings to extract animation clips
            ConfigureFBXForAnimation(runFbxPath, "Dog_Run");
            ConfigureFBXForAnimation(idleFbxPath, "Dog_Idle");

            // Force reimport to apply the settings
            AssetDatabase.Refresh();
            
            // Reload the FBX assets after reimport
            GameObject runFbx = AssetDatabase.LoadAssetAtPath<GameObject>(runFbxPath);
            GameObject idleFbx = AssetDatabase.LoadAssetAtPath<GameObject>(idleFbxPath);
            
            if (runFbx == null || idleFbx == null)
            {
                Debug.LogError("Could not find animation FBX files!");
                EditorUtility.DisplayDialog("Error", "Could not find animation FBX files at:\n" + runFbxPath + "\n" + idleFbxPath, "OK");
                return;
            }

            // Try to find animation clips from the FBX files
            AnimationClip runClip = FindAnimationClipInFBX(runFbxPath);
            AnimationClip idleClip = FindAnimationClipInFBX(idleFbxPath);

            if (runClip == null || idleClip == null)
            {
                Debug.Log("Animation clips not found in FBX, creating procedural clips...");
                
                // Create procedural animation clips as fallback
                if (runClip == null)
                {
                    runClip = CreateProceduralRunClip();
                    AssetDatabase.CreateAsset(runClip, "Assets/Scripts/Demo/Dog_Run.anim");
                    AssetDatabase.SaveAssets();
                    runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Scripts/Demo/Dog_Run.anim");
                    Debug.Log("Created procedural run animation clip");
                }

                if (idleClip == null)
                {
                    idleClip = CreateProceduralIdleClip();
                    AssetDatabase.CreateAsset(idleClip, "Assets/Scripts/Demo/Dog_Idle.anim");
                    AssetDatabase.SaveAssets();
                    idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Scripts/Demo/Dog_Idle.anim");
                    Debug.Log("Created procedural idle animation clip");
                }
            }
            else
            {
                Debug.Log($"Found animation clips: Run={runClip.name}, Idle={idleClip.name}");
            }

            if (runClip == null || idleClip == null)
            {
                Debug.LogError("Could not create animation clips!");
                EditorUtility.DisplayDialog("Error", "Could not create animation clips.", "OK");
                return;
            }

            // Set animation clip properties
            runClip.wrapMode = WrapMode.Loop;
            runClip.legacy = false;
            idleClip.wrapMode = WrapMode.Loop;
            idleClip.legacy = false;

            // Load or create the animator controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Clear existing layers
            while (controller.layers.Length > 0)
            {
                controller.RemoveLayer(0);
            }

            // Create base layer
            controller.AddLayer("Base Layer");
            AnimatorControllerLayer layer = controller.layers[0];
            layer.defaultWeight = 1f;

            // Create states
            AnimatorStateMachine sm = layer.stateMachine;

            // Idle state
            AnimatorState idleState = sm.AddState("Idle");
            idleState.motion = idleClip;
            idleState.speed = 1f;

            // Run state
            AnimatorState runState = sm.AddState("Run");
            runState.motion = runClip;
            runState.speed = 1.2f;

            // Jump state (use run clip as placeholder)
            AnimatorState jumpState = sm.AddState("Jump");
            jumpState.motion = runClip; // Use run clip as placeholder until we have a jump clip
            jumpState.speed = 1.5f;

            // Set default state
            sm.defaultState = idleState;

            // Add parameters
            bool hasSpeedParam = false;
            bool hasIsMovingParam = false;
            bool hasJumpParam = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == "Speed") hasSpeedParam = true;
                if (param.name == "IsMoving") hasIsMovingParam = true;
                if (param.name == "Jump") hasJumpParam = true;
            }
            if (!hasSpeedParam)
                controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            if (!hasIsMovingParam)
                controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            if (!hasJumpParam)
                controller.AddParameter("Jump", AnimatorControllerParameterType.Bool);

            // Create transitions
            // Idle -> Run
            AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToRun.duration = 0.15f;
            idleToRun.exitTime = 0f;
            idleToRun.hasExitTime = false;

            // Run -> Idle
            AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            runToIdle.duration = 0.15f;
            runToIdle.exitTime = 0f;
            runToIdle.hasExitTime = false;

            // Any State -> Jump
            AnimatorStateTransition toJump = sm.AddAnyStateTransition(jumpState);
            toJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            toJump.duration = 0.1f;
            toJump.exitTime = 0f;
            toJump.hasExitTime = false;

            // Jump -> Idle (when not moving and not jumping)
            AnimatorStateTransition jumpToIdle = jumpState.AddTransition(idleState);
            jumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Jump");
            jumpToIdle.duration = 0.2f;
            jumpToIdle.exitTime = 0.5f;
            jumpToIdle.hasExitTime = true;

            // Jump -> Run (when moving and not jumping)
            AnimatorStateTransition jumpToRun = jumpState.AddTransition(runState);
            jumpToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Jump");
            jumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            jumpToRun.duration = 0.2f;
            jumpToRun.exitTime = 0.5f;
            jumpToRun.hasExitTime = true;

            // Save changes
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log("Dog animation controller setup complete!");
            Debug.Log($"  Idle clip: {idleClip.name} ({idleClip.length:F2}s)");
            Debug.Log($"  Run clip: {runClip.name} ({runClip.length:F2}s)");

            EditorUtility.DisplayDialog("Success!", 
                "Dog animation controller has been set up!\n\n" +
                $"Idle: {idleClip.name}\n" +
                $"Run: {runClip.name}\n\n" +
                "Transitions configured:\n" +
                "- Idle -> Run when Speed > 0.1\n" +
                "- Run -> Idle when Speed < 0.1", "OK");
        }

        static void ConfigureFBXForAnimation(string fbxPath, string clipName)
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (modelImporter == null)
            {
                Debug.LogWarning($"Could not get ModelImporter for {fbxPath}");
                return;
            }

            // Set animation type to Humanoid (value 3)
            modelImporter.animationType = (ModelImporterAnimationType)3;
            modelImporter.avatarSetup = (ModelImporterAvatarSetup)1; // CreateFromThisModel
            
            // Configure animation import
            modelImporter.importAnimation = true;
            modelImporter.animationCompression = (ModelImporterAnimationCompression)1; // Optimal
            modelImporter.animationRotationError = 0.5f;
            modelImporter.animationPositionError = 0.5f;
            modelImporter.animationScaleError = 0.5f;
            
            // Extract animation clips
            ModelImporterClipAnimation[] clips = modelImporter.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
            {
                // Create a clip animation for the entire animation
                clips = new ModelImporterClipAnimation[1];
                clips[0] = new ModelImporterClipAnimation();
                clips[0].name = clipName;
                clips[0].takeName = "Take 001";
                clips[0].firstFrame = 0;
                clips[0].lastFrame = 100; // Will be adjusted based on actual animation length
                clips[0].loop = true;
                clips[0].loopPose = true;
                clips[0].cycleOffset = 0f;
            }
            
            modelImporter.clipAnimations = clips;
            
            // Apply changes
            EditorUtility.SetDirty(modelImporter);
            Debug.Log($"Configured FBX import settings for {fbxPath}");
        }

        static AnimationClip FindAnimationClipInFBX(string fbxPath)
        {
            // Look for sub-assets within the FBX file
            string fbxDir = System.IO.Path.GetDirectoryName(fbxPath);
            string fbxName = System.IO.Path.GetFileNameWithoutExtension(fbxPath);
            
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { fbxDir });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Check if the path is a sub-asset of the FBX or contains the FBX name
                if (path.Contains(fbxName) || (path.StartsWith(fbxPath + "/")))
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (clip != null)
                    {
                        Debug.Log($"Found animation clip: {clip.name} at {path}");
                        return clip;
                    }
                }
            }
            return null;
        }

        static AnimationClip CreateProceduralRunClip()
        {
            AnimationClip clip = new AnimationClip();
            clip.name = "Dog_Run";
            clip.wrapMode = WrapMode.Loop;
            clip.legacy = false;

            // Create a simple procedural running animation
            // This animates the spine and creates a bouncing motion
            
            // Spine bobbing
            AnimationCurve spineCurve = new AnimationCurve();
            for (int i = 0; i <= 30; i++)
            {
                float time = i / 30f;
                float value = Mathf.Sin(time * Mathf.PI * 4) * 0.03f; // 4 steps per cycle
                spineCurve.AddKey(time, value);
            }
            clip.SetCurve("", typeof(Transform), "localPosition.y", spineCurve);
            
            // Slight rotation oscillation
            AnimationCurve rotCurve = new AnimationCurve();
            for (int i = 0; i <= 30; i++)
            {
                float time = i / 30f;
                float value = Mathf.Sin(time * Mathf.PI * 4) * 5f;
                rotCurve.AddKey(time, value);
            }
            clip.SetCurve("", typeof(Transform), "localEulerAngles.x", rotCurve);

            return clip;
        }

        static AnimationClip CreateProceduralIdleClip()
        {
            AnimationClip clip = new AnimationClip();
            clip.name = "Dog_Idle";
            clip.wrapMode = WrapMode.Loop;
            clip.legacy = false;

            // Create a simple idle breathing animation
            
            // Gentle breathing motion
            AnimationCurve breatheCurve = new AnimationCurve();
            for (int i = 0; i <= 30; i++)
            {
                float time = i / 30f;
                float value = Mathf.Sin(time * Mathf.PI * 2) * 0.005f; // Very subtle
                breatheCurve.AddKey(time, value);
            }
            clip.SetCurve("", typeof(Transform), "localPosition.y", breatheCurve);

            return clip;
        }

        static AnimationClip FindAnimationClip(string fbxPath, string clipName)
        {
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { System.IO.Path.GetDirectoryName(fbxPath) });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(clipName) || path.Contains(System.IO.Path.GetFileNameWithoutExtension(fbxPath)))
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (clip != null) return clip;
                }
            }
            return null;
        }

        static AnimationClip CreateAnimationClipFromFBX(string fbxPath, string clipName)
        {
            // Get all sub-assets
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssets)
            {
                if (assetPath.StartsWith(fbxPath + "/"))
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                    if (clip != null)
                    {
                        // Create a copy
                        AnimationClip newClip = new AnimationClip();
                        newClip.name = clipName;
                        EditorUtility.CopySerialized(clip, newClip);
                        return newClip;
                    }
                }
            }
            return null;
        }
    }
}
#endif
