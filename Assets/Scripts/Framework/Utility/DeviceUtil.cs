using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class DeviceUtil 
{
	// cached values
	private static string _languageCode;
	private static string _countryCode;
	private static string _deviceId;
    private static int _uiThreadId = 0;

    // In Main method:
    // NOTE: Currently the main thread appears to always be 1 but doing it this way in case it changes in the future.
    // Do this when you start your application
    public static void SetUIThread()
    {
        _uiThreadId = GetCurrentThreadID();
    }
    public static int GetCurrentThreadID()
    {
        return System.Threading.Thread.CurrentThread.ManagedThreadId;
    }
    public static int GetUIThreadID()
    {
        return _uiThreadId;
    }
    public static bool IsUIThread()
    {
        return _uiThreadId == GetCurrentThreadID();
    }

#if UNITY_IPHONE
	/*private const string LIBRARY_NAME = "__Internal";
	
    [DllImport (LIBRARY_NAME)]
    private static extern string getCountryCode();
    
    [DllImport (LIBRARY_NAME)]
    private static extern string getLanguageCode();

    [DllImport (LIBRARY_NAME)]
    private static extern void crashApplication();

	// Used for keychain persistence of identifierForVendor
	private static readonly string DEVICE_ID_KEYCHAIN_IDENTIFIER = "com.ea.starwarscapital.bv";
	private static readonly string DEVICE_ID_KEYCHAIN_KEY = "persistedId";
	// Persisted deviceId, if already read from Keychain
	private static string _persistedDeviceId;
	// Current identifierForVendor, if already retrieved from system API
	private static string _identifierForVendor;*/
#endif

	public static string GetDeviceLanguageCode() 
	{
		// return cached value if available
		if ( !String.IsNullOrEmpty(_languageCode) ) return _languageCode;

#if !UNITY_EDITOR && UNITY_IPHONE
		//_languageCode = getLanguageCode();
		_languageCode = "en";
#elif !UNITY_EDITOR && UNITY_ANDROID
/*
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) { 
	        AndroidJavaObject daActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
			_languageCode = daActivity.CallStatic<string>("getLanguageCode");
		}
*/
		_languageCode = "en";
#else
		_languageCode = "en";
#endif
		return _languageCode;
	}
	
	public static string GetDeviceCountryCode() 
	{

		// return cached value if available
		if ( !String.IsNullOrEmpty(_countryCode) ) return _countryCode;

#if !UNITY_EDITOR && UNITY_IPHONE
		//_countryCode = getCountryCode();
		_countryCode = "US";
#elif !UNITY_EDITOR && UNITY_ANDROID
/*
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) { 
	        AndroidJavaObject daActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
			_countryCode = daActivity.CallStatic<string>("getCountryCode");
		}
*/
		_countryCode = "US";
#else
		_countryCode = "US";
#endif
		return _countryCode;
	}
	
	public static string GetDeviceId()
	{
		// return cached value if available
		if ( !String.IsNullOrEmpty(_deviceId) ) return _deviceId;

	#if UNITY_ANDROID && !UNITY_EDITOR        
	        // Android device ID logic - try getting the Android ID
	        // first - if that fails, try the MAC ID (unavailable
	        // if WiFi not enabled).
			_deviceId = GetAndroidId();
			if (string.IsNullOrEmpty(_deviceId)) {
	            _deviceId = GetAndroidMacAddress();
	        }
	
	        // MD5 hash the selected android ID
	        if (!string.IsNullOrEmpty(_deviceId)) {
	            _deviceId = MD5Hash(_deviceId);
	        }
	#elif UNITY_IOS && !UNITY_EDITOR
			// Use deviceId from Keychain, if it exists
			_deviceId = GetPersistedDeviceId();
			if ( String.IsNullOrEmpty( _deviceId ) ) {
				_deviceId = GetIdentifierForVendor();
				// Persist deviceID to keychain
				PersistDeviceId( _deviceId );
			}
	#else
	        _deviceId = SystemInfo.deviceUniqueIdentifier;
	#endif
		
		return _deviceId;
	}

	private static void PersistDeviceId( string deviceId )
	{
		#if UNITY_IOS && !UNITY_EDITOR
/*		if (!SecurityUtils.AddKeychainStringForKey( deviceId, DEVICE_ID_KEYCHAIN_KEY, DEVICE_ID_KEYCHAIN_IDENTIFIER )) {
			Log.Error("DeviceUtil: Failed to persist deviceID!");
        }*/
        #endif		
    } 
    
    private static string GetPersistedDeviceId()
	{
		#if UNITY_IOS && !UNITY_EDITOR
/*			if (!String.IsNullOrEmpty( _persistedDeviceId )) return _persistedDeviceId;
			_persistedDeviceId = SecurityUtils.GetKeychainStringForKey( DEVICE_ID_KEYCHAIN_KEY, DEVICE_ID_KEYCHAIN_IDENTIFIER );
			return _persistedDeviceId;*/
			return null;
		#else
			return null;
		#endif
	}

	public static void ClearPersistedDeviceId()
	{
		#if UNITY_IOS && !UNITY_EDITOR
/*		if (!SecurityUtils.RemoveKeychainKey( DEVICE_ID_KEYCHAIN_KEY, DEVICE_ID_KEYCHAIN_IDENTIFIER )) {
			Log.Error("DeviceUtil: Failed to remove deviceID!");
		}*/
        #endif	
    }

	public static string GetIdentifierForVendor()
	{
		#if UNITY_IOS && !UNITY_EDITOR
/*			if (!String.IsNullOrEmpty( _identifierForVendor ) ) return _identifierForVendor;
			_identifierForVendor = SystemInfo.deviceUniqueIdentifier;
			return _identifierForVendor;*/
			return string.Empty;
		#else
			return string.Empty;
		#endif
	}

    public static string GetAndroidId()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            using (AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
            using (AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure"))
            {
                return secure.CallStatic<string>("getString", contentResolver, "android_id");
            }
        } catch (AndroidJavaException aje) {
            Log.Exception(aje);
            return null;
        }
#else
        return null;
#endif
    }

    private static string GetAndroidMacAddress()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            using (AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject mWiFiManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            using (AndroidJavaObject connectionInfo = mWiFiManager.Call<AndroidJavaObject>("getConnectionInfo"))
            {
                return connectionInfo.Call<string>("getMacAddress");
            }
        } catch (AndroidJavaException aje) {
            Log.Exception(aje);
            return null;
        }
#else
        return null;
#endif
    }

    private static string MD5Hash(string source)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
            StringBuilder hash = new StringBuilder();
            
            for (int i = 0; i < data.Length; i++) {
                hash.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return hash.ToString();
        }
    }
	
    public static bool TryGetMajorOperatingSystemVersion (out int version)
    {
        version = -1;
		
        Regex majorVersionPattern = new Regex("[0-9]+$");
        Match majorVersionMatch = majorVersionPattern.Match(SystemInfo.operatingSystem.Split('.')[0]);
        if (majorVersionMatch == null)
        {
            return false;
        }
		
        return System.Int32.TryParse(majorVersionMatch.Value, out version);
    }

    public static void CrashApplication()
    {
        #if UNITY_IPHONE && !UNITY_EDITOR
        //crashApplication();
        #elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) { 
                AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                currentActivity.Call("crashApplication");
            }
        #else
        Log.Info("CrashApplication() not implemented on this platform");
        #endif
    }

	public static string GetVendorId()
	{
		#if UNITY_IPHONE && !UNITY_EDITOR
		return UnityEngine.iOS.Device.vendorIdentifier;
		#else
		return null;
		#endif
	}

	public static string GetAdvertisingId()
	{
		#if UNITY_IPHONE && !UNITY_EDITOR
		return UnityEngine.iOS.Device.advertisingIdentifier;
		#else
		return null;
		#endif
	}
}
