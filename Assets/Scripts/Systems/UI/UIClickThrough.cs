using UnityEngine;

/// <summary>
/// This component handles click events coming from some other ui element, then raycasts through the specified camera, sending "OnClick" to any collisions
/// Intended use is to handle world clicks via a ui collider without resorting to global fall back input
/// </summary>
public class UIClickThrough : MonoBehaviour
{
    [SerializeField] private Camera _sceneCamera;
    [SerializeField] private LayerMask _masksToHit;

    public void SetCamera(Camera camera)
    {
        _sceneCamera = camera;
    }

    // NGUI on click message handler
    private void OnClick()
    {
        if (_sceneCamera == null) { return; }

        Ray ray = _sceneCamera.ScreenPointToRay(UICamera.currentTouch.pos);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, _masksToHit)) {
            GameObject foundObject = hitInfo.collider.gameObject;
            if (foundObject == this.gameObject) {
                // Safety check to prevent recursion...
                return;
            }
            foundObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver); // mimic UICamera's behavior
        }
    }

}
