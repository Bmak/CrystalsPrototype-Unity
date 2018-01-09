using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LoginState : State
{
	[Inject]
	private PlayerService _playerService;

	[Inject]
	private CoroutineCreator _coroutineCreator;

	[Inject]
	private AuthenticationDomainController _authenticationDC;

    [Inject]
    private FacebookDomainController _facebookDC;

    [Inject]
	private UISystem _uiSystem;

	[Inject]
	private PlayerRecordDomainController _playerRecordDC;

	[Inject]
	private NetworkSystem _networkSystem;

	private float _progress;
	const float _numCallbacks = 2f;

	// Behavior Overrides

	public override bool IsPreLogin()
	{
		return true;
	}

	public override bool SC_Enter(object transitionInfo, SC_Callback onCompleteCallback = null)
	{
		// reset timeout to system settings
		Screen.sleepTimeout = SleepTimeout.SystemSetting;

//		local
		Primary();
		GetLocalData();

//		network
//		_playerService.AddOnActionDataReceived(OnDataReceived);
//        _networkSystem.OnConnectionSuccess = Primary;
//        _coroutineCreator.StartCoroutine(_networkSystem.Connect());

		return true;
	}

	private void OnDataReceived(NetworkSystem.GameResponse obj)
	{		
		UpdateProgress(_progress + 1f / _numCallbacks);
	}

	private void Primary()
	{
		_playerService.GetCommonData(CommonDataReceiveSuccesses, CommonDataReceiveFailed);
		_authenticationDC.DoLogin();
        _facebookDC.DoInitialize();


        _uiSystem.SetOnProceed(ProceedToBaseState);
	}

	private void GetLocalData()
	{
		UpdateProgress(1f);
    }

	private void CommonDataReceiveFailed(ResponseCode obj)
	{
	}

	private void CommonDataReceiveSuccesses(string data)
	{
	}

	private void ProceedToBaseState()
	{
		_uiSystem.DestroyInitialLoadingSplash();

		HomeBaseTransitionInfo transitionInfo = new HomeBaseTransitionInfo();
		_stateController.EnterState<HomeBaseState>(transitionInfo);
	}

	private void UpdateProgress(float progress)
	{
		_progress = progress;
		_uiSystem.UpdateProgressLoading(progress);
	}
}
