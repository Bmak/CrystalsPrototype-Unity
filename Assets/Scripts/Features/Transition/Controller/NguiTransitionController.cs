using System;

public enum TransitionType
{
    FastTransition,
    BusyWait,
    LoadingScreen
}

/// <summary>
/// Manages transition views for the NGUI ui
/// Each of the transition views should have a full screen collider built in to handle blocking input
/// </summary>
public class NguiTransitionController : ILoggable, IInitializable
{
    [Inject]
    private ViewProvider _viewProvider;

    private NguiBusyWaitView _busyWaitView;
    private NguiFastTransitionView _fastTransitionView;
    private NguiLoadingView _loadingView;

    public void Initialize(InstanceInitializedCallback initializedCallback)
    {
        AsyncJobTracker loadTracker = new AsyncJobTracker(3, () => {
            if (initializedCallback != null) {
                initializedCallback(this);
            }
        }, null);

        _viewProvider.Get<NguiBusyWaitView>( (view) => {
            _busyWaitView = view;
            loadTracker.Success();
        });

        _viewProvider.Get<NguiFastTransitionView>( (view) => {
            _fastTransitionView = view;
            loadTracker.Success();
        });

        _viewProvider.Get<NguiLoadingView>( (view) => {
            _loadingView = view;
            loadTracker.Success();
        });
    }

    /// <summary>
    /// Returns true if any transition screen is currently in use
    /// </summary>
    public bool IsAnyTransitionActive()
    {
        if (_busyWaitView != null && _busyWaitView.ViewActive) {
            return true;
        }
        if (_fastTransitionView != null && _fastTransitionView.ViewActive) {
            return true;
        }
        if (_loadingView != null && _loadingView.ViewActive) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Hides all transition views
    /// </summary>
    public void HideAllTransitions(Action onTransitionHidden = null)
    {
        AsyncJobTracker transitionHideTracker = new AsyncJobTracker(3, () => {
            if (onTransitionHidden != null) {
                onTransitionHidden();
            }
        }, null);

        HideFastTransition( () => { transitionHideTracker.Success(); } );
        HideBusyWait( () => { transitionHideTracker.Success(); } );
        HideLoadingScreen( () => { transitionHideTracker.Success(); } );
    }

    /// <summary>
    /// Show transition screen specified in parameter, see individual methods below for description
    /// </summary>
    public void ShowTransition(TransitionType transitionType)
    {
        switch(transitionType) {
            case TransitionType.FastTransition:
                ShowFastTransition();
                break;
            case TransitionType.BusyWait:
                ShowBusyWait();
                break;
            case TransitionType.LoadingScreen:
                ShowLoadingScreen();
                break;
        }
    }

    /// <summary>
    /// The fast transition view should be used any time you need to perform an async operation that you expect will be very fast
    /// The view does not have a full screen image, but will pop a spinner after a couple seconds, the idea being that if the operation
    /// finishes quickly, we didn't "blip" anything on screen
    /// </summary>
    public void ShowFastTransition()
    {
        if (_fastTransitionView == null) {
            this.LogWarning("Fast transition view not ready!");
            return;
        }

        HideAllTransitions(() => _fastTransitionView.SetViewActive(true));
    }

    public void HideFastTransition(Action onTransitionHidden = null)
    {
        if (_fastTransitionView != null) {
            _fastTransitionView.SetViewActive(false, onTransitionHidden);
        }
    }

    /// <summary>
    /// The busy wait transition should be used when you need to perform an async operation that you expect will take some time, or if the ux
    /// flow would benefit from immediately putting something on screen to indicate to the user that their request was received and is in progress
    /// This transition is intended to have a "lighter" look and will likely be semi-transparent, so this should not be used when you need to hide
    /// game objects popping in and repositioning
    /// </summary>
    public void ShowBusyWait()
    {
        if (_busyWaitView == null) {
            this.LogWarning("Busy wait overlay not ready!");
            return;
        }

        HideAllTransitions(() => _busyWaitView.SetViewActive(true));
    }

    public void HideBusyWait(Action onTransitionHidden = null)
    {
        if (_busyWaitView != null) {
            _busyWaitView.SetViewActive(false, onTransitionHidden);
        }
    }

    /// <summary>
    /// The loading screen is fully opaque and should be used when starting long load operations
    /// </summary>
    public void ShowLoadingScreen()
    {
        if (_loadingView == null) {
            this.LogWarning("Loading screen not ready!");
            return;
        }

        HideAllTransitions(() => _loadingView.SetViewActive(true));
    }

    /// <summary>
    /// Will hide the loading screen, but not do it immediately.  Will trigger an animation out first.
    /// </summary>
    public void HideLoadingScreen(Action onTransitionHidden = null)
    {
        if (_loadingView != null) {
            _loadingView.SetViewActive(false, onTransitionHidden);
        }
    }

    /// <summary>
    /// Will immediately hide the loading screen without an out animation.
    /// </summary>
    public void HideLoadingScreenImmediate(Action onTransitionHidden = null)
    {
        if (_loadingView != null) {
            _loadingView.SetViewActive(false, onTransitionHidden);
        }
    }
}
