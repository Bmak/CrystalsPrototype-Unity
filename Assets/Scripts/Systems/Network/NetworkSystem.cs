using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JsonFx.Json;

public class NetworkSystem : MonoBehaviour, IInitializable, ILifecycleAware, ILoggable
{
    [Inject] private CoroutineCreator _coroutineCreator;
    [Inject] private Config _config;
    [Inject] private IProvider<CountDownTimer> _timerProvider;
    [Inject] private FacebookDomainController _facebookDC;

    private WebSocket _ws = null;
    private byte[] _commandCode;

    public Action OnConnectionSuccess;
    public Action OnDisconnect;
    public Action<GameResponse> OnReceiveSuccess;

    private int _responseTimeOut = 10;
    private bool _isConnected = false;


    private class Request
    {
        public GameRequests RequestType;
        public Action<ResponseCode> OnFail;
        public Action<string> OnSuccess;
    }

    private Dictionary<GameResponse, Request> _waitingRequests = new Dictionary<GameResponse, Request>();
    private Dictionary<GameResponse, CountDownTimer> _timerRequest = new Dictionary<GameResponse, CountDownTimer>();

    #region Constants from Server

    public enum Servers : byte
    {
        Game = 4
    }
    public enum GameRequests : byte
    {
        LoginRequest = 1,
        SetNicknameRequest = 3,
        SetMaxScoreRequest = 5,
        LoginByDeviceId = 7,
    }
    public enum GameResponse : byte
    {
        LoginResponse = 2,
        SetNicknameResponse = 4,
        SetMaxScoreResponse = 6,
        LoginByDeviceId = 8,
    }
    public enum NetworkErrorCode : byte
    {
        Ok = 0,
        Error = 1,
        GameIsNotRegistered = 100,
        AllServersAreBusy = 101,
        ServerIsNotAvailable = 102,
        CorruptedToken = 103,
        InvalidUserId = 104
    }

    #endregion

    public void Initialize(InstanceInitializedCallback initializedCallback = null)
    {
        _isConnected = false;
        _ws = new WebSocket(new Uri(_config.GetServerUrl()));
        if (initializedCallback != null) initializedCallback(this);
        _commandCode = new byte[3];

        StartCoroutine(Connect());
    }

    public void Reset()
    {
        this.DestroyAll();
    }

    void OnDestroy()
    {
        Disconnect();
    }
    private void Update()
    {
        if (_ws == null || !_isConnected)
        {
            return;
        }

        byte[] byteBuffer = _ws.Recv();
        if (byteBuffer != null)
        {
			this.Log("Received packet: Server = " + (Servers)byteBuffer[0]
                + ", MessageCode = " + (GameResponse)byteBuffer[1]
                + ", ErrorCode = " + (NetworkErrorCode)byteBuffer[2]);
            string result = Encoding.UTF8.GetString(byteBuffer, 3, byteBuffer.Length - 3);
			this.Log("Received bytes, len = " + byteBuffer.Length + ", Data: " + result);
        }
        if (_ws.error != null)
        {
			this.LogError("Error: " + _ws.error);
            _isConnected = false;
        }
    }
    
    public IEnumerator Connect()
    {
        if (_ws == null)
        {
            yield break;
        }

        if (_ws.isConnected)
        {
            this.LogError("Already connected!");
            yield break;
        }

        this.Log("Connecting...");
        yield return _ws.Connect();
        if (_ws.error != null)
        {
            this.LogError("Connected error: " + _ws.error);
        }
        else
        {
            _isConnected = true;
            this.Log("Connected!");
            if (OnConnectionSuccess != null)
            {
                OnConnectionSuccess();
            }
        }
    }

    private void Disconnect()
    {
        if (_ws == null)
        {
            this.LogError("Disconnect ws errr: WS is null!");
            return;
        }

        _ws.Close();
        if (_ws.error != null)
        {
            this.LogError("Disconnect error: " + _ws.error);
        }
        else
        {
            _isConnected = false;
            this.Log("Disconnected!");
        }
    }

    private void OnTimeOut(GameResponse response)
    {
        Request waitingRequest = _waitingRequests[response];
        _timerRequest.Remove(response);
        _waitingRequests.Remove(response);
        //SendRequest(Servers.Game, waitingRequest.RequestType, response, waitingRequest.OnSuccess, waitingRequest.OnFail);
    }
    
    public void SendRequest(GameRequests request, GameResponse response, 
        Action<string> onSuccess, Action<ResponseCode> onFail,
        Message message = null,
        Servers server = Servers.Game)
    {
        if (_ws.isConnected)
        {
            _commandCode[0] = (byte)server;
            _commandCode[1] = (byte)request;
            _commandCode[2] = (byte)ResponseCode.Ok;

            if (message != null)
            {
                string serializedString = JsonWriter.Serialize(message);
                byte[] messageBuffer = Encoding.UTF8.GetBytes(serializedString);
                byte[] sendBuffer = new byte[messageBuffer.Length + 3];
                Buffer.BlockCopy(_commandCode, 0, sendBuffer, 0, 3);
                Buffer.BlockCopy(messageBuffer, 0, sendBuffer, 3, messageBuffer.Length);

                _ws.Send(sendBuffer);
                this.Log("Send: server = " + server + ", request = " + request + ", message = " + serializedString);
            }
            else
            {
                _ws.Send(_commandCode);
                this.Log("Send: server = " + server + ", request = " + request);
            }

            Request newRequest = new Request
            {
                RequestType = request,
                OnSuccess = onSuccess,
                OnFail = onFail
            };
            _waitingRequests.Add(response, newRequest);

            CountDownTimer timer = _timerProvider.Get();
            timer.StartTimer(_responseTimeOut, null, () => OnTimeOut(response));
            _timerRequest.Add(response, timer);

        }
        else
        {
            this.LogError("Connect before send!");
        }
    }

    public void SendFacebookLogin(string facebookId)
    {
        if (0 < facebookId.Length)
        {
            SendRequest(GameRequests.LoginRequest, GameResponse.LoginResponse, OnSuccess, OnFail, new LoginByFacebookIdRequest() { FacebookId = facebookId });
        }
        else
        {
			this.LogError("Input facebook id!");
        }
    }

    public void SendMaxScore(int maxScore)
    {
        SendRequest(GameRequests.SetMaxScoreRequest, GameResponse.SetMaxScoreResponse, OnSuccess, OnFail, new SetMaxScoreRequest() { MaxScore = maxScore });
    }

    public void SendNickName(string nickname)
    {
        if (0 < nickname.Length)
        {
            SendRequest(GameRequests.SetNicknameRequest, GameResponse.SetNicknameResponse, OnSuccess, OnFail, new SetNicknameRequest() { Nickname = nickname });
        }
        else
        {
			this.LogError("Input nickname!");
        }
    }

    private void OnSuccess(string data)
    {
		this.Log(String.Format("OnRequestSuccess: {0}", data));
    }

    private void OnFail(ResponseCode response)
    {
		this.Log(String.Format("OnRequestFailed: {0}", response));
    }


    public class Message { }
    public class LoginByFacebookIdRequest : Message
    {
        public string FacebookId;
    }
    public class SetNicknameRequest : Message
    {
        public string Nickname;
    }
    public class SetMaxScoreRequest : Message
    {
        public int MaxScore;
    }
}
