using System;

/// <summary>
/// Attribute indicating that a field should be autowired by
/// the injector.
///
/// Example:
///
///     [Inject( Name = "foo" )]
///     private IFoo _foo;
///
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class Inject : Attribute {

    /// <summary>
    /// Binding name for named injection. Used to specify which
    /// implementation of an interface, out of many, should be used 
    /// when fulfilling the injection. Optional, and should only
    /// be used when multiple implementations of an interface exist.
    ///
    /// To be satisfied, a binding of the form:
    ///     Bind<I>().To<T>("Name");
    ///
    /// must be registered with the injector, where 'Name' corresponds
    /// to this parameter.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; }
    
    /// <summary>
    /// Name of the GameObject to which the underlying field object will be
    /// added. This parameter is only applicable if the field type is a
    /// Component, and if this injection will result in the instantiation of
    /// the component. If the field is not a Component, or a singleton
    /// corresponding to the injection has already been instantiated, this 
    /// parameter has no effect.
    /// </summary>
    /// <value>The name of the object.</value>
    public string ObjectName { get; set; }
    
    /// <summary>
    /// Default attribute constructor.
    /// </summary>
    public Inject() { }

}
