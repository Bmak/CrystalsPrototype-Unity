using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Surrogate MonoBehaviour - allows StartCoroutine() calls from regular,
/// old-fashioned, boring POCOs.
/// </summary>
public class CoroutineCreator : MonoBehaviour, ILifecycleAware, ILoggable {

	private bool _resetInProgress;

	private static CoroutineCreator _instance;
	public static CoroutineCreator Instance
	{
        get { return _instance ?? ( _instance = Injector.Instance.Get<CoroutineCreator>() ); }
	}

	public void SetResetInProgress (bool value)
	{
		_resetInProgress = value;
	}

    public void Reset() {
        _instance = null;
        StopAllCoroutines();
        CancelInvoke();
        this.DestroyAll();
    }
	
    /// <summary>
    /// Utility method which performs the specified action after
    /// the specified delay.
    /// </summary>
    /// <param name="onFinish">the action to be delayed</param>
    /// <param name="delay">the amount to delay the action</param>
    public Coroutine DelayAction( Action onFinish, float delay ) {
        return StartCoroutine( DelayedActionHelper( onFinish, delay ) );
    }

    private IEnumerator DelayedActionHelper( Action onFinish, float delay ) {
        yield return new WaitForSeconds( delay );
		if (!_resetInProgress) {
			onFinish();
		}
    }

    /// <summary>
    /// Utility method which performs the specified action after
    /// one frame.
    /// </summary>
    /// <param name="onFinish">the action to be delayed</param>
    public void DelayActionOneFrame( Action onFinish )
    {
        StartCoroutine( DelayedActionHelper(onFinish) );
    }

    private IEnumerator DelayedActionHelper(Action onFinish) {
        yield return null;
		if (!_resetInProgress) {
        	onFinish();
		}
    }

    /// <summary>
    /// Utility method which performs the specified action after
    /// at the end of the current frame.
    /// </summary>
    /// <param name="onFinish">the action to be delayed</param>
    public void DelayActionEndOfFrame( Action onFinish )
    {
        StartCoroutine( DelayedEndOfFrameActionHelper( onFinish ) );
    }

    private IEnumerator DelayedEndOfFrameActionHelper( Action onFinish ) {
        yield return (new WaitForEndOfFrame());
		if (!_resetInProgress) {
			onFinish();
		}
    }

}
