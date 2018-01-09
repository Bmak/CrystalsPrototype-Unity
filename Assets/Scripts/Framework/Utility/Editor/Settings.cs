using System;
using System.Collections.Generic;

public class Settings<TKey,TValue> : Dictionary<TKey, TValue>, ILoggable {

	public Settings():base() {}
	public Settings( int capacity ):base( capacity ) {}

	public Settings( IDictionary<TKey, TValue> dictionary ) : base (dictionary) {}

	public TValue Get( TKey key ) {
		TValue value;
		TryGetValue( key, out value );
		return value;
	}
	
	public string Get( TKey key, string defaultValue ) {
		try {
			TValue value;
			if ( TryGetValue( key, out value ) ) return Convert.ToString( value );
		} catch (Exception e) {
			this.LogWarning( "Unable to convert key '" + key.ToString() + "' to string: " + e.ToString() );
		}
		return defaultValue;
	}

	public bool Get( TKey key, bool defaultValue ) {
		try {
			TValue value;
			if ( TryGetValue( key, out value )) return Convert.ToBoolean( value );
		} catch (Exception e) {
			this.LogWarning( "Unable to convert key '" + key.ToString() + "' to bool: " + e.ToString() );
		}
		return defaultValue;
	}

	public int Get( TKey key, int defaultValue ) {
		try {
			TValue value;
			if ( TryGetValue( key, out value ) ) return Convert.ToInt32( value );
		} catch (Exception e) {
			this.LogWarning( "Unable to convert key '" + key.ToString() + "' to int: " + e.ToString() );
		}
		return defaultValue;
	}

	public long Get( TKey key, long defaultValue ) {
		try {
			TValue value;
			if ( TryGetValue( key, out value ) ) return Convert.ToInt64( value );
		} catch (Exception e) {
			this.LogWarning( "Unable to convert key '" + key.ToString() + "' to long: " + e.ToString() );
		}
		return defaultValue;
	}

	public float Get( TKey key, float defaultValue ) {
		try {
			TValue value;
			if ( TryGetValue( key, out value ) ) return Convert.ToSingle( value );
		} catch (Exception e) {
			this.LogWarning( "Unable to convert key '" + key.ToString() + "' to float: " + e.ToString() );
		}
		return defaultValue;
	}

	public double Get( TKey key, double defaultValue ) {
		try {
			TValue value;
			if ( TryGetValue( key, out value ) ) return Convert.ToDouble( value );
		} catch (Exception e) {
			this.LogWarning( "Unable to convert key '" + key.ToString() + "' to double: " + e.ToString() );
		}
		return defaultValue;
	}

}