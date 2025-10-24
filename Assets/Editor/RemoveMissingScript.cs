#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RemoveMissingScriptsTool
{
    [MenuItem("Tools/Remove Missing Scripts In Scene")]
    static void RemoveMissingScriptsInScene()
    {
        int count = 0;
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
        }
        Debug.Log($"Removed {count} missing script components from active scene.");
    }

    [MenuItem("Tools/Remove Missing Scripts In Selection")]
    static void RemoveMissingScriptsInSelection()
    {
        int count = 0;
        foreach (var go in Selection.gameObjects)
        {
            count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }
        Debug.Log($"Removed {count} missing script components from selection.");
    }
}
#endif
