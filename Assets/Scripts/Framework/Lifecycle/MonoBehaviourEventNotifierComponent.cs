using UnityEngine;
using System;

public class MonoBehaviourEventNotifierComponent : MonoBehaviour
{
    public event OnApplicationPauseDelegate OnApplicationPaused;
    public delegate void OnApplicationPauseDelegate (MonoBehaviourEventNotifierComponent sender, bool paused);

    public event OnApplicationFocusDelegate OnApplicationFocused;
    public delegate void OnApplicationFocusDelegate (MonoBehaviourEventNotifierComponent sender, bool focused);

    public event OnUpdateDelegate OnUpdated;
    public delegate void OnUpdateDelegate (MonoBehaviourEventNotifierComponent sender);

	private void OnApplicationPause (bool paused)
	{
		if (OnApplicationPaused != null) {
			OnApplicationPaused (this, paused);
		}
	}

	private void OnApplicationFocus (bool focused)
	{
		if (OnApplicationFocused != null) {
			OnApplicationFocused (this, focused);
		}
	}

	protected void ClearEventDelegates ()
	{
		OnApplicationPaused = null;
		OnApplicationFocused = null;
		OnUpdated = null;
	}

	private void Update ()
	{
		if (OnUpdated != null) {
			OnUpdated (this);
		}
	}
}
