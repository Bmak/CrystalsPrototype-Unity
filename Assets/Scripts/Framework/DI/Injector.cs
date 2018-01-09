using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Primary implementation of an <see cref="IInjector"/>.
/// </summary>
public class Injector : IInjector, ILifecycleAware, ILoggable
{
    private readonly IInstantiator _instantiator;    
    private readonly IBinder _binder;
    private static bool _debug;

    // This is to allow use of a single global injector in static contexts.
    // This should be primarily used for refactoring legacy code.
    private static IInjector _instance;
    public static IInjector Instance
    {
        get {
            _instance.LogStaticWarning();
            return _instance;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Injector"/> class.
    /// </summary>
    /// <param name="instantiator">Instantiator for creation of new object instances</param>
    /// <param name="modules">Collection of binding modules backing this injector</param>
    /// <param name="debug">Enable for debug mode, which includes additional logging</param>
    public Injector( IInstantiator instantiator, IEnumerable<IModule> modules, bool debug = false )
    {
        _instance = this;
        _instantiator = instantiator;
        _debug = debug;
        _binder = new Binder( _debug );
        // Allow injection of the Injector and Instantiator
        _binder.Install( new InjectorModule( this, instantiator ) );
        _binder.Install( modules );
        _binder.Configure();
        ConstructEagerSingletons();
    }

    public void Reset()
    {
        _binder.Reset();
        _instance = null;
    }

    public object Get( Type type, string name = null, string objectName = null )
    {
        IBinding binding = _binder.GetBinding( type, name );

        if ( binding == null )
            throw new ArgumentException("'" + type.Name + "' has no registered bindings");

        return binding.GetInstance() ?? Construct( binding, objectName );
    }

    public T Get<T>( string name = null, string objectName = null )
    {
        return (T)Get( typeof(T), name, objectName );
    }

    /// <summary>
    /// Construct an instance for the given binding. The instance will be 
    /// injected, post-constructed, and saved for later requests if 
    /// the binding has singleton scope.
    /// </summary>
    /// <param name="binding">Binding.</param>
    /// <param name="objectName">Object name.</param>
    private object Construct( IBinding binding, string objectName = null )
    {
        #if METRICS_ENABLED && INCLUDE_DEV_METRICS
        Metrics.Start( GetType().Name + ":ConstructAndInject:" + binding.GetImplementationType().Name );
        #endif      

        Type implType = binding.GetImplementationType();
        object instance = _instantiator.New( implType, objectName );

        // Prototype scoped bindings will yield a new instance on each call to Get()
        // We set the object reference in the binding prior to Injection.Execute() to avoid issues with circular references        
		if ( !Scope.PROTOTYPE.Equals( binding.GetScope() ) && ( instance != null ) )
            binding.SetInstance( instance );

        new Injection( this, _binder, instance ).Execute();

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
        Metrics.End( GetType().Name + ":ConstructAndInject:" + binding.GetImplementationType().Name );
        #endif

        return instance;
    }

    public void Inject( object target )
    {
        new Injection( this, _binder, target ).Execute();
    }

    /// <summary>
    /// Instantiate all eager singletons, in order by binding rank, ascending.
    /// Note that instantiation of a singleton will be followed by injection of its
    /// fields, which will each then be instantiated. Hence, the dependencies of an
    /// eager singleton, even if they are not eager themselves, will be instantiated
    /// during this call.
    /// </summary>
    private void ConstructEagerSingletons()
    {
        foreach( IBinding binding in _binder.GetBindingsByScope( Scope.EAGER_SINGLETON ) )
        {
            try {
                if ( _debug ) this.LogTrace("Constructing eager singleton: " + binding.GetImplementationType().Name + ", rank: " + binding.GetRank(), LogCategory.INJECTOR );        
                Construct( binding );
            } catch ( Exception e ) {
                this.LogError("Exception constructing eager singleton '" + binding.GetImplementationType().Name +"': " + e.ToString() );  
            }
        }
    }

    public static void VerifyInject<T>( ref T injectProperty )
    {
        if (injectProperty == null)
            injectProperty = Instance.Get<T>();
    }

    public void Info()
    {
        this.LogInfo("Info() start " + "*".Repeat(50));
        _binder.Info();
        this.LogInfo("Info() end " + "*".Repeat(50));
    }

    public void LogStaticWarning()
    {
        if ( _debug )
            this.LogWarning("Detected use of Injector.Instance property. Consider refactoring the calling code to use proper dependency injection.");
    }

    /// <summary>
    /// Builder for construction of an injector. As injector construction
    /// involves immutable state, this allows incremental configuration
    /// of the injector for convenience.
    /// </summary>
    public class Builder
    {
        private IInstantiator _instantiator;
        private readonly List<IModule> _modules = new List<IModule>();
        private bool _debug;
        
        public Builder Instantiator( IInstantiator instantiator )
        {
            _instantiator = instantiator;
            return this;
        }

        public Builder Module( IModule module )
        {
            _modules.Add( module );
            return this;
        }

        public Builder Debug( bool debug )
        {
            _debug = debug;
            return this;
        }

        public Injector Build()
        {
            return new Injector( _instantiator, _modules, _debug );
        }
    }
}

public static class InjectorExtensions {

    public static void Inject( this Component target ) {
        Injector.Instance.Inject( target );
    }
}




















