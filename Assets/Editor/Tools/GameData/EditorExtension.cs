using UnityEngine;
using UnityEditor;

public class EditorExtension
{
    // Deep clone

    [MenuItem("OctoBox/Erase player prefs", false, 500)]
    static void DoErasePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.LogWarning("PlayerPrefs erased");
    }

    [MenuItem("Edit/Move Down #DOWN")]
    static void DoSortDown()
    {
        foreach (Transform transform in Selection.transforms)
        {
            int index = transform.GetSiblingIndex();
            transform.SetSiblingIndex(index + 1);
        }
    }

    [MenuItem("Edit/Move Up #UP")]
    static void DoSortUp()
    {
        foreach (Transform transform in Selection.transforms)
        {
            int index = transform.GetSiblingIndex();
            transform.SetSiblingIndex(index - 1);
        }
    }

}

