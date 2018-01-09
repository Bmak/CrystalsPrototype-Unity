using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Responsible for dynamically populating a UIWrapGridContent instance with content. Will spawn enough
/// pooled prefab instances to fill the visible area of a scroll view, and will recycle them as the player
/// scrolls. Exposes event callbacks for detecting when the player clicks and item, or drags and item 
/// outside of the scroll view.
/// </summary>
public class ScrollListProvider
{
	// The number of extra lines of content to add to the pool. For a vertical scroll list, this is the number
	// of extra rows, and for a horizontal scroll list, this is the number of extra columns. This should be set to
	// at least 1, so that there's always at least an extra line for wrapping.
	private const int NUM_EXTRA_LINES = 1;
	
	private bool _wasShutdown;
	private UIWrapGridContent _wrapGrid;
	private GameObject _childPrefab;
	private Dictionary<GameObject, int> _itemMapping = new Dictionary<GameObject, int>();
    private List<GameObject> _createdObjects = new List<GameObject>();

	// Tracks whether the number of items in list fills the list area
	private bool _scrollListFilled = false;
	public bool ScrollListFilled
	{
		get { return _scrollListFilled;}
	}

    // Callback that's executed after the UIWrapGridContent has finished initializing such as setting
    // up panel sizes and scrollbars. Note: Only executes once 
    public Action OnWrapGridInitialized;

    // Callback that's executed after the UIWrapGridContent has finished repositioning its cild elements 
    public Action OnWrapGridContentPositioned;

	// Callback that's executed when the data-binding on a GameObject needs to be updated. Also use this
	// callback to setup whatever event callbacks (click, drag, etc) you need on the prefab instance
	public Action<ScrollListProvider, GameObject, int> ItemInitialized;

	/// <summary>
	/// Forces the ItemInitialize callback to be executed for each prefab instance in the
	/// scroll view. Useful for when you need to refresh the data binding for all the currently
	/// displayed views.
	/// </summary>
	public void ForceItemInitialize ()
	{
		_itemMapping.Clear();
        if (_wrapGrid != null) {
            _wrapGrid.ForceItemInitialize();
        }
	}

	public void Initialize (UIWrapGridContent wrapGrid, GameObject childPrefab, int itemCount)
	{
		_wasShutdown = false;

		_itemMapping.Clear();

		_wrapGrid = wrapGrid;

        _wrapGrid.onWrapGridInitialized = WrapGridIntialized;
        _wrapGrid.onContentRepositionComplete = WrapGridContentPositioned;
		_wrapGrid.onInitializeItem = InitializeItem;
		_wrapGrid.ItemCount = itemCount;
		_wrapGrid.ScrollView.disableDragIfFits = true;
		_wrapGrid.ScrollView.ResetPosition();
		_wrapGrid.onPanelSizeChanged = PanelSizeChanged;
		_childPrefab = childPrefab;

		HandleLayout();
	}

	private void HandleLayout ()
	{
        if (_wrapGrid == null) {
            return;
        }

		Vector2 viewSize = _wrapGrid.Panel.GetViewSize();
		float width = viewSize.x;
		float height = viewSize.y;
		
		int poolSize = 0;

		// Figure out how many rows/columns we need to fill the scroll view
		if (_wrapGrid.ScrollView.movement == UIScrollView.Movement.Horizontal)
		{
			_wrapGrid.MaxPerLine = Mathf.FloorToInt(height / _wrapGrid.ItemHeight);
			int maxCols = Mathf.FloorToInt(width / _wrapGrid.ItemWidth);
			poolSize = (Mathf.CeilToInt(width / _wrapGrid.ItemWidth) + NUM_EXTRA_LINES) * _wrapGrid.MaxPerLine;
			_scrollListFilled = (_wrapGrid.ItemCount > (_wrapGrid.MaxPerLine * maxCols));
		}
		else if (_wrapGrid.ScrollView.movement == UIScrollView.Movement.Vertical)
		{
			_wrapGrid.MaxPerLine = Mathf.FloorToInt(width / _wrapGrid.ItemWidth);
			int maxRows = Mathf.FloorToInt(height / _wrapGrid.ItemHeight);
			poolSize = (Mathf.CeilToInt(height / _wrapGrid.ItemHeight) + NUM_EXTRA_LINES) * _wrapGrid.MaxPerLine;
			_scrollListFilled = (_wrapGrid.ItemCount > (_wrapGrid.MaxPerLine * maxRows));
		}
		else
		{
			Debug.LogError("Don't know how to support movement mode " + _wrapGrid.ScrollView.movement);
		}

		if (poolSize < _wrapGrid.transform.childCount)
		{
			poolSize = Mathf.CeilToInt((float)_wrapGrid.transform.childCount / (float)_wrapGrid.MaxPerLine) * _wrapGrid.MaxPerLine;
		}

		List<IScrollListProviderItem> scrollListItems = new List<IScrollListProviderItem>();
		// Just in case there are already children added to the pool
		for (int i = _wrapGrid.transform.childCount; i < poolSize; i++)
		{
			GameObject child = NGUITools.AddChild(_wrapGrid.gameObject, _childPrefab);
			child.GetComponentsInChildren<IScrollListProviderItem>(includeInactive:true, results:scrollListItems);
			foreach (IScrollListProviderItem listItem in scrollListItems) {
				listItem.Initialize();
			}
			scrollListItems.Clear();
			_createdObjects.Add(child);
		}

		// Ask the grid to reposition all child elements
		_wrapGrid.ResetChildren();
	}

    private void WrapGridIntialized()
    {
        if(OnWrapGridInitialized != null) {
            OnWrapGridInitialized();
        }
    }

    private void WrapGridContentPositioned()
    {
        if (OnWrapGridContentPositioned != null)
        {
            OnWrapGridContentPositioned();
        }
    }

	private void InitializeItem (GameObject obj, int wrapIndex, int realIndex)
	{
		if (_wasShutdown) 
		{
			return;
		}

		// Only fire the ItemIntialize() delegate if the GameObject mapping has changed
		int previousRealIndex = -1;
		if (_itemMapping.TryGetValue(obj, out previousRealIndex) && previousRealIndex == realIndex)
		{
			return;
		}

		_itemMapping[obj] = realIndex;
        if (ItemInitialized != null)
        {
            ItemInitialized(this, obj, realIndex);
        }
    }

	private void PanelSizeChanged ()
	{
		// Whenever the panel size changes, recompute the 
		// number of rows/columns for the scroll view
		HandleLayout();
	}
    
    public void Shutdown ()
    {
		_wasShutdown = true;

		if(_wrapGrid != null) {
        	_wrapGrid.onInitializeItem -= InitializeItem;

            // reset scroll position before clearing everything
            // this is to ensure the next time this scroll list is used we're back in a good starting state
            // otherwise the scroll bars might be offset incorrectly
            _wrapGrid.Scroll(0);

			List<IScrollListProviderItem> scrollListItems = new List<IScrollListProviderItem>();
            // destroy all items we created dynamically
            for (int i = 0; i < _createdObjects.Count; ++i) {
                // check to see if this gameobject had a tile prefab presentation on it before calling destroy
                // so that we can release instantiated bits to the resource cache first
				_createdObjects[i].GetComponentsInChildren<IScrollListProviderItem>(includeInactive:true, results:scrollListItems);
				foreach (IScrollListProviderItem listItem in scrollListItems) {
					listItem.Shutdown();
				}
				scrollListItems.Clear();
				NGUITools.Destroy(_createdObjects[i]);
            }
            _createdObjects.Clear();

            // reset the wrap grid since the children have now changed
            _wrapGrid.ResetChildren();
			_wrapGrid.onPanelSizeChanged -= PanelSizeChanged;
            _wrapGrid = null;
		}
        _childPrefab = null;
    }

    // Call anytime the content of the scroll view changes or if the number of elements it contains changes
    public void ReInitialize(int newItemCount)
    {
        _itemMapping.Clear();
        if (_wrapGrid != null) {
            _wrapGrid.ItemCount = newItemCount;
        }
        HandleLayout();
        // We need to call SetScrollPosition(0) at the end to reinitialization. If we call SetScrollPosition(0) at the beginning of layout changing and it can call callback from its owner view (if scolling is active at the same time).
        // In this case its owner view's content is new, but UIWrapGridContent has a refernce to old content. It can cause wrong behavior or even ArgumentOutOfRangeException when new content is empty
        SetScrollPosition(0);
    }

    public float GetScrollPosition ()
    {
        return Mathf.Clamp01(_wrapGrid != null? _wrapGrid.GetScrollPosition(): 0);
    }

	public void SetScrollPosition (float percentage, float time = 0.0f, Action finishedCallback = null)
	{
        if (_wrapGrid != null) {
            _wrapGrid.Scroll(percentage, time, finishedCallback);
        }
	}

	public void ScrollToElement (int index, float time = 0.0f, Action finishedCallback = null)
	{
        if (_wrapGrid != null) {
            _wrapGrid.ScrollToElement(index, time, finishedCallback);
        }
	}

    public void CancelScrollToElement()
    {
        if (_wrapGrid != null) {
            _wrapGrid.CancelScrollToElement();
        }
    }

	/// <summary>
	/// Returns the GameObject that's being used to render the given element index. Note that if the element index
	/// is offscreen, then this method could return null;
	/// </summary>
	public GameObject GetGameObjectForIndex (int index)
	{
		foreach (KeyValuePair<GameObject, int> kvp in _itemMapping)
		{
			if (kvp.Value == index)
			{
				return kvp.Key;
			}
		}
		return null;
	}

    /// <summary>
    /// Returns the range of data elements that currently have a game object
    /// </summary>
    public void GetGameObjectRange(out int startIndex, out int endIndex)
    {
        startIndex = _itemMapping.Count - 1;
        endIndex = 0;
        foreach (KeyValuePair<GameObject, int> kvp in _itemMapping)
        {
            startIndex = Math.Min(startIndex, kvp.Value);
            endIndex = Math.Max(endIndex, kvp.Value);
        }
    }
}