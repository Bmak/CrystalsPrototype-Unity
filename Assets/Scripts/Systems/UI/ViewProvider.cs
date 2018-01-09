using UnityEngine;
using System;

public class ViewProvider : ILoggable
{
	private const string UI_LAYER_NAME = "NGUI UI";

    [Inject]
    private Config _config;

    [Inject]
    private IInjector _injector;

	// assetLoadCallback provides an additional callback AFTER finishCallback that is asynchronous, allowing the view to load resources.
	// The View must override the LoadAssets function to take advantage of this.
	public void Get<ViewT>(Action<ViewT> finishCallback, Action assetLoadCallback = null) where ViewT : NguiView
	{
		string prefabName = "UI/" + _config.GetViewPrefabName(typeof(ViewT));

		ResourceHandle viewHandle = new ResourceHandle(prefabName);
		UnityEngine.Object prefab = Resources.Load(prefabName, typeof(GameObject));
		if (prefab == null)
		{
			_viewLoadError();
			return;
		}
		viewHandle._set(GameObject.Instantiate(prefab));
		viewHandle._acquire();
		if (viewHandle.Resource != null)
		{
			GameObject viewGO = viewHandle.GO;
			viewGO.SetActive(false);
			ViewT viewObject = viewGO.GetComponent<ViewT>();
			_injector.Inject(viewObject);

			// Turn off NGUI cameras
			Camera[] cams = viewGO.GetComponentsInChildren<Camera>();
			foreach (Camera cam in cams) {
				if ((cam.cullingMask & (1 << LayerMask.NameToLayer(UI_LAYER_NAME))) > 0) {
					cam.enabled = false;
				}
			}

			// let the view do any initialization it needs before informing the caller its ready
			viewObject._initialize(viewHandle, finishedCallback: () => {
				if (finishCallback != null) {
					finishCallback(viewObject);
				}
			});

			if (assetLoadCallback != null)
				viewObject.LoadAssets(assetLoadCallback);
		} else {
			_viewLoadError();
		}
    }

    private void _viewLoadError()
    {
        this.LogError("Error acquiring view");
    }
}
