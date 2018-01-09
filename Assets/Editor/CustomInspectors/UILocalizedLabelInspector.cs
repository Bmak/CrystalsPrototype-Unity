using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UILocalizedLabel))]
public class UILocalizedLabelInspector : UILabelInspector
{
    protected override bool ShouldDrawProperties()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(76f));
        GUILayout.Space(3f);
        GUILayout.Label("Loc Key");
        GUILayout.EndVertical();
        GUILayout.BeginVertical();

        SerializedProperty sp = serializedObject.FindProperty("_localizationKey");

        GUIStyle style = new GUIStyle(EditorStyles.textField);
        float height = style.CalcHeight(new GUIContent(sp.stringValue), Screen.width - 100f);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(height));

        string text = EditorGUI.TextArea(rect, sp.stringValue, style);
        if (GUI.changed) sp.stringValue = text;

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        sp = NGUIEditorTools.DrawProperty("Auto Localize", serializedObject, "_autoLocalizeOnStart", GUILayout.Width(100f));

        return base.ShouldDrawProperties();
    }
}
