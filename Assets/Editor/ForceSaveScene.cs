using UnityEditor;
using UnityEditor.SceneManagement;

public class ForceSaveScene
{
    public static void Execute()
    {
        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        UnityEngine.Debug.Log("[ForceSaveScene] Scene saved.");
    }
}
