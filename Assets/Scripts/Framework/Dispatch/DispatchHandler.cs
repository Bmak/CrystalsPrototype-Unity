using System;
using System.Collections.Generic;

/// <summary>
/// Base class for DispatchHandlers, implement this class to handle objects sent to the system.
/// </summary>
public abstract class DispatchHandler
{
	private class HandlerMethodInfo
	{
		public System.Reflection.MethodInfo Method;
		public object Target;
	}

	private Dictionary<Type, HandlerMethodInfo> _handlerMethods = new Dictionary<Type, HandlerMethodInfo>();
	
	internal void _handleInternal( IDispatchable eventToHandle )
	{
		HandlerMethodInfo handlerMethodInfo = null;
		_handlerMethods.TryGetValue( eventToHandle.GetType(), out handlerMethodInfo );
		if( handlerMethodInfo != null && eventToHandle != null )
		{
			handlerMethodInfo.Method.Invoke( handlerMethodInfo.Target, new object[] { eventToHandle } );
		}
	}

	/// <summary>
	/// Register individual IDispatchable type handler methods.
	/// NOTE: We could use reflection to populate the _handlerMethods dict at startup automatically,
	///   but I'm not sure that the potential ambiguity outweighs having to type _registerTypeHandler
	/// </summary>
	/// <param name="handlerAction">Handler method to register. Will be called when objects are sent.</param>
	protected void _registerTypeHandler<T>( Action<T> handlerAction )
	{
		Type typeToRegister = typeof( T );
		HandlerMethodInfo handlerMethodInfo = null;
		if( _handlerMethods.TryGetValue( typeToRegister, out handlerMethodInfo ) )
		{
			// already registered, update method
			handlerMethodInfo.Target = handlerAction.Target;
			handlerMethodInfo.Method = handlerAction.Method;
			return;
		}

		handlerMethodInfo = new HandlerMethodInfo();
		handlerMethodInfo.Target = handlerAction.Target;
		handlerMethodInfo.Method = handlerAction.Method;
		
		_handlerMethods.Add( typeToRegister, handlerMethodInfo );
	}
}
