using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FileUtils : ILoggable
{

	private static readonly FileUtils _instance = new FileUtils(); // Only for logging
	public const string PATH_SEPARATOR = @"/";
	private static string _projectRoot = null;
	
	public static void RemoveReadOnly( string targetFile )
	{
		try {
			if (!File.Exists( targetFile )) return;
			FileAttributes attributes = File.GetAttributes( targetFile );
			File.SetAttributes( targetFile, attributes & ~FileAttributes.ReadOnly );
		} catch (Exception e) {
			_instance.LogError( "RemoveReadOnly exception: " + e.ToString () );
		}
	}

	public static void DeleteFile( string targetFile, bool force = true) 
	{
		try {
			if ( force ) RemoveReadOnly( targetFile );
			File.Delete( targetFile );
		} catch (Exception e) {
			_instance.LogError( "DeleteFile exception: " + e.ToString () );
		}
	}

	public static string ReadFile( string targetFile )
	{
		string result = null;

		try {
			result = File.ReadAllText( targetFile );

		} catch (Exception e) {
			_instance.LogWarning( "Error reading file '" + targetFile + "': " + e.ToString() );
		}
		                 
		return result;
	}

	public static void SetLastWriteTimeUtc( string fullFilePath, DateTime dateTime )
	{
		try
		{
			// touch it
			System.IO.File.SetLastWriteTimeUtc(fullFilePath, DateTime.UtcNow);
		}
		catch (Exception)
		{
			try
			{
				// just to make sure (supposedly works even if file is locked)
				(new FileInfo(fullFilePath)).LastWriteTimeUtc = DateTime.UtcNow;
			} catch (Exception e) {
				_instance.LogError( "SetLastWriteTimeUtc exception: " + e.ToString() );
			}
		}
	}
    
    public static void SetNoBackupFlag( string filePath ) {
        #if UNITY_IOS
        UnityEngine.iOS.Device.SetNoBackupFlag( filePath );        
        #endif
    }
    
    public static void DeletePath( string targetPath, bool recursive = true ) 
    {
        try {            
            Directory.Delete( targetPath, recursive );
        } catch (Exception e) {
            _instance.LogError( "DeletePath exception: " + e.ToString () );
        }
    }

	public static string GetProjectRoot()
	{
		return _projectRoot ?? (_projectRoot = Application.dataPath.Substring( 0, Application.dataPath.LastIndexOf( PATH_SEPARATOR ) ) );
	}

	/// <summary>
	/// Converts all backslashes in the string to forward slashes for uniformity of pathing.
	/// </summary>
	/// <param name="value">value to clean</param>
	public static string CleanPath( string value )
	{
		return value.Replace(@"\", PATH_SEPARATOR);
	}
	
	// Android apk compatibile file existence check
	public static bool Exists(string filePath)
	{
		if(string.IsNullOrEmpty(filePath)) { return false; }
		
		#if UNITY_ANDROID && !UNITY_EDITOR
		
		// jar:file:// protocol URI ?
		// Note: In this case we assume it's the app's own APK that's being referenced. 
		//		 In that case we trim off everything to the APK file name including protocol then treat as a relative path using the native code.
		const string jarProtocol = "jar:file://";
		const string apkExt = ".apk";
		if (filePath.StartsWith(jarProtocol, StringComparison.OrdinalIgnoreCase)) {
			int endOfAPKExt = filePath.IndexOf(apkExt, StringComparison.OrdinalIgnoreCase);
			if (endOfAPKExt < 0) {
				_instance.LogError ("FileUitls:Exists - Unsupported filePath parameter: " + filePath);
				return false;
			}
			endOfAPKExt += apkExt.Length; 
			filePath = filePath.Substring(endOfAPKExt + PATH_SEPARATOR.Length);
			// filePath should now be reduced to a path relative to the application APK
		} else {
			// Non-relative path?
			if (filePath.StartsWith(PATH_SEPARATOR)) {
				// Use traditional functions to determine file existance outside of the virtual APK path
				return File.Exists(filePath);
			}
		}
		
		// Test APK relative path using native function in the game's expansion jar
		bool result = false;
		try {			
	        using ( AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer") ) { 
		        AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
				result = currentActivity.Call<bool>( "assetExists", filePath );
			}
		} catch (Exception e ) {
			_instance.LogError("Exists( " + filePath + " ) exception: " + e.ToString());
		}
		return result;
		
		#else
		
		// Support URLs like file://blah... by stripping such protocols off
		const string protoTell = "://";
		int endProtoTell = filePath.IndexOf(protoTell, StringComparison.OrdinalIgnoreCase);
		if (endProtoTell > 0) {
			endProtoTell += protoTell.Length;
			filePath = filePath.Substring(endProtoTell);
		}
		
		return File.Exists( filePath );
		
		#endif
	}
	

	//
	// Joint path segments
	
	/// <summary>
	/// Join path segments with separators as needed. 
	/// </summary>
	/// <returns>End of string will be the last used segment with no additional separator which allows use for the file name as well.</returns>
	/// <param name="pathSegs">Path segements/file name to be joined</param>
	public static string JoinPaths(IList<string> pathSegs) 
	{
		string joined = "";
		foreach (string seg in pathSegs) {
			if (String.IsNullOrEmpty(seg)) { continue; }
			
			// Add separator slash if needed
			if ( !String.IsNullOrEmpty(joined) && !joined.EndsWith(PATH_SEPARATOR) && !seg.StartsWith(PATH_SEPARATOR) ) {
				joined += PATH_SEPARATOR;
			}
			// Append next segment
			joined += seg;
		}
		return joined;
	}
	
	// Alternate simpler call API taking 2 to 5 arguments
	// Note: Tried using params here to open this up to any number of arguments but Unity crashes on accessing the resulting array
	public static string JoinPaths(string path1, string path2, string path3 = null, string path4 = null, string path5 = null) 
	{
		string[] segs = { path1, path2, path3, path4, path5 };
		return JoinPaths( segs );
	}
}
