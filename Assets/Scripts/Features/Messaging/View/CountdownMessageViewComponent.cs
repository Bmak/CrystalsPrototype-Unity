using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A component that counts down a time for a message view
/// </summary>
public class CountdownMessageViewComponent : ILoggable
{
    private NguiView _associatedView;
    private IEnumerator _updateCoroutine;

    public void Initialize(NguiView associatedView)
    {
        _associatedView = associatedView;
    }

    public void Shutdown()
    {
        if (_updateCoroutine != null) {
	        _associatedView.StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
	    }
        _associatedView = null;
    }

    /// <summary>
    /// Kicks off a coroutine which will call the passed in function.
    /// The passed in function should return a bool for whether the coroutine should continue firing
    /// </summary>
    public void StartUpdateFunction(Func<ICountdownMessageView, bool> updateAction, Action timerCompleteAction = null)
    {
        _updateCoroutine = UpdateCoroutine(updateAction, timerCompleteAction);
        _associatedView.StartCoroutine(_updateCoroutine);
    }

    public IEnumerator UpdateCoroutine(Func<ICountdownMessageView, bool> updateAction, Action timerCompleteAction = null)
    {
        while (true) {
            bool keepUpdating = updateAction(_associatedView as ICountdownMessageView);
            if (!keepUpdating) {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        if (timerCompleteAction != null) {
            timerCompleteAction();
        }

        _associatedView.Release();
    }
}
