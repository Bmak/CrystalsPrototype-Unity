
public interface IScopedBindingBuilder {

    IScopedBindingBuilder In( Scope scope );

    IScopedBindingBuilder Rank( int rank );

	IScopedBindingBuilder ObjectName( string objectName );

}
