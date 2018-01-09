using System;
using System.Collections;
using System.Collections.Generic;

public class Binder : IBinder, ILoggable
{

    private readonly LinkedList<IModule> _modules = new LinkedList<IModule>();
    private readonly Dictionary<Type, IBinding> _typeMap = new Dictionary<Type, IBinding>();
    private readonly Dictionary<string, IBinding> _nameMap = new Dictionary<string, IBinding>();
    private readonly bool _debug;

    public Binder() { }

    public Binder( bool debug )
    {
        _debug = debug;
    }

    public void Install( IEnumerable<IModule> modules )
    {
        foreach ( IModule module in modules )
            Install( module );
    }

    public void Install( IModule module ) {
        _modules.AddLast( module );
    }

    public void Configure()
    {
        // load modules, configure each
        // note that Configure() may cause the _modules collection to be modified,
        // hence the while over the linked list in lieu of iteration.
        LinkedListNode<IModule> node = _modules.First;
        while ( node != null ) {            
            IModule module = node.Value;
			if ( _debug ) this.LogTrace("Configuring module: " + module.GetType().Name, LogCategory.INJECTOR );
            module.Configure( this );
            node = node.Next;
        }
    }

    public void Reset()
    {
        _modules.Clear();
        _typeMap.Clear();
        _nameMap.Clear();        
    }

    public void Info()
    {
        this.LogInfo("Begin Named Bindings " + "*".Repeat(50) );
        Info( _nameMap );
        this.LogInfo("End Named Bindings " + "*".Repeat(50) );

        this.LogInfo("Begin Type Bindings " + "*".Repeat(50) );
        Info( _typeMap );
        this.LogInfo("End Type Bindings " + "*".Repeat(50) );
    }

    private void Info<KeyType>( Dictionary<KeyType, IBinding> map ) {
        foreach( KeyValuePair<KeyType, IBinding> kvp in map ) {
            KeyType fromName = kvp.Key;
            string toName = kvp.Value.GetImplementationType().Name;
            Scope scope = kvp.Value.GetScope();
            string instance = ( kvp.Value.GetInstance() ?? string.Empty ).ToString();
            this.LogInfo( fromName + " -> " + toName + ", scope: " + scope.ToString() + ", instance: " + instance );
        }
    }

    public IBindingBuilder<InterfaceType> Bind<InterfaceType>( string name = null )
    {
		if ( _debug ) this.LogTrace("Bind<" + typeof(InterfaceType).Name + ">" + ( name != null ? " name: " + name : ""), LogCategory.INJECTOR );
        return new BindingBuilder<InterfaceType>( this, name );
    }

    // bind interface->impl
    public IBinding Bind<InterfaceType, ImplementationType>( string name = null )
    {
        AssertTypeInvariants<ImplementationType, ImplementationType>();

		if ( _debug ) this.LogTrace("Bind<" + typeof(InterfaceType).Name + ", " + typeof(ImplementationType).Name + ">" + ( name != null ? " name: " + name : ""), LogCategory.INJECTOR );

        Type implType = typeof( ImplementationType );
        IBinding binding = GetOrCreateBinding<InterfaceType>( name );

        // If the impl is a concrete type, map the interface to this impl
        if ( !implType.IsInterface ) 
            binding.SetImplementationType( implType );

        return binding;
    }

    // bind interface->the given instance
    public IBinding Bind<InterfaceType>( InterfaceType instance, string name = null )
    {
		if ( _debug ) this.LogTrace("Bind<" + typeof(InterfaceType).Name + "> ( " + instance.ToString() + " )" + ( name != null ? " name: " + name : ""), LogCategory.INJECTOR );
        IBinding binding = GetOrCreateBinding<InterfaceType>( name );
        binding.SetImplementationType( typeof( InterfaceType ) );
        binding.SetInstance( instance );
        return binding;        
    }

    public IBinding GetBinding( Type type, string name = null)
    {
        IBinding binding = null;

        if ( name != null ) {
            _nameMap.TryGetValue( name, out binding );
        } else {
            _typeMap.TryGetValue( type, out binding );
        }
        return binding;
    }

    public IBinding GetBinding<T>( string name = null )
    {
        return GetBinding( typeof(T), name );
    }

    public IBinding GetOrCreateBinding( Type type, string name = null )
    {
        IBinding binding = GetBinding( type, name );    
        if ( binding != null ) return binding;
        binding = new Binding( type, name );
        if ( name != null ) {
            _nameMap[name] = binding;
        } else {
            _typeMap[type] = binding;
        }
        return binding;        
    }

    public IBinding GetOrCreateBinding<T>( string name = null )
    {
        return GetOrCreateBinding( typeof(T), name );
    }

    public IEnumerable<IBinding> GetBindingsByScope( Scope scope )
    {
        List<IBinding> result = new List<IBinding>();
        PopulateByScope( result, _nameMap.Values, scope );
        PopulateByScope( result, _typeMap.Values, scope );

        result.Sort( ( a, b ) => a.GetRank().CompareTo( b.GetRank() ) );

        return result;
    }

    private void PopulateByScope( List<IBinding> target, IEnumerable<IBinding> source, Scope scope )
    {
        foreach( IBinding binding in source )
            if ( binding.GetScope() == scope )
                target.Add( binding );
    }

    private void AssertTypeInvariants<InterfaceType, ImplementationType>()
    {
        Type interfaceType = typeof( InterfaceType );
        Type implType = typeof( ImplementationType );

        if ( !interfaceType.IsAssignableFrom( implType ) )
            throw new ArgumentException( implType.Name + " does not implement " + interfaceType.Name );
    }
}
