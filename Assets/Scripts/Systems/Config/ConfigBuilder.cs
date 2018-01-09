using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System;

/// <summary>
/// Populates Config object with configuration constants from the server.
/// </summary>
public class ConfigBuilder<T> : ILoggable where T : class, new() 
{	
	private static readonly Regex _fieldNameRegex = new Regex(@"-([a-z])");
	
	// The config object to populate
	private T _configTarget;
	
	// Type of the above configTarget
	private Type _configTargetType;
	
	public ConfigBuilder( T configTarget ) {
		_configTarget = configTarget;
		_configTargetType = configTarget.GetType();	
	}
	
	public void Populate( List<ConfigEntry> configEntries )
	{
		// No config values to populate
		if ( ( configEntries == null ) || configEntries.Count <= 0) return;
		
		foreach ( ConfigEntry entry in configEntries ) {
			ProcessConfigEntry( entry ); 
		}
	}	
	
	private void ProcessConfigEntry( ConfigEntry entry )
	{
		string key = entry.Key;
		string value = entry.Value;
		string fieldName = GetFieldNameForKey( key );
		
		FieldInfo fieldInfo = _configTargetType.GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Instance );
		
		if ( fieldInfo == null ) {
			this.LogWarning( "Unrecognized config entry [ key: " + key + " value: " + value + " ]: no matching field '" + fieldName + "'" );
			return;
		}
		
		Type fieldType = fieldInfo.FieldType;
		
		try {
			object convertedValue = ConvertValue( value, fieldType );			
			if ( convertedValue != null ) fieldInfo.SetValue( _configTarget, convertedValue );			
		} catch (Exception e) {
			this.LogError( "Exception processing config entry [ key: " + key + " value: " + value + " ]: " + e.ToString() );
		}
		
	}
	
	private object ConvertValue( string value, Type targetType )
	{
		
		try {
			return Convert.ChangeType( value, targetType );
		} catch (Exception e) {
			this.LogError("Exception converting '" + value + "' to type '" + targetType.ToString() + "': " + e.ToString() );
		}
		
		// Fall-through on exception
		return null;
	}
	
	/// <summary>
	/// Converts a lowercase-hyphenated key name to a private field name in this class,
	/// according to the naming convention.
	/// 
	/// Example:
	/// 	in:		test-key-name
	/// 	out:	_testKeyName
	/// </summary>
	/// <returns>The field name corresponding to key.</returns>
	/// <param name="key">Key.</param>
	private string GetFieldNameForKey( string key )
	{
		return @"_" + _fieldNameRegex.Replace( key, m => m.Groups[1].Value.ToUpper() );
	}
}
