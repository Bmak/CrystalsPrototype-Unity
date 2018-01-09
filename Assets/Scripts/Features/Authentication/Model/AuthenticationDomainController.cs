using System;
using JsonFx.Json;

/// <summary>
/// Authentication domain controller interface to manipulate Auth data via services
/// </summary>
public class AuthenticationDomainController : ILoggable, ILifecycleAware, IDomainController
{
	[Inject]
	private LocalPrefs _localPrefs;
    [Inject]
    private PlayerRecordDomainController _playerRecordDomainController;
    [Inject]
    private PlayerService _playerService;

	void ILifecycleAware.Reset ()
	{
	}

	public AuthenticationDomainController ()
	{
		Init();
	}

	void IDomainController.Reset ()
	{
		Init();
	}

	private void Init ()
	{
	}

    public bool IsLoggedIn ()
    {
        return true;
    }

    public void DoLogin()
    {
    	RequestPlayerData();
    }

	private void RequestPlayerData()
	{
		_playerService.GetLocalPlayerData(RequestLocalPlayerDataSuccesess, RequestLocalPlayerDataSuccesess, RequestPlayerDataFailed);
//		network
//		_playerService.GetNetworkPlayerData(UnityEngine.SystemInfo.deviceUniqueIdentifier, RequestPlayerDataSuccesess, code => {});
	}

	private void RequestLocalPlayerDataSuccesess(string data)
	{
		_playerRecordDomainController.InitializeLocalPlayerRecord(data);
	}

    private void RequestPlayerDataSuccesess(string data)
    {
        _playerRecordDomainController.InitializePlayerRecord(data);
    }

	private void RequestPlayerDataFailed(ResponseCode code)
	{
		this.LogError("-> Get player data failed! Erroe code: " + code);
	}
}
