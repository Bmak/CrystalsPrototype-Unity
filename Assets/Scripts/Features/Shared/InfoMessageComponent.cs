using System;
using UnityEngine;
using System.Collections;

public class InfoMessageComponent : MonoBehaviour, ILoggable
{
	[SerializeField]
	private UILabel _label;

	private UITweener _alphaTweener;
	private UITweener _positionTweener;

	private void Awake()
	{		
		_positionTweener = _label.GetComponentInChildren<TweenPosition>();
		_alphaTweener = _label.GetComponentInChildren<TweenAlpha>();

		_positionTweener.SetOnFinished(new EventDelegate(this, "OnFinished_Position"));
		_alphaTweener.SetOnFinished(new EventDelegate(this, "OnFinished_Alpha"));
	}

	public void Initialize(string text)
	{
		_label.text = text;
	}

	private void OnFinished_Position()
	{
		if (TweenPosition.current.direction == AnimationOrTween.Direction.Forward)//shown
		{
			_alphaTweener.PlayReverse();
		}
		else if (TweenPosition.current.direction == AnimationOrTween.Direction.Reverse)//hided
		{
		}
	}

	private void OnFinished_Alpha()
	{
		if (TweenPosition.current.direction == AnimationOrTween.Direction.Forward)//shown
		{
		}
		else if (TweenPosition.current.direction == AnimationOrTween.Direction.Reverse)//hided
		{
			Destroy(gameObject);
		}
	}
}
