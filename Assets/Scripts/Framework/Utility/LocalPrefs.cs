using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Proxy to PlayerPrefs with methods that, by default, prefix all string keys
/// with the current Player's playerID. This is to allow preferences to be saved to the local
/// device while sharing accounts (multiple facebook, guest accounts, etc) without cross-mixing
/// of values between players.
/// 
/// Example: we wish for COPPA acceptance to not carry over to a new guest account.
///
/// </summary>

public class LocalPrefs : ILoggable {
    public static readonly List<string> PERSISTENT_ON_DELETE_KEYS = new List<string>() { "Debug_SerializedServerConfigList" };

	private const string PREFIX_SEPARATOR = ":";
	private string _playerId;

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.GetInt(), key auto-prefixed by playerId.
	/// </summary>
	/// <returns>The int.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public int GetInt(string key , int defaultValue = 0) {
		return PlayerPrefs.GetInt( AsPerPlayerUniqueKey(key), defaultValue );
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.GetInt(), shared by all players on this device.
	/// </summary>
	/// <returns>The shared int.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public int GetSharedInt( string key , int defaultValue = 0 ) {
		return PlayerPrefs.GetInt( key, defaultValue );
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.SetInt(), key auto-prefixed by playerId.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetInt( string key, int value, bool save = true ) {
		PlayerPrefs.SetInt( AsPerPlayerUniqueKey(key), value );
		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.SetInt(), shared by all players on this device.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetSharedInt( string key, int value, bool save = true ) {
		PlayerPrefs.SetInt( key, value );
		if ( save ) Save();
	}
   
	/// <summary>
	/// Player-specific proxy to PlayerPrefs.GetString(); The long value is
	/// stored as a string, key auto-prefixed by playerId.
	/// </summary>
	/// <returns>The long.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public long GetLong( string key, long defaultValue = 0L ) {
		string value = GetString( key, null );
		return string.IsNullOrEmpty( null ) ? defaultValue : ToLong( value );
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.GetString(); The long value is
	/// stored as a string. Shared by all players on this device.
	/// </summary>
	/// <returns>The shared long.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public long GetSharedLong( string key, long defaultValue = 0L ) {
		string value = GetSharedString( key, null );
		return string.IsNullOrEmpty( value ) ? defaultValue : ToLong( value );
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.SetString(); The long value is
	/// stored as a string. Key auto-prefixed by playerId.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetLong( string key, long value, bool save = true ) {
		PlayerPrefs.SetString( AsPerPlayerUniqueKey(key), value.ToString() );
		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.SetString(); The long value is
	/// stored as a string. Shared by all players on this device.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetSharedLong( string key, long value, bool save = true ) {
		PlayerPrefs.SetString( key, value.ToString() );
		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.GetInt() with 0 = false and 1 = true, key auto-prefixed by playerId.
	/// </summary>
	/// <returns>true if internal key == 1, else false</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
    public bool GetBool( string key, bool defaultValue = false ) {
        return GetSharedInt( AsPerPlayerUniqueKey(key), defaultValue ? 1 : 0 ) == 1;
    }
 
	/// <summary>
	/// Direct proxy to PlayerPrefs.GetInt() with 0 = false and 1 = true, shared by all players on this device.
	/// </summary>
	/// <returns>true if internal key == 1, else false</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
    public bool GetSharedBool( string key, bool defaultValue = false ) {
        return GetSharedInt( key, defaultValue ? 1 : 0 ) == 1;
    }
    
	/// <summary>
	/// Direct proxy to PlayerPrefs.SetInt() with 0 = false and 1 = true, key auto-prefixed by playerId.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetBool( string key, bool value, bool save = true ) {
        SetSharedInt( AsPerPlayerUniqueKey(key), value ? 1 : 0 );
		if ( save ) Save();
    }

	/// <summary>
	/// Direct proxy to PlayerPrefs.SetInt() with 0 = false and 1 = true, shared by all players on this device.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetSharedBool( string key, bool value, bool save = true ) {
        SetSharedInt( key, value ? 1 : 0 );
		if ( save ) Save();
    }

  	/// <summary>
	/// Player-specific proxy to PlayerPrefs.GetFloat(), key auto-prefixed by playerId.
	/// </summary>
	/// <returns>The float.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public float GetFloat( string key , float defaultValue = 0.0f ) {
		return PlayerPrefs.GetFloat( AsPerPlayerUniqueKey(key), defaultValue );
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.GetFloat(), shared by all players on this device.
	/// </summary>
	/// <returns>The shared float.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public float GetSharedFloat( string key , float defaultValue = 0.0f ) {
		return PlayerPrefs.GetFloat( key, defaultValue );
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.SetFloat(), key auto-prefixed by playerId.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetFloat( string key , float value, bool save = true ) {
		PlayerPrefs.SetFloat( AsPerPlayerUniqueKey(key), value );
		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.SetFloat(), shared br all players on this device.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetSharedFloat( string key, float value, bool save = true ) {
		PlayerPrefs.SetFloat( key, value );
		if ( save ) Save();
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.GetString(), key auto-prefixed by playerId.
	/// </summary>
	/// <returns>The string.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public string GetString(string key , string defaultValue = "") {
		return PlayerPrefs.GetString( AsPerPlayerUniqueKey(key), defaultValue );
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.GetString(), shared by all players on this device.
	/// </summary>
	/// <returns>The shared string.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public string GetSharedString( string key , string defaultValue = "" ) {
		return PlayerPrefs.GetString( key, defaultValue );
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.SetString(), key auto-prefixed by playerId.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetString( string key , string value, bool save = true ) {
		PlayerPrefs.SetString( AsPerPlayerUniqueKey(key), value );
		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.SetString(), shared by all players on this device.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetSharedString( string key , string value, bool save = true ) {
		PlayerPrefs.SetString( key, value );
		if ( save ) Save();
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.GetString(), key auto-prefixed by playerId.
	/// Returns value of key as an Enum constant of type T, or defaultValue if the key
	/// does not exist or is not a defined constant in type T.
	/// </summary>
	/// <returns>The enum constant.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public T GetEnum<T>( string key , T defaultValue ) where T : struct, IComparable, IConvertible, IFormattable {
		return EnumUtil.Parse( PlayerPrefs.GetString( AsPerPlayerUniqueKey(key), null ), defaultValue );
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.GetString(), shared by all players on this device.
	/// Returns value of key as an Enum constant of type T, or defaultValue if the key
	/// does not exist or is not a defined constant in type T.
	/// </summary>
	/// <returns>The enum constant.</returns>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	public T GetSharedEnum<T>( string key , T defaultValue ) where T : struct, IComparable, IConvertible, IFormattable {
		return EnumUtil.Parse( PlayerPrefs.GetString( key, null ), defaultValue );
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.SetString(), key auto-prefixed by playerId.
	/// Enum constant will be saved as its string representation.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetEnum<T>( string key, T value, bool save = true ) where T : struct, IComparable, IConvertible, IFormattable {
		PlayerPrefs.SetString( AsPerPlayerUniqueKey(key), value.ToString() );
		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.SetString(), shared by all players on this device.
	/// Enum constant will be saved as its string representation.
	/// </summary>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	public void SetSharedEnum<T>( string key, T value, bool save = true ) where T : struct, IComparable, IConvertible, IFormattable {
		PlayerPrefs.SetString( key, value.ToString() );
		if ( save ) Save();
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.HasKey(), key auto-prefixed by playerId.
	/// </summary>
	/// <returns><c>true</c> if has key the specified key; otherwise, <c>false</c>.</returns>
	/// <param name="key">Key.</param>
	public bool HasKey( string key ) {
		return PlayerPrefs.HasKey( AsPerPlayerUniqueKey(key) );
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.HasKey(), shared by all players on this device.
	/// </summary>
	/// <returns><c>true</c> if has shared key the specified key; otherwise, <c>false</c>.</returns>
	/// <param name="key">Key.</param>
	public bool HasSharedKey( string key ) {
		return PlayerPrefs.HasKey( key );
	}

	/// <summary>
	/// Player-specific proxy to PlayerPrefs.DeleteKey(), key auto-prefixed by playerId.
	/// </summary>
	/// <param name="key">Key.</param>
	public void DeleteKey( string key, bool save = true ) {
		PlayerPrefs.DeleteKey( AsPerPlayerUniqueKey(key) );
		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy to PlayerPrefs.DeleteKey(), shared by all players on this device.
	/// </summary>
	/// <param name="key">Key.</param>
	public void DeleteSharedKey( string key, bool save = true ) {
		PlayerPrefs.DeleteKey( key );
		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy of PlayerPrefs.DeleteAll(). Nukes everything. Be careful.
	/// </summary>
	public void DeleteAll( bool save = true ) {
        Dictionary<string, string> persistentPrefs = new Dictionary<string, string>();
        foreach ( string persistentKey in PERSISTENT_ON_DELETE_KEYS ) {
            persistentPrefs.Add(persistentKey, GetString(persistentKey));
        }

        PlayerPrefs.DeleteAll();

        foreach ( KeyValuePair<string, string> persistentKVPair in persistentPrefs ) {
            SetString(persistentKVPair.Key, persistentKVPair.Value);
        }

		if ( save ) Save();
	}

	/// <summary>
	/// Direct proxy of PlayerPrefs.Save(). Forces flushing of preferences to disk, normally done only on shutdown.
	/// </summary>
	public void Save() {
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Sets the playerId used to prefix player-specific preference keys.
	/// </summary>
	/// <param name="playerId">Player identifier.</param>
	public void SetPlayerId( string playerId ) {
		_playerId = playerId;
	}

	/// <summary>
	/// Prefixes the passed key with the current playerId, as retrieved from the global domain. If PlayerId is 
	/// null or blank, the PREFIX_SEPARATOR is excluded and the return value defaults to 'key'.
	/// </summary>
	/// <returns>The unique player key.</returns>
	/// <param name="key">Key.</param>
	private string AsPerPlayerUniqueKey( string key ) {
		return String.IsNullOrEmpty( _playerId ) ? key : _playerId + PREFIX_SEPARATOR + key;
	}

	private long ToLong( string value ) {
		if ( String.IsNullOrEmpty(value) ) return 0L;
		try {
			return Convert.ToInt64( value );
		} catch {
			this.LogWarning ("Could not convert value '" + value + "' to long, defaulting to 0");
		}
		return 0L;
	}
}


