using System;
using System.Collections.Generic;

public abstract class Module : IModule, ILoggable {

    private IBinder _binder; 

    public void Configure( IBinder binder ) {    
        _binder = binder;
        try {
            Configure();
        } finally {
            _binder = null;
        }
    }

    protected abstract void Configure();


    protected void Install( IEnumerable<IModule> modules ) {
        _binder.Install( modules );
    }

    protected void Install( IModule module ) {
        _binder.Install( module );
    }

    protected IBindingBuilder<InterfaceType> Bind<InterfaceType>( string name = null ) {
        return _binder.Bind<InterfaceType>( name );
    }

}
