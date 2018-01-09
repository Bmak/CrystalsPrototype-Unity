using UnityEngine;
using System;

public class DebugUtils
{
	// Utility assert() function
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
	public static void Assert( bool condition, string assertString, bool breakOnFail = true )
	{
		if( !condition )
		{
			Log.Error( "Debug assert failed! " + assertString );
			if( breakOnFail )
				Debug.Break();
		}
	}
	
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
	public static void Break( string assertString )
	{
        Log.Error( "Debug break: " + assertString );
		Debug.Break();
	}
}

