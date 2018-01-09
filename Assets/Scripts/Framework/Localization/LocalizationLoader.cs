using System;
using System.Collections;
using UnityEngine;

public class LocalizationLoader : IInitializable, ILoggable {
	private InstanceInitializedCallback _initializedCallback;
	
	[Inject]
	private LocalizationDomainController _localizeDC;
	
	public void Initialize ( InstanceInitializedCallback initializedCallback = null )
	{
		_initializedCallback = initializedCallback;
		GetLocalizationBundles();
	}
	
	private void GetLocalizationBundles()
	{
		if (string.IsNullOrEmpty(_localizeDC.Localize.LatestLocalizationBundleVersion))
		{
			this.Log("The localization bundle version hasn't been assigned");
			return;
		}
		
		if (_localizeDC.Localize.HasCachedVersion(_localizeDC.Localize.LatestLocalizationBundleVersion))
		{
			Succeeded();
			return;
		}
		
		_localizeDC.GetLocalizeRPC(Succeeded, Failed);
	}
	
	private void Failed (ResponseCode errorCode)
	{
		
	}
	
	private void Succeeded ()
	{
		if ( _initializedCallback != null )
			_initializedCallback( this );    
	}
}
