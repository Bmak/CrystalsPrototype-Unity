using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public delegate void SC_Callback( State state );

public enum ReturnToStateEnum
{
    HOMEBASE_STATE,
}

public class StateController : MonoBehaviour, ILifecycleAware, ILoggable
{
    private const int PAST_STATES_CAPACITY = 11;

    private List<State> _states;
    private List<String> _pastStateHistory;
	private List<State> _pastMEStateStack;

	private State _existingExclusiveState = null;

    private uint _stateEnterCounter = 0;

    [Inject]
    private IInjector _injector;

    [Inject]
    private LifecycleController _lifecycleController;

	[Inject]
	private CoroutineCreator _coroutineCreator;

    [Inject]
    private Client _client;

    [Inject]
    private Config _config;
/*
    [Inject]
    private NavigationDispatch _navDispatch;
*/
    //
    // Initialize

    private static StateController _instance;
    public static StateController Instance
    {
        get { return _instance ?? ( _instance = Injector.Instance.Get<StateController>() ); }
    }

    private void Initialize()
    {
        _states = new List<State>();
        _pastStateHistory = new List<String>();
		_pastMEStateStack = new List<State>();
		
		_existingExclusiveState = null;

        // Register for OnReset event that will be fired globally prior to Reset()
        _lifecycleController.SubscribeOnReset( OnLifecycleReset );

        this.Log("*".Repeat(50) + " Initialize " + "*".Repeat(50));
    }

    [PostConstruct]
    public void PostConstruct()
    {
        Initialize();
    }



    //
    // ILifecycleAware

    // Reset StateController to allow full reinitialization of the game
    public void Reset()
    {
        // Clear static instance reference to force re-instantiation
        _instance = null;
        _lifecycleController.UnsubscribeOnReset( OnLifecycleReset );
        this.DestroyAll();
    }

    // Prepare for full reset of the game
    public void OnLifecycleReset()
    {
        ExitAndClearAllStates();
    }

    private void ExitAndClearAllStates()
    {
        // Exit current state(s)
        while (_states.Count > 0) {
            _states[0].SC_Exit (false, null);
            _states.RemoveAt(0);
        }
    }

    private void Update()
    {
        // Fwd tick to all active States
		if (_states != null) {
			foreach(State state in _states) {
				state.SC_Update();
			}
        }
    }

    //
    // Active States / Transitions
	
	public TState GetState<TState>() where TState : State
	{
		Type tState = typeof(TState);
		foreach(State state in _states) {
			if (state.GetType() == tState) {
				return (TState)state;
			}
		}
		return null;
	}
	public State GetState(string stateClassName)
	{
		foreach(State state in _states) {
			if (state.GetType().Name == stateClassName) {
				return state;
			}
		}
		return null;
	}
	
	public State GetMutuallyExclusiveState() 
	{ 
		foreach(State state in _states) {
			if (state.IsMutuallyExclusive()) {
				return state;
			}
		}
		return null;
	}

	
	public bool IsStateActive(Type viewType)
	{
		return (GetState(viewType.ToString()) != null);
	}
	
    private State CreateState(Type type)
    {

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( GetType().Name + ":CreateState:" + type.Name );
		#endif	

        State state = CreateStateInstance(type);

        if ( null == state ) {
			#if METRICS_ENABLED && INCLUDE_DEV_METRICS
			Metrics.End( GetType().Name + ":CreateState:" + type.Name );
			#endif	
			return null;
		}

        // Initialize State ahead of entering/activating state
        state.SC_Init();

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name + ":CreateState:" + type.Name );
		#endif

        return state;
    }

    private TState CreateState<TState>() where TState : State, new()
    {
        return CreateState(typeof(TState)) as TState;
    }

    private State CreateStateInstance(Type type)
    {
        State state = Activator.CreateInstance(type) as State;
        _injector.Inject(state);
        return state;
    }
	
    //
    // Create a new state instance that has been properly dependency injected
    //
    private TState CreateStateInstance<TState>( ) where TState : State, new()
    {
        return CreateStateInstance(typeof(TState)) as TState;
    }    
    
	// Previous mutually exclusive state stack for dynamic progressions
	
	public bool PushMEState()
	{
		// Move the current mutually exclusive state to the top of the state stack
		if (_existingExclusiveState != null) {
			State topState = ((_pastMEStateStack.Count > 0) ? _pastMEStateStack[_pastMEStateStack.Count-1] : null);
			if ( topState == null || _existingExclusiveState.GetType() != topState.GetType() ) { // Top state in stack isn't the same type as one potentially being added?
				_pastMEStateStack.Add(_existingExclusiveState);
				return true;
			}
		}
		return false;
	}
	public State PopMEState()
	{
		// Restore the top mutually exclusive state
		if (_pastMEStateStack.Count > 0) {
			State state = _pastMEStateStack[_pastMEStateStack.Count-1];
			_pastMEStateStack.RemoveAt(_pastMEStateStack.Count-1);
			return state;
		}
		return null;
	}
    [Obsolete("State stacking behavior has not been tested and should currently not be utilized.")]
	public bool EnterPrevMEState()
	{
		// Restore the top mutually exclusive state
		State state = PopMEState();
		if (state != null) {
            _states.Add(state);
			return EnterState(state, null);
		}
		return false;
	}
	
	// State Enter/Exit

    public State EnterState(System.Type type, object transitionInfo = null)
    {
        // Check for and reuse state if already active
        State enterState = GetState(type.ToString());
        bool stateAlreadyActive = (enterState != null);

        // Need to create a new one and possibly exit another state if not reusing existing instance
        if (!stateAlreadyActive)
        {
            // Create a new instance of the State
            enterState = CreateState(type);
            if (null == enterState)
            {
                return null;
            }
        }
        if (!EnterState(enterState, transitionInfo))
        {
            // Failed to start so no cleanup
            return null;
        }
        return enterState;
        // Note: BusyOverlay will be hidden on callback
    }

    public TEnterState EnterState<TEnterState>( object transitionInfo = null ) where TEnterState : State, new()
    {
        return EnterState(typeof(TEnterState), transitionInfo) as TEnterState;
	}

	public bool EnterState( State enterState, object transitionInfo = null )
    {
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( "State:" + enterState.Name );
		#endif
		
		_states.Add(enterState);

		// Exit existing state if needed and/or Enter new state
		bool stateAlreadyActive = enterState.IsActive;
		if ( stateAlreadyActive ) {
			// Exit the state before re-entering it
			ExitState( enterState, true, delegate (State exitedState) { 
				    ContinueEnteringState( enterState, transitionInfo ); } );
		} else {
			// Exit current mutually exclusive state
			if (enterState.IsMutuallyExclusive() && _existingExclusiveState != null) {
			    ExitState( _existingExclusiveState, true, delegate (State exitedState) { 
				    ContinueEnteringState( enterState, transitionInfo, () => {
				        /*_navDispatch.StateTransitioned(exitedState, enterState);*/
				    } );
                } );
			} else {
				ContinueEnteringState( enterState, transitionInfo );
			}
		}
		
		return true;
    }
	private void ContinueEnteringState( State enterState, object transitionInfo, Action preEnterStateCallback = null )
	{
		//TODO This is a temporary fix for making sure that the Exiting of a current state and entering of the new state do not occur in the same frame.
		//Cleaner solution would be to make the EnterState function a co routine and yielding a frame after Exiting and before calling the enter function of the next state.
		_coroutineCreator.DelayActionOneFrame( () =>
      	{
			#if METRICS_ENABLED && INCLUDE_DEV_METRICS
			Metrics.Start( GetType().Name + ":Enter:" + enterState.Name );
			#endif	

            int cleanupFrequency = _config.GetUnloadUnusedAssetsEachStateFrequency();
            if (cleanupFrequency > 0 && ++_stateEnterCounter >= cleanupFrequency) {
                Resources.UnloadUnusedAssets();
                GC.Collect();
                _stateEnterCounter = 0;
            }

            // Set TimeEntered before SC_Enter because State implementations can perform initialization functions before
            // the base class SC_Enter gets called.  So we want this called before all that happens.
		    enterState.TimeEntered = Time.realtimeSinceStartup;

            // Callback used for actions that need to happen before SC_Enter, but after DelayActionOneFrame and setting of TimeEntered
            if (preEnterStateCallback != null) {
		        preEnterStateCallback();
		    }

			// Initialize & in the process, kick off the transition sequence
	        if (!enterState.SC_Enter(transitionInfo, OnEnterComplete))
	        {
				#if METRICS_ENABLED && INCLUDE_DEV_METRICS
				Metrics.End( GetType().Name + ":Enter:" + enterState.Name );
				#endif
				// State failed to start
				ExitState( enterState, false );
			}
		});
	}
	public void OnEnterComplete( State state )
	{
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name+ ":Enter:" + state.Name );
		#endif

		// Track mutually exclusive state
		if (state.IsMutuallyExclusive()) {
			_existingExclusiveState = state;
		}
			
        // Track state history for debugging etc.
        _pastStateHistory.Insert(0, state.Name );
        if ( _pastStateHistory.Count >= PAST_STATES_CAPACITY ) {
            _pastStateHistory.RemoveAt(_pastStateHistory.Count-1);
        }
	}
	
	// popState - set to true to restore the state from the top of the state stack
	public void ExitState<TExitState>( bool result = true, SC_Callback onCompleteCallback = null ) where TExitState : State
    {
		TExitState exitState = GetState<TExitState>();
		if (exitState != null) {
			ExitState( exitState, result, onCompleteCallback);
			
			// Clear mutually exclusive reference if exiting the current instance			
			if (_existingExclusiveState == exitState) {
				_existingExclusiveState = null;
			}
		}
	}
	public void ExitState( State exitState, bool result = true, SC_Callback onCompleteCallback = null )
    {
        if ( null == exitState ) { return; }

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( "State:" + exitState.Name, "State:" + exitState.Name );
		#endif

		// Remove from currently active state collection
		foreach(State state in _states) {
			if (state == exitState) {
				_states.Remove(state);
				state.SC_Exit( result, delegate ( State exitedState ) { 
					#if METRICS_ENABLED && INCLUDE_DEV_METRICS
					Metrics.End( GetType().Name+ ":" + exitState.Name );
					#endif
					if ( onCompleteCallback != null ) { onCompleteCallback( exitedState ); } }
				);
				return;
			}
		}

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name+ ":" + exitState.Name );
		#endif

		// No match found so just fire callback if any
		if ( onCompleteCallback != null ) { 
			onCompleteCallback( exitState );
		}
	}

	// Exit callback useful when chaining/grouping states to trigger another state
	// In this case the enter routine sets up some ongoing proccess serviced presumably from the SC_OnUpdate tick
	// then calls back to the State Manager
	public void OnStateExited( State state, bool result )
	{
	}

    //
    // State History

    public String PastStateAt( int index )
    {
        if ( index < 0 || index >= _pastStateHistory.Count ) { return ""; }
        return  _pastStateHistory[index];
    }


    //
    // Boot

    public void Boot()
    {
		StartCoroutine( AsyncBoot() );        
    }

	private IEnumerator AsyncBoot()
	{
		yield return null;
#if PRODUCTION
        if ( _client.GetBootstrapperEnabled() ) {
            EnterState<BootstrapperState>();
        } else {
            EnterState<BootupState>();
        }
#else
        EnterState<BootupState>();
#endif
    }

    public bool CanSoftBoot()
    {
		bool anyPreLoginState = false;
		foreach(State state in _states) {
			if (state.IsPreLogin()) {
				anyPreLoginState = true;
				break;
			}
		}

		// No activeState or before full login, force hard reboot		
        return (_states.Count > 0 && !anyPreLoginState);
    }

    public void SoftBoot( bool forceAccountUpgrade = false )
    {
        _instance.Log( "SoftBoot()" );

        if ( !CanSoftBoot() ) {
            _instance.LogError( "Cannot soft boot, aborting. There is a logic error." );
            return;
        }
		
		// during state transition, "SoftBoot" may have triggered after "EnterState" but before "OnEnterComplete" 
        // said state, not being set current, would never be formally exited by StateController
        // and would result in a data integrity violation (e.g., two duplicate states)
        ExitAndClearAllStates();

        _instance.Log( "Transitioning to LoginState" );

		//TODO Support soft reboot of client
        EnterState<LoginState>() ;
    }

    public void FeatureInitializeFinish()
    {
        // _existingExclusiveState may not be set by this point since FeatureInitializeFinish
        // can be called synchronously within SC_Enter, and _existingExclusiveState doesn't
        // get set until the finish callback of SC_Enter.
        // So we have to reverse iterate and find the most recent mutually exclusive state
        State stateInitialized = null;
        for (int i = _states.Count - 1; i >= 0; --i) {
            if (_states[i].IsMutuallyExclusive()) {
                stateInitialized = _states[i];
                break;
            }
        }
    }
}

