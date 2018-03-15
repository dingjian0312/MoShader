using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(MoComponent))]

public class MoComponentInspector : Editor 
{
    SerializedProperty boolValue;
    SerializedProperty floatValue;
    SerializedProperty colorValue;
    SerializedProperty curveValue;
    SerializedProperty refrenceValue;
    SerializedProperty lengthValue;
    SerializedProperty colorType;
    MoComponent mo;


    void OnEnable()
    {
        boolValue = serializedObject.FindProperty("boolValue");
        floatValue = serializedObject.FindProperty("floatValue");
        colorValue = serializedObject.FindProperty("colorValue");
        curveValue = serializedObject.FindProperty("curveValue");
        refrenceValue = serializedObject.FindProperty("goRefrence");
        lengthValue = serializedObject.FindProperty("length");
        colorType = serializedObject.FindProperty("colorType");
        mo = (MoComponent)target;
    }


    public override void OnInspectorGUI()
    {
        // DrawDefaultInspector告诉Unity按照默认的方式绘制面板，这种方法在我们仅仅想要自定义某几个属性的时候会很有用
        DrawDefaultInspector();

        GUIStyle title = MoEditorHelper.TitleStyle();
        GUIStyle redBtnStyle = MoEditorHelper.ThinButtonRedStyle();

        Undo.RecordObject(mo, "Change mo");
        MoEditorHelper.DrawGuiDivider();

        EditorGUILayout.PropertyField(boolValue);
        EditorGUILayout.PropertyField(floatValue);
        EditorGUILayout.Separator(); //空行
        EditorGUILayout.PropertyField(colorValue);
        EditorGUILayout.Space(); //空行
        GUI.enabled = false;
        EditorGUILayout.PropertyField(curveValue);
        GUI.enabled = true;
        GUILayout.Space(20f);
        EditorGUILayout.PropertyField(refrenceValue);

        MoEditorHelper.DrawGuiDivider();

        //BeginHorizontal 表示在一行里面
        Rect rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(100));
        {
            GUI.Box(rect, "");
            EditorGUILayout.PropertyField(boolValue);
            EditorGUILayout.PropertyField(floatValue);
            //EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(colorValue);
            EditorGUILayout.PropertyField(curveValue);
            colorValue.colorValue = EditorGUI.ColorField(new Rect(rect.x + 20, rect.y + 20, 100, 20), colorValue.colorValue);
            mo.colorValue = EditorGUI.ColorField(new Rect(rect.x + 20, rect.y+40, 100, 20), mo.colorValue);
        }
        EditorGUILayout.EndHorizontal();

        mo.boolValue = EditorGUILayout.Toggle("Bool Value", mo.boolValue);

        GameObject go = EditorGUILayout.ObjectField("goRefrence", mo.goRefrence, typeof(GameObject)) as GameObject;
        mo.goRefrence = go;
        if (go == null)
        {
            EditorGUILayout.HelpBox("Mo Warning.", MessageType.Warning, false);
        }

        //按钮
        if (GUILayout.Button("Display"))
        {
            EditorUtility.DisplayDialog("Look", "Yes", "OK");
        }

        if (GUILayout.Button("Select", redBtnStyle, GUILayout.Width(60)))
        {
            Selection.activeGameObject = mo.goRefrence;
        }

        MoEditorHelper.DrawGuiDivider();

        EditorGUILayout.LabelField("BeginVertical", title);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("First Vertical");
        mo.boolValue = EditorGUILayout.Toggle(mo.boolValue);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Second Vertical", title);
        EditorGUILayout.PropertyField(boolValue, new GUIContent(""));
        mo.intValue = EditorGUILayout.IntSlider("IntValue", mo.intValue, 0, 10);
        EditorGUILayout.EndVertical();

        MoEditorHelper.DrawGuiDivider();

        EditorGUILayout.PropertyField(lengthValue);
        EditorGUILayout.Slider(mo.length, 0, 30);

        EditorGUILayout.PropertyField(colorType);
        //mo.colorType = EditorGUILayout.Popup(mo.colorType, );

        serializedObject.ApplyModifiedProperties();
    }

    
}
