
public class SystemModule : Module
{

    override protected void Configure()
    {
        Bind<LifecycleController>();
        Bind<TimeInfo>();
        Bind<Client>();
        Bind<Config>();
/*
        Bind<PushNotificationSystem>();
		Bind<IPushNotificationModule>("energy-push-notification").To<EnergyPushNotificationModule>();
		Bind<IPushNotificationModule>("events-push-notification").To<EventsPushNotificationModule>();

		#if !PRODUCTION
        Bind<Console>();
		Bind<IConsoleModule>("console-default").To<ConsoleTestingHarness>();
		Bind<IConsoleModule>("console-server").To<ConsoleServerModule>();
        Bind<DebugMenu>();        
		Bind<BuildInfo>();
		Bind<PerformanceInfo>();
		#endif
*/
        // Localization
		Bind<LocalizationConfig>();
		Bind<LocalizationLoader>();
		Bind<LocalizationManager>();
//		Bind<LocalizationLogger>();
		Bind<LocalizationDO>().In( Scope.PROTOTYPE );

//		Bind<FontManager>();

        // Logging
        Bind<LogDispatcher>();
/*		Bind<Logger>();
        Bind<RemoteLogger>();
        Bind<ClientLogger>();*/

        // Misc
        Bind<MemoryManager>();
/*		Bind<TelemetryDispatch>();
        Bind<TelemetrySessionTracker>();*/

        // UI
        Bind<UISystem>();
		Bind<UIGenericInputHandler>().In(Scope.SINGLETON);
//		Bind<NavigationDispatch>();

		// Network
		Bind<NetworkSystem>();

		// Audio
		Bind<AudioSystem>();

        /* Utilities */
        Bind<CoroutineCreator>();
        Bind<LocalPrefs>();
//		Bind<Profile>();
		Bind<ProfilerUtil>();
		
		Bind<IniLoader>();
		Bind<MonoBehaviourEventNotifierSystem>().In(Scope.SINGLETON);

        /* Time */
        Bind<TimeScaleManager>();
        Bind<CountDownTimer>().In(Scope.PROTOTYPE);
        Bind<SpecificCountDownTimer>().In(Scope.PROTOTYPE);
    }
}
