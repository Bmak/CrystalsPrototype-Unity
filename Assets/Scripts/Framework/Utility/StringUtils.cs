using System;
using System.Text;

/// <summary>
/// Utility class containing string utilities and extension methods.
/// </summary>
public static class StringUtils
{
	private const string COMMA_SEPARATOR = 						",";
	private static readonly char[] COMMA_LIST_SPLIT_CHARS = 	new char[] { ',', ';', ' ' };

	/// <summary>
	/// Extension method for perl-esqe string/character multiplication (concatenation).
	///     Example:
	///         "a" x 5 => aaaaa (perl)
	///         "a".Repeat(5) => aaaaa
	/// </summary>
	/// <param name="value">the string to multiply</param>
	/// <param name="count">number of times to concatenate</param>
	public static string Repeat( this string value, int count )
	{
		if (count <= 0)
			return string.Empty;
		if (count == 1)
			return value;
		return (new StringBuilder ().Insert (0, value, count)).ToString ();
	}

	/// <summary>
	/// Extension method for Contains() taking a StringComparison constant,
	/// e.g. StringComparison.OrdinalIgnoreCase will perform the search
	/// in a case-insensitive manner.
	/// </summary>
	/// <param name="source">Source string</param>
	/// <param name="target">Target string</param>
	/// <param name="comparison">Comparison constant</param>
	public static bool Contains( this string source, string target, StringComparison comparison )
	{
		return source.IndexOf (target, comparison) >= 0;
	}

	/// <summary>
	/// Truncate the specified string to maxLength.
	/// </summary>
	/// <param name="value">Value.</param>
	/// <param name="maxLength">Max length.</param>
	public static string Truncate( this string value, int maxLength )
	{
		if (string.IsNullOrEmpty (value)) {
			return value;
		}

		return value.Length <= maxLength ? value : value.Substring (0, maxLength); 
	}

	public static string[] CommaDelimitedToArray( string inValue ) {
		if ( String.IsNullOrEmpty( inValue ) )
			return new string[0];
		
		return inValue.Split( COMMA_LIST_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries );
	}
	
	public static string ArrayToCommaDelimited( string[] inValue ) {
		if ( inValue == null || inValue.Length == 0 )
			return string.Empty;
		
		return String.Join( COMMA_SEPARATOR, inValue );
	}
}
