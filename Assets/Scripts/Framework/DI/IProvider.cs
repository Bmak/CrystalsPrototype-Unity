public interface IProvider<T>
{
    T Get( string name = null, string objectName = null );
}
