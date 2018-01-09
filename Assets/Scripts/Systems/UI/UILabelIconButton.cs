using UnityEngine;
using System.Collections;
using System;

public class UILabelIconButton : UILabelButton {
    [SerializeField]
    private UILabel _amount;

    [SerializeField]
    private UISprite _icon;

    public int? Amount
    {
        get
        {
            if ( _amount != null ) {
                int amount;
                if ( int.TryParse(_amount.text, out amount) ) {
                    return amount;
                }
            }

            return null;
        }

        set
        {
            if ( _amount != null ) {
                if ( value.HasValue ) {
                    _amount.text = value.ToString();
                } else {
                    _amount.text = string.Empty;
                }
            }
        }
    }

	public void SetAmountLabel (string text)
	{
		_amount.text = text;
	}

    public void ShowSprite(bool show)
    {
        if (_icon != null) {
            _icon.gameObject.SetActive(show);
        }
    }

    public string Sprite
    {
        get
        {
            if ( _icon != null ) {
                return _icon.spriteName;
            }

            return string.Empty;
        }

        set
        {
            if ( _icon != null ) {
                _icon.spriteName = value;
            }
        }
    }

    public void ShowCost(bool show)
    {
        _amount.gameObject.SetActive(show);
        _icon.gameObject.SetActive(show);
    }
}
