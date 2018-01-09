using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Memory manager - this class must be a MonoBehaviour attached to a GameObject so that  
/// UnitySendMessage can invoke DidReceiveMemoryWarning from native code.
/// </summary>
public class MemoryManager : MonoBehaviour, ILifecycleAware, ILoggable
{
	private const float MEMORY_WARNING_EXPIRATION_THRESHOLD_SECONDS = 30f; 
	private const float MEMORY_WARNING_COOLDOWN_PERIOD_SECONDS = 2f; 

    private event EventHandler<LowMemoryWarningEventArgs> _lowMemoryWarningEvent;
	public event EventHandler<LowMemoryWarningEventArgs> LowMemoryWarningEvent {
        add {
            _lowMemoryWarningEvent -= value; // Event handler subscription should be idempotent
            _lowMemoryWarningEvent += value;
        }

        remove {
            _lowMemoryWarningEvent -= value;
        }
    }

	private float _lastReceivedLowMemoryWarningTime = 0f;

    public void Reset() {
        _lowMemoryWarningEvent = null;
        this.DestroyAll();
    }

    public bool IsDeviceLowOnMemory() {
		return _lastReceivedLowMemoryWarningTime > 0f && (Time.time - _lastReceivedLowMemoryWarningTime) < MEMORY_WARNING_EXPIRATION_THRESHOLD_SECONDS;
	}

	/// <summary>
	/// This method is called when the device is low on memory. 
	/// Invoked via UnitySendMessage in AppController.mm when applicationDidReceiveMemoryWarning is called by iOS.
	/// </summary>
	/// <param name='message'>
	/// Message.
	/// </param>
	public void DidReceiveMemoryWarning( string message ) {
		this.LogInfo("Received low memory warning");
		if(_lastReceivedLowMemoryWarningTime == 0f || Time.time - _lastReceivedLowMemoryWarningTime >= MEMORY_WARNING_COOLDOWN_PERIOD_SECONDS)
		{
			OnLowMemoryWarning( new LowMemoryWarningEventArgs( message ) );
		}

		#if METRICS_ENABLED
		Metrics.Count( "LowMemoryWarning" );
		#endif

		_lastReceivedLowMemoryWarningTime = Time.time;
	}
	
	public void OnLowMemoryWarning(LowMemoryWarningEventArgs args) {		
		//If any listeners are attached to this event, fire the event.
		if (_lowMemoryWarningEvent != null) _lowMemoryWarningEvent(this, args);
	}
}

/// <summary>
/// Low memory warning event arguments.
/// </summary>
public class LowMemoryWarningEventArgs : EventArgs  {		
	public readonly string message;
	public LowMemoryWarningEventArgs( string message )  {
    	this.message = message;
	}
}