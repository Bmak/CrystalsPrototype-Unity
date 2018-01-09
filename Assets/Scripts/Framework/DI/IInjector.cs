using System;

/// <summary>
/// Interface representing the public API provided by an injector.
/// 
/// An injector is the primary interface to the depdendency injection
/// system.
/// </summary>
public interface IInjector  {

    /// <summary>    
    /// Requests an instance of the given type from this injector.
    ///
    ///   Note: Use of this method is discouraged if field injection via
    ///     the [Inject] attribute can be used instead. Please evaluate
    ///     your use case before using this method.
    ///
    /// If the requested instance has not yet been instantiated, the instantiation
    /// will occur during this call, respecting the given name and binding scope.
    ///
    /// Instances returned by this method must have a binding registered
    /// in a module that has been loaded by the injector, or a warning will
    /// be emitted and this method will return null.
    ///
    /// If the 'name' parameter is provided, only named bindings will be 
    /// searched when fulfilling the injection request. In this case,
    /// a binding of the form:
    ///
    ///     Bind<I>().To<T>("name");
    /// 
    /// must be registered with the injector to fulfill the injection request.
    ///    
    /// </summary>
    /// <typeparam name="T">The requested type.</typeparam>
    /// <param name="name">
    ///     Request an instance via named binding. Allows retrieval of a
    ///     particular implementation out of many when bound by string name.
    /// </param>
    /// <param name="objectName">
    ///     When requesting a Unity component from the injector, specifies the
    ///     target GameObject name that the component will be attached to. If
    ///     a GameObject with the given name does not exist, it will be created.
    ///     This parameter is ignored if the requested object is not a component,
    ///     or is a singleton and has already been instantiated.
    /// </param>
    /// 
    T Get<T>( string name = null, string objectName = null );

    /// <summary>
    /// Requests an instance of the given type from this injector.
    ///
    /// See Get<T> for full use semantics.
    ///
    /// </summary>
    /// <param name="type">The requested type.</param>
    /// <param name="name">
    ///     Request an instance via named binding. Allows retrieval of a
    ///     particular implementation out of many when bound by string name.
    /// </param>
    /// <param name="objectName">
    ///     When requesting a Unity component from the injector, specifies the
    ///     target GameObject name that the component will be attached to. If
    ///     a GameObject with the given name does not exist, it will be created.
    ///     This parameter is ignored if the requested object is not a component,
    ///     or is a singleton and has already been instantiated.
    /// </param>
    object Get( Type type, string name = null, string objectName = null );

    /// <summary>
    /// Inject fields into the given target instance.
    ///
    ///   Note: Use of this method is generally discouraged if the same result
    ///     can be accomplished via standard injection. Please evaluate your
    ///     use case before using this method.
    ///
    /// The injector will set fields in the target instance that are annotated
    /// with the [Inject] attribute. If the instance referenced by a field has
    /// not yet been instantiated, the instantiation will occur during this call.
    /// After injection, this method will execute all methods in the target
    /// instance annotated with the [PostConstruct] annotation, in definition order.
    /// 
    /// This method is intended to allow use of injected fields in objects whose
    /// lifecycle is managed outside of the injection system. For types with values
    /// known at compile time, this is rarely necessary and injection should be
    /// be accomplished via bindings and injection of the parent type.
    ///
    /// Example use case:
    ///
    ///     T GetOrCreate<T>() where T : new() {
    ///         T result = new T();
    ///         _injector.Inject( result );
    ///         return result;
    ///     }
    ///
    ///     
    /// This allows the injector to operate on dynamic or data-driven instances
    /// without explicit bindings for the underlying type.
    /// </summary>
    /// <param name="target">Target instance for injection.</param>
    void Inject( object target );
    
    /// <summary>
    /// Reset the internal state of the injector. After this call, this injector
    /// will have no registered bindings and cannot satisfy injection requests.
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Logs all bindings registered with the injector to the console.
    /// </summary>
    void Info();

    /// <summary>
    /// For internal use only. If the injector was configured in debug mode,
    /// use of the .Instance static property will execute this method to alert
    /// that the static accessor was used, as this is discouraged.
    /// </summary>
    void LogStaticWarning();

}
