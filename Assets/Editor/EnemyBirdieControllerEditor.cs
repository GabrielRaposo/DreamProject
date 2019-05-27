using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EBirdie))]
public class EnemyBirdieControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EnemyController myScript = (EnemyController)target;
        if(GUILayout.Button("Swap Mode"))
        {
            myScript.ChangePhase();
        }

        DrawDefaultInspector();
    }
}

