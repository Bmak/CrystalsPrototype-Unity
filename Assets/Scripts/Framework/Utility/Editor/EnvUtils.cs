using System;
using System.Collections;
using System.Collections.Generic;

public class EnvUtil : ILoggable {

	private static readonly EnvUtil _instance = new EnvUtil(); // Only for logger

	// Local genericized cache of System.Environment.GetEnvironmentVariables()
	private static IDictionary<string,string> _environmentVariables;

	static EnvUtil() {
		Refresh();
	}

	/// <summary>
	/// Returns a fresh copy of the system environment as an IDictionary<string,string>.
	/// </summary>
	/// <returns>The environment variables.</returns>
	public static IDictionary<string,string> GetEnvironmentVariables()
	{
		IDictionary environmentVariables = System.Environment.GetEnvironmentVariables();
		IDictionary<string,string> result = new Dictionary<string,string>(environmentVariables.Count);

		foreach ( DictionaryEntry entry in environmentVariables ) {
			try {
				result.Add( (string)entry.Key, (string)entry.Value );
			} catch {
				_instance.LogWarning( "Could not read environment variable '" + entry + "', skipping.");
			}
		}
		
		return result;
	}

	/// <summary>
	/// Sets an environment variable within the context of this process
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public static void SetEnvironmentVariable( string key, string value ) {
		System.Environment.SetEnvironmentVariable( key, value );
		_environmentVariables[key] = value; // Update internal cache for convenience
	}

	/// <summary>
	/// Sets a dictionary of environment variables
	/// </summary>
	/// <param name="environmentVariables">Environment variables.</param>
	/// <param name="overwrite">If enabled, existing environment variables with same name will be overwritten, otherwise set will be skipped.</param>
	public static void SetEnvironmentVariables( IDictionary<string,string> environmentVariables, bool overwrite = true ) {
		foreach ( KeyValuePair<string,string> kvp in environmentVariables )
			if ( overwrite || !_environmentVariables.ContainsKey( kvp.Key ) )
				SetEnvironmentVariable( kvp.Key, kvp.Value );
	}

	/// <summary>
	/// Refresh the environment cache so that any modifications made during the execution
	/// of this process will be reflected.
	/// </summary>
	public static void Refresh() {
		_environmentVariables = GetEnvironmentVariables();
	}

	/// <summary>
	/// Get the value of a particular environment variable. This method
	/// will only return environment variables as they were at the time of application start (or rather,
	/// the execution of the static field initializer). Environment variables updated during execution
	/// will not be reflected in values returned by this method unless Refresh() is called;
	/// </summary>
	/// <returns>The environment variable, or null if the key was not found.</returns>
	/// <param name="key">Name of the environment variable to return</param>
	public static string Get( string key, string defaultValue = null ) {
		string value;
		if ( _environmentVariables.TryGetValue( key, out value ) ) return value;
		return defaultValue;
	}

	public static bool Get( string key, bool defaultValue ) {
		try {
			string value;
			if ( _environmentVariables.TryGetValue( key, out value ) ) return Convert.ToBoolean( value );
		} catch {
			_instance.LogWarning( "Could not convert '" + key + "' to bool, return default value '" + defaultValue + "'" );
		}
		return defaultValue;
	}

	public static int Get( string key, int defaultValue ) {
		try {
			string value;
			if ( _environmentVariables.TryGetValue( key, out value ) ) return Convert.ToInt32( value );
		} catch {
			_instance.LogWarning( "Could not convert environment variable '" + key + "' to int, return default value '" + defaultValue + "'" );
		}
		return defaultValue;
	}

	public static long Get( string key, long defaultValue ) {
		try {
			string value;
			if ( _environmentVariables.TryGetValue( key, out value ) ) return Convert.ToInt64( value );
		} catch {
			_instance.LogWarning( "Could not convert '" + key + "' to long, return default value '" + defaultValue + "'" );
		}
		return defaultValue;
	}

	public static float Get( string key, float defaultValue ) {
		try {
			string value;
			if ( _environmentVariables.TryGetValue( key, out value ) ) return Convert.ToSingle( value );
		} catch {
			_instance.LogWarning( "Could not convert '" + key + "' to float, return default value '" + defaultValue + "'" );
		}
		return defaultValue;
	}

	public static double Get( string key, double defaultValue ) {
		try {
			string value;
			if ( _environmentVariables.TryGetValue( key, out value ) ) return Convert.ToDouble( value );
		} catch {
			_instance.LogWarning( "Could not convert '" + key + "' to double, return default value '" + defaultValue + "'" );
		}
		return defaultValue;
	}

}
