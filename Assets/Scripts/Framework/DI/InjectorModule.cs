

public class InjectorModule : Module
{
    private IInjector _injector;
    private IInstantiator _instantiator;
    public InjectorModule( IInjector injector, IInstantiator instantiator )
    {
        _injector = injector;
        _instantiator = instantiator;
    }

    override protected void Configure()
    {
        Bind<IInjector>().ToInstance( _injector );
        Bind<IInstantiator>().ToInstance( _instantiator );
    }
}
