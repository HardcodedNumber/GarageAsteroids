using UnityEditor;
using UnityEngine;

/// <summary>
/// Clear any saved data on the asteroid as each one is created during runtime
/// </summary>
public sealed class AsteroidDisable : AssetModificationProcessor
{
    private static string[] OnWillSaveAssets(string[] paths)
    {
        foreach (string path in paths) {
            if (path.EndsWith(".prefab")) {
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefabAsset != null) {
                    Asteroid asteroid = prefabAsset.GetComponent<Asteroid>();

                    if (asteroid != null) {
                        asteroid.ClearRenderer();
                        EditorUtility.SetDirty(asteroid);
                    }
                }
            }
        }

        return paths;
    }
}