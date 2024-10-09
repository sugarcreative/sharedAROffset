using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneSwitcher))]
public class SceneSwitcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneSwitcher switcher = (SceneSwitcher)target;

        if (GUI.changed)
        {
            switcher.SwitchScene();
            EditorUtility.SetDirty(switcher); 
        }
    }
}
