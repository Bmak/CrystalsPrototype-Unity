using System;
using System.Collections;
using UnityEngine;

public class Instantiator : IInstantiator, ILoggable {

    private static readonly Type COMPONENT_TYPE = typeof( Component );
    private static readonly string GAME_OBJECT_NAME_PREFIX = "_";
    private static readonly string INTERFACE_PREFIX = "I";  
    private const bool USE_INTERFACE_NAME_FOR_NEW_GAME_OBJECTS = true;

    private bool _resetInProgress = false;

    private event EventHandler<OnInstantiateArgs> _onInstantiate;
    public event EventHandler<OnInstantiateArgs> OnInstantiate {
        add {
            _onInstantiate -= value; // Event handler subscription should be idempotent
            _onInstantiate += value;
        }

        remove {
            _onInstantiate -= value;
        }
    }

    public T New<T>( string objectName = null ) where T: new() {
        if ( _resetInProgress ) return default(T);
        return (T)New( typeof(T), objectName );
    }

    public object New( Type type, string objectName = null ) {
        if ( _resetInProgress ) return null;

        AssertConcrete( type );

        object instance = COMPONENT_TYPE.IsAssignableFrom( type ) ? NewComponent( type, objectName ) : NewObject( type );
        // We only need to fire the event here, as all instantiation below is called via the above line.
        FireInstantiateEvent( instance );
        return instance;
    }

    private object NewObject( Type type ) {
        try { 
            return Activator.CreateInstance( type );
        } catch ( Exception e ) {
            this.LogError("Exception creating instance of type '" + type.Name + "': " + e.ToString());
        }
        return null;
    }

    private T NewObject<T>() where T : new() {        
        try {
            return new T();
        } catch ( Exception e ) {
            this.LogError("Exception creating instance of type '" + typeof(T).Name + "': " + e.ToString());
        }        
        return default(T);
    }

    private object NewComponent( Type type, string objectName = null ) {
        return NewComponent( type, FindOrCreateGameObject( objectName ?? GameObjectNameForType( type ) ) );
    }

    private T NewComponent<T>( string objectName = null ) where T : Component, new() {
        return NewComponent<T>( FindOrCreateGameObject( objectName ?? GameObjectNameForType<T>() ) );
    }

    private object NewComponent( Type type, GameObject gameObject ) {
        return gameObject.GetComponent( type ) ?? gameObject.AddComponent( type );
    }

    private T NewComponent<T>( GameObject gameObject ) where T : Component {
        return ( gameObject.GetComponent<T>() ) ?? gameObject.AddComponent<T>();
    }

    private GameObject FindOrCreateGameObject( string objectName ) {
        GameObject go = GameObject.Find( objectName );
        return go != null ? go : new GameObject( objectName );        
    }

    private string GameObjectNameForType<T>() {
        return GameObjectNameForType( typeof(T) );  
    }

    #pragma warning disable 0429
    private string GameObjectNameForType( Type type ) {
        return GAME_OBJECT_NAME_PREFIX + (
            USE_INTERFACE_NAME_FOR_NEW_GAME_OBJECTS ?
            GetPrimaryInterface( type ) :
            type 
        ).Name;  
    }
    #pragma warning restore 0429

    private Type GetPrimaryInterface( Type type ) {
        foreach( Type interfaceType in type.GetInterfaces() ) {
            string interfaceBaseName = GetInterfaceNameBase( interfaceType );
            if ( type.Name.StartsWith( interfaceBaseName ) )
                return interfaceType;
        }
        return type;
    }

    private string GetInterfaceNameBase( Type type ) {
        string typeName = type.Name;
        if ( !typeName.StartsWith( INTERFACE_PREFIX ) ) return typeName;
        return typeName.Remove(0, 1);
    }

    public void SetResetInProgress( bool value ) {
        _resetInProgress = value;
    }
    
    public void Subscribe( EventHandler<OnInstantiateArgs> handler ) {
        OnInstantiate += handler;
    }

    public void Unsubscribe( EventHandler<OnInstantiateArgs> handler ) {
        OnInstantiate -= handler;
    }

    private void FireInstantiateEvent( object instance ) {
        if ( _onInstantiate == null ) return; // Bail early if no subscribers
        OnInstantiateArgs args = new OnInstantiateArgs( instance );
        _onInstantiate( this, args );
    }

    public void AssertConcrete<T>( bool invert = false ) {
        AssertConcrete( typeof( T ), invert );
    }

    public void AssertConcrete( Type type, bool invert = false ) {

        if ( invert) {
            if ( !type.IsAbstract || !type.IsInterface )
                throw new ArgumentException( type.Name + " is a concrete type" );
        } else {
            if ( type.IsAbstract || type.IsInterface )
                throw new ArgumentException( type.Name + " is not a concrete type and cannot be instantiated" );
        }
    }
}

public static class InstantiatorExtensions {
    /// <summary>
    /// Destroy the specified target. Note that this internally calls
    /// UnityEngine.Object.Destroy(), which delays the actual destroy
    /// until the end of the frame.
    /// </summary>
    /// <param name="target">Target.</param>
    public static void DestroyAll( this UnityEngine.Object target ) {
        Log.Debug("DestroyAll(" + target + ")");
        
        try {            
            if ( target is Component ) { // Destroy parent GameObject, which will also destroy all sibling components
                UnityEngine.Object.DestroyImmediate( ((Component)target).gameObject );
            } else { // Else, destroy the object itself            
				UnityEngine.Object.DestroyImmediate( target );
            }
        } catch (Exception e) {
            Log.Error("Destroy() exception on target '" + target + "': " + e.ToString() );
        }
    }


}
