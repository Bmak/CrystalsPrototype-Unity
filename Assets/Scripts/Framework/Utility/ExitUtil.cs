using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitUtil : ILoggable {

    private static readonly ExitUtil _instance = new ExitUtil(); // Only for logger

	#pragma warning disable 0414
	private static readonly string BATCH_MODE_ARGUMENT_NAME = "-batchmode";
	#pragma warning restore 0414

    public static void CleanExit() {
        _instance.Log("CleanExit()");
        Application.Quit();
    }

    /// <summary>
    /// Android behaves strangely in some cases on Application.Quit(),
    /// sometimes showing error messages and other times leaving the app
    /// in the task list (in an undefined state) after exit.
    /// This method forcefully kills the process that holds the mono runtime,
    /// which is like a sledgehammer, but seems to work.
    /// </summary>
    public static void HardExit() {
        _instance.Log("HardExit()");
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    /// <summary>
    /// Exits the editor with the specified exit code.
    ///
    /// If the editor is running in batch mode, the Unity process will 
    /// exit with:
    ///     exitCode == 0 -> success
    ///     exitCode != 0 -> error condition with given code
    /// 
    /// If the editor is running in graphical mode, script execution 
    /// will be halted instead.
    /// </summary>
    /// <param name="exitCode">Exit code.</param>
    public static void ExitEditor( int exitCode = 0 ) {        
        #if UNITY_EDITOR
			// If we are running in batch mode, fully exit Unity with exitCode.
            if ( IsBatchMode() ) { 
                _instance.Log("ExitEditor( " + exitCode + " )");
                EditorApplication.Exit( exitCode ); 
            } else { // We are in the graphical editor, stop script execution. exitCode is ignored.
                _instance.Log("ExitEditor( " + exitCode + " ) isPlaying = false");
                UnityEditor.EditorApplication.isPlaying = false;
            }

        #else
            _instance.LogWarning("ExitEditor(): no-op, called in non-editor environment.");
        #endif
    }

	/// <summary>
	/// Indicates whether Unity is executing in batch/headless mode, such as during
	/// a Jenkins build or Unity process invoked from the command line.
	/// </summary>
	/// <returns><c>true</c> if is batch mode; otherwise, <c>false</c>.</returns>
	public static bool IsBatchMode()
	{
		#if UNITY_EDITOR
		try {
			foreach ( string arg in Environment.GetCommandLineArgs() ) {
				if ( !String.IsNullOrEmpty( arg ) && arg.ToLower().Contains ( BATCH_MODE_ARGUMENT_NAME ) ) return true;
			}
		} catch ( Exception e ) {
			_instance.LogError("Cannot determine batch mode status: " + e.ToString() );
		}
		#endif

		return false;
	}

	public static void MinimizeAndroidApplication()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		_instance.Log("MinimizeAndroidApplication()");
		AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
		activity.Call<bool>("moveTaskToBack", true);    
		#endif
	}

}
