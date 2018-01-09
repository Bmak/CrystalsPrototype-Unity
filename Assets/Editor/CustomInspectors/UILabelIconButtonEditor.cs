using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
#if UNITY_3_5
[CustomEditor(typeof(UILabelIconButton))]
#else
[CustomEditor(typeof(UILabelIconButton), true)]
#endif
public class UILabelIconButtonEditor : UILabelButtonEditor
{
    protected override void DrawProperties()
    {
        base.DrawProperties();

        NGUIEditorTools.DrawProperty("Amount", serializedObject, "_amount");
        NGUIEditorTools.DrawProperty("Icon", serializedObject, "_icon");
    }
}
