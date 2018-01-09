using System;
using System.Reflection;

/// <summary>
/// This class provides extension methods for C#'s Type class
/// </summary>
public static class TypeExtensions 
{
    /// <summary>
    /// Returns MethodInfo for a method implemented through an interface
    /// </summary>
    public static MethodInfo GetInterfaceMethod<implementedInterface>(this Type type, string methodName)
    {
        foreach(MethodInfo methodInfo in type.GetInterfaceMap(typeof(implementedInterface)).InterfaceMethods) {
            if(methodInfo.Name == methodName) {
                return methodInfo;
            }
        }

        return null;
    }
}
