using UnityEngine;

// Based on: http://wiki.unity3d.com/index.php/FindMissingScripts
public class FindMissingScripts : ILoggable
{
    private static int _goCount = 0;
    private static int _componentsCount = 0;
    private static int _missingCount = 0;

    // Used to access logger in a static context. Otherwise unused.
    private static readonly FindMissingScripts _instance = new FindMissingScripts();

    public static void FindInSelected(GameObject selectedGO)
    {
        GameObject[] _gameObjectArray = new GameObject[1];
        _gameObjectArray[0] = selectedGO;
        FindInSelected(_gameObjectArray);
    }

    public static void FindInSelected(GameObject[] selectedGOs)
    {
        _goCount = 0;
        _componentsCount = 0;
        _missingCount = 0;
        foreach (GameObject g in selectedGOs) {
            FindInGO(g);
        }
        string logString = string.Format("Searched {0} GameObjects, {1} components, found {2} missing", _goCount,
                                         _componentsCount, _missingCount);
        if (_missingCount == 0)
            Log(logString, false);
        else {
            Log(logString, true);
        }
    }

    private static void FindInGO(GameObject go)
    {
        _goCount++;
        Component[] components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++) {
            _componentsCount++;
            if (components[i] == null) {
                _missingCount++;
                string name = go.name;
                Transform transform = go.transform;
                while (transform.parent != null) {
                    name = transform.parent.name + "/" + name;
                    transform = transform.parent;
                }
                Log(name + " has an empty script attached in position: " + i, true);
            }
        }

        // Now recurse through each child GO (if there are any):
        foreach (Transform childT in go.transform) {
            FindInGO(childT.gameObject);
        }
    }

    private static void Log(string logString, bool error)
    {
        if (Application.isEditor) {
            if (error)
                Debug.LogError(logString);
            else
                Debug.Log(logString);
        } else {
            if (error)
                _instance.LogError(logString);
            else
                _instance.Log(logString);
        }
    }
}
