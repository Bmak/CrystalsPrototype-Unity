using System;
using System.Collections.Generic;


// TODO: Should we maybe add support for a timeout with an expiration time and action to be called on timeout ?? 
//       This way we don't leave the client in an awkward state if the user looses internet connection in the middle of a batch of actions etc.

public class Waiter : ILoggable
{
    private readonly Dictionary<Action, int> _waitCallbacks = new Dictionary<Action, int>( 5 );
    private Action _onFinished = delegate { };
	
	public bool IsWaiting() { return ( (_waitCallbacks != null) && (_waitCallbacks.Count > 0) ); }

	/// <summary>
	/// Is waiting the specified callback?
	/// </summary>
	private bool Waiting( Action waitCallback )
	{
		return _waitCallbacks.ContainsKey( waitCallback );
	}

    /// <summary>
    /// Adds a callback to a list of functions that must be called (informed by the Called() function) before performing some OnFinished() function
    /// </summary>
    /// <param name="callback">The function that must be called as part of the chain towards calling the OnFinished() function</param>
    /// <returns>Reference to self, to allow chaining</returns>
    public Waiter Wait( Action callback )
    {
        // Add callback
        Add( callback );

        return this;
    }

    /// <summary>
    /// Allows you to wait for a method to be called some number of times.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public Waiter Wait( Action callback, int count )
    {
		for ( int i = 0; i < count; ++i ) {
            Add( callback );
        }

        return this;
    }

    private void Add( Action waitCallback )
    {
        if ( !_waitCallbacks.ContainsKey( waitCallback ) ) {
            _waitCallbacks.Add( waitCallback, 0 );
        }

        _waitCallbacks[waitCallback] += 1;
    }
	
	
    /// <summary>
    /// At the bottom of any Wait() functions, call this function to inform the Waiter that the function has been called
    /// </summary>
    /// <param name="callback">The function callback that was just called</param>
    public void Called( Action callback )
    {
		if (!IsWaiting()) { 
			this.Log("Waiter Called() function called but waiter is no longer waiting");
			return; 
		}
		
        // Callback is finished
        FinishWait( callback );

        // Not yet finished, do nothing
        if ( _waitCallbacks.Count > 0 ) { return; }

        // Finished all the callbacks, call OnFinished
        _onFinished.Invoke();
    }

    /// <summary>
    /// Calls the callback when all previous Wait() functions have occurred (informed by the Called() function)
    /// </summary>
    /// <param name="callback">The function to call when previous Wait functions have been called</param>
    /// <returns>Reference to self, to allow chaining</returns>
    public Waiter OnFinished( Action callback )
    {
        // Protect against empty _waitCallbacks list
        if ( _waitCallbacks.Count == 0 ) {
#if UNITY_EDITOR || DEBUG
            throw new IndexOutOfRangeException( "You must specify at least one Wait() callback before calling OnFinished()" );
#endif
        }

        // Add finished callback
        _onFinished = callback;

        return this;
    }
	// Mark a specific wait as completed
    private void FinishWait( Action waitCallback )
    {
        if ( !_waitCallbacks.ContainsKey( waitCallback ) ) {
#if UNITY_EDITOR || DEBUG
            throw new IndexOutOfRangeException( "Waiter received unexpected completion signal" );
#else
			return; 
#endif
		}

        _waitCallbacks[waitCallback] -= 1;

        if ( _waitCallbacks[waitCallback] == 0 ) {
            _waitCallbacks.Remove( waitCallback );
        }
    }
}