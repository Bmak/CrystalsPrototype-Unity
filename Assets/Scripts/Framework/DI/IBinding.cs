using System;

public interface IBinding 
{
    
    Type GetImplementationType();

    void SetImplementationType( Type type );

    string GetName();

    void SetName( string name );

    Scope GetScope();

    void SetScope( Scope scope );

    int GetRank();

    void SetRank( int rank );

    object GetInstance();

    void SetInstance( object instance );

	string GetObjectName();

	void SetObjectName( string objectName );

}