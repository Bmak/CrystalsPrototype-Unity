using System;
using UnityEngine;

public interface IInstantiator {

    T New<T>( string objectName = null ) where T : new();

    object New( Type type, string objectName = null );
    
    void Subscribe( EventHandler<OnInstantiateArgs> handler );

    void Unsubscribe( EventHandler<OnInstantiateArgs> handler );

    void AssertConcrete<T>( bool invert = false );

    void AssertConcrete( Type type, bool invert = false );

    void SetResetInProgress( bool value );

}

public class OnInstantiateArgs : EventArgs  {       
    private readonly object _instance;
    public OnInstantiateArgs( object instance )  {
        _instance = instance;
    }

    public object GetInstance() {
        return _instance;
    }
}
