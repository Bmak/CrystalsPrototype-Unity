using System;

public static class JavaHashCode
{
	/// <summary>
	/// Gets a hash code for the provided string.
	/// This hash code implementation has been copied from the source code of Java 1.8.0_40, and allows the client to compute hash codes that match those computed by the server.
	/// </summary>
	/// <returns>The hash code of the string</returns>
	/// <param name="str">The string to compute a hash code for</param>
	public static int GetHashCode(String str) {
		int h = 0;
		char[] val = str.ToCharArray();
		for (int i = 0; i < val.Length; i++) {
			h = 31 * h + val[i];
		}
		return h;
	}
}