using System;

public class BindingBuilder<InterfaceType> : IBindingBuilder<InterfaceType>, IScopedBindingBuilder, ILoggable
{

    private IBinder _binder;
    private IBinding _binding;
    private string _name;
    private InterfaceType _instance;

    public BindingBuilder( IBinder binder, string name = null )
    {
        _binder = binder;
        _name = name;
        _binding = _binder.Bind<InterfaceType, InterfaceType>( _name );
    }

	public IScopedBindingBuilder To<ImplementationType>( string name = null ) where ImplementationType : InterfaceType, new()
	{
		if ( name != null ) _name = name;
		_binding = _binder.Bind<InterfaceType, ImplementationType>( _name );
		return this;
	}

    public void ToInstance( InterfaceType instance, string name = null ) {
        if ( instance != null ) _instance = instance;
        if ( name != null ) _name = name;
        _binding = _binder.Bind<InterfaceType>( _instance, _name );
    }

    public IScopedBindingBuilder In( Scope scope ) {
        _binding.SetScope( scope );
        return this;
    }

    public IScopedBindingBuilder Rank( int rank ) {
        _binding.SetRank( rank );
        return this;
    }

	public IScopedBindingBuilder ObjectName( string objectName ) {
		_binding.SetObjectName( objectName );
		return this;
	}
}
