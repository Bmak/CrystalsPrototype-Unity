using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public static class BuildHelper
{
	[MenuItem ("Build/Build for iOS")]
	public static void BuildIOS()
	{
		String[] levels = {"Assets/Scenes/Run.unity", "Assets/Scenes/Edit.unity"};
		string errorMsg = BuildPipeline.BuildPlayer(levels, "Builds/iOS", BuildTarget.iOS, BuildOptions.AcceptExternalModificationsToPlayer);
		if (errorMsg != string.Empty) {
			EditorUtility.DisplayDialog("Build for iOS", "There was an errors during the build process: " + errorMsg, "Oh, well", "");
		}
	}
}
