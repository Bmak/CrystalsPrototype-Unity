
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
#if UNITY_3_5
[CustomEditor(typeof(UILabelButton))]
#else
[CustomEditor(typeof(UILabelButton), true)]
#endif
public class UILabelButtonEditor : UIButtonEditor
{
    protected override void DrawProperties()
    {
        base.DrawProperties();

        NGUIEditorTools.DrawProperty("Label", serializedObject, "_label");
    }
}
