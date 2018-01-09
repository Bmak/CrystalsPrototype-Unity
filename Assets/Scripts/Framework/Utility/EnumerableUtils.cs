using System;
using System.Collections.Generic;


/// <summary>
/// Enumerable utilities and extension methods.
///	
/// <author>dadler@ea.com</author>
/// </summary>
public static class EnumerableUtils {

	
	/// <summary>
	/// Extension method that parallels Python's enumerate(). Allows
	/// enumeration with convenient access to current index.
	/// </summary>
	/// <param name="enumerable">the enumerable to, well, enumerate.</param>
	/// <param name="action">block to execute for each element</param>
	public static void Each<T>( this IEnumerable<T> enumerable, Action<T, int> action ) {
	    int index = -1;
	    foreach ( var element in enumerable ) action( element, ++index );
	}

	/// <summary>
	/// Extension method that parallels Python's enumerate(), with support for break.
	/// Allows enumeration with convenient access to current index.
	///	Returns true if enumeration completed, false if break was encountered.
	/// </summary>
	/// <param name="enumerable">the enumerable to, well, enumerate.</param>
	/// <param name="action">block to execute for each element, return value of false breaks enumeration, continues otherwise.</param>
	public static bool Each<T>( this IEnumerable<T> enumerable, Func<T, int, bool> action ) {
	    int index = -1;
		foreach ( var element in enumerable ) if ( !action( element, ++index ) ) return false;
		return true;
	}

}
