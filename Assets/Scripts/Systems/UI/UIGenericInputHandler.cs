using System;
using UnityEngine;

public class UIGenericInputHandler : IInitializable, ILifecycleAware, ILoggable
{
    public event OnPressDelegate OnPress;
    public delegate void OnPressDelegate(Vector2 pressLocation);

	public event OnUnPressDelegate OnUnPress;
	public delegate void OnUnPressDelegate(Vector2 pressLocation);

    public event OnSwipeEndDelegate OnSwipeEnd;
    public delegate void OnSwipeEndDelegate(Vector2 swipeDelta);

    private const float SWIPE_DISTANCE_TOLERANCE = 20f;

    public void Initialize(InstanceInitializedCallback initializedCallback = null) 
    {
        UICamera.onPress = HandleOnPress;

        this.LogTrace("Initialize()", LogCategory.INITIALIZATION);

        if (initializedCallback != null)
        {
            initializedCallback(this);
        }
    }

    private void HandleOnPress(GameObject go, bool pressed)
    {
        if (pressed)
        {
            if (OnPress != null)
            {
                OnPress(UICamera.currentTouch.pos);
            }
        }
        else 
        {
			if (OnUnPress != null) {
				OnUnPress(UICamera.currentTouch.pos);
			}
            Vector2 totalDelta = UICamera.currentTouch.totalDelta;

            if (totalDelta.magnitude >= SWIPE_DISTANCE_TOLERANCE)
            {
                if (OnSwipeEnd != null)
                {
                    OnSwipeEnd(totalDelta);
                }
            }
        }
    }    

    public void Reset() 
    {
        UICamera.onPress = null;
		OnPress = null;
		OnSwipeEnd = null;
    }
}
