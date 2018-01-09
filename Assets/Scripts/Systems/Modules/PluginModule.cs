using UnityEngine;

public class PluginModule : Module
{
	override protected void Configure()
	{
/*
		if ( Application.isEditor ) {
			Bind<IGameServicePlugin>().To<EditorGameServicePlugin>();
		} else if ( Application.platform == RuntimePlatform.IPhonePlayer ) {
			Bind<IGameServicePlugin>().To<GameCenterPlugin>();
		} else if ( Application.platform == RuntimePlatform.Android ) {
			Bind<IGameServicePlugin>().To<GooglePlayGameServicesPlugin>().ObjectName( "GooglePlayGameServicesManager" );
		}
		
		if ( Application.isEditor ) {
			Bind<IEmailAndSmsPlugin>().To<EmailAndSmsEditorPlugin>();
		} else if ( Application.platform == RuntimePlatform.IPhonePlayer ) {
			Bind<IEmailAndSmsPlugin>().To<EmailAndSmsiOSPlugin>();
		} else if ( Application.platform == RuntimePlatform.Android ) {
			Bind<IEmailAndSmsPlugin>().To<EmailAndSmsAndroidPlugin>();
		} else {
			Bind<IEmailAndSmsPlugin>().To<EmailAndSmsNoOpPlugin>();
		}
		
		Bind<EmailAndSmsPluginHelper>();
		
		if ( Application.isEditor ) {
			Bind<ILocalPushNotificationPlugin>().To<LocalPushNotificationNoOpPlugin>();
		} else if ( Application.platform == RuntimePlatform.IPhonePlayer ) {
			Bind<ILocalPushNotificationPlugin>().To<LocalPushNotificationiOSPlugin>();
		} else if ( Application.platform == RuntimePlatform.Android ) {
			Bind<ILocalPushNotificationPlugin>().To<LocalPushNotificationAndroidPlugin>();
		} 
		
		Bind<HockeyAppPlugin>();		
		Bind<UpsightPlugin>();		
        Bind<UpsightController>();
*/
		Bind<NativeStore>();		
	    Bind<DOTweenManager>();
	}
}
