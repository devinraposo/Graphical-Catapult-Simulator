using UnityEngine;
using UnityEngine.SceneManagement;
#if (UNITY_EDITOR)
using UnityEditor;
using UnityEditor.SceneManagement;


// ensure class initializer is called whenever scripts recompile
[InitializeOnLoad]
public class AutosaveOnRun : MonoBehaviour
{
    // register an event handler when the class is initialized
    static AutosaveOnRun()
    {
        EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
            // If we're about to run the scene...
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                // Save the scene and the assets.
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
            }
        };
    }
}
#endif