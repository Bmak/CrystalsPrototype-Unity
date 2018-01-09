using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for creating our UI cameras and delivering hardware back button events
/// </summary>
public class UISystem : MonoBehaviour, IInitializable, ILoggable
{
	private const string UI_LAYER_NAME = "NGUI UI";
	private const string UI_LAYER_FRONT_NAME = "NGUI UI Front";
	private const string UI_LAYER_CARDS_NAME = "Gameplay";
	private const string VPAINT_LAYER_NAME = "VPaint";
	private const float NEAR_CLIP = -100.0f;
	private const float FAR_CLIP = 1.0f;
	private const float UI_CAMERA_DEPTH = 10.0f;
	private const int DEBUG_CONSOLE_OPEN_TOUCH_COUNT = 3;
	private const int DEBUG_MENU_OPEN_TOUCH_COUNT = 4;
	private const int DEBUG_TOGGLE_UI_CAMERA = 5;
	private const float LONG_PRESS_DELAY_TIME = 0.35f;

	// It is for Android. These values get from getRequestedOrientation Java function
	// Need for fix WIZ-19358
	private const int ANDROID_ORIENTATION_LANDSCAPE = 0;
	private const int ANDROID_ORIENTATION_REVERSE_LANDSCAPE = 8;

	[Inject]
	private ViewController _viewController;

	[Inject]
	private Config _config;

	[Inject]
	private Client _client;

	[Inject]
	private LocalizationManager _localizationManager;

	private Camera _camera;
	private UICamera _uiCamera;
	private BackdropCamera _backdrop;

	public Camera Camera { get { return _camera; } }

	public void Initialize(InstanceInitializedCallback initializedCallback = null)
	{
		if (initializedCallback != null) {
			initializedCallback(this);
		}
	}

	public void InitializeCamera(Action gameSplashShownCallback = null)
	{
		_createCameraComponents();

		_localizationManager.Initialize();

		LoadBackdrop(gameSplashShownCallback);

		Screen.orientation = ScreenOrientation.Portrait;
	}

	private void LoadBackdrop(Action gameSplashShownCallback)
	{
		// We are calling Resources.Load() because the ResourceCache is not yet available when this method is invoked.
		// This asset is not used elsewhere and hence does not need the caching/loading behavior of the ResourceCache.
		GameObject backdropGO = Instantiate<GameObject>(Resources.Load<GameObject>("UI/" + _config.GetUIGlobalBackdropPrefab()));
		if (backdropGO != null) {
			backdropGO.SetActive(true);
			_backdrop = backdropGO.GetComponent<BackdropCamera>();
			_backdrop.Init();
			_backdrop.WireWidgets();
			if (_camera != null)
				_camera.clearFlags = CameraClearFlags.Depth;            
		}
	}

	private void OnPreRender()
	{
		// In case if we have only one camera on scene we should set its clearFlags as SolidColor to clear backbuffer and avoid flickering
		if (Camera.allCamerasCount == 1 && _camera.enabled) {
			_camera.clearFlags = CameraClearFlags.SolidColor;
		} else if (Camera.allCamerasCount > 1 && _camera.clearFlags != CameraClearFlags.Depth) {
			_camera.clearFlags = CameraClearFlags.Depth;
		}
	}

	private void Update()
	{
		if (_getInputEnabled() && Input.GetKeyDown(KeyCode.Escape)) {
			NguiView focusView = _viewController.GetFocusView();
			if (focusView != null) {
				focusView.OnBackClick();
			}
		}

		// When this define is set, we toggle the visibility of 
		// the UI when we detect a transition to device face down. 
		#if UI_HIDE_ENABLED
		if ( ++_checkDeviceOrientationFrameCount > CHECK_DEVICE_ORIENTATION_FRAME_INTERVAL )
			CheckDeviceOrientation();
		#endif

		// in debug mode, handle tap input to show debug console or menu
		// debug finger input should take precedence over any other processing
/*
		#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if ( _client.IsDebug() ) {
			#if UNITY_EDITOR
			if ( Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.BackQuote) ) {
				_uiCamera.cachedCamera.enabled = !_uiCamera.cachedCamera.enabled;
				return;
			} else if ( Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.BackQuote) ) {
				#if !PRODUCTION
                _debugMenu.Toggle();
				#endif
                return;
            } else if ( Input.GetKeyDown(KeyCode.BackQuote) ) {
				#if !PRODUCTION
                if (!_console.IsVisible()) {
                    _console.Show();
                    return;
                }
				#endif
            }
			#endif

			#if !PRODUCTION
            if ( Input.touchCount == DEBUG_MENU_OPEN_TOUCH_COUNT ) {
				if ( !_debugMenu.IsVisible() ) _debugMenu.Show();
            } else if ( Input.touchCount == DEBUG_CONSOLE_OPEN_TOUCH_COUNT ) {
				if ( !_console.IsVisible() ) _console.Show();
            }
			#endif
			 
        }
		#endif
*/
	}

	// When this define is set, we toggle the visibility of
	// the UI when we detect a transition to device face down.
	#if UI_HIDE_ENABLED
	private readonly int CHECK_DEVICE_ORIENTATION_FRAME_INTERVAL = 10;
	private int _checkDeviceOrientationFrameCount;
	private DeviceOrientation _lastDeviceOrientation = DeviceOrientation.Unknown;

	private void CheckDeviceOrientation() {
		// Reset frame interval
		_checkDeviceOrientationFrameCount = 0;
		
		// Input.deviceOrientation is extern into native code, so we only 
		// access it periodically to avoid a performance hit
		DeviceOrientation currentDeviceOrientation = Input.deviceOrientation;
		
		// Avoid repeated triggering by requiring an orientation state change
		if ( currentDeviceOrientation == _lastDeviceOrientation ) return;
		
		// On transition to device face down, we toggle UI visibility
		if ( currentDeviceOrientation == DeviceOrientation.FaceDown ) {
			_uiCamera.cachedCamera.enabled = !_uiCamera.cachedCamera.enabled;
		
#if !PRODUCTION
		_buildInfo.SetEnabled( _uiCamera.cachedCamera.enabled );
		#endif
		}
		
		// Save last orientation for next update check
		_lastDeviceOrientation = currentDeviceOrientation;
	}	
	#endif
	
	
	private void _createCameraComponents()
	{
		_camera = this.gameObject.AddComponent<Camera>();
		_camera.cullingMask = 1 << LayerMask.NameToLayer(UI_LAYER_NAME) | 1 << LayerMask.NameToLayer(UI_LAYER_FRONT_NAME) | 1 << LayerMask.NameToLayer(UI_LAYER_CARDS_NAME);
		_camera.orthographic = true;
		_camera.orthographicSize = 1.0f;
		_camera.farClipPlane = FAR_CLIP;
		_camera.nearClipPlane = NEAR_CLIP;
		_camera.depth = UI_CAMERA_DEPTH;
		_camera.useOcclusionCulling = false;
		_camera.clearFlags = CameraClearFlags.SolidColor; // initialize to solid color, once the backdrop gets loaded we'll switch it to depth only
		_camera.backgroundColor = Color.black;

		_uiCamera = this.gameObject.AddComponent<UICamera>();
		_uiCamera.eventType = UICamera.EventType.UI_3D;
		_uiCamera.eventReceiverMask = ~(1 << LayerMask.NameToLayer(VPAINT_LAYER_NAME)); // VPaint fix, so input doesn't get blocked by VPaint colliders
		_uiCamera.allowMultiTouch = false;
		_uiCamera.useController = false;
		_uiCamera.useKeyboard = false;
		_uiCamera.tooltipDelay = LONG_PRESS_DELAY_TIME;
		_uiCamera.longPressTooltip = true;

		_setInputEnabled(true);
	}

	// Should only be called by NguiView so that the enabled state doesn't get out of sync
	// If you need to disable input for some reason (waiting on server response, gameplay logic) then
	// you should either make use of the transition view (which has a full screen collider) or add
	// a full screen collider to your view prefab
	public void _setInputEnabled(bool enabled)
	{
		_uiCamera.useTouch = enabled;
		_uiCamera.useMouse = enabled;
	}

	private bool _getInputEnabled()
	{
		if (_uiCamera != null) {
			return _uiCamera.useTouch;
		}
		return false;
	}

	// Enable/disable the backdrop camera to save the extra buffer clear and overdraw
	// For use by controllers when they have their own "scene" camera, such as in homebase and battle
	public void SetBackdropCameraActive(bool active)
	{
		if (_backdrop != null)
			_backdrop.gameObject.SetActive(active);
	}

	/// <summary>
	/// Destroys the backdrop camera's initial loading sprite. This should be called
	/// after all inital loading is finished so that we don't keep the texture
	/// for that splash screen in memory.
	/// </summary>
	public void DestroyInitialLoadingSplash()
	{
		_backdrop.OnRelease();
		NGUITools.Destroy(_backdrop.gameObject);
	}

	/// <summary>
	/// Gets the enabled state of the backdrop camera
	/// </summary>
	public bool GetBackdropCameraActive()
	{
		return (_backdrop != null) && _backdrop.gameObject.activeInHierarchy;
	}

	public void ShakeCamera(AnimationClip animclip)
	{
		Animation animationcomponent = GetComponent<Animation>();
		if (animationcomponent == null) {
			animationcomponent = gameObject.AddComponent<Animation>();
		}

		if (!animationcomponent.GetClip("screenShake"))
			animationcomponent.AddClip(animclip, "screenShake");

		animationcomponent.Play("screenShake");
	}

	public void UpdateProgressLoading(float value)
	{
		_backdrop.UpdateProgressValue(value);
	}

	public void SetOnProceed(Action OnProceed)
	{
		_backdrop.OnProceedClicked = OnProceed;
	}
}
