
    using System;
    using System.Collections.Generic;
    using Facebook.Unity;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;

/// <summary>
///     Feature controller which controls behavior for the home base
/// </summary>
public class HomeBaseController : FeatureController, IOccludable
{
    [Inject] private FacebookDomainController _facebookDC;
    private HomeBaseView _lobbyView;

    [Inject] private NetworkSystem _networkSystem;

    [Inject] private PlayerRecordDomainController _playerDC;

    [Inject] private PlayerService _playerService;

    private HomeBaseTransitionInfo _transitionInfo;

    void IOccludable.OnOccluded()
    {
        if (_lobbyView != null)
        {
            _lobbyView.SetViewActive(false);
        }
    }

    void IOccludable.OnRevealed()
    {
        // Need to null check views here because if we deep link to certain areas (unit details),
        // we use the home base game state as a proxy since unit details has no state.
        // And if this gets called in that situation, home base view will be null

        if (_lobbyView != null)
        {
            _lobbyView.SetViewActive(true);
        }
    }

    public void Initialize(HomeBaseTransitionInfo transitionInfo)
    {
        Initialize();

        _transitionInfo = transitionInfo;
        LoadResources();
    }

    private void LoadResources()
    {
        _viewProvider.Get<HomeBaseView>(view =>
        {
            _lobbyView = view;

            var data = new HomeBaseRenderData();
            data.StartGame = OnInGameButtonClicked;
            data.ConnectFacebook = OnConnectFacebook;
            data.MaxScore = _playerDC.GetCurrentMaxScore();
            data.LastGameScore = _playerDC.GetLastGameScore();

            _lobbyView.InitializeViewData(data);
            _lobbyView.SetViewActive(true);

            if (FB.IsLoggedIn)
            {
                OnFacebookConnected();
            }

            ResourcesLoaded();
        });

        _networkSystem.OnDisconnect += OnDisconnect;
        _networkSystem.OnConnectionSuccess += OnReconnect;
    }

    private void OnInGameButtonClicked()
    {
        _lobbyView.FadeIn(OnStartGame);
    }

    private void OnStartGame()
    {
        var transitionData = new GameTransitionInfo();

        transitionData.BonusTypes = _lobbyView.GetBonusTypes();
        
        EnterFeature<GameState>(transitionData);
    }

    private void OnConnectFacebook()
    {
        _facebookDC.DoLogin(OnFacebookConnected);
    }

    private void OnFacebookConnected()
    {
        try
        {
            _lobbyView.SetHelloPlayer(_facebookDC.Profile.userName);
            _lobbyView.SetPlayerAvatar(_facebookDC.Profile.image);
        }
        catch (Exception)
        {
            Log.Debug("FaceBook already LoggedIn");
        }
        finally
        {
            _facebookDC.FetchFBProfile(OnFacebookConnected);
        }
        
    }

    private void OnDisconnect()
    {
    }

    private void OnReconnect()
    {
    }

    private void ToggleMusicSwitched(bool value)
    {
        _audioSystem.SetMusicMuted(!value);
    }

    private void ToggleSoundSwitched(bool value)
    {
        _audioSystem.SetSoundMuted(!value);
    }

    private void EffectsSoundToggle()
    {
        _audioSystem.SetSoundMuted(!_audioSystem.GetSoundMuted());
    }

    private void MusicSoundToggle()
    {
        _audioSystem.SetMusicMuted(!_audioSystem.GetSoundMuted());
    }

    private void ResourcesLoaded()
    {
        if (_transitionInfo != null)
        {
        }
        FeatureInitializeFinish();

        _lobbyView.FadeOut();
    }

    protected override void OnBackButtonClicked()
    {
        ExitUtil.MinimizeAndroidApplication();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        if (_lobbyView != null)
        {
            _lobbyView.DeactivateAndRelease();
            _lobbyView = null;
        }
    }

    private void BackKeyPressed()
    {
#if UNITY_EDITOR
        EditorApplication.Exit(0);
#elif UNITY_ANDROID
		AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
		activity.Call<bool>("moveTaskToBack", true);
#endif
    }
}