using UnityEngine;

public enum BonusType
{
    Bomb = 1,
    Time = 2,
    Color = 3,
    BlackHole = 4,
    SuperCube = 5
}

public class BonusPanelView : MonoBehaviour
{
    [SerializeField] private UISprite _icon;
    [SerializeField] private UILabel _bonusLabel;
    [SerializeField] private UIButton _addBonusButton;
    [SerializeField] private UILabel _bonusCountLabel;
    [SerializeField] public BonusType Type;

    public int Count { get; set; }

    private void Start()
    {
        EventDelegate.Add(_addBonusButton.onClick, OnAddBonus);
    }

    private void OnDestroy()
    {
        EventDelegate.Remove(_addBonusButton.onClick, OnAddBonus);
    }

    private void OnAddBonus()
    {
        Count++;
        _bonusCountLabel.text = Count.ToString();
    }
}
