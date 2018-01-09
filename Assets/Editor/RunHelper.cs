using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SingleEntryPoint
{
	static SingleEntryPoint()
	{
		EditorApplication.playmodeStateChanged += OnPlayModeChanged;
	}

	private static void OnPlayModeChanged()
	{
		SceneSwitchHandler.HandlePlayChanged();
	}
}

static public class SceneSwitchHandler
{
	static public void HandlePlayChanged()
	{
		if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
			Debug.Log("Switch to Run");
			EditorSceneManager.SaveOpenScenes();
			EditorSceneManager.OpenScene("Assets/Scenes/Run.unity");
		} else if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
			Debug.Log("Switch to Edit");
			EditorSceneManager.OpenScene("Assets/Scenes/Edit.unity");
		}
	}
}
