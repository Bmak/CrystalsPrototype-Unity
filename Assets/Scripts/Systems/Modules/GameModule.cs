public class GameModule : Module
{
    override protected void Configure()
    {       
        /* Injectables used by game code */
        Bind<MessageController>();
/*
		Bind<ErrorMessageController>();
        Bind<SystemMessageController>();

        Bind<GameEventDispatch>();
        Bind<DeepLinkHandler>();

        Bind<MessageCapacityHelper>();

        Bind<RequirementEvaluator>();
        Bind<FeatureLockController>();

		Bind<UnitSortingUtil>();
*/
		Bind<NumberFormatUtil>();
    }
}
