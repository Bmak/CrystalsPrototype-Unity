using System.Collections.Generic;

/// <summary>
/// Abstract base class for Dispatch systems.
/// Inherit from this class in your application code and register DispatchHandlers to handle sent objects
/// </summary>
public abstract class Dispatch
{
	private List<DispatchHandler> _handlers = new List<DispatchHandler>();


	/// <summary>
	/// Sends an object to the Dispatch system to be dispatched to all handlers.
	/// </summary>
	/// <param name="objectToSend">Object to send.</param>
	public void SendObject<TDispatchable>( TDispatchable objectToSend ) where TDispatchable : IDispatchable
	{
		// Calling in reverse order so that the ContextualMessageDispatch handler gets called last.
		// This is done so that we let other handlers process the game event before the contextual message controller
		for( int index = _handlers.Count-1; index >= 0; --index)
		{
			_handlers[index]._handleInternal( objectToSend );
		}
	}

	
	/// <summary>
	/// Registers a DispatchHandler object. Implement and register individual handler methods for
	/// dispatchable object types you want to handle
	/// </summary>
	/// <param name="handleToRegister">DispatchHandler to register.</param>
	public void RegisterHandler( DispatchHandler handlerToRegister )
	{
		if( _handlers.Contains( handlerToRegister ) )
		{
			// already registered, nothing to do
			return;
		}

		_handlers.Add( handlerToRegister );
	}


	/// <summary>
	/// Removes the handler from the system
	/// </summary>
	/// <param name="handler">DispatchHandler to remove</param>
	public void RemoveHandler( DispatchHandler handler )
	{
		_handlers.Remove( handler );
	}

}
