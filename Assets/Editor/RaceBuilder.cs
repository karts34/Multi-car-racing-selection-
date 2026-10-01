using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

public static class RaceBuilder
{
    [MenuItem("Afterglow/Build Windows Game")]
    public static void Build()
    {
        PlayerSettings.companyName = "Afterglow Studio";
        PlayerSettings.productName = "APEX Afterglow";
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = true;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
        QualitySettings.antiAliasing = 4;
        QualitySettings.shadowDistance = 160;
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.High;
        QualitySettings.vSyncCount = 0;
        // Keep runtime-created Standard materials and their emission variant in the player.
        Directory.CreateDirectory("Assets/Resources");
        if (AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/RuntimeSurface.mat") == null) {
            var surface = new Material(Shader.Find("Standard"));
            surface.enableInstancing = true;
            surface.EnableKeyword("_EMISSION"); surface.SetColor("_EmissionColor", Color.white);
            AssetDatabase.CreateAsset(surface, "Assets/Resources/RuntimeSurface.mat");
            var plain = new Material(Shader.Find("Standard")); plain.enableInstancing = true;
            AssetDatabase.CreateAsset(plain, "Assets/Resources/RuntimePlain.mat");
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        new GameObject("APEX - Race Director").AddComponent<Afterglow.RaceGame>();
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Afterglow.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Afterglow.unity", true) };
        AssetDatabase.SaveAssets();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/Afterglow.unity" },
            locationPathName = "Build-v8/APEX Afterglow.exe", target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Debug.Log("AFTERGLOW_BUILD: " + report.summary.result + " / " + report.summary.totalSize + " bytes");
        if (report.summary.result != BuildResult.Succeeded) throw new System.Exception("Windows build failed");
    }
}
