public static class SceneLoadContext
{
    public static string TargetSceneName { get; private set; }

    public static void SetTargetScene(string sceneName)
    {
        TargetSceneName = sceneName;
    }

    public static void Clear()
    {
        TargetSceneName = string.Empty;
    }
}
