using System;
using UnityEngine;

public class NativeStore : ILoggable
{
	[Inject]
	private Config _config;
	
	[Inject]
	private Client _client;	

	public void Open()
	{
		Application.OpenURL( GetRateThisAppUrl() );
	}

	public string GetRateThisAppUrl()
	{
		Platform platform = _client.GetPlatform();

		if ( platform == Platform.iOS )
			return _config.GetRateThisAppAppleUrl();

		if ( platform == Platform.Android )
			return _config.GetRateThisAppGoogleUrl();

		// Fall-through. Should never happen.
		return _config.GetRateThisAppAppleUrl();	
	}
}

