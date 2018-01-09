using UnityEngine;
using System;

public enum LogLevel {
    NONE = -1,
    OUTPUT = 0,
    EXCEPTION = 1,
    ASSERT = 2,
    ERROR = 3,    
    WARN = 4,
    INFO = 5,
    DEBUG = 6,
    TRACE = 7,
    ALL = 99
}


// Must be powers of two as these are used for bitwise operations to determine matching logging categories
[Flags]
public enum LogCategory {
	NONE = 0, 				// Do not use for bitwise operations. Recommended by C# spec. See https://msdn.microsoft.com/en-us/library/system.flagsattribute(v=vs.110).aspx
	DEFAULT = 1,			// Standard whitelist. Allows LogCategory.ALL to match bitwise.
	INJECTOR = 2,
	ASSET_DOWNLOAD = 4,
	RPC = 8,
	INITIALIZATION = 16,
/*
	CATEGORY_c = 32,
	CATEGORY_D = 64,
	CATEGORY_E = 128,
	CATEGORY_F = 256,
	CATEGORY_G = 512,
	CATEGORY_H = 1024,
	CATEGORY_I = 2048,
	CATEGORY_J = 4096,
	CATEGORY_K = 8192,
	CATEGORY_L = 16384,
	CATEGORY_M = 32768,
	CATEGORY_N = 65536,	
	HIGHEST_ALLOWED_POWER_OF_2 = 1073741824, // 2^30
*/
	ALL = Int32.MaxValue // All categories enabled. Value: 2147483647 -> 01111111111111111111111111111111

}

public static class Logging {

    private const string DATE_TIME_FORMAT_STRING = @"HH:mm:ss yyyy-MM-dd";

    // Set this flag to enable/disable prepending of DateTime info to log output
    private static bool _dateTimeEnabled = true;

    public static void SetLogLevel( LogLevel value ) {
        // Delegate to underlying loggers for log level filtering
        Log.SetLogLevel( value );
        LogExtensions.SetLogLevel( value );		
    }
    
	public static bool SetLogLevel( string levelName )
	{
		try { 
			LogLevel logLevel = EnumUtil.Parse<LogLevel>( levelName );
			SetLogLevel( logLevel );
		} catch ( ArgumentException e ) {
			Log.Warning("Logging.SetLogLevel(): invalid log level '" + levelName + "': " + e.ToString());
            return false;
        }        
        return true;
	}
    /// <summary>
	/// Sets the logging category mask.
	/// This is a whitelist of allowed categories. Specify multiple categories via bitwise OR.	
	/// </summary>
	/// <param name="category">Category.</param>
	public static void SetLogCategoryMask( LogCategory category )
	{
		// Delegate to underlying loggers for category filtering
		Log.SetLogCategoryMask( category );
		LogExtensions.SetLogCategoryMask( category );
    }

	public static void SetLogCategories( string[] categoryNames )
	{
		LogCategory categoryMask = EnumUtil.ToBitFieldEnum<LogCategory>( categoryNames );
		SetLogCategoryMask( categoryMask );
	}
    
	/// <summary>
	/// Enable a logging category in the whitelist. 
	/// The specified value will be bitwise-OR'ed against the existing log category mask.
	/// </summary>
	/// <param name="category">Category.</param>
    public static void EnableLogCategory( LogCategory category )
    {
		// Delegate to underlying loggers for category filtering
		Log.EnableLogCategory( category );
		LogExtensions.EnableLogCategory( category );
	}

	public static bool EnableLogCategory( string categoryName )
	{
		try { 
			LogCategory category = EnumUtil.Parse<LogCategory>( categoryName );
			EnableLogCategory( category );
		} catch ( ArgumentException e ) {
			Log.Warning("Logging.EnableLogCategory(): invalid log category '" + categoryName + "': " + e.ToString());
			return false;
		}
		return true;
	}

	/// <summary>
	/// Disable a logging category in the whitelist. 
	/// The specified value will be bitwise-OR'ed against the existing log category mask.
	/// </summary>
	/// <param name="category">Category.</param>
	public static void DisableLogCategory( LogCategory category )
	{
		// Delegate to underlying loggers for category filtering
		Log.DisableLogCategory( category );
		LogExtensions.DisableLogCategory( category );
	}
	
	public static bool DisableLogCategory( string categoryName )
	{
		try { 
			LogCategory category = EnumUtil.Parse<LogCategory>( categoryName );
			DisableLogCategory( category );
		} catch ( ArgumentException e ) {
			Log.Warning("Logging.DisableLogCategory(): invalid log category '" + categoryName + "': " + e.ToString());
			return false;
		}
		return true;
	}

	public static void SetDateTimeEnabled( bool value ) {
		_dateTimeEnabled = value;   
	}
    
    /// <summary>
    /// Builds the Log output of the form CLASSNAME: message. We assume that caller will never
    /// be null, which should be a safe bet.
    /// </summary>
    /// <returns>The message</returns>
    /// <param name="message">Message object, will be converted to string.</param>
    /// <param name="caller">Calling object whose type will be prepended to message.</param>
    public static string BuildOutput( object message, LogLevel logLevel, ILoggable caller = null) 
    {
        return ( _dateTimeEnabled ? GetDateTimeOutput() : "" ) + logLevel.ToString() + " " + ( caller == null ?  "" : caller.GetType().ToString() + ": " ) + ( message == null ? "Null" : message.ToString() );
    }

    /// <summary>
    /// Builds the formatted DateTime output string to be prepended to log output
    /// </summary>
    private static string GetDateTimeOutput() 
    {
        return "[ " + DateTime.UtcNow.ToString( DATE_TIME_FORMAT_STRING ) + " ] ";
    } 
}

public static class Log {

    // Local field and setter are a runtime optimization
    private static LogLevel _logLevel = LogLevel.ERROR;
	private static LogCategory _logCategoryMask = LogCategory.DEFAULT;
	
	public static void SetLogLevel( LogLevel value )
	{
		_logLevel = value;
	}   
	
	public static void SetLogCategoryMask( LogCategory category )
	{
		_logCategoryMask = category;
	}
	
	public static void EnableLogCategory( LogCategory category )
	{
		_logCategoryMask = _logCategoryMask | category;
    }

	public static void DisableLogCategory( LogCategory category )
	{
		_logCategoryMask = _logCategoryMask & ~category;
	}
    
    public static void Output( object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.OUTPUT > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
        UnityEngine.Debug.Log( Logging.BuildOutput( message, LogLevel.OUTPUT ) );
    }      
    
    // Direct pass-through (provided for consistency)
	public static void Exception( Exception exception, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.EXCEPTION > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
        UnityEngine.Debug.LogException( exception);
    }    

	public static void Assert( object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.ASSERT > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
        UnityEngine.Debug.LogError( Logging.BuildOutput( message, LogLevel.ASSERT ) );
    }
    
	public static void Error( object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.ERROR > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
        UnityEngine.Debug.LogError( Logging.BuildOutput( message, LogLevel.ERROR ) );
    }    
    
	public static void Warning( object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.WARN > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
        UnityEngine.Debug.LogWarning( Logging.BuildOutput( message, LogLevel.WARN ) );        
    }
    
	public static void Info( object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.INFO > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
        UnityEngine.Debug.Log( Logging.BuildOutput( message, LogLevel.INFO ));        
    }      
    
	public static void Debug( object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.DEBUG > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
        UnityEngine.Debug.Log( Logging.BuildOutput( message, LogLevel.DEBUG ));        
    }

	public static void Trace( object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.TRACE > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
        UnityEngine.Debug.Log( Logging.BuildOutput( message, LogLevel.TRACE ) );        
    }        
}


// The interface and class in this file together act as a mix-in
// to allow more convenient logging.

// Implement the ILoggable interface on any non-static class
// and gain access to the convenient extension methods below. 
//
// This allows logging of the form:
//		this.Debug("message here")
// with output:
//		CLASSNAME: message here
//
// That is, it prepends the type of the calling object (this) to the debug
// output to increase readability of log lines and reduce embedded class
// names in debug messages.


// Tagging interface 
public interface ILoggable { }

// Extension methods
// The interface of each of these extensions methods mirrors those in UnityEngine.Debug,
// except the leading ILoggable caller has been added as the first parameter to all.

public static class LogExtensions {
    
    // Local field and setter are a runtime optimization
    private static LogLevel _logLevel = LogLevel.ERROR;
	private static LogCategory _logCategoryMask = LogCategory.DEFAULT | LogCategory.ASSET_DOWNLOAD;
    
    public static void SetLogLevel( LogLevel value )
	{
        _logLevel = value;
    }   

	public static void SetLogCategoryMask( LogCategory category )
	{
		_logCategoryMask = category;
	}

	public static void EnableLogCategory( LogCategory category )
	{
		_logCategoryMask = _logCategoryMask | category;
	}

	public static void DisableLogCategory( LogCategory category )
	{
		_logCategoryMask = _logCategoryMask & ~category;
	}

	public static void LogOutput( this ILoggable caller, object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.OUTPUT > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
		UnityEngine.Debug.Log( Logging.BuildOutput( message, LogLevel.OUTPUT, caller ) );
    }     
    
    // Direct pass-through (provided for consistency)
	public static void LogException( this ILoggable caller, Exception exception, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.EXCEPTION > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;		
		UnityEngine.Debug.LogException( exception );
    } 

	public static void LogAssert( this ILoggable caller, object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.ASSERT > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;		
		UnityEngine.Debug.LogError( Logging.BuildOutput( message, LogLevel.ASSERT, caller ));
    }     
    
	public static void LogError( this ILoggable caller, object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.ERROR > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;		
		UnityEngine.Debug.LogError( Logging.BuildOutput( message, LogLevel.ERROR, caller ) );
    }

	public static void LogWarning( this ILoggable caller, object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.WARN > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
		UnityEngine.Debug.LogWarning( Logging.BuildOutput( message, LogLevel.WARN, caller ) );
    }

	public static void LogInfo( this ILoggable caller, object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.INFO > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
		UnityEngine.Debug.Log( Logging.BuildOutput( message, LogLevel.INFO, caller ) );
    }    
    
	public static void Log( this ILoggable caller, object message, LogCategory category = LogCategory.ALL )
	{
        if ( LogLevel.DEBUG > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
		UnityEngine.Debug.Log( Logging.BuildOutput( message, LogLevel.DEBUG, caller ) );
	}
    
	public static void LogTrace( this ILoggable caller, object message, LogCategory category = LogCategory.ALL )
    {
        if ( LogLevel.TRACE > _logLevel ) return;
		if ( ( category & _logCategoryMask ) == 0 ) return;
		UnityEngine.Debug.Log( Logging.BuildOutput( message, LogLevel.TRACE, caller ) );
    }    
}