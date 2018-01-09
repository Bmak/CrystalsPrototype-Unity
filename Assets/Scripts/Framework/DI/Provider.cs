public class Provider<T> : IProvider<T>, ILoggable 
{

    [Inject]
    private IInjector _injector;

    public T Get( string name = null, string objectName = null ) {
        this.LogTrace("Get( " + typeof(T).Name + " )", LogCategory.INJECTOR);
        return _injector.Get<T>( name, objectName );
    }
}
