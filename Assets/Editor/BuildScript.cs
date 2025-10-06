using UnityEditor;

public static class BuildScript
{
    public static void PerformBuild()
    {
        string[] scenes = { "Assets/Scenes/Prototype.unity" };
        BuildPipeline.BuildPlayer(scenes, "build/WebGL", BuildTarget.WebGL, BuildOptions.None);
    }
}