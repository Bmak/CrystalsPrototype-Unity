using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Field persistor.
/// This class persists object field values to PlayerPrefs, and later reads
/// these PlayerPrefs and restores the original fields with the persisted values.
/// </summary>
public class FieldPersistor<T> : ILoggable where T : class, new() 
{	
	private static readonly BindingFlags FIELD_BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;

	// The target object to populate
	private readonly T _target;
	
	// Type of the above target
	private readonly Type _targetType;
	
	// Key prefix when reading/saving PlayerPrefs
	private readonly string _prefKeyPrefix;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="FieldPersistor"/> class.
	/// </summary>
	/// <param name="target">Target object. Fields in this object will be persisted and restored via reflection.</param>
	public FieldPersistor( T target )
	{
		_target = target;
		_targetType = target.GetType();
		_prefKeyPrefix = _targetType.Name + ":";
	}
	
	public void Restore( params string[] fieldNames )
	{
		for( int i = 0; i<fieldNames.Length; ++i) {
			string fieldName = fieldNames[i];
			string prefKey = _prefKeyPrefix + fieldName;
			// Continue if no value has been previously persisted for this field
			// This is necessary so we can distinguish the lack of a value and a true empty/null
			if  ( !PlayerPrefs.HasKey( prefKey ) ) continue;

			string persistedValue = PlayerPrefs.GetString( prefKey );
			FieldInfo fieldInfo = _targetType.GetField( fieldName, FIELD_BINDING_FLAGS );
			
			if ( fieldInfo == null ) {
				this.LogWarning( "Unrecognized field '" + fieldName + "' in class '" + _targetType.Name + ", skipping" );
				continue;
			}
			
			Type fieldType = fieldInfo.FieldType;
			
			try {
				object convertedValue = ConvertValue( persistedValue, fieldType );
				// If conversion to target type succeeded, set value via reflection			
				if ( convertedValue != null )
					fieldInfo.SetValue( _target, convertedValue );				
			} catch (Exception e) {
				this.LogError( "Exception restoring field '" + fieldName + "' with value '" + persistedValue + "': " + e.ToString() );
			}			
		}
	}
	
	public void Persist( params string[] fieldNames )
	{
		for( int i = 0; i<fieldNames.Length; ++i) {
			string fieldName = fieldNames[i];
			
			FieldInfo fieldInfo = _targetType.GetField( fieldName, FIELD_BINDING_FLAGS );
			
			if ( fieldInfo == null ) {
				this.LogWarning( "Unrecognized field '" + fieldName + "' in class '" + _targetType.Name + ", skipping" );
				continue;
			}

			try {
				object fieldValue = fieldInfo.GetValue( _target );
				if ( fieldValue != null ) {
					string prefKey = _prefKeyPrefix + fieldName;
					PlayerPrefs.SetString( prefKey, Convert.ToString( fieldValue ) );					
				}	
			} catch (Exception e) {
				this.LogError( "Exception persisting field '" + fieldName + "': " + e.ToString() );
			}
		}
		
		PlayerPrefs.Save();
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
}