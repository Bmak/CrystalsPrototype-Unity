using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityEngineExtensions
{
    public static void Invoke(this MonoBehaviour monoBehavior, Action onFinish, float delay)
    {
        monoBehavior.StartCoroutine(DelayedActionHelper(onFinish, delay));
    }

    private static IEnumerator DelayedActionHelper(Action onFinish, float delay)
    {
        yield return new WaitForSeconds(delay);
        onFinish();
    }

	public static void SetChildrenActive(this GameObject rootObject, bool active)
	{
		foreach (Transform childTransform in rootObject.transform)
		{
			SetActiveRecursively(childTransform.gameObject, active);
		}
	}
	
	public static void SetActiveRecursively(this GameObject rootObject, bool active)
	{
		rootObject.SetActive(active);
		
		foreach (Transform childTransform in rootObject.transform)
		{
			SetActiveRecursively(childTransform.gameObject, active);
		}
	}

    /// <summary>
    /// Helper method which will clear the local position, rotation,
    /// and scale of a GameObject.
    /// </summary>
    public static void ClearLocalTransform(this GameObject rootObject)
    {
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.identity;
        rootObject.transform.localScale = Vector3.one;
    }

	/// <summary>
	/// Recursively walks the hierarchy of the GameObject, and assigns it to have the new layer value. Optionally,
	/// you can also pass in a dictionary to capture the original layer values, so that they can be restored later.
	/// You can also pass in a collection of layers to ignore.
	/// You can pass in a dictionary of alternative layers to be used instead of the default layer based on the rootObject's layer. 
	///   (If the rootObject's layer is found in the keys of the alternative layers dictionary, the corresponding value is used
	///   as the layer instead of the default layer)
	/// </summary>
	public static void SetLayerRecursively(this GameObject rootObject, int defaultLayer, IDictionary<GameObject, int> originalLayers = null, ICollection<int> ignoreLayers = null, Dictionary<int, int> alternativeLayers = null)
	{
		foreach (Transform child in rootObject.transform)
		{
            child.gameObject.SetLayerRecursively(defaultLayer, originalLayers, ignoreLayers, alternativeLayers);
		}

		if (originalLayers != null)
		{
			originalLayers[rootObject] = rootObject.layer;
		}

	    if (ignoreLayers == null || !ignoreLayers.Contains(rootObject.layer)) {
	        int targetLayer;
            // Check alternative layers for rootObject's layer and if found use alternative mapping for target layer,
            // otherwise use default layer
            if ((alternativeLayers == null) || !alternativeLayers.TryGetValue(rootObject.layer, out targetLayer)) {
                targetLayer = defaultLayer;
	        }

            rootObject.layer = targetLayer; 
	    }
	}

    public static GameObject FindRecursively(this GameObject go, string childName)
    {
        if (go.name == childName) {
            return go;
        }

        for (int i = 0; i < go.transform.childCount; i++) {
            GameObject result = FindRecursively(go.transform.GetChild(i).gameObject, childName);
            if (result != null) {
                return result;
            }
        }

        return null;
    }


	/// <summary>
	/// Helper function that iterates over even the disabled game objects to find components since Unity default funtion do not
	/// </summary>
	/// <returns>The component of generic type </returns>
	public static T GetComponentInChildrenForInactiveGO<T>(this GameObject go) where T : MonoBehaviour
	{
		T returnComponent = go.GetComponent<T>();

		if(returnComponent == null) {
			Transform transform = go.transform;

			for(int childIndex=0; childIndex <  transform.childCount; ++childIndex) {
				returnComponent = GetComponentInChildrenForInactiveGO<T>(transform.GetChild(childIndex).gameObject);
				if(returnComponent != null) {
					break;
				}
			}

		}
		return returnComponent;
	}
}