using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class State : ILoggable
{

    [Inject]
    protected IInstantiator _instantiator;

    [Inject]
    protected LifecycleController _lifecycleController;

    [Inject]
    protected StateController _stateController;

    public string Name { get { return GetType().Name; } }

    // The time (in seconds) that this State was entered.  Uses Unity Time.realtimeSinceStartup
    public float TimeEntered { get; set; }

	
	// Mutually exclusive state management
	
	public virtual bool IsMutuallyExclusive() { return true; }

	
	//
	//
		
    public bool IsActive { get; private set; }

    // State grouping/chaining
    public List<State> SC_SubStates;
    public State SC_ParentState;

    // Sub-states should be configured during parent state initialization to avoid
    // race conditions and keep a clear standardized usage
    public bool AddSubState<TSubState>() where TSubState : State, new()
    {
        // Create state
        State subState = StateController.Instance.EnterState<TSubState>();
        if ( null == subState ) { return false; }

        // Won't be used in most cases so init on first use
        if ( SC_SubStates == null ) {
            SC_SubStates = new List<State>();
        }

        // Track what we're dependent on
        SC_SubStates.Add( subState );

        // Link sub-state to this parent state
        subState.SC_ParentState = this;
		
		return true;
    }

    // Notify the configured parent state of this sub-state's completion with succes or failure result
    // Usage: Called automatically when a state exits that was wired to a parent state OR by user when some
    //        expected critical section of the state has been completed such as initialization but the
    //        state will continue to execute
    public void NotfyParentStateOfSubStateExit( bool result )
    {
        // Ignore if there's no parent configured
        if ( null == SC_ParentState ) { return; }

        SC_ParentState.NotifyOfSubStateExit( this, result );
    }
    // Notify THIS state that one of the sub-states has exited and it's success or failure result
    private void NotifyOfSubStateExit( State state, bool result )
    {
        // Ignore if not tracking this state instance
        if ( !SC_SubStates.Contains( state ) ) {
            return;
        }

        Debug.Log( "Sub-state  " + state.Name + " finished for " + this.Name + " with result " + ( result ? "SUCCESS" : "FAILURE" ) );


        // Remove from set
        SC_SubStates.Remove( state );

        // Trigger subStates complete failure if this sub-state return failure result
        if ( !result ) {
            SC_ParentState.SubStatesFinished( false );
        }
        // Trigger subStates complete if none remain
        else if ( 0 == SC_SubStates.Count ) {
            SC_ParentState.SubStatesFinished( true );
        }
    }

    // Place your state progression etc. here to be executed when all sub-states are complete
    public virtual void SubStatesFinished( bool overallResult )
    {
        Debug.Log( "Sub-states finished for " + this.Name + " with overall " + ( overallResult ? "SUCCESS" : "FAILURE" ) );
    }
	
	
    //
    // LifeCycle

    // State Controller Calls for sub-class override

    public virtual bool SC_Init()
    {
		return true;
    }

    public virtual bool SC_Enter(object transitionInfo, SC_Callback onCompleteCallback=null)
    {
        IsActive = true;

		if (onCompleteCallback != null) { onCompleteCallback( this ); }

        return true;
    }

    public virtual void SC_Exit( bool result, SC_Callback onCompleteCallback )
    {
        IsActive = false;

        // Notify parent state if any
        NotfyParentStateOfSubStateExit( result );

        // Notify controller
        StateController.Instance.OnStateExited( this, result );
		
		if (onCompleteCallback != null) { onCompleteCallback( this ); }
    }

    public virtual void SC_Destroy()
    {
    }

    // Convenience Member Control

	public void Enter(object transitionInfo = null)
    {
        // Defer to State Controller for consistency
        StateController.Instance.EnterState( this, transitionInfo );
    }

    // Note: Should be called by self-exiting states such as those involved in BootStrapping
    public void Exit( bool result )
    {
        // Defer to State Controller for consistency
        StateController.Instance.ExitState( this, result );
    }


    //
    // Behaviors


    //
    // Update Tick

    public virtual void SC_Update() { }


    //
    // Strange left overs that I'd love to see moved

    // TODO: Can this logic be moved somewhere else or eliminated completely since no UI object should know it's relationship to Login?
    public virtual bool IsPreLogin() { return false; }
}