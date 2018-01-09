using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// The controller for showing Initial Loading Screen
/// This is where all initialization of the global Systems takes place before going to LoginState.
/// </summary>
public class InitialLoadState : State
{
	[Inject]
	private LocalPrefs _localPrefs;
	
	[Inject]
	private Client _client;

	[Inject]
	private Config _config;

	[Inject]
	private LocalizationManager _localizationManager;

	[Inject]
	private CoroutineCreator _coroutineCreator;

	[Inject]
	private TimeInfo _timeInfo;
    
	[Inject]
	private NguiTransitionController _nguiTransitionController;

	private static long _assetInitStartTime;

	//
	// Behaviors

	public override bool IsPreLogin()
	{
		return true;
	}

	//
	// State Controller

	public override bool SC_Enter(object transitionInfo, SC_Callback onCompleteCallback = null)
	{
		base.SC_Enter(transitionInfo, onCompleteCallback);

		// Halt initialization if the build is expired
		if (BuildExpired())
			return true;

		// Kick off first of several Initialization blocks leading up to Loading Screen display
		Primary();
		
		return true;
	}


	//
	// Phased Systems Initialization

	private void Primary()
	{
		new Initializer(
			// Chained initialization in named order
			new IInitializable[] {
			},			
			Secondary, // onSuccess			
			( error) => _lifecycleController.Reboot(), // onFail
			null, // no progress callback
			true, // true -> Serial initialization order (chained), false -> initialize in parallel
			Name + "::Primary" // Display name
		);

	}

	// Configuration has now been retrieved from the server.
	private void Secondary()
	{
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
        Metrics.Start( Name + "::Secondary" );
		#endif	

		InitializeDeviceInfo();

		Tertiary();
	}

	// NOTE: The AssetLoader (and indirectly the ResourceCache, for bundled assets) cannot be used until the AssetLoaderInitialized callback fires.
	private void Tertiary()
	{
		new Initializer(
			// instances to initialize in parallel
			new IInitializable[] {
				_nguiTransitionController,
			},			
			InitialLoadComplete, // onComplete
			( error) => _lifecycleController.Reboot(), // onFail
			(IInitializable instance, int totalCount, int currentIndex) => { // progress callback
			},
			false, // false -> Parallel initialization order (non-chained)
			Name + "::Tertiary" // Display name
		);
	}

	private void InitialLoadComplete()
	{
		// Re-initialize LocalizationManager now that we have loaded localization in phase above
		_localizationManager.Initialize();

		_coroutineCreator.StartCoroutine(ProceedToLoginState());
	}

	private IEnumerator ProceedToLoginState()
	{
		// Break previous call chain
		yield return null;

		// Notify the LifecycleController that the client is initialized
		_lifecycleController.ClientInitComplete();

		// Loading Screen ready. On to Login a player
//		AutoLoginTransitionInfo transitionInfo = new AutoLoginTransitionInfo { FinishedCallback = () => _stateController.EnterState<LoginState>() };
//		_stateController.EnterState<AutoLoginState>(transitionInfo);
		_stateController.EnterState<LoginState>();
	}

	private void InitializeDeviceInfo()
	{
		// Cap our framerate at 30FPS to reduce battery usage (or whatever is configured in GameData)
		// Note that we may use a different target frame rate on debug vs release builds, to better discern performance
		Application.targetFrameRate = (_client.IsDebug() ? _config.GetClientTargetFrameRateDebug() : _config.GetClientTargetFrameRate());

		Time.timeScale = _config.GetClientDefaultTimeScale();

		// Only enable multitouch in debug mode for the debug console
		Input.multiTouchEnabled = false;//_client.IsDebug();
	}

	/// <summary>
	/// Temporary method for disabling expired builds. Returns true if the build 
	/// has expired, indicating to calling code to halt execution. The asset
	/// cache will be cleared and an expiration notice will be shown to the player.
	///
	/// This code will likely be removed once the game goes into production, so
	/// leaving it here instead of somewhere more architecturally appropriate.
	/// </summary>
	private bool BuildExpired()
	{
		if (!_client.IsBuildExpired())
			return false;
		return true;
	}
}

