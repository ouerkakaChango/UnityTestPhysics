using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicGroup))]
public class PhysicGroupEditor : Editor
{
    PhysicGroup Target;
    void OnEnable()
    {
        Target = (PhysicGroup)target;
    }

    //@@@
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //SerializedProperty weaponTime = serializedObject.FindProperty("weaponTime");
        //EditorGUILayout.PropertyField(weaponTime, new GUIContent("weaponTime"), true);

        //Target.weaponTime.up = EditorGUILayout.FloatField("weaponUp", Target.weaponTime.up);
        //var newcd = EditorGUILayout.FloatField("weaponCD", Target.weaponTime.cd);
        //if(newcd!=Target.weaponTime.cd)
        //{
        //    Target.weaponTime.cd = newcd;
        //}
        //Target.weaponTime.down = EditorGUILayout.FloatField("weaponDown", Target.weaponTime.down);

        if(GUILayout.Button("CreateGroup"))
        {
            //Target.AddImpulse();
            Target.CreateGroup();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
