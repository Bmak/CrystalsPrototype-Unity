using System;
using System.Reflection;
using System.Collections;

public class MemberContext<MemberType> 
    where MemberType : MemberInfo
{

    private readonly MemberType _info;
    private readonly string _name;
    private readonly string _objectName;


    public MemberContext( MemberType info, string name = null, string objectName = null ) {
        _info = info;
        _name = name;
        _objectName = objectName;
    }

    public MemberType GetInfo() {
        return _info;
    }

    public string GetName() {
        return _name;
    }

    public string GetObjectName() {
        return _objectName;
    }

}
