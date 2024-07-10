using UnityEditor.Callbacks;

public class PostBuildActions
{
    [PostProcessBuild]
    public static void OnPostprocessBuild()
    {
        JsonUpdater.UpdateJson();
    }
}
