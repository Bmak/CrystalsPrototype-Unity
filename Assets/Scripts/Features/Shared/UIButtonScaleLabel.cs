using UnityEngine;

class UIButtonScaleLabel: MonoBehaviour
{
    [SerializeField]
    private UILabel _label;
    [SerializeField]
    private UISprite _sprite;
    [SerializeField]
    private Vector3 _sizeSpritePressed;
    [SerializeField]
    private int _pressed;
    [SerializeField]
    private Color _colorPressed;
    [SerializeField]
    private float _offsetPressed = -2;
    

    private int _initSize;
    private Color _initColor;
    private Vector3 _initPosition;
    private Vector3 _initSizeSprite;

    void Start()
    {
        if (_sprite != null)
        {
            _initSizeSprite = _sprite.transform.localScale;
        }
        if (_label != null)
        {
            _initSize = _label.fontSize;
            _initColor = _label.color;
            _initPosition = _label.transform.localPosition;
        }
    }

    void OnPress(bool isPressed)
    {
        UIButton button = GetComponentInChildren<UIButton>();

        if (button.isEnabled)
        {
            if (_label != null)
            {
                _label.fontSize = isPressed ? _pressed : _initSize;
                _label.color = isPressed ? _colorPressed : _initColor;
                Vector3 offsetPos = _initPosition;
                offsetPos.y += _offsetPressed;
                _label.transform.localPosition = isPressed ? offsetPos : _initPosition;
            }
            if (_sprite != null)
            {
                _sprite.transform.localScale = isPressed ? _sizeSpritePressed : _initSizeSprite;
            }
        }
    }
}

