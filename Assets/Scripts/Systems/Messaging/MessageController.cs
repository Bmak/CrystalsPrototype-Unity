using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// A controller that handles showing message views
public class MessageController : FeatureController, ILifecycleAware, IInitializable
{
    [Inject]
    private LocalizationManager _localizationManager;

    [Inject]
    private LocalizationConfig _localizationConfig;

	[Inject]
	private NumberFormatUtil _numberFormatUtil;

    //
    // IInitializable
    public void Initialize( InstanceInitializedCallback initializedCallback = null )
    {
        if (initializedCallback != null) {
            initializedCallback(this);
        }
    }

    // ILifecycleAware
    public void Reset()
    {
	}

    public void ShowMessage(
        string title,
        string message,
        Action callback = null,
        string buttonText = null,
        bool enableScrimButton = true)
    {
        _uiSystem._setInputEnabled(false); // special case for dialogs, instead of using a transition view to block input during load, we'll disable input globablly
                                           // so that we don't interfere with any usage of transition views by the caller
		_viewProvider.Get<OneButtonMessageView>( (OneButtonMessageView view) => {
            SetupMessageView(title, message, callback, buttonText, enableScrimButton, view);
		});
    }

    private void SetupMessageView(string title, string message, Action callback, string buttonText, bool enableScrimButton, OneButtonMessageView view)
    {
        view.Initialize(title, message, buttonText, enableScrimButton);
        view.SetViewActive(true);
        view.CenterButtonClick = () => {
            if (callback != null) {
                callback();
            }
			view.SetViewActive(false);
			view.DeactivateAndRelease();
        };
    }

    public void ShowCountdownMessage(
        string title,
        string message,
        Func<ICountdownMessageView, bool> updateFunction,
        Action callback = null,
        string buttonText = null,
        bool enableScrimButton = true,
        Action timerCompleteAction = null)
    {
        _uiSystem._setInputEnabled(false); // special case for dialogs, instead of using a transition view to block input during load, we'll disable input globablly
                                           // so that we don't interfere with any usage of transition views by the caller


		_viewProvider.Get<OneButtonMessageView>( (OneButtonMessageView view) => {
		    SetupMessageView(title, message, callback, buttonText, enableScrimButton, view);
            view.StartUpdateFunction(updateFunction, timerCompleteAction);
		});
    }

    public void ShowConfirmMessage(
    string title,
    string message,
    Action yesCallback = null,
    Action noCallback = null,
    string confirmYesString = null,
    string confirmNoString = null)
    {
        _uiSystem._setInputEnabled(false); // special case for dialogs, instead of using a transition view to block input during load, we'll disable input globablly
        // so that we don't interfere with any usage of transition views by the caller
        _viewProvider.Get<TwoButtonMessageView>((TwoButtonMessageView view) =>
        {
            view.Initialize(title, message, confirmNoString, confirmYesString);
            view.SetViewActive(true);
            view.LeftButtonClick = () =>
            {
                if (noCallback != null)
                {
                    noCallback();
                }
				view.SetViewActive(false);
                view.DeactivateAndRelease();
            };
            view.RightButtonClick = () =>
            {
                if (yesCallback != null)
                {
                    yesCallback();
                }
				view.SetViewActive(false);
				view.DeactivateAndRelease();
            };
        });
    }

    public void ShowDismissableConfirmMessage(
        string title,
        string message,
        Action yesCallback = null,
        Action noCallback = null,
        Action backgroundScrimCallback = null,
        string confirmYesString = null,
        string confirmNoString = null)
    {
        _uiSystem._setInputEnabled(false); // special case for dialogs, instead of using a transition view to block input during load, we'll disable input globablly
                                           // so that we don't interfere with any usage of transition views by the caller
		_viewProvider.Get<TwoButtonTapDismissableMessageView>( (TwoButtonTapDismissableMessageView view) => {
			view.Initialize(title, message, confirmNoString, confirmYesString);
			view.SetViewActive(true);
            view.BackgroundScrimClick = () =>
            {
                if (backgroundScrimCallback != null)
                {
                    backgroundScrimCallback();
                }
				view.SetViewActive(false);
				view.DeactivateAndRelease();
            };
            view.LeftButtonClick = () =>
            {
                if (noCallback != null)
                {
                    noCallback();
                }
				view.SetViewActive(false);
				view.DeactivateAndRelease();
            };
			view.LeftButtonClick = () => {
                if(noCallback != null ) {
                    noCallback(); 
                }
				view.SetViewActive(false);
				view.DeactivateAndRelease(); 
			};
			view.RightButtonClick = () => {
                if(yesCallback != null ) {
                    yesCallback(); 
                }
				view.SetViewActive(false);
                view.DeactivateAndRelease(); 
			};
		});
    }
/*
	public void ShowBuyMessage(
		string title,
		string message,
		int value,
		CurrencyTypeEnum currencyType,
		Action yesCallback = null,
		Action noCallback = null,
		string confirmYesString = null,
		string confirmNoString = null)
	{
        _uiSystem._setInputEnabled(false); // special case for dialogs, instead of using a transition view to block input during load, we'll disable input globablly
                                           // so that we don't interfere with any usage of transition views by the caller
		_viewProvider.Get<BuyMessageView>( (BuyMessageView view) => {
			SetupBuyMessageView(title, message, value, currencyType, yesCallback, noCallback, confirmYesString, confirmNoString, view);
		});
	}

    private void SetupBuyMessageView(string title, string message, int value, CurrencyTypeEnum currencyType, Action yesCallback, Action noCallback, string confirmYesString, string confirmNoString, BuyMessageView view)
    {
        view.Initialize(title, message, confirmNoString, confirmYesString, _numberFormatUtil.Format (NumberFormatUtil.FormatType.COMMA_NO_DECIMAL, value), _inventoryDC.GetCurrencyQuantity(currencyType).ToString(), currencyType);
        view.SetViewActive(true);
        view.LeftButtonClick = () => {
            if (noCallback != null) {
                noCallback();
            }
            view.Release();
        };
        view.RightButtonClick = () => {
            if (yesCallback != null) {
                yesCallback();
            }
            view.Release();
        };
    }

    /// <summary>
    /// Show a buy message view that can have a function called from within a coroutine
    /// </summary>
    /// <param name="updateFunction">A function that has BuyMessageView as a parameter and returns whether the coroutine should continue being called.</param>
    public void ShowBuyCountdownMessage(string title,
		string message,
		int value,
		CurrencyTypeEnum currencyType,
        Func<ICountdownMessageView, bool> updateFunction,
		Action yesCallback = null,
		Action noCallback = null,
		string confirmYesString = null,
		string confirmNoString = null,
        Action timerCompleteAction = null)
    {
        _uiSystem._setInputEnabled(false); // special case for dialogs, instead of using a transition view to block input during load, we'll disable input globablly
                                           // so that we don't interfere with any usage of transition views by the caller
		_viewProvider.Get<BuyMessageView>( (BuyMessageView view) => {
		    SetupBuyMessageView(title, message, value, currencyType, yesCallback, noCallback, confirmYesString, confirmNoString, view);
            view.StartUpdateFunction(updateFunction, timerCompleteAction);
		});
    }
*/	
    // For now our error message is exactly the same as the regular message
    // But having this function allows us to adapt the message view to show error icons or whatever
    // if we want to in the future
    public void ShowErrorMessage(
        string title,
        string message,
        Action callback = null,
        string buttonText = null,
        bool enableScrimButton = true)
    {
        ShowMessage( title, message, callback, buttonText, enableScrimButton );
    }
/*
	public void ShowGameServiceSignInMessage(
		string title,
		string message,
		Action gameServiceCallback = null,
		Action guestCallback = null,
		string gameServiceString = null,
		string guestString = null)
	{
        _uiSystem._setInputEnabled(false); // special case for dialogs, instead of using a transition view to block input during load, we'll disable input globablly

	    _progressiveDownloadController.BlockingDownload(new List<AssetSet>{ AssetSet.Shared, AssetSet.GameServiceSignIn}, () => {
            // so that we don't interfere with any usage of transition views by the caller
		    _viewProvider.Get<GameServiceSignInMessageView>((GameServiceSignInMessageView view) => {
			    view.Initialize(title, message, guestString, gameServiceString);
			    view.SetViewActive(true);
			    view.GuestButtonClick = () => {
				    if (guestCallback != null) {
					    guestCallback();
				    }
				    view.Release();
			    };
			    view.GameServiceButtonClick = () => {
				    if (gameServiceCallback != null) {
					    gameServiceCallback();
				    }
				    view.Release();
			    };
		    });
	    });
	}
*/
}
