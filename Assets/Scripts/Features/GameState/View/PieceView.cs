using System;
using DG.Tweening;
using UnityEngine;

public enum PieceType
{
    NONE = 0,
	SIMPLE_1 = 1,
	SIMPLE_2 = 2,
	SIMPLE_3 = 3,
	SIMPLE_4 = 4,
	SIMPLE_5 = 5,
	//SIMPLE_6 = 6
    FRIEND_DIAMOND = 7,
    MAGIC_DIAMOND = 8,
}

public enum DiamondBarState
{
    Full = 50,
    Match = 1,
    Pause = -1,
    Miss = -10
}

public class PieceView : MonoBehaviour, ILoggable
{
	[SerializeField] private UISprite _spriteIcon;
	[SerializeField] private UISprite _spriteBonus;
	[SerializeField] private UISprite _spriteBonusBorder;

	public PieceDO GetData { get; private set; }
    public CellView Cell { get; set; }
    public bool IsMoving { get; set; }
    public bool HasBonus { get; set; }
    public BonusType BonusType { get; set; }

    private void Awake()
    {
        _spriteBonus.gameObject.SetActive(false);
        _spriteBonusBorder.gameObject.SetActive(false);
    }

	public void Initialize(PieceDO renderData)
	{
		GetData = renderData;
        _spriteIcon.spriteName = GetData.Sprite;

		GetData.View = this;
	}

	public void Change()
	{
        _spriteIcon.transform.localScale = Vector3.one;
        _spriteIcon.spriteName = GetData.Sprite;
		GetData.Checked = false;
    }

    public void ShowRemove()
    {
        //TODO This system is not good need pool for repeating effects
        GameObject effect = Instantiate(gameObject, transform.parent) as GameObject;
        effect.transform.position = transform.position;
        effect.transform.localPosition = transform.localPosition;
        effect.transform.DOScale(0, 0.2f).OnComplete(() => Destroy(effect));

        if (_spriteBonus.isActiveAndEnabled)
        {
            _spriteBonus.gameObject.SetActive(false);
            _spriteBonusBorder.gameObject.SetActive(false);
            HasBonus = false;
        }
    }

    public void ActivateBonusBorder(bool state)
    {
        _spriteBonusBorder.gameObject.SetActive(state);
    }

    public void SetBonus(BonusType type)
    {
        _spriteBonus.spriteName = String.Format("ui.bonus{0}", (int)type);
        _spriteBonus.gameObject.SetActive(true);
        _spriteBonusBorder.gameObject.SetActive(true);
        //_spriteBonus.transform.localScale = Vector3.one;
        //DOTween.Complete(_spriteBonus.transform);
        //_spriteBonus.transform.DOScale(1, 0.2f).SetEase(Ease.OutBack);
        HasBonus = true;
        BonusType = type;
    }

    public void RemoveBonus(Action onFinish = null)
    {
        _spriteBonus.gameObject.SetActive(false);
        HasBonus = false;
        _spriteBonusBorder.gameObject.SetActive(false);

        //TODO add animations
        if (onFinish != null)
            onFinish();
    }
}