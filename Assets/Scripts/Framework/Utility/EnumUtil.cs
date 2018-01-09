using System;
using System.Collections.Generic;

/* @author dadler */

/// <summary>
/// Enum convenience methods.        
/// C# will not allow generic type constraints on Enums.    
/// The best we can do is: where T : struct, IComparable, IConvertible, IFormattable
/// </summary>
public static class EnumUtil  {

	public static bool IsEnum<T>() {
		return typeof(T).IsEnum;
	}

	public static T Parse<T>( string constantName )  where T : struct, IComparable, IConvertible, IFormattable {
		return (T)Enum.Parse( typeof(T), constantName, true );
	}

	public static T Parse<T>( string constantName, T defaultValue ) where T : struct, IComparable, IConvertible, IFormattable {
		return IsDefined<T>( constantName ) ? (T)Enum.Parse( typeof(T), constantName, true ) : defaultValue;
	}

	public static bool IsDefined<T>( string constantName ) where T : struct, IComparable, IConvertible, IFormattable {
		return !String.IsNullOrEmpty( constantName ) && Enum.IsDefined( typeof(T), constantName );
	}

	public static IEnumerable<T> GetValues<T>() where T: struct {
		return (IEnumerable<T>)Enum.GetValues(typeof(T));
	}

	public static T ToBitFieldEnum<T>( string[] constantNames ) where T : struct, IComparable, IConvertible, IFormattable {

		Type enumType = typeof(T);
		HashSet<string> constantNameSet = new HashSet<string>( constantNames, StringComparer.OrdinalIgnoreCase );

		int flags = 0;
		foreach ( var flag in Enum.GetValues( enumType ) )
			if ( constantNameSet.Contains( flag.ToString() ) ) 
			    flags = flags | (int)flag;

		return (T)Enum.ToObject( enumType, flags );
	}
    
    public static string ToPrefsKey(this Enum e)
    {
        string str = string.Format("{0}.{1}", e.GetType().FullName, e.ToString());
        str = str.Replace('+', '.');
        return str;
    }

// NOTE: The following method fails AOT cross-compile on iOS due to the nullable generic type.

/*
	public static T? ParseSafe<T>( string constantName) where T : struct, IComparable, IConvertible, IFormattable {
		return IsValid<T>( constantName ) ? (T?)Enum.Parse( typeof(T), constantName, true ) : null;
	}
*/

}
