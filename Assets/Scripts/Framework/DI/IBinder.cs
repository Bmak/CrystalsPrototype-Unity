using System;
using System.Collections.Generic;

public interface IBinder {

    void Install( IEnumerable<IModule> modules );

    void Install( IModule module );

    void Configure();

    void Reset();

    void Info();

    IBindingBuilder<InterfaceType> Bind<InterfaceType>( string name = null );

    IBinding Bind<InterfaceType, ImplementationType>( string name = null );

    IBinding Bind<InterfaceType>( InterfaceType instance, string name = null );

    IBinding GetBinding<T>( string name = null );

    IBinding GetBinding( Type type, string name = null );

    IBinding GetOrCreateBinding<T>( string name = null );

    IBinding GetOrCreateBinding( Type type, string name = null );

    IEnumerable<IBinding> GetBindingsByScope( Scope scope );

}
