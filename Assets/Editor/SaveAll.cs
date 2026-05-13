using UnityEditor;
using UnityEditor.SceneManagement;

public static class SaveAll
{
    public static void Execute()
    {
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        UnityEngine.Debug.Log("[SaveAll] Escena y assets guardados.");
    }
}
