using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System;

public partial class ConfigEntry
{
	public string Key = "";
	public string Value = "";
}

/// <summary>
/// Config singleton that provides access to GameConstants from the game server.
/// Fields, default values, and getters are specified in Config.Fields.cs
/// </summary>
public partial class Config : ILifecycleAware, ILoggable 
{	

	private bool _initialized = false;
    private List<ConfigEntry> _cachedConfigEntries;
    private bool _hasOverridesApplied = false;

	public void Reset()
	{ 
		_initialized = false; 
	}

	public void Initialize( List<ConfigEntry> configEntries, bool debugMode = false )
	{
        if ( _initialized ) {
            this.Log("Already initialized, skipping config reprocessing.");
            return;
        }

        _cachedConfigEntries = configEntries;

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( GetType().Name + ":Initialize" );
		#endif	

		ConfigBuilder<Config> configBuilder = new ConfigBuilder<Config>( this );
		configBuilder.Populate( configEntries );

		_initialized = true;
	
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name + ":Initialize" );
		#endif	
	}

    public void ApplyOverrides(Dictionary<string, string> overrideConfigEntries)
    {
        List<ConfigEntry> convertedOverrideConfigEntries = new List<ConfigEntry>();
        foreach (string key in overrideConfigEntries.Keys) {
            convertedOverrideConfigEntries.Add(new ConfigEntry() { Key = key, Value = overrideConfigEntries[key] });
        }
        ApplyOverrides(convertedOverrideConfigEntries);
    }

    public void ApplyOverrides(List<ConfigEntry> overrideConfigEntries)
    {
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
        Metrics.Start(GetType().Name + ":SetOverrides");
        #endif

        // Overrides are only valid if there was initially a default config entry value.
        // This is mainly to make sure we always get back to the default configuration whenever the player changes accounts.
        HashSet<string> validConfigKeys = new HashSet<string>();
        foreach (ConfigEntry entry in _cachedConfigEntries)
        {
            validConfigKeys.Add(entry.Key);
        }
        List<ConfigEntry> validOverrideConfigs = new List<ConfigEntry>();
        foreach (ConfigEntry entry in overrideConfigEntries)
        {
            if (validConfigKeys.Contains(entry.Key))
            {
                validOverrideConfigs.Add(entry);
            }
        }

        ConfigBuilder<Config> configBuilder = new ConfigBuilder<Config>(this);
        if (_hasOverridesApplied)
        {
            configBuilder.Populate(_cachedConfigEntries);
        }
        _hasOverridesApplied = true;
        configBuilder.Populate(validOverrideConfigs);

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
        Metrics.End(GetType().Name + ":SetOverrides");
        #endif
    }
}
