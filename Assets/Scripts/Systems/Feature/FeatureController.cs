using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Feature controller.Transition login for controllers.
/// </summary>
public abstract class FeatureController : IFeatureController, ILoggable
{
	[Inject]
	protected StateController _stateController;

	[Inject]
	protected CoroutineCreator _coroutineCreator;

	[Inject]
	protected UISystem _uiSystem;

	[Inject]
	protected AudioSystem _audioSystem;

	[Inject]
	private NetworkSystem _networkSystem;

    [Inject]
    protected Config _config;

    [Inject]
    protected LocalPrefs _localPrefs;

	[Inject]
	protected ViewProvider _viewProvider;

	[Inject]
	protected NguiTransitionController _nguiTransitionController;

	private bool _wasShutdown = false;

#pragma warning disable 414 // Value is never used
    private bool _initialized = false;
#pragma warning restore 414

    #if METRICS_ENABLED
    protected virtual bool MetricsEnabled {
        get { return true; }
    }

    private string _metricsKey;
    protected virtual string MetricsKey {
        get { 
            return !String.IsNullOrEmpty( _metricsKey ) ? 
                _metricsKey : 
                ( _metricsKey = "Feature:" + GetType().Name.Replace("Controller", "") );
        }       
    }
    #endif

	protected bool WasShutdown { get { return _wasShutdown; } }

	public virtual void Initialize()
	{       
        _initialized = true;
        _wasShutdown = false;
		#if METRICS_ENABLED && ( INCLUDE_PERFORMANCE_METRICS || INCLUDE_DEV_METRICS )
        if ( MetricsEnabled ) {            
            Metrics.StartFPS( MetricsKey );
            Metrics.StartMem( MetricsKey );
        }
        #endif          
	}

	public virtual void Shutdown()
	{
		_wasShutdown = true;

		#if METRICS_ENABLED && ( INCLUDE_PERFORMANCE_METRICS || INCLUDE_DEV_METRICS )
        // Only end metrics if they were started
        // We do not seem to consistently call these base class methods (Initialize/Shutdown)
        if ( MetricsEnabled && _initialized ) { 
            Metrics.EndFPS( MetricsKey );
            Metrics.EndMem( MetricsKey );
        }
        #endif    
	}

	#if METRICS_ENABLED && ( INCLUDE_PERFORMANCE_METRICS || INCLUDE_DEV_METRICS )
    public virtual void MetricsAddMeta( params string[] meta )
    {
        // Only add metadata to events if they have been started
        if ( MetricsEnabled && _initialized ) { 
			Metrics.AddFPSMeta( MetricsKey, meta );
			Metrics.AddMemMeta( MetricsKey, meta );
        }
    }
    #endif   
	
	protected void TransitionViewHelper(Action<Action> viewEntryHandler, 
	                                    TransitionType transitionType = TransitionType.FastTransition, Action callback = null)
    {
        _nguiTransitionController.ShowTransition(transitionType);

        viewEntryHandler( () => 
            { 
            _nguiTransitionController.HideAllTransitions(callback);
            } ); 
    }

	public void EnterFeature<TEnterState>(object transitionInfo = null,
										  TransitionType transitionType = TransitionType.FastTransition) where TEnterState : State
	{
		_nguiTransitionController.ShowTransition(transitionType);

		// Delay one frame to allow transition screen to show, just in case this EnterState will block immediately
		_coroutineCreator.DelayActionOneFrame(() => {
			_stateController.EnterState(typeof(TEnterState),transitionInfo);
		});
	}

	public void EnterFeature(Type enterStateType,
	                         object transitionInfo = null,
	                         TransitionType transitionType = TransitionType.FastTransition)
	{
		_nguiTransitionController.ShowTransition(transitionType);
			
		// Delay one frame to allow transition screen to show, just in case this EnterState will block immediately
		_coroutineCreator.DelayActionOneFrame(() => {
			_stateController.EnterState(enterStateType, transitionInfo);
		});
	}

	// Exit this feature and navigate to top waypoint
	public void FeatureInitializeFinish(Action onTransitionHidden = null)
	{
	    _stateController.FeatureInitializeFinish();

        _nguiTransitionController.HideAllTransitions(onTransitionHidden);
	}

	protected virtual void ProceedToPreviousGameState()
	{
		EnterFeature<HomeBaseState>();  
	}

	protected virtual void OnBackButtonClicked()
	{
		EnterFeature<HomeBaseState>();
	}
}
