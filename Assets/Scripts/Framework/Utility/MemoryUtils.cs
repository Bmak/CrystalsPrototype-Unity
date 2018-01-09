using System;
using UnityEngine;
using UnityEngine.Profiling;
using System.Runtime.InteropServices;


// Default - Not necessary, here as a reminder.
[StructLayout(LayoutKind.Sequential)]
public struct MemoryInfo
{
	public int resident_size;
	public int virtual_size;
}

public static class MemoryUtils {

	// Calls to MemoryUtils.mm
	#if UNITY_IPHONE && !UNITY_EDITOR
	/*[DllImport ("__Internal")]
	private static extern void MemoryUtils_GetMemoryInfo( out MemoryInfo memoryInfo );
    [DllImport ("__Internal")]
    private static extern long MemoryUtils_GetStorageInfo();*/
    #elif UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject _currentActivity;
	#endif

    private static readonly float MEMORY_INFO_CACHE_SECONDS = 3.0f;
    private static float _nextMemoryInfoUpdateTime;
    private static MemoryInfo _memoryInfo;

    #if UNITY_ANDROID && !UNITY_EDITOR
    static MemoryUtils() {        
        // Save off reference for future JNI calls
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
        _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"); 
    }
    #endif

	public static MemoryInfo GetMemoryInfo()
	{
        // Return cached value, if available
        if ( Time.realtimeSinceStartup < _nextMemoryInfoUpdateTime )
            return _memoryInfo;

        _nextMemoryInfoUpdateTime = Time.realtimeSinceStartup + MEMORY_INFO_CACHE_SECONDS;

		#if UNITY_IPHONE && !UNITY_EDITOR
			//MemoryUtils_GetMemoryInfo( out _memoryInfo );
		#elif UNITY_ANDROID && !UNITY_EDITOR
			_memoryInfo.resident_size = _currentActivity.Call<int>("getMemoryInfo");
			_memoryInfo.virtual_size = 0;
		#elif UNITY_EDITOR
			_memoryInfo.resident_size = (int)Profiler.usedHeapSize;
			_memoryInfo.virtual_size = 0;			
		#else
			_memoryInfo.resident_size = 0;
			_memoryInfo.virtual_size = 0;
		#endif

		return _memoryInfo;
	}

    /// <summary>
    /// Gets the available device storage in bytes.
    /// </summary>
    /// <returns>The available storage in bytes, or -1 if no data is available for this platform</returns>
    public static long GetAvailableStorageInBytes()
    {
        long result = -1;
        #if UNITY_IPHONE && !UNITY_EDITOR
            //result = MemoryUtils_GetStorageInfo();   
        #elif UNITY_ANDROID &&! UNITY_EDITOR
            result = _currentActivity.Call<long>("getAvailableFreeSpaceBytes");
        #endif
        return result;
    }
}
