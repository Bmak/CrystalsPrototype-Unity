using UnityEngine;

public class TransformUtil
{
    /// <summary>
    /// Places child inside parent while preventing the default modification of the child's local transform properties
    /// </summary>
    /// <param name="parent">The transform to contain the child</param>
    /// <param name="child">The transform to be placed in the parent</param>
    public static void Reparent(Transform parent, Transform child)
    {
        var localPosition = child.localPosition;
        var localRotation = child.localRotation;
        var localScale = child.localScale;

        child.parent = parent;

        child.localPosition = localPosition;
        child.localRotation = localRotation;
        child.localScale = localScale;
    }

    /// <summary>
    /// Utility function that will search the specified GameObject for
    /// a mesh child and will scale it according to the dimensions
    /// of the mesh. This can be useful when trying to keep consistent
    /// sizing across variable shaped objects (e.g. spaceships).
    /// 
    /// VFX will also be artificially scaled - be sure to call
    /// Restore() on the returned VFXScaleRestorer when done with
    /// the asset.
    /// </summary>
    public static VFXScaleRestorer MeshVolumeScale(GameObject go, float scale, float sizeExponent = 1.0f)
    {
        MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>(includeInactive:true);

        // Need to initialize the bounds to the first meshRenderer we find instead of default Bounds constructor
        // or else the bounds will be incorrectly placed
        Bounds combinedBounds = meshRenderers[0].bounds;
        for (int i = 1; i < meshRenderers.Length; ++i) {
            combinedBounds.Encapsulate(meshRenderers[i].bounds);
        }

        go.transform.localScale = Vector3.one;
        float size = combinedBounds.extents.x + combinedBounds.extents.y + combinedBounds.extents.z;
        go.transform.localScale *= scale / Mathf.Pow(size, sizeExponent);

        return VFXUtils.ScaleVFX(go);
    }
}