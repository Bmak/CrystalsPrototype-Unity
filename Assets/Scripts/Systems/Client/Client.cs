using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum Platform
{
	iOS,
	Android,
	WebGL
}

public class Client : ILoggable
{
    [Inject]
    private LocalPrefs _localPrefs;

	private bool _editorModeEnabled = false;

    private LogLevel _logLevel = LogLevel.ALL;

    [PostConstruct]
	private void PostConstruct() 
	{
	    _editorModeEnabled = Application.isEditor;
	}
	
	public void Initialize(Action initializeComplete)
	{
		LoadClientInfo(initializeComplete);
	}

	private void LoadClientInfo(Action clientInfoLoaded) 
	{
		if (clientInfoLoaded != null) {
			clientInfoLoaded();
		}
	}
    
    public LogLevel GetLogLevel() {
        return _logLevel;   
    }

	public bool GetEditorModeEnabled()
	{
		return _editorModeEnabled;
	}

	public Platform GetPlatform()
	{
#if UNITY_ANDROID
		return Platform.Android;
#elif UNITY_IPHONE
		return Platform.iOS;
#else //if UNITY_IPHONE
		return Platform.WebGL;
#endif
	}

	public bool IsDebug()
	{
		return true;
	}

	public bool IsRelease()
	{
		return !IsDebug();
	}

	public bool IsBuildExpired()
	{
		return !IsDebug();
	}
}

