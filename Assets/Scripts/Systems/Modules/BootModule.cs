
public class BootModule : Module
{
    override protected void Configure()
    {
        Install( new SystemModule() );
        Install( new AssetModule() );
        Install( new DomainModule() );
        Install( new ServiceModule() );
        Install( new ControllerModule() );
        Install( new StateModule() );
        Install( new GameModule() );
        Install( new PluginModule() );
    }
}
