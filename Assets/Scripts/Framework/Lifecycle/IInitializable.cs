/// <summary>
/// Interface for uniform handling of parallel and chained initialization,
/// primarily used by Initializer, but also used in various places for 
/// interface consistency and easy refactoring.
/// </summary>

public delegate void InstanceInitializedCallback( IInitializable instance );

public interface IInitializable {
    void Initialize( InstanceInitializedCallback initializedCallback = null );
}