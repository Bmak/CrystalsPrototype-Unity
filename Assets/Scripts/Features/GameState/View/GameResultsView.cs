using System;
using UnityEngine;

public class GameResultsView : MonoBehaviour
{
    [SerializeField] private UIWidget _window;
    [SerializeField] private UIButton _menuButton;
    [SerializeField] private UILabel _scoreLabel;

    private TweenScale _tweetScale;
    private Action _getToMenu;

    private void Awake()
    {
        _tweetScale = _window.GetComponent<TweenScale>();
    }
    public void InitializeViewData(Action getToMenu)
    {
        _getToMenu = getToMenu;
        WireWidgets();
    }

    public void SetScore(int score)
    {
        _scoreLabel.text = String.Format("You scored:\n{0}", score);
    }

    public void WireWidgets()
    {
        EventDelegate.Add(_menuButton.onClick, OnCloseWindow);
    }

    private void OnDestroy()
    {
        EventDelegate.Remove(_menuButton.onClick, OnCloseWindow);
    }

    private void OnCloseWindow()
    {
        _tweetScale.AddOnFinished(OnGetToMenu);
        _tweetScale.PlayReverse();
    }

    private void OnGetToMenu()
    {
        if (_getToMenu != null)
        {
            _getToMenu();
        }
    }
}
