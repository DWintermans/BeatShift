using System.Linq;
using UnityEditor;

public static class BuildScript
{
    public static void PerformBuild()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        BuildPipeline.BuildPlayer(scenes, "build/WebGL", BuildTarget.WebGL, BuildOptions.None);
    }
}
    