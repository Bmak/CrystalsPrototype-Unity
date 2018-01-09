
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for managine the "layering" of views so that they draw in the order intended
/// </summary>
public class ViewController : ILoggable
{
    // this list is insert-sorted based on intended depth, views meant to be closer to the camera should be at the end of the list
    private List<NguiView> _views = new List<NguiView>();

    public void _registerView(NguiView view)
    {
        // Iterate until we find a view whose depth is higher than the new view and insert
        int insertAtIndex = 0;
        for (; insertAtIndex < _views.Count; ++insertAtIndex) {
            if (view.InitialDepth < _views[insertAtIndex].InitialDepth) {
                break;
            }
        }
        _views.Insert(insertAtIndex, view);
    }

    public void _unregisterView(NguiView view)
    {
        // no need to update view depths on removal
        _views.Remove(view);
    }

    // Moves a view to reside in the list immediately after the other
    public void MoveViewOnTopOfOther(NguiView viewToMove, NguiView other)
    {
        if (viewToMove == null || other == null) {
            this.LogError(string.Format("MoveViewOnTopOfOther was passed a null argument - viewToMove null? {0}, other null? {1}", viewToMove == null, other == null ));
            return;
        }

        for (int i = 0; i < _views.Count; ++i) {
            if (_views[i] == other) {
                _views.Remove(viewToMove);
				_views.Insert(Math.Min(i + 1, _views.Count), viewToMove);
                UpdateViewDepths();
                return;
            }
        }
    }

    // Updates depth values in _views list order
    public void UpdateViewDepths()
    {
        int currentDepth = 0;
        float currentZ = 0.0f;
        for (int i = 0; i < _views.Count; ++i) {
            NguiView view = _views[i];
            currentDepth += view._setDepth(currentDepth);
            view._setPosition(new Vector3( 0.0f, 0.0f, -currentZ ));
            currentZ += view.ZSeparation;
        }
    }

    public NguiView GetFocusView(HashSet<Type> excludeViews = null)
    {
        for (int i = _views.Count - 1; i >= 0; --i) {
            NguiView view = _views[i];

            if (excludeViews != null && excludeViews.Contains(view.GetType())) {
                continue;
            }

			if (view.CanGainFocus && view.ViewActive) {
                return view;
            }
        }
        return null;
    }

}
