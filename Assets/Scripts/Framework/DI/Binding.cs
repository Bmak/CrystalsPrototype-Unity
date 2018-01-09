using System;

public class Binding : IBinding, ILoggable
{
    
    private Type _type;    
    private string _name;
    private Scope _scope = Scope.SINGLETON;
    private int _rank;
	private string _objectName;
    private object _instance;

    public Binding( string name ) {
        _name = name;
    }

    public Binding( Type type, string name = null, Scope scope = Scope.SINGLETON) {
        _type = type;
        _name = name;
        _scope = scope;
    }

    public Type GetImplementationType() {
        return _type;
    }

    public void SetImplementationType( Type type ) {
        _type = type;
    }

    public string GetName() {
        return _name;
    }

    public void SetName( string name ) {
        _name = name;
    }

    public Scope GetScope() {
        return _scope;
    }

    public void SetScope( Scope scope ) {
        _scope = scope;
    }

    public int GetRank() {
        return _rank;
    }

    public void SetRank( int rank ) {
        _rank = rank;
    }

    public object GetInstance() {
        return _instance;
    }

    public void SetInstance( object instance ) {
        _instance = instance;
    }

	public string GetObjectName() {
		return _objectName;
	}

	public void SetObjectName( string objectName ) {
		_objectName = objectName;
	}
}