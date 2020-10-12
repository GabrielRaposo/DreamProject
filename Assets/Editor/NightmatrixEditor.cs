using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Nightmatrix))]
public class NightmatrixEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Nightmatrix myScript = (Nightmatrix)target;
        if(GUILayout.Button("Invert Direction"))
        {
            myScript.InvertDirection();
        }

        DrawDefaultInspector();
    }
}
