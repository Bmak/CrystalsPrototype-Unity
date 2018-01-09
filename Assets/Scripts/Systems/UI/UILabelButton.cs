using System;
using UnityEngine;

/// <summary>
/// Custom button component that provides access to a nested label's text
/// </summary>
public class UILabelButton : UIDisableClickableButton
{
    [SerializeField]
    protected UILabel _label;

    public string Text
    {
        get {
            if (_label != null) {
                return _label.text;
            } else {
                return string.Empty;
            }
        }

        set {
            if (_label != null) {
                _label.text = value;
            }
        }
    }
}