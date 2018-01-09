using UnityEngine;
using System.Collections;

public class HomeBaseState : State
{
	[Inject]
	private IProvider<HomeBaseController> _controllerProvider;

	[Inject]
	private UISystem _uiSystem;

	[Inject]
	private Config _config;

	[Inject]
	private LocalPrefs _localPrefs;

	private HomeBaseController _controller;

	public override bool SC_Enter(object transitionInfo, SC_Callback onCompleteCallback = null)
	{	
		_uiSystem.SetBackdropCameraActive(false);

		_controller = _controllerProvider.Get();
		
		HomeBaseTransitionInfo lobbyTransitionInfo = transitionInfo as HomeBaseTransitionInfo;
		_controller.Initialize(lobbyTransitionInfo);

		return base.SC_Enter(transitionInfo, onCompleteCallback);
	}

	public override void SC_Exit(bool result, SC_Callback onCompleteCallback)
	{
		if (_controller != null) {
			_controller.Shutdown();
		}

		base.SC_Exit(result, onCompleteCallback);
	}
}