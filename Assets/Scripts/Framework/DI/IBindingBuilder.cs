using System;

public interface IBindingBuilder<InterfaceType> : IScopedBindingBuilder {

	IScopedBindingBuilder To<ImplementationType>( string name = null ) where ImplementationType : InterfaceType, new(); 

    void ToInstance( InterfaceType instance, string name = null );

}
