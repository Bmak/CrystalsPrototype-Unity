using UnityEngine;
using System;

public class TwoButtonTapDismissableMessageView : NguiView {

	[SerializeField]
	private UILabelButton _leftButton;

	[SerializeField]
	private UILabelButton _rightButton;
	
	[SerializeField]
	private UIButton _backgroundScrimButton;

	[SerializeField]
	private UILabel _title;
	
	[SerializeField]
	private UILabel _message;
	
	public override DepthEnum InitialDepth { get { return DepthEnum.Message; } }

	public Action LeftButtonClick;
	public Action RightButtonClick;
    public Action BackgroundScrimClick;
	
	protected override void WireWidgets()
	{
		base.WireWidgets();
		
		EventDelegate.Add(_leftButton.onClick, LeftButtonClicked);
		EventDelegate.Add(_rightButton.onClick, RightButtonClicked);
		EventDelegate.Add(_backgroundScrimButton.onClick, BackgroundScrimButtonClicked);
	}
	
	private void LeftButtonClicked()
	{
		if(LeftButtonClick != null) {
			LeftButtonClick();
		}
	}

	private void RightButtonClicked()
	{
		if(RightButtonClick != null) {
			RightButtonClick();
		}
	}
	
	private void BackgroundScrimButtonClicked()
	{
        if (BackgroundScrimClick != null) {
            BackgroundScrimClick();
        }
	}

    public override void OnBackClick()
    {
        if (BackgroundScrimClick != null) {
            BackgroundScrimClick();
        }
    }

	public void Initialize(string title, string message, string leftButtonText, string rightButtonText)
	{
		_title.text = title;
		_message.text = message;
		_leftButton.Text = !string.IsNullOrEmpty(leftButtonText) ? leftButtonText : _localizationManager.Localize(_lc.GetMessageViewCancel());
		_rightButton.Text = !string.IsNullOrEmpty(rightButtonText) ? rightButtonText : _localizationManager.Localize(_lc.GetMessageViewOk());
	}

	protected override void OnRelease()
	{
		EventDelegate.Remove(_leftButton.onClick, LeftButtonClicked);
		EventDelegate.Remove(_rightButton.onClick, RightButtonClicked);
		EventDelegate.Remove(_backgroundScrimButton.onClick, BackgroundScrimButtonClicked);
		base.OnRelease();
	}
}
