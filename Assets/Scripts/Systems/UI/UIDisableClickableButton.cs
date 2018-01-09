using System;
using UnityEngine;

/// <summary>
/// Button component that provides a way to be put into the disabled state but still be clickable
/// </summary>
public class UIDisableClickableButton : UIButton
{
    private bool _disabledAndClickable = false;
	public bool disableColors = false;

    public bool DisabledAndClickable
    {
        get {
            return _disabledAndClickable;
        }
        set {
            bool enablingButton = (_disabledAndClickable && !value);
            _disabledAndClickable = value;
            if (enablingButton) {
                // when turning off the disabled-clickable mode, force the state back to normal
                SetState(State.Normal, immediate:true);
            } else {
                SetState(mState, immediate:true);
            }
        }
    }

    // override to always set state to disabled if we're in the DisabledAndClickable mode
    public override void SetState (State state, bool immediate)
    {
        if (_disabledAndClickable) {
			state = State.Disabled;
		}

        base.SetState(state, immediate);
    }

    // button's default behavior is to always set the button back to default state on disable,
    // (assuming this is to account for killing a hover tween or some such), so in our case
    // we'll choose normal or disabled based on the DisabledAndClickable mode
    protected override void OnDisable ()
    {
        if (this.enabled == true) {
            // OnDisable is calling, when object is destroying.
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        if (mInitDone)
        {
            if (_disabledAndClickable) {
                SetState(State.Disabled, true);
            } else {
                SetState(State.Normal, true);
            }
        }
    }

	public override void UpdateColor (bool instant)
	{
		if (!disableColors) {
			base.UpdateColor(instant);
		}
	}
}
