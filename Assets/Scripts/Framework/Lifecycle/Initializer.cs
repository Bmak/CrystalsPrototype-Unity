using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// This class handles parallel and chained initialization of the passed IInitializable instances.
/// Given an array of IInitializable instances, this class will fire successCallback after it has
/// invoked Initialize(initializedCallback) on each and the instances subsequently have each invoked 
/// the passed initializedCallback. failedCallback will be fired if an exception is encoutered while 
/// invoking Initialize() on any instance. 
/// 
/// Pass chained = true and the intitialization will be forced in serial order - the same order 
/// the instances were initially passed in.
/// </summary>
public class Initializer : ILoggable
{    
    private static readonly Type INITIALIZABLE_TYPE = typeof(IInitializable);

	private IInitializable[] _instances;
	private Action _successCallback;
    private Action<string> _failedCallback;
	private Action<IInitializable,int,int> _progressCallback;
	private bool _chained = false;
    private string _displayName;
	private int _instanceCount = 0;
	private int _instanceIdx = 0;

	private int _remainingCount {
		get { return _instanceCount - _instanceIdx; }
	}

	public Initializer( Type[] instanceTypes, Action successCallback = null, Action<string> failedCallback = null, Action<IInitializable,int,int> progressCallback = null, bool chained = false, string displayName = null )  {

        List<IInitializable> instances = new List<IInitializable>( instanceTypes.Length );
        foreach( Type instanceType in instanceTypes ) {
            if ( !INITIALIZABLE_TYPE.IsAssignableFrom( instanceType ) ) continue;
            try { 
                instances.Add( (IInitializable)Activator.CreateInstance( instanceType ) );
            } catch (Exception e) {
                this.LogError("Exception creating instance of type '" + instanceType.Name + "', aborting: " + e.ToString());
                
            }
        }

        if ( instances.Count <= 0 ) {
            this.LogWarning("No instances to initialize, continuing.");
            if ( successCallback != null ) successCallback();
            return;
        }
        
        Execute( instances.ToArray(), successCallback, failedCallback, progressCallback, chained, displayName );
    }

    /// <summary>
    /// Initialize the given instances, and call successCallback when all have completed initialization.
    /// </summary>
    /// <param name="instances">Array of IInitializable instances.</param>
    /// <param name="successCallback">Success callback fired after all instances have initialized.</param>
    /// <param name="failedCallback">Failed callback fired if an exception is encountered during initialiation of any instance.</param>
	/// <param name="progressCallback">progress callback fired after initialiation of any instance with instance, total count, current index.</param>
    /// <param name="chained">If set to <c>true</c>, initialization will be chained in the order given (chained forces serial behavior. default is parallel).</param>
	public Initializer( IInitializable[] instances, Action successCallback = null, Action<string> failedCallback = null, Action<IInitializable,int,int> progressCallback = null, bool chained = false, string displayName = null )  {        
        Execute( instances, successCallback, failedCallback, progressCallback, chained, displayName );
    }

	/// <summary>
	/// Initialize the given instances, and call successCallback when all have completed initialization.
	/// </summary>
	/// <param name="instances">Array of IInitializable instances.</param>
	/// <param name="successCallback">Success callback fired after all instances have initialized.</param>
	/// <param name="failedCallback">Failed callback fired if an exception is encountered during initialiation of any instance.</param>
	/// <param name="progressCallback">progress callback fired after initialiation of any instance with instance, total count, current index.</param>
	/// <param name="chained">If set to <c>true</c>, initialization will be chained in the order given (chained forces serial behavior. default is parallel).</param>
	private void Execute( IInitializable[] instances, Action successCallback = null, Action<string> failedCallback = null, Action<IInitializable,int,int> progressCallback = null, bool chained = false, string displayName = null) {

		_instances = instances;
		_successCallback = successCallback;
		_failedCallback = failedCallback;
		_progressCallback = progressCallback;
		_chained = chained;
        _displayName = displayName;

        if (_instances != null) _instanceCount = _instances.Length;

		this.LogTrace( "Start" + (!String.IsNullOrEmpty( _displayName )  ? " ( " + _displayName + " )" : "" ), LogCategory.INITIALIZATION );

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( GetType().Name+ ":" + _displayName );
		#endif	

		if ( _instanceCount == 0 ) {
			this.LogTrace("No instances to wait on, continuing.", LogCategory.INITIALIZATION);
            Success();
			return;
		}

		if ( Debug.isDebugBuild ) LogProgress();

		if ( _chained ) {
			Initialize( _instances[0] );
		} else {
			foreach ( IInitializable instance in _instances ) {
				if ( !Initialize( instance ) ) break;
			}
		}
	}

	/// <summary>
	/// Initialize the specified instance. All exceptions from the invocation of instance.Initialize() will be swallowed.
	/// </summary>
	/// <param name="instance">Instance to intialize.</param>
	private bool Initialize( IInitializable instance ) {

		string instanceName = instance.GetType().Name;
		try {
			this.LogTrace("Initializing " + instanceName, LogCategory.INITIALIZATION);
			#if METRICS_ENABLED && INCLUDE_DEV_METRICS
			Metrics.Start( "Initialize:" + instanceName );
			#endif			
			instance.Initialize( InstanceInitialized );
		} catch ( Exception e ) {

			#if METRICS_ENABLED && INCLUDE_DEV_METRICS
			Metrics.End( "Initialize:" + instanceName );
			#endif	

			// An exception was thrown, so we will never complete. Invoke failedCallback.
			this.LogError( "Exception initializing " + ( instance != null ? instance.ToString () : "null instance" ) + ": " + e.ToString() );

            Failure( e.ToString() );

			// Stop initialization of any future instances
			return false;
		}

		// Continue initialization
		return true;
	}

	/// <summary>
	/// Callback invoked internally by an instance when it has completed initialization. 
	/// </summary>
	/// <param name="instance">Instance that completed.</param>
	private void InstanceInitialized( IInitializable instance ) {

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( "Initialize:" + instance.GetType().Name );
		#endif	

		try {

			if ( _progressCallback != null ) {
				_progressCallback( _instances[_instanceIdx], _instanceCount, _instanceIdx);
			}

			++_instanceIdx;

			if ( Debug.isDebugBuild ) LogProgress();

			if ( _remainingCount <= 0 ) {
				Success();				
			} else if ( _chained ) {
				Initialize( _instances[_instanceIdx] );
			}
		
		// Swallow exception to prevent firing _failedCallback as a result of exception in InstanceInitialized/_successCallback
		} catch ( Exception e) { 
			this.LogError( "Exception in InstanceInitialized() at index " + _instanceIdx + ": " + e.ToString () );
		}
	}

    private void Failure( string reason ) {
		Completed();

        if (_failedCallback != null) {
            _failedCallback( reason );
        } else {
            this.LogError("No failedCallback to invoke, halt.");                                
        }
    }

    private void Success() {
		Completed();

        if ( _successCallback != null ) {
            _successCallback();
        } else {
            this.LogTrace("No successCallback callback to invoke, halt.");
        }
    }
    
    private void Completed() {
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name+ ":" + _displayName );
		#endif	
        this.LogTrace( "End" + (!String.IsNullOrEmpty( _displayName )  ? " ( " + _displayName + " )" : "" ), LogCategory.INITIALIZATION );
    }

	/// <summary>
	/// Convenience method for debugging.
	/// </summary>
	private void LogProgress() {
		this.LogTrace(_displayName + " waiting on " + _remainingCount + " initialization event(s) before completion.", LogCategory.INITIALIZATION );
	}

}