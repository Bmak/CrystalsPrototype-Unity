using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// This script makes it possible for a scroll view to wrap its content, creating endless scroll views. Typically
/// used in conjunction with a ScrollListProvider, to dynamically create a pool of child elements, and compute
/// the number of required rows/columns.
/// Usage: simply attach this script underneath your scroll view where you would normally place a UIGrid:
/// 
/// + Scroll View
/// |- UIWrapGridContent
/// |-- Item 1
/// |-- Item 2
/// |-- Item 3
/// </summary>

[AddComponentMenu("NGUI/Interaction/Wrap Grid Content")]
public class UIWrapGridContent : MonoBehaviour
{
	public delegate void OnPanelSizeChanged ();
	public delegate void OnInitializeItem (GameObject go, int wrapIndex, int realIndex);
    public delegate void OnWrapGridInitialized ();
    public delegate void OnContentRepositionComplete();

	[SerializeField]
	private int _maxPerLine = 1;

	[SerializeField]
	private int _itemHeight = 100;

	[SerializeField]
	private int _itemWidth = 100;

	[SerializeField]
	private int _itemCount = 0;

    [SerializeField]
    private bool _unlimitedItems = false;

    [SerializeField]
    private UIScrollBar _scrollbar;

	[SerializeField]
	private Vector2 _startingClipPosition;

	[SerializeField]
	private Vector3 _startingTransformPosition;

    [SerializeField]
    private UIScrollView _scrollView;

    private const int SCROLLBAR_RESTING_FUDGE_PIXELS = 10;

	private Vector2 _offset;
	private Transform _trans;
	private UIPanel _panel;
	private UIScrollView _scroll;
	private bool _horizontal = false;
	private bool _firstTime = true;
	private bool _performedInitialLayout = false;
	private Vector2 _scrollPanelSize;
	private List<Transform> _children = new List<Transform>();
	private Sequence _scrollingSequence;
    private bool _dragging = false;

	public int MaxPerLine 
	{ 
		get { return Mathf.Max(_maxPerLine, 1); } 
		set { _maxPerLine = Mathf.Max(value, 1); }
	}

	public int ItemHeight { get { return Mathf.Max(_itemHeight, 1); } }
	
	public int ItemWidth { get { return Mathf.Max(_itemWidth, 1); } }
	
	public int ItemCount 
	{ 
		get { return Mathf.Max(_itemCount, 0); } 
		set { _itemCount = Mathf.Max(value, 0); }
	}

	public UIScrollView ScrollView
	{
		get
		{
			if (_scroll == null)
			{
				CacheScrollView();
			}
			return _scroll;
		}
	}

    public UIPanel Panel
    {
        get
        {
            if (_panel == null)
            {
                CacheScrollView();
            }
            return _panel;
        }
    }

    public bool UnlimitedItems
    {
        get { return _unlimitedItems; }
        set { _unlimitedItems = value; }
    }

    /// <summary>
    /// Callback that's executed after Start() has finished and the wrap grid is properly initialized
    /// with the scroll panel. Note: Only executes once as Start() is only called once for a component
    /// </summary>
    public OnWrapGridInitialized onWrapGridInitialized;

	/// <summary>
	/// Callback that will be called every time an item needs to have its content updated.
	/// The 'wrapIndex' is the index within the child list, and 'realIndex' is the index using position logic.
	/// </summary>
	
	public OnInitializeItem onInitializeItem;

	public OnPanelSizeChanged onPanelSizeChanged;

    /// <summary>
    /// Called when the repositioning of the greid elements is complete
    /// </summary>
    public OnContentRepositionComplete onContentRepositionComplete;

	/// <summary>
	/// Initialize everything and register a callback with the UIPanel to be notified when the clipping region moves.
	/// </summary>

	private void Start ()
	{
		if (!_performedInitialLayout) 
		{
			ResetChildren();
			WrapContent();
			_firstTime = false;
		}

		if (_scroll != null) 
		{
			_scroll.GetComponent<UIPanel>().onClipMove = OnMove;
			_scrollPanelSize = _scroll.panel.GetViewSize();
		}

        if (_scrollbar != null)
		{
            if (_scrollView == null) {
                _scrollView = GetComponentInParent<UIScrollView>();

                if (_scrollView == null) {
                    Debug.LogError("Error: Could not find UIScrollView for UIWrapGridContent. Either serialize the variable or parent a UIScrollView to UIWrapGridContent");
                }
            }

            _scrollView.onDragStarted += DragStarted;
            _scrollView.onStoppedMoving += DragEnded;
		}

	    if(onWrapGridInitialized != null) {
	        onWrapGridInitialized();
	    }
	}

    void OnEnable()
    {
        if (_scrollbar != null) {
            _scrollbar.alpha = 0f;
        }
    }

    void DragStarted()
    {
        _dragging = true;
    }

    void DragEnded()
    {
        _dragging = false;
    }

	/// <summary>
	/// Callback triggered by the UIPanel when its clipping region moves (for example when it's being scrolled).
	/// </summary>

	protected virtual void OnMove (UIPanel panel) { WrapContent(); }

	/// <summary>
	/// Immediately reposition all children. Call this method after
	/// adding and/or removing children.
	/// </summary>

	public void ResetChildren ()
	{
		if (!CacheScrollView()) 
		{
			return;
		}

		// Cache all children and place them in order
		_children.Clear();
		for (int i = 0; i < _trans.childCount; ++i)
		{
            if (UnlimitedItems || i < ItemCount)
			{
				_trans.GetChild(i).gameObject.SetActive(true);
				_children.Add(_trans.GetChild(i));
			}
			else
			{
				_trans.GetChild(i).gameObject.SetActive(false);
			}
		}

		// Sort the list of children so that they are in order
		if (_horizontal) 
		{
			_children.Sort(UIGrid.SortHorizontal);
		}
		else 
		{
			_children.Sort(UIGrid.SortVertical);
		}

		ResetChildPositions();
		_performedInitialLayout = true;

        _panel.SetDirty();
	}

	/// <summary>
	/// Cache the scroll view and return 'false' if the scroll view is not found.
	/// </summary>

	protected bool CacheScrollView ()
	{
		_trans = transform;
		_panel = NGUITools.FindInParents<UIPanel>(gameObject);
		_scroll = _panel.GetComponent<UIScrollView>();
		if (_scroll == null) 
		{
			return false;
		}
		if (_scroll.movement == UIScrollView.Movement.Horizontal) 
		{
			_horizontal = true;
		}
		else if (_scroll.movement == UIScrollView.Movement.Vertical) 
		{
			_horizontal = false;
		}
		else
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Helper function that resets the position of all the children.
	/// </summary>

	private void ResetChildPositions ()
	{
        float previousScrollPosition = 0;

        // reset panel location back to its original position so that the scroll offsets
        // remain the same relative to the panel parent
        // if we don't do this the scroll bars get all kinds of wonky
        if (!_firstTime) {
            UIPanel panel = Panel;
            previousScrollPosition = GetScrollPosition();
            panel.clipOffset = _startingClipPosition;
            panel.transform.localPosition = _startingTransformPosition;
            panel.Refresh();
        }

		UIScrollView scrollView = ScrollView;
		if (scrollView != null)
		{
			if (_horizontal)
			{
				Vector3[] corners = scrollView.GetComponent<UIPanel>().worldCorners;
				transform.position = (corners[0] + corners[1]) / 2.0f;
				_offset = new Vector2(_itemWidth * 0.5f, (_itemHeight * (MaxPerLine - 1)) * 0.5f);
			}
			else
			{
				Vector3[] corners = scrollView.GetComponent<UIPanel>().worldCorners;
				transform.position = (corners[1] + corners[2]) / 2.0f;
				_offset = new Vector2((_itemWidth * (MaxPerLine - 1)) * -0.5f, _itemHeight * -0.5f);
			}
		}

		for (int i = 0, imax = _children.Count; i < imax; ++i)
		{
			Transform t = _children[i];
			t.localPosition = _horizontal ? 
				new Vector3(_offset.x + (i / MaxPerLine) * ItemWidth, _offset.y + (i % MaxPerLine) * -ItemHeight, 0f) : 
				new Vector3(_offset.x + (i % MaxPerLine) * ItemWidth, _offset.y + (i / MaxPerLine) * -ItemHeight, 0f);
			UpdateItem(t, i);
		}

		_firstTime = false;
		_startingClipPosition = _panel.clipOffset;
		_startingTransformPosition = _panel.transform.localPosition;

		if (_scrollingSequence != null)
		{
			_scrollingSequence.Kill();
			_scrollingSequence = null;
		}


        // reset scroll position back to what it was before this repositions happened
        if (onContentRepositionComplete != null) {
            Scroll(previousScrollPosition, 0.0f, OnRepositionComplete);
        } else {
            Scroll(previousScrollPosition);
        }
	}

    private void OnRepositionComplete()
    {
        if (onContentRepositionComplete != null) {
            onContentRepositionComplete();
        }
    }

	/// <summary>
	/// Wrap all content, repositioning all children as needed.
	/// </summary>

	private void WrapContent ()
	{
		float extents = _horizontal ? ItemWidth * Mathf.Ceil((float)_children.Count / (float)MaxPerLine)  * 0.5f : _itemHeight * Mathf.Ceil((float)_children.Count / (float)MaxPerLine)  * 0.5f;
		Vector3[] corners = _panel.worldCorners;
		
		for (int i = 0; i < 4; ++i)
		{
			Vector3 v = corners[i];
			v = _trans.InverseTransformPoint(v);
			corners[i] = v;
		}
		
		Vector3 center = Vector3.Lerp(corners[0], corners[2], 0.5f);
		bool allWithinRange = true;
		float ext2 = extents * 2f;


		if (_horizontal)
		{
			for (int i = 0, imax = _children.Count; i < imax; ++i)
			{
				Transform t = _children[i];

                // The children transforms aren't expected to be null
                // but HockeyApp was reporting NREs on accessing it's
                // local position, so this defensive null check was
                // added to protect against unforseen situations.
                if (t == null) {
                    continue;
                }

				float distance = t.localPosition.x - center.x;

				if (distance < -extents)
				{
					Vector3 pos = t.localPosition;
					int amount = Mathf.Abs(Mathf.RoundToInt(distance / ext2));
					pos.x += ext2 * amount;
					int realIndex = ToRealIndex(pos);

					if (IndexIsInRange(realIndex))
					{
						t.localPosition = pos;
						UpdateItem(t, i);
					}
					else 
					{
						allWithinRange = false;
					}
				}
				else if (distance > extents)
				{
					Vector3 pos = t.localPosition;
					int amount = Mathf.Abs(Mathf.RoundToInt(distance / ext2));
					pos.x -= ext2 * amount;
					int realIndex = ToRealIndex(pos);

					if (IndexIsInRange(realIndex))
					{
						t.localPosition = pos;
						UpdateItem(t, i);
					}
					else 
					{
						allWithinRange = false;
					}
				}
				else if (_firstTime)
				{
					UpdateItem(t, i);
				}
			}
		}
		else
		{
			for (int i = 0, imax = _children.Count; i < imax; ++i)
			{
				Transform t = _children[i];
				float distance = t.localPosition.y - center.y;

				if (distance < -extents)
				{
					Vector3 pos = t.localPosition;
					int amount = Mathf.Abs(Mathf.RoundToInt(distance / ext2));
					pos.y += ext2 * amount;
					int realIndex = ToRealIndex(pos);

					if (IndexIsInRange(realIndex))
					{
						t.localPosition = pos;
						UpdateItem(t, i);
					}
					else 
					{
						allWithinRange = false;
					}
				}
				else if (distance > extents)
				{
					Vector3 pos = t.localPosition;
					int amount = Mathf.Abs(Mathf.RoundToInt(distance / ext2));
					pos.y -= ext2 * amount;
					int realIndex = ToRealIndex(pos);

					if (IndexIsInRange(realIndex))
					{
						t.localPosition = pos;
						UpdateItem(t, i);
					}
					else 
					{
						allWithinRange = false;
					}
				}
				else if (_firstTime) 
				{
					UpdateItem(t, i);
				}
			}
		}
		_scroll.restrictWithinPanel = !allWithinRange;
        _scroll.InvalidateBounds();

        UpdateScrollbars();
	}

    /// <summary>
    /// Update scroll bar size
    /// </summary>
    private void UpdateScrollbars()
    {
        if (_scrollbar == null || _panel == null) { return; }

        // get scroll view "window" size
        Vector4 clip = _panel.finalClipRegion;
        float viewSize = _horizontal ? clip.z : clip.w;
        if (_panel.clipping == UIDrawCall.Clipping.SoftClip) {
            viewSize -= ((_horizontal ? _panel.clipSoftness.x : _panel.clipSoftness.y) * 2);
        }

        // calculate "virtual" content total size -> num items * width
        float contentSize = _horizontal ? ItemWidth * Mathf.Ceil((float)_itemCount / (float)MaxPerLine) : ItemHeight * Mathf.Ceil((float)_itemCount / (float)MaxPerLine);

        // find scroll location percentage
        Transform panelTx = _panel.transform;
        float currentPosition = _horizontal ? panelTx.localPosition.x : panelTx.localPosition.y;
        float scrollMax = contentSize - viewSize;
        // apply some fudge to scrollMax so that the bar doesn't rest a few pixels away from the end of the window
        scrollMax -= SCROLLBAR_RESTING_FUDGE_PIXELS;
        float scrollPercent = (scrollMax > 0 ? currentPosition / (_horizontal ? -scrollMax : scrollMax) : 0);

        // and finally calculate ratio of visible scroll area to total content size
        // (view size shrinks as it scrolls beyond ends)
        float scrolledBeyondPercent = 0;
        if (scrollPercent < 0) {
            scrolledBeyondPercent = (scrollPercent * -1);
        } else if (scrollPercent > 1) {
            scrolledBeyondPercent = (scrollPercent - 1);
        }
        float scrolledBeyondDist = scrolledBeyondPercent * scrollMax;
        float adjustedViewSize = viewSize - scrolledBeyondDist;
        float visibleAreaPercent = (contentSize > 0 ? adjustedViewSize / contentSize : 0);

        // tell the scrollbar
        _scrollbar.value = Mathf.Clamp01(scrollPercent);
        _scrollbar.barSize = Mathf.Clamp01(visibleAreaPercent);
    }

    /// <summary>
    /// Returns the scroll position of the scroll view, in range between 0.0f and 1.0f
    /// </summary>
    public float GetScrollPosition ()
    {
        float panelSize = _horizontal ? _scrollPanelSize.x : _scrollPanelSize.y;
        float contentSize = _horizontal ? ItemWidth * Mathf.Ceil((float)_itemCount / (float)MaxPerLine) : ItemHeight * Mathf.Ceil((float)_itemCount / (float)MaxPerLine);
        Vector3 transformPosition = _panel.transform.localPosition - _startingTransformPosition;
        float amount = _horizontal ? transformPosition.x : -transformPosition.y; 
        float percentage = amount / (panelSize - contentSize);
        return percentage;
    }

	/// <summary>
	/// Updates the scroll position of the scroll view, where the percentage is a value between 0.0f and 1.0f, also
	/// has an option to animate the scroll position changing over time.
	/// </summary>
	public void Scroll (float percentage, float time = 0.0f, Action finishedCallback = null)
	{
		float panelSize = _horizontal ? _scrollPanelSize.x : _scrollPanelSize.y;
		float contentSize = _horizontal ? ItemWidth * Mathf.Ceil((float)_itemCount / (float)MaxPerLine) : ItemHeight * Mathf.Ceil((float)_itemCount / (float)MaxPerLine);
		float amount = percentage * (panelSize - contentSize);
		ScrollAbsolute(amount, time, finishedCallback);
    }

	private void ScrollAbsolute (float amount, float time, Action finishedCallback)
	{
		if (_scrollingSequence != null)
		{
			_scrollingSequence.Kill();
			_scrollingSequence = null;
		}

		float panelSize = _horizontal ? _scrollPanelSize.x : _scrollPanelSize.y;
		float contentSize = _horizontal ? ItemWidth * Mathf.Ceil((float)_itemCount / (float)MaxPerLine) : ItemHeight * Mathf.Ceil((float)_itemCount / (float)MaxPerLine);
		// Figoure out how much stuff is currently offscreen, by subtracting the panel size from the content size
		amount = Mathf.Min(Mathf.Clamp(amount, panelSize - contentSize, 0.0f), 0.0f);
		time = Mathf.Max(time, 0.0f);

		Vector3 finalTransformPosition = _startingTransformPosition + (_horizontal ? new Vector3(amount, 0.0f, 0.0f) : new Vector3(0.0f, -amount, 0.0f));
		Vector2 finalClippingOffset = _panel.clipOffset;
		finalClippingOffset.x = _startingClipPosition.x - (_horizontal ? amount : 0.0f);
		finalClippingOffset.y = _startingClipPosition.y - (_horizontal ? 0.0f : -amount);

        if (Mathf.Approximately(time, 0.0f) || Mathf.Approximately(amount, 0.0f))
		{
			_panel.transform.localPosition = finalTransformPosition;
			_panel.clipOffset = finalClippingOffset;
            WrapContent();

			if (finishedCallback != null)
			{
				finishedCallback();
			}
		}
		else
		{
			_scrollingSequence = DOTween.Sequence().
				Append(_panel.transform.DOLocalMove(finalTransformPosition, time)).
				// !!! Требует проверки
				Join(DOTween.To(() => _panel.clipOffset, newClipOffset => _panel.clipOffset = newClipOffset, finalClippingOffset, time)).
				SetEase(Ease.InOutCubic).
				AppendCallback(() => 
				{
					if (finishedCallback != null)
					{
						finishedCallback();
					}
				}).
				Play();
		}
	}

	/// <summary>
	/// Adjusts the scroll position so that the item at the desired list index is centered in the scroll view. Can
	/// also be animated over time.
	/// </summary>
	public void ScrollToElement (int index, float time = 0.0f, Action finishedCallback = null)
	{
		index = Mathf.Clamp(index, 0, _itemCount);
		int line = Mathf.FloorToInt((float)index / (float)_maxPerLine);

		ScrollAbsolute(_horizontal ? 
			-line * _itemWidth + (_scrollPanelSize.x - _itemWidth) / 2.0f : 
	        -line * _itemHeight + (_scrollPanelSize.y - _itemHeight) / 2.0f, time, finishedCallback);
    }
    
    /// <summary>
    /// Aborts a pending scroll sequence.
    /// </summary>
    public void CancelScrollToElement()
    {
        if ( _scrollingSequence != null )
        {
            _scrollingSequence.Kill();
            _scrollingSequence = null;
        }
    }

    private bool IndexIsInRange (int index)
	{
        // Should index < 0 log an error?
		return index >= 0 && (index < _itemCount || _unlimitedItems);
	}

	private int ToRealIndex (Vector3 pos)
	{
		var output = _horizontal ? 
			  Mathf.RoundToInt((pos.x - _offset.x) / _itemWidth) * MaxPerLine - Mathf.RoundToInt((pos.y - _offset.y) / _itemHeight) : 
			-(Mathf.RoundToInt((pos.y - _offset.y) / _itemHeight) * MaxPerLine - Mathf.RoundToInt((pos.x - _offset.x) / _itemWidth));
		return output;
	}

	private void OnValidate ()
	{
		_itemCount = Mathf.Max(0, _itemCount);
		_maxPerLine = Mathf.Max(1, _maxPerLine);
		_itemHeight = Mathf.Max(1, _itemHeight);
		_itemWidth = Mathf.Max(1, _itemWidth);
	}

	protected virtual void UpdateItem (Transform item, int index)
	{
		if (onInitializeItem != null)
		{
			bool isInRange = IndexIsInRange(ToRealIndex(item.localPosition));
			item.gameObject.SetActive(isInRange);
			if (isInRange)
			{
				onInitializeItem(item.gameObject, index, ToRealIndex(item.localPosition));
			}
		}
	}

	public void ForceItemInitialize ()
	{
		foreach (Transform child in _children)
		{
			UpdateItem(child, ToRealIndex(child.localPosition));
		}
	}

	private void Update ()
	{
		Vector2 newScrollPanelSize = _scroll.panel.GetViewSize();
		if (newScrollPanelSize != _scrollPanelSize)
		{
			_scrollPanelSize = newScrollPanelSize;
			if (onPanelSizeChanged != null)
			{
				onPanelSizeChanged();
			}
		}
	}

    // Since we've implemented our own handling of scroll bars in this class,
    // we also need to implement auto-hiding scroll bars.  This function mimics UIScrollView's LateUpdate()
    // with the variable ShowCondition showScrollBars = ShowCondition.WhenDragging
    void LateUpdate ()
	{
		if (!Application.isPlaying) return;
		float delta = RealTime.deltaTime;

		// Fade the scroll bars if needed
		if (_scrollbar)
		{
			float alpha = _scrollbar.alpha;
			alpha += _dragging ? delta * 6f : -delta * 3f;
			alpha = Mathf.Clamp01(alpha);
			if (_scrollbar.alpha != alpha) _scrollbar.alpha = alpha;
		}
	}
}
