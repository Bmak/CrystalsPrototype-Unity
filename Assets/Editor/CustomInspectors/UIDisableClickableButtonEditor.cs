
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(UIDisableClickableButton), true)]
public class UIDisableClickableButtonEditor : UIButtonEditor
{
    protected override void DrawProperties()
    {
		NGUIEditorTools.DrawProperty("Disable color tweening", serializedObject, "disableColors");
        base.DrawProperties();
    }
}
