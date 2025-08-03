using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Asteroid))]
public sealed class AsteroidEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the existing UnityEngine.SerializeField
        base.OnInspectorGUI();

        Asteroid asteroid = target as Asteroid;

        // If the button is press and we have a valid target
        // Generate a new example shape
        if (asteroid && GUILayout.Button("Generate")) {
            asteroid.Awake();
            asteroid.ClearRenderer();
            asteroid.Initialize(Vector2.zero, Vector2.zero);
        }
    }

}
