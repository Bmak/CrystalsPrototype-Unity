using UnityEngine;
using System.Collections;

public static class MathUtils 
{
	private const float TOLERANCE = 0.000001f;

	/// <summary>
	/// Returns whether or not two floating point values are equal. This duplicates the functionality
	/// of Mathf.Approximately(), which is broken currently when compiling for 32-bit architecture
	///  using IL2CPP.
	/// </summary>
	public static bool Approximately (float a, float b)
	{
		return Mathf.Abs(a - b) < TOLERANCE;
	}
}
