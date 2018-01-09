using UnityEngine;
using System.Collections;

public class EntryPoint : ILoggable
{
	private IInjector _injector;
	private LifecycleController _lifecycleController;
	private Client _client;
	private TimeInfo _timeInfo;

	public void Initialize()
	{
		_injector = new Injector.Builder()
			.Instantiator(new Instantiator())
			.Module(new BootModule())
			.Build();
	}

	public void Execute()
	{
#if METRICS_ENABLED
		Metrics.Clear();
#endif

		// Reset internal time/stopwatch as we may have just rebooted
		TimeUtil.Reset();

#if METRICS_ENABLED
		// This hook times startup to HomeBase, and is not closed until  
		// the player is given UI control in the HomeBase screen		
		Metrics.Start("Boot:HomeBase");
#endif

#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( "Initialize:Injector" );
#endif
		if (_injector == null) Initialize();
#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( "Initialize:Injector" );
#endif

#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( "Initialize:LifecycleController" );
#endif

		// LifecycleController must be instantiated first so that
		// it can receive OnInstantiate events from IInstantiator
		_lifecycleController = _injector.Get<LifecycleController>();

		_client = _injector.Get<Client>();
		_client.Initialize(ClientLoadCompleted);
	}

	private void ClientLoadCompleted()
	{
		this.LogOutput("Client: " + _client.ToString());
		// Set log level as early as possible
		Logging.SetLogLevel(_client.GetLogLevel());
		//Logging.SetLogCategories(_client.GetLogCategories());
		bool enableTimestamp = false;
#if !PRODUCTION
		enableTimestamp = true;
#endif
		Logging.SetDateTimeEnabled(enableTimestamp);

		// Log bindings for debugging purposes
		// _injector.Info();

		// Set startup timestamp and schedule update coroutine 
		_timeInfo = _injector.Get<TimeInfo>();
		_timeInfo.Initialize();

		// Create NimbleBridge_CallbackHelper and register the instance for reset/destroy on reboot
		//_lifecycleController.Register(NimbleBridge_CallbackHelper.Get());

		// Initialize the boot sequence
		_lifecycleController.Initialize();

#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( "Initialize:LifecycleController" );
#endif
	}
}

