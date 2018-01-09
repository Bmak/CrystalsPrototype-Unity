using System.Runtime.InteropServices;

public static class SecurityUtils {
	// Extern to SecurityUtils.mm
	#if UNITY_IOS && !UNITY_EDITOR
/*	private const string LIBRARY_NAME = "__Internal";
	[DllImport (LIBRARY_NAME)]
	private static extern string SecurityUtils_GetKeychainStringForKey( string key, string identifier, string accessGroup );
	[DllImport (LIBRARY_NAME)]
	private static extern bool SecurityUtils_AddKeychainStringForKey( string value, string key, string identifier, string accessGroup );
	[DllImport (LIBRARY_NAME)]
	private static extern bool SecurityUtils_RemoveKeychainKey( string key, string identifier, string accessGroup );*/
	#endif	

	/// <summary>
	/// Finds the given key in the keychain with the given identifier and accessGroup.
	/// </summary>
	/// <returns>string value if found, null otherwise</returns>
	/// <param name="key">string key to find in keychain</param>
	/// <param name="identifier">Unique identifier, for identification within the keychain</param>
	/// <param name="accessGroup">Access group, used to share keychain data across applications. Requires configuration of keychain-access-groups.</param>    
	public static string GetKeychainStringForKey( string key, string identifier, string accessGroup = null )
	{
		#if UNITY_IOS && !UNITY_EDITOR
			return null;//SecurityUtils_GetKeychainStringForKey(key, identifier, accessGroup);
		#else
			return null;
		#endif
	}

	/// <summary>
	/// Adds the given key/value pair to the keychain with the given identifier and accessGroup.
	/// Note: This call will fail if the key already exists in the keychain.
	/// </summary>
	/// <returns><c>true</c>, if keychain value was added (or not supported on this platform), <c>false</c> otherwise.</returns>
	/// <param name="value">string value to add to keychain</param>
	/// <param name="key">string key to add to keychain</param>
	/// <param name="identifier">Unique identifier, for identification within the keychain</param>
	/// <param name="accessGroup">Access group, used to share keychain data across applications. Requires configuration of keychain-access-groups.</param>
	public static bool AddKeychainStringForKey( string value, string key, string identifier, string accessGroup = null )
	{
		#if UNITY_IOS && !UNITY_EDITOR
			return true;//SecurityUtils_AddKeychainStringForKey(value, key, identifier, accessGroup);
		#else
			return true;
		#endif
	}

	/// <summary>
	/// Removes the given key from the keychain at the given identifier and accessGroup.
	/// </summary>
	/// <returns><c>true</c>, if keychain key was removed (or not supported on this platform), <c>false</c> otherwise.</returns>
	/// <param name="key">string key to remove from keychain</param>
	/// <param name="identifier">Unique identifier, for identification within the keychain</param>
	/// <param name="accessGroup">Access group, used to share keychain data across applications. Requires configuration of keychain-access-groups.</param>        
	public static bool RemoveKeychainKey( string key, string identifier, string accessGroup = null )
	{
		#if UNITY_IOS && !UNITY_EDITOR
			return true;//SecurityUtils_RemoveKeychainKey(key, identifier, accessGroup);
		#else
			return true;
		#endif
	}
}
