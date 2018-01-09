using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class Injection : ILoggable
{
    private const BindingFlags INJECTABLE_FIELD_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
    private const BindingFlags POSTCONSTRUCT_METHOD_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
    private static readonly Type PROVIDER_INTERFACE_TYPE = typeof( IProvider<> );
    private static readonly Type PROVIDER_TYPE = typeof( Provider<> );
    private static readonly Type COMPONENT_TYPE = typeof( Component );
    private static readonly string AWAKE_METHOD_NAME = "Awake";

    private readonly IInjector _injector;
    private readonly IBinder _binder;

    private readonly object _target;
    private readonly Type _targetType;
    private readonly bool _debug;

    public Injection( IInjector injector, IBinder binder, object target, bool debug = false )
    {
        _injector = injector;
        _binder = binder;
        _target = target;
        _targetType = _target == null ? null : _target.GetType();
        _debug = debug;
    }

    public object GetTarget()
    {
        return _target;
    }

    public Type GetTargetType()
    {
        return _targetType;
    }

    public void Execute()
    {
		if ( _target == null ) {
			this.LogWarning("Execute(): Injection attempted on invalid target, aborting.");
			return;
		}
        CheckForAwake();
        Inject();        
        InvokePostConstruct();
    }

    public void Inject()
    {
        if ( _debug ) this.LogTrace("Inject<" + _targetType.Name + ">()", LogCategory.INJECTOR);

        foreach( MemberContext<FieldInfo> member in GetInjectableFields() ) {

            FieldInfo field = member.GetInfo();
            string name = member.GetName();
            string objectName = member.GetObjectName();

            Type fieldType = field.FieldType;

			if ( _debug ) this.LogTrace("Field: " + field.Name + ", Type: " + fieldType.Name, LogCategory.INJECTOR);

            // If this field is a provider, construct it
            object fieldValue = ConstructProvider( fieldType, name );
            if ( fieldValue != null ) { // Found provider
                field.SetValue( _target, fieldValue );
                continue;
            }

            IBinding binding = _binder.GetBinding( fieldType, name );
            
            // If this type is not registered with the injector, skip it
            if ( binding == null ) { 
                this.LogWarning( fieldType.Name + " has no registered bindings, skipping field '" + field.Name + "'" );
                continue;
            }

			// Use ObjectName from Inject attribute first, falling back
			// to any ObjectName specified on the binding itself
			if ( String.IsNullOrEmpty( objectName ) )
				objectName = binding.GetObjectName();

            fieldValue = _injector.Get( fieldType, name, objectName );

            field.SetValue( _target, fieldValue );
			if ( _debug ) this.LogTrace("Injected field with value: " + fieldValue.ToString(), LogCategory.INJECTOR );            
        }
    }

    private object ConstructProvider( Type type, string name = null)
    {        
        object result = null;

        if ( !type.IsGenericType || ( type.GetGenericTypeDefinition() != PROVIDER_INTERFACE_TYPE ) )
            return result; // This is not an IProvider, return null

        Type containedType = type.GetGenericArguments()[0]; // IProvider<containedType>

        IBinding binding = _binder.GetBinding( containedType, name );
        
        // If this type is not registered with the injector, skip it
        if ( binding == null ) { 
            this.LogWarning( containedType.Name + " has no registered bindings, skipping provider '" + type.Name + "'" );
            return result;
        }

        Type parameterizedType = PROVIDER_TYPE.MakeGenericType( new Type[] { containedType } );
        result = Activator.CreateInstance( parameterizedType );

        new Injection( _injector, _binder, result, _debug ).Execute();

        return result;
    }

    public void InvokePostConstruct()
    {
		if ( _debug ) this.LogTrace("InvokePostConstruct() " + _targetType.Name, LogCategory.INJECTOR);

        foreach( MemberContext<MethodInfo> member in GetPostConstructMethods() ) {
            MethodInfo method = member.GetInfo();
			if ( _debug ) this.LogTrace("Invoking PostConstruct: " + _targetType.Name + "." + method.Name + "()", LogCategory.INJECTOR);
            method.Invoke( _target, null );
        }
    }

    public IEnumerable<MemberContext<FieldInfo>> GetInjectableFields( BindingFlags bindingFlags = INJECTABLE_FIELD_BINDING_FLAGS )
    {
        FieldInfo[] fields = _targetType.GetFields( bindingFlags );
        List<MemberContext<FieldInfo>> result = new List<MemberContext<FieldInfo>>( fields.Length );

        foreach( FieldInfo field in fields )
        {
            // Check to see if the attribute exists before trying to get it,
            // since getting an attribute temporarily allocates memory even if it doesn't exist.
            if (HasAttribute<Inject>(field))
            {
                Inject attribute = GetAttribute<Inject>(field);
                if (attribute == null) continue; // No attribute on field, continue
                result.Add(new MemberContext<FieldInfo>(field, attribute.Name, attribute.ObjectName));
            }
        }
        return result;
    }

    public IEnumerable<MemberContext<MethodInfo>> GetPostConstructMethods( BindingFlags bindingFlags = POSTCONSTRUCT_METHOD_BINDING_FLAGS )
    {
        MethodInfo[] methods = _targetType.GetMethods( bindingFlags );
        List<MemberContext<MethodInfo>> result = new List<MemberContext<MethodInfo>>( methods.Length );

        foreach( MethodInfo method in methods )
        {
            if (HasAttribute<PostConstruct>(method))
            {
                result.Add(new MemberContext<MethodInfo>(method));
            }
        }
        return result;
    }

    private AttributeType GetAttribute<AttributeType>( MemberInfo target )
        where AttributeType : Attribute
    {
        AttributeType[] attrs = (AttributeType[])target.GetCustomAttributes( typeof(AttributeType), false );
        return attrs.Length <= 0 ? default(AttributeType) : attrs[0];
    }

    private bool HasAttribute<T>( MemberInfo target )
        where T : Attribute
    {
        return target.IsDefined(typeof(T), false);
    }

    private void CheckForAwake() {
        if ( !_debug ) return;
        if ( COMPONENT_TYPE.IsAssignableFrom( _targetType ) && ( _targetType.GetMethod( AWAKE_METHOD_NAME ) != null ) )
            this.LogWarning( _targetType.Name + " is an injected component with a defined Awake() method. Use [PostConstruct] instead to avoid issues related to instantiation order.");            
    }

    public class Builder
    {
        private IInjector _injector;
        private IBinder _binder;
        private object _target;
        private bool _debug;
        
        public Builder Injector( IInjector injector )
        {
            _injector = injector;
            return this;
        }

        public Builder Binder( IBinder binder )
        {
            _binder = binder;
            return this;
        }

        public Builder Target( object target ) {
            _target = target;
            return this;
        }

        public Builder Debug( bool debug )
        {
            _debug = debug;
            return this;
        }

        public Injection Build()
        {
            return new Injection( _injector, _binder, _target, _debug );
        }
    }
}



















