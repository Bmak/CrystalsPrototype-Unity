using UnityEngine;

public class ServiceModule : Module
{
    override protected void Configure()
    {
        Bind<PlayerService>();
		Bind<LocalizationService>();
/*
        Bind<BootStrapLoader>();
        Bind<MetaDataLoader>();
        Bind<GameDataLoader>();

        Bind<ServiceErrorHandler>();
		Bind<ResponseInterceptor>();

        // RPC Services
		if ( Application.platform == RuntimePlatform.IPhonePlayer ) {
			Bind<IAuthService>().To<AuthGameCenterService>();
		} else if ( Application.platform == RuntimePlatform.Android || Application.isEditor ) {
			Bind<IAuthService>().To<AuthGooglePlayService>();
		}

		Bind<AuthLogOutService>();

		Bind<IAuthService>( "authGuestService" ).To<AuthGuestService>();

		Bind<AuthAgeGameService>();
        Bind<GeolocationService>();
        Bind<MatchMakingService>();
        Bind<LeaderboardService>();
        Bind<BattleService>();
        Bind<GameService>();
        Bind<MetadataService>();
        Bind<PlayerService>();
		Bind<StoreService>();
		Bind<UnitService>();
        Bind<BattleSupportService>();
        Bind<ContextualMessageInterceptor>();
		Bind<CampaignStatusUpdateMessageInterceptor>();
		Bind<StoreInterceptor>();
		Bind<InboxInterceptor>();
        Bind<EventStatusUpdateInterceptor>();
        Bind<CraftingService>();
        Bind<AllyService>();
        Bind<ChallengeService>();
		Bind<ChallengeInterceptor>();
		Bind<NimbleSessionIdExtractor>();
        Bind<TelemetryService>();
        Bind<DailyActionCapStatusInterceptor>();
        Bind<CooldownStatusInterceptor>();
		Bind<WarCampaignService>();

		Bind<MTXProcessingSystem>().In(Scope.SINGLETON);
		Bind<MTXFallbackContext>().In(Scope.SINGLETON);
		Bind<MTXFallbackHandler>().In(Scope.SINGLETON);
		Bind<MTXProcessingContext>().In(Scope.SINGLETON);
		Bind<MTXErrorHandler>().In(Scope.SINGLETON);
#if UNITY_EDITOR
		Bind<PrototypeSynergyService>().To<SynergyEmulation>();
        Bind<SynergyEmulationTelemetryManager>();
        Bind<Rest.CoreModule>();
        Bind<Rest.RiverModule>();            
        Bind<Rest.LogEventCache>();
#else
		Bind<PrototypeSynergyService>().To<SynergyProxy>().In(Scope.SINGLETON);
#endif

		Bind<NimbleListenerContext>();
*/
    }
}
