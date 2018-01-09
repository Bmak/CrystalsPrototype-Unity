
public class StateModule : Module
{
    override protected void Configure()
    {
        /* Initialization */
    
        Bind<BootupState>();
        Bind<InitialLoadState>();    
        Bind<LoginState>();
    }
}
