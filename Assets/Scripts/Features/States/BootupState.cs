using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootupState : State
{
	[Inject]
	private Client _client;

	[Inject]
	private MemoryManager _memoryManager;

	[Inject]
	private AudioSystem _audioSystem;

	[Inject]
	private NetworkSystem _networkSystem;

	[Inject]
	private DOTweenManager _dotweenManager;

	[Inject]
	private CoroutineCreator _coroutineCreator;

	[Inject]
	private UISystem _uiSystem;

	[Inject]
	private UIGenericInputHandler _uiGenericInputHandler;

	[Inject]
	private MonoBehaviourEventNotifierSystem _monoBehaviourEventNotifierSystem;
    


	[Inject]
	private LocalPrefs _localPrefs;

	//
	// Behavior Overrides

	public override bool IsPreLogin()
	{
		return true;
	}

	//
	// State Controller

	public override bool SC_Enter(object transitionInfo, SC_Callback onCompleteCallback = null)
	{
		base.SC_Enter(transitionInfo, onCompleteCallback);

		// Initialize the UI system as early as possible.
		// This creates the Camera and loads/displays the backdrop texture
		_uiSystem.InitializeCamera();

		// Kick off first of several Initialization blocks
		Primary();

		return true;
	}

	//
	// Phased Systems Initialization


	// LocalizationManager, UISystem, AudioSystem

	private void Primary()
	{
		// Record Main thread identity
		DeviceUtil.SetUIThread();

		Secondary();
	}

	private void Secondary()
	{

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
        Metrics.Start( "BootupState::Secondary" );
		#endif
		new Initializer(
			// Chained initialization in named order
			new IInitializable[] {
				_networkSystem,
				_audioSystem,
				_dotweenManager,
			},
			Tertiary, // onSuccess
			(error) => { 
				this.LogError("Secondary initialization failed: " + error); // onFail
				_lifecycleController.Reboot();
				//_errorMessageController.ShowInitializationErrorMessage(ErrorCodes.BOOTUP_SECONDARY,error);
			},
			null, // no progress callback
			false, // false -> Parallel initialization order (non-chained)
			Name + "::Secondary" // Display name
		);

	}

	// Logging, Asset Unloader, Camera Raycaster, Camera Transition Manager & Low Mem Sim

	private void Tertiary()
	{

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
        Metrics.End( "BootupState::Secondary" );
		#endif

		new Initializer(
            // Chained initialization in named order
			new IInitializable[] {
				_uiGenericInputHandler,
				_monoBehaviourEventNotifierSystem
			},
			() => { // onSuccess
				Quaternary();
			}, 
			( error) => { 
				this.LogError("Tertiary initialization failed: " + error); // onFail
				_lifecycleController.Reboot();
				//_errorMessageController.ShowInitializationErrorMessage(ErrorCodes.BOOTUP_TERTIARY,error);
			},
			null, // no progress callback
			false, // false -> Parallel initialization order (non-chained)
			Name + "::Tertiary" // Display name
		);
	}

	// (Debug Only) Build Info

	private void Quaternary()
	{
		#if !PRODUCTION
//		if ( _client.GetBuildInfoEnabled() ) _buildInfo.Initialize();
		#endif

		_coroutineCreator.StartCoroutine(BootupCompleted());
	}

	// Boot strapping complete.
	// On to Login!

	private IEnumerator BootupCompleted()
	{
		// Break previous call chain
		yield return null;

		_stateController.EnterState<InitialLoadState>(null);
	}
}
