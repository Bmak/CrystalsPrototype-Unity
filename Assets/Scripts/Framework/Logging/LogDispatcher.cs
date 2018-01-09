using UnityEngine;

/// <summary>
/// Log dispatcher. 
/// Abstracts access to Unity's Application.logMessageReceived event.
/// </summary>
public class LogDispatcher : ILoggable
{
    public void RegisterLogCallback( Application.LogCallback handler ) 
    {
		// Event registration should be idempotent
		Application.logMessageReceived -= handler;
		Application.logMessageReceived += handler;
    }

    public void UnregisterLogCallback( Application.LogCallback handler )
    {
		Application.logMessageReceived -= handler;
    }
}
