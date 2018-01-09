using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Facebook.MiniJSON;
using Facebook.Unity;
using JsonFx.Json;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FacebookDomainController : ILoggable, ILifecycleAware, IDomainController
{
    [Inject] private CoroutineCreator _coroutineCreator;
    [Inject] private NetworkSystem _networkSystem;

    private readonly List<string> perms = new List<string> {"public_profile", "email", "user_friends"};
    private AccessToken _currentToken;
    private UserProfile _profile;
    public UserProfile Profile { get { return _profile; } }

    private Action _loginCallBack;

    public FacebookDomainController()
    {
        Init();
    }

    void IDomainController.Reset()
    {
        Init();
    }

    void ILifecycleAware.Reset()
    {
    }

    private void Init()
    {
    }

    public void DoInitialize()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            //...
        }
        else
        {
            this.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isunityshown)
    {
        if (!isunityshown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }


    public void DoLogin(Action callBack = null)
    {
        _loginCallBack = callBack;
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            _currentToken = Facebook.Unity.AccessToken.CurrentAccessToken;
			//this.Log("MY TOKEN: " + _currentToken);
            
            // Print current access token's User ID
			//this.Log("MY ID: " + _currentToken.UserId);
            

            _networkSystem.SendFacebookLogin(_currentToken.UserId);

            FetchFBProfile();
        }
        else
        {
			this.Log("User cancelled login");
        }
    }

    public string AccessToken
    {
        get { return _currentToken.TokenString; }
    }

    public string UserId
    {
        get { return _currentToken.UserId; }
    }

    public void FetchFBProfile(Action callBack = null)
    {
        if (_loginCallBack == null)
        {
            _loginCallBack = callBack;
        }
        FB.API("/me?fields=name,picture", HttpMethod.GET, FetchProfileCallback, new Dictionary<string, string>() { });
    }

    private void FetchProfileCallback(IGraphResult result)
    {
        _profile = new UserProfile();

        _profile.SetUserID(UserId);
        _profile.SetUserName(String.Format("{0}", result.ResultDictionary["name"]));

        if (result.ResultDictionary.ContainsKey("picture"))
        {
            IDictionary picture = result.ResultDictionary["picture"] as IDictionary;
            IDictionary data = picture["data"] as IDictionary;
            _coroutineCreator.StartCoroutine(FetchFBProfilePicture((string)data["url"]));
        }

        _networkSystem.SendNickName(_profile.userName);
    }

    private IEnumerator FetchFBProfilePicture(string url)
    {
        WWW www = new WWW(url);

        yield return www;

        _profile.SetImage(www.texture);

        if (_loginCallBack != null)
        {
            _loginCallBack();
            _loginCallBack = null;
        }
    }

}