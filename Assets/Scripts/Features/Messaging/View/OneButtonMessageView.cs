using System;
using UnityEngine;

public class OneButtonMessageView : NguiView, ICountdownMessageView
{
	[SerializeField]
	private UILabelButton _centerButton;

	[SerializeField]
	private UIButton _backgroundScrimButton;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private UILabel _message;

    private bool _enableScrimButton;
	
	public override DepthEnum InitialDepth { get { return DepthEnum.Message; } }

	public Action CenterButtonClick;

    private CountdownMessageViewComponent _countdownComponent = new CountdownMessageViewComponent();

	protected override void WireWidgets()
	{
		base.WireWidgets();
		
		EventDelegate.Add(_centerButton.onClick, CentreButtonClicked);
		EventDelegate.Add(_backgroundScrimButton.onClick, BackgroundScrimButtonClicked);
	}
	
	private void CentreButtonClicked()
	{
		if(CenterButtonClick != null) {
			CenterButtonClick();
		}
	}
	
	private void BackgroundScrimButtonClicked()
	{
		if(_enableScrimButton && CenterButtonClick != null) {
			CenterButtonClick();
		}
	}
    
    public override void OnBackClick()
    {
        if(CenterButtonClick != null) {
            CenterButtonClick();
        }
    }

    public void Initialize(string title, string message, string buttonText, bool enableScrimButton = true)
    {
        _title.text = title;
        _message.text = message;
		_centerButton.Text = !string.IsNullOrEmpty(buttonText) ? buttonText : _localizationManager.Localize(_lc.GetMessageViewOk());
        _enableScrimButton = enableScrimButton;

        _countdownComponent.Initialize(this);
	}

	protected override void OnRelease()
	{
        _countdownComponent.Shutdown();

		EventDelegate.Remove(_centerButton.onClick, CentreButtonClicked);
		EventDelegate.Remove(_backgroundScrimButton.onClick, BackgroundScrimButtonClicked);
		base.OnRelease();
	}

    public void StartUpdateFunction(Func<ICountdownMessageView, bool> updateAction, Action timerCompleteAction = null)
    {
        _countdownComponent.StartUpdateFunction(updateAction, timerCompleteAction);
    }

    public void SetMessage (string message)
	{
		_message.text = message;
	}

    public void SetTitle(string title)
    {
        _title.text = title;
    }
}
