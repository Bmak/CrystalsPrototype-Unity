using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The base class for all views, concrete views should extend this class and override as needed for their custom behaviors
/// </summary>
public abstract class NguiView : MonoBehaviour, ILoggable
{
	// Public properties intended to be overridden by each view
	public virtual DepthEnum InitialDepth { get { return DepthEnum.Screen; } }

	public virtual bool CanGainFocus { get { return true; } }

	public event Action<NguiView> OnViewReleased;

	// Amount of z-space requested by this view for things like interlacing 3d objects
	// Views on top of this one in the stack will be pushed towards the camera by this amount in world space
	[SerializeField]
	private float _zSeparation = 0.0f;

	// These animations are played when the view is activated
	[SerializeField]
	private NguiAnimGroup _showAnims;

	// These animations are played when the view is deactivated
	[SerializeField]
	private NguiAnimGroup _hideAnims;

	[Inject]
	protected ViewController _viewController;

	[Inject]
	protected UISystem _uiSystem;

	[Inject]
	protected AudioSystem _audioSystem;

	[Inject]
	protected AudioSystem _networkSystem;

	[Inject]
	protected LocalizationManager _localizationManager;

	[Inject]
	protected LocalizationConfig _lc;

	[Inject]
	protected CoroutineCreator _coroutineCreator;

	[Inject]
	protected Config _config;

	private ResourceHandle _prefabResourceHandle;

	private List<FieldInfo> _cachedFieldInfo = null;
	// point at element search uses reflection to find elements within the view to point at,
	// this will hold cached field info so we don't need to incur the reflection cost every call

	private bool _wasReleased = false;
	private bool _deactivating = false;
	private List<Action> _deactivatedCallbacks = new List<Action>();

	protected bool WasReleased { get { return _wasReleased; } }

	public bool ViewActive {
		get { return this.gameObject.activeInHierarchy; }
	}

	public float ZSeparation { get { return _zSeparation; } }

	public void _initialize(ResourceHandle handle, Action finishedCallback)
	{
		_wasReleased = false;

		_prefabResourceHandle = handle;

		// set up button delegates and label text etc.
		WireWidgets();

		_viewController._registerView(this);

		// Load all required assets and then update the view depths before finishing initialization
		LoadRequiredAssets(assetsLoaded: () => {
			_viewController.UpdateViewDepths();

			if (finishedCallback != null) {
				finishedCallback();
			}
		});
	}

	/// <summary>
	/// Load assets that are required before a view can finish its initialization. This includes
	/// assets that may contain panels as the view needs all panels before ViewController
	/// updates the depths of the panels in the view.
	/// </summary>
	protected virtual void LoadRequiredAssets(Action assetsLoaded)
	{
		if (assetsLoaded != null) {
			assetsLoaded();
		}
	}

	public virtual void LoadAssets(Action assetLoadCallback)
	{
		// Override to perform your view's custom asset loading in this function
	}

	// - Find every child component derived from a UIRect and call Update on it,
	// so its dimensions are sized properly.
	// Otherwise we run into situations where for one frame a widget resizes
	// from the dimensions the prefab was saved to the screen's dimensions
	// and there is a visual snapback effect.
	// - This is a useful function to call on Start() so it executes once
	// and the main view object is guaranteed to be active.
	// WARNING: If you call this function and the UIRect child is not active
	// then box colliders that are set to auto adjust based on widget dimensions
	// may not resize properly.
	protected void ForceUpdateAnchors(GameObject rootObject)
	{
		UIRect[] widgets = rootObject.GetComponentsInChildren<UIRect>(includeInactive: true);
		foreach (UIRect widget in widgets) {
			widget.Update();
		}
	}

	// To be overridden by the view to hook up any delegates needed
	protected virtual void WireWidgets()
	{
        
	}

	// called on the focus view when hardware back is pressed
	public virtual void OnBackClick()
	{
		//_audioSystem.PostEvent("PLAY_UI_STANDARDHUD_BACK_BUTTON");
	}

	// called when the view is being released, override this to clean up any loaded resources
	protected virtual void OnRelease()
	{

	}

	// convenience method to call SetViewActive(false) and destroy after anims are finished
	public void DeactivateAndRelease()
	{
		SetViewActive(false, Release);
	}

	public void Release()
	{
		_wasReleased = true;

		Action<NguiView> onReleaseCallback = OnViewReleased;
		OnViewReleased = null;
		if (onReleaseCallback != null) {
			onReleaseCallback(this);
		}

		OnRelease();
		_viewController._unregisterView(this);

		if (_prefabResourceHandle != null) {
			_prefabResourceHandle.Release();
		}
	}

	// Update this view's depth for NGUI's context
	// Returns the amount of depth taken up by this view
	// This should probably only ever need to be called by the ViewController
	public int _setDepth(int depth)
	{
		// find all panels that are a part of this view's GO to find relative depths
		UIPanel[] panels = GetComponentsInChildren<UIPanel>(includeInactive: true);
		Array.Sort(panels, (a, b) => {
			return a.depth.CompareTo(b.depth);
		});

		// reset depths for all panels using the passed in depth as a base value
		for (int i = 0; i < panels.Length; ++i) {
			panels[i].depth = depth + i;
		}

		return panels.Length;
	}

	// sets the view gameobject's position in 3d space
	public void _setPosition(Vector3 pos)
	{
		this.transform.localPosition = pos;
	}

	// Turns the view on/off completely (not visible, not updating)
	// calls onFinish once the view is done animating in/out
	public void SetViewActive(bool active, Action onFinish = null)
	{
		if (active == ViewActive) {
			if (onFinish != null) {
				onFinish();
			}
			return;
		}

		// disable input while views are animating in/out
		_uiSystem._setInputEnabled(false);

		// if activating, set the game object active immediately and start anims...
		// if we're deactivating, we'll wait for the anims to play first
		if (active) {
			this.gameObject.SetActive(true);
			_deactivatedCallbacks.Clear();
		} else if (_deactivating) {
			// if we're already deactivating from a previous call, save off the passed in onFinish
			// to be called once the anim is done - then bail out of here so we don't play the anim twice
			if (onFinish != null) {
				_deactivatedCallbacks.Add(onFinish);
			}
			return;
		}

		_deactivating = !active;
		NguiAnimGroup groupToPlay = (active ? _showAnims : _hideAnims);

		groupToPlay.Play(this, _deactivating, () => {
			if (!active) {
				this.gameObject.SetActive(false);
			} else {
				//_navDispatch.SendViewActivated(this);
			}
			if (onFinish != null) {
				// if activating, wait at least one frame so that anchors have a chance to update
				if (active) {
					_coroutineCreator.DelayActionOneFrame(onFinish);
				} else {
					onFinish();
				}
			}
			for (int i = 0; i < _deactivatedCallbacks.Count; ++i) {
				_deactivatedCallbacks[i]();
			}
			_deactivatedCallbacks.Clear();

			_uiSystem._setInputEnabled(true);
		});
	}

	public virtual Vector3? GetScreenCoordinatesForTag(string tag, out Rect? bounds)
	{
		FieldInfo fieldInfoMatch = null;

		// build up and cache a list of member variables that inherit from monobehaviour
		if (_cachedFieldInfo == null) {
			_cachedFieldInfo = new List<FieldInfo>();
			_cachedFieldInfo.AddRange(GetPointAtFields(this.GetType(), ref fieldInfoMatch));
		}

		// search our cached list of monobehaviour fields (if we didn't already find it while building the cache)
		if (fieldInfoMatch == null) {
			for (int i = 0; i < _cachedFieldInfo.Count; ++i) {
				FieldInfo fieldInfo = _cachedFieldInfo[i];
				if (fieldInfo.Name == tag) {
					fieldInfoMatch = fieldInfo;
					break;
				}
			}
		}

		bounds = null;

		// found the field name match, grab the monobehaviour or game object isntance and return coords
		if (fieldInfoMatch != null) {
			MonoBehaviour fieldValueMonoBehaviour = fieldInfoMatch.GetValue(this) as MonoBehaviour;
			if (fieldValueMonoBehaviour != null) {
				bounds = NguiUtils.GetScreenRectForWidget(fieldValueMonoBehaviour, _uiSystem.GetComponent<Camera>());
				return _uiSystem.GetComponent<Camera>().WorldToScreenPoint(fieldValueMonoBehaviour.transform.position);
			}
			GameObject fieldValueGameObject = fieldInfoMatch.GetValue(this) as GameObject;
			if (fieldValueGameObject != null) {
				bounds = NguiUtils.GetScreenRectForGameObject(fieldValueGameObject, _uiSystem.GetComponent<Camera>());
				return _uiSystem.GetComponent<Camera>().WorldToScreenPoint(fieldValueGameObject.transform.position);
			}
		}

		this.LogWarning("Point at element [" + tag + "] not found.");
		return null;
	}

	// used by console testing harness (and maybe other tools) for quick access to potential point at targets by design
	public List<FieldInfo> _getCachedFieldInfoList()
	{
		return _cachedFieldInfo;
	}


	// Convenience gizmos for representing z-separation in editor
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1, 1, 0, 0.5f);
		Gizmos.DrawWireCube(transform.position + (Vector3.forward * -_zSeparation * 0.5f), new Vector3(2.6666f, 2.0f, -_zSeparation));
	}

	// Recrusively get all monobehavior and gameobject fields for the point at tags
	private HashSet<FieldInfo> GetPointAtFields(Type type, ref FieldInfo fieldInfoMatch)
	{
		if (type == null || type == typeof(MonoBehaviour)) {
			return null;
		}

		HashSet<FieldInfo> fieldInfoList = new HashSet<FieldInfo>(new FieldInfoNameComparer());
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		FieldInfo[] fields = type.GetFields(bindingFlags);
		for (int i = 0; i < fields.Length; ++i) {
			FieldInfo fieldInfo = fields[i];
			if (typeof(MonoBehaviour).IsAssignableFrom(fieldInfo.FieldType)
			             || typeof(GameObject).IsAssignableFrom(fieldInfo.FieldType)) {
				fieldInfoList.Add(fieldInfo);

				// if this field is the one we're looking for, assign it now so we don't need to iterate over the cache list immediately
				if (fieldInfoMatch != null && fieldInfo.Name == tag) {
					fieldInfoMatch = fieldInfo;
				}
			}
		}

		HashSet<FieldInfo> baseClassFieldInfoList = GetPointAtFields(type.BaseType, ref fieldInfoMatch);
		if (baseClassFieldInfoList != null) {
			fieldInfoList.AddRange(baseClassFieldInfoList);
		}

		return fieldInfoList;
	}

	private class FieldInfoNameComparer : EqualityComparer<FieldInfo>
	{
		public override int GetHashCode(FieldInfo fieldInfo)
		{
			return fieldInfo.Name.GetHashCode();
		}

		public override bool Equals(FieldInfo a, FieldInfo b)
		{
			return a.Name.Equals(b.Name);
		}
	}
}
