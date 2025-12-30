using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class PlayModeSceneSetup
{
    static PlayModeSceneSetup()
    {
        // This path must match exactly where your MainMenu.unity is located.
        // Based on your project structure: Assets/Scenes/MainMenu.unity
        string scenePath = "Assets/Scenes/MainMenu.unity";
        
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        
        if (sceneAsset != null)
        {
            // This forces the Editor to always start playing from this scene
            EditorSceneManager.playModeStartScene = sceneAsset;
            // Debug.Log("Play Mode Start Scene set to: " + scenePath); 
        }
        else
        {
            Debug.LogError($"PlayModeSceneSetup: Could not find Main Menu scene at '{scenePath}'. Please verify the file path.");
        }
    }
}
