using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Collection of utility methods for use with NGUI.
/// </summary>
public class NguiUtils
{
    /// <summary>
    /// Render queue value for gameobjects (typically VFX) who
    /// want to render over on top of the NGUI UI layer.
    /// </summary>
    public const int OVERLAY_VFX_RENDER_QUEUE = 4000;

    public static readonly Color DISABLED_BUTTON_COLOR = new Color(1.0f, 1.0f, 1.0f, 0.5f);

    /// <summary>
    /// Sets the position of the specified NGUI object
    /// to the specified screen position. This function
    /// will account for NGUI's fixed height scaling.
    /// </summary>
    public static void SetScreenToOrthoWorldPosition(Vector3 screenPosition, Camera nguiCamera, Transform target)
    {
        target.position = nguiCamera.ScreenToWorldPoint(screenPosition);
        Vector3 localPosition = target.localPosition;
        localPosition.z = 0;
        target.localPosition = localPosition;
    }

    /// <summary>
    /// Similar to NGUITools.AdjustDepth but only for widgets, and does not check for parent panel match, and includes disabled components
    /// </summary>
    public static void AdjustWidgetDepthBlindly(GameObject go, int depthAdjust)
    {
        UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>(includeInactive:true);
        for (int i = 0; i < widgets.Length; ++i) {
            widgets[i].depth += depthAdjust;
        }
    }

    /// <summary>
    /// Works like EventDelegate::Execute, but allows you to specify a single parameter (a C# object reference) to pass back
    /// NOTE: This requires that when the EventDelegate is setup that the callback function be specified by method name and *not* passed a Callback object
    /// </summary>
    static public void ExecuteEventDelegateListWithParam(List<EventDelegate> list, object paramObject)
    {
        if (list != null) {
            for (int i = 0; i < list.Count; ++i) {
                EventDelegate del = list[i];

                if (del != null) {
                        // Set params
                        if (del.parameters.Length != 1) {
                            Debug.LogError("EventDelegate callback handler \"" + del.methodName + "\" has incorrect number of parameters.  It should have exactly 1 parameter.");
                        }
                        del.parameters[0].value = paramObject;
                }
            }

            EventDelegate.Execute(list);
        }
    }

    /// <summary>
    /// Helper method which recursively searches for particle systems
    /// on the specified gameobject and sets them to the constant
    /// overlay VFX render queue value.
    /// </summary>
    public static void SetOverlayVFXRenderQueue(GameObject gameObject)
    {
        foreach (ParticleSystem particleSystem in gameObject.GetComponentsInChildren<ParticleSystem>(true)) {
            particleSystem.GetComponent<Renderer>().material.renderQueue = OVERLAY_VFX_RENDER_QUEUE;
        }
    }

    /// <summary>
    /// Returns true if the screen coordinate is inside the passed in widget's bounds
    /// </summary>
    public static bool ScreenCoordInsideWidget(Vector2 pos, UIWidget widget)
    {
        Vector2 localPos = NGUIMath.ScreenToPixels(pos, widget.transform);

        float x0 = -widget.pivotOffset.x * widget.width;
        float y0 = -widget.pivotOffset.y * widget.height;
        float x1 = x0 + widget.width;
        float y1 = y0 + widget.height;

        bool isInside = x0 < localPos.x && localPos.x < x1 && y0 < localPos.y && localPos.y < y1;

        return isInside;
    }

    /// <summary>
    /// Attempts to calculate a screen space rect for the passed in monobehavior. If the MB is a widget, those bound are used, otherwise
    /// this method will look for a widget on the same game object as the passed in MB.
    /// Returns null if no widget found.
    /// </summary>
    public static Rect? GetScreenRectForWidget(MonoBehaviour monoBehavior, Camera nguiCamera)
    {
        UIWidget widget = monoBehavior as UIWidget;
        if (widget == null) {
            // try to get the widget off of the associated game object
            widget = monoBehavior.GetComponent<UIWidget>();
        }
        // fill out the bounds rect if this is a widget
        if (widget != null) {
            Vector3 screenTopLeft = nguiCamera.WorldToScreenPoint(widget.worldCorners[1]);
            Vector3 screenBottomRight = nguiCamera.WorldToScreenPoint(widget.worldCorners[3]);
            Rect screenBounds = new Rect(
                screenTopLeft.x,
                screenTopLeft.y,
                screenBottomRight.x - screenTopLeft.x,
                screenBottomRight.y - screenTopLeft.y);
            return screenBounds;
        }
        return null;
    }

    /// <summary>
    /// Same as above, but uses the first widget found on this gameobject (does not search children)
    /// </summary>
    public static Rect? GetScreenRectForGameObject(GameObject gameObject, Camera nguiCamera)
    {
        UIWidget widget = gameObject.GetComponent<UIWidget>();
        if (widget != null) {
            Vector3 screenTopLeft = nguiCamera.WorldToScreenPoint(widget.worldCorners[1]);
            Vector3 screenBottomRight = nguiCamera.WorldToScreenPoint(widget.worldCorners[3]);
            Rect screenBounds = new Rect(
                screenTopLeft.x,
                screenTopLeft.y,
                screenBottomRight.x - screenTopLeft.x,
                screenBottomRight.y - screenTopLeft.y);
            return screenBounds;
        }
        return null;
    }

    /// <summary>
    /// Same as above but gathers aggregate bounds by walking the scene heirarchy from "root" down
    /// </summary>
    public static Rect? GetScreenRectForHierarchy(Transform root, Camera nguiCamera)
    {
        Bounds totalBounds = NGUIMath.CalculateAbsoluteWidgetBounds(root);
        if (totalBounds.min == totalBounds.max) {
            return null;
        }

        Vector3 screenBottomLeft = nguiCamera.WorldToScreenPoint(totalBounds.min);
        Vector3 screenTopRight = nguiCamera.WorldToScreenPoint(totalBounds.max);
        Rect screenBounds = new Rect(
            screenBottomLeft.x,
            screenTopRight.y,
            screenTopRight.x - screenBottomLeft.x,
            screenBottomLeft.y - screenTopRight.y);
        return screenBounds;
    }

    public static void SetButtonEnabled(UIButton button, bool enabled)
    {
        button.defaultColor = enabled ? Color.white : DISABLED_BUTTON_COLOR;
        button.pressed = enabled ? Color.white : DISABLED_BUTTON_COLOR;
        button.hover = enabled ? Color.white : DISABLED_BUTTON_COLOR;
        button.disabledColor = enabled ? Color.white : DISABLED_BUTTON_COLOR;
    }
}
