using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HomeBaseView : NguiView
{
    [Header("Containers")]
    [SerializeField] private UIWidget _lobbyContainer;
    [SerializeField] private UIWidget _chooseBonusContainer;

    [Header("Lobby")]
	[SerializeField] private UIButton _playButton;
	[SerializeField] private UIButton _fbButton;
    [SerializeField] private UILabel _maxScoreLabel;
    [SerializeField] private UILabel _lastGameScore;
    [SerializeField] private UILabel _helloPlayerLabel;
    [SerializeField] private UI2DSprite _playerAvatar;

    [Header("BonusPanel")]
    [SerializeField] private List<BonusPanelView> _bonuses;
    [SerializeField] private UIButton _closeBonusPanelButton;
    [SerializeField] private UIButton _startGameButton;

    [Header("Fade In/Out")]
    [SerializeField] private TweenAlpha _tweenFadeIn;
    [SerializeField] private TweenAlpha _tweenFadeOut;


    private Action _startGame;
	private Action _connectFacebook;

	public void InitializeViewData(HomeBaseRenderData data)
	{
		_startGame = data.StartGame;
	    _connectFacebook = data.ConnectFacebook;

        _maxScoreLabel.text = String.Format("{0}", data.MaxScore);
        _lastGameScore.text = String.Format("{0}", data.LastGameScore);

        _chooseBonusContainer.gameObject.SetActive(false);
        _lobbyContainer.gameObject.SetActive(true);
    }

    public void SetHelloPlayer(string nickname)
    {
        _helloPlayerLabel.gameObject.SetActive(true);
        _helloPlayerLabel.text = String.Format("Hello, {0}", nickname);
    }

    public void SetPlayerAvatar(Texture2D tex)
    {
        _playerAvatar.sprite2D = Sprite.Create(tex,new Rect(0,0, tex.width, tex.height), new Vector2(0,0));
    }

	protected override void WireWidgets()
	{
		base.WireWidgets();
		EventDelegate.Add(_playButton.onClick, OnShowBonusPanel);
		EventDelegate.Add(_fbButton.onClick, OnConnectFB);
		EventDelegate.Add(_closeBonusPanelButton.onClick, OnCloseBonusPanel);
		EventDelegate.Add(_startGameButton.onClick, OnStartGame);
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		EventDelegate.Remove(_playButton.onClick, OnShowBonusPanel);
		EventDelegate.Remove(_fbButton.onClick, OnConnectFB);
		EventDelegate.Remove(_closeBonusPanelButton.onClick, OnCloseBonusPanel);
		EventDelegate.Remove(_startGameButton.onClick, OnStartGame);
	}

    private void OnCloseBonusPanel()
    {
        _chooseBonusContainer.gameObject.SetActive(false);
        _lobbyContainer.gameObject.SetActive(true);
    }

    private void OnShowBonusPanel()
    {
        _lobbyContainer.gameObject.SetActive(false);
        _chooseBonusContainer.gameObject.SetActive(true);
    }

    private void OnStartGame()
	{
		if (_startGame != null)
		{
			_startGame();
		}
	}

    private void OnConnectFB()
    {
        if (_connectFacebook != null)
        {
            _connectFacebook();
        }
    }

    public void FadeIn(EventDelegate.Callback onFinish)
    {
        _tweenFadeIn.AddOnFinished(onFinish);
        _tweenFadeIn.PlayForward();
    }

    public void FadeOut()
    {
        _tweenFadeOut.PlayForward();
    }

    public List<BonusType> GetBonusTypes()
    {
        List<BonusType> result = new List<BonusType>();
        foreach (BonusPanelView bonus in _bonuses)
        {
            if (bonus.Count > 0)
                result.Add(bonus.Type);
        }
        return result;
    }
}