using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System;

/// <summary>
/// LocalizationConfig singleton that provides access to locazation key constants from the game server.
/// Fields, default values, and getters are specified in LocalizationConfig.Fields.cs
/// </summary>
public partial class LocalizationConfig : ILifecycleAware, ILoggable 
{	
	private bool _initialized = false;
	
	public void Reset()
	{ 
		_initialized = false; 
	}
	
	public void Initialize( List<ConfigEntry> configEntries, bool debugMode = false )
	{
		
		if ( _initialized ) {
			this.Log("Already initialized, skipping LocalizationConfig reprocessing.");
			return;
		}
		
		ConfigBuilder<LocalizationConfig> configBuilder = new ConfigBuilder<LocalizationConfig>( this );
		configBuilder.Populate( configEntries );
		
		_initialized = true;
	}
}
