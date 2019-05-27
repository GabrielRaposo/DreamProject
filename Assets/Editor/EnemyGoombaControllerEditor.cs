using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EGoomba))]
public class EnemyGoombaControllerEditor : Editor
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
