using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EBunny))]
public class EnemyBunnyControllerEditor : Editor
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
