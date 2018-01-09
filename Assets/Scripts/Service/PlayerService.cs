using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerService : ILoggable
{
    [Inject]
    private LocalPrefs _localPrefs;
    [Inject]
    private PlayerRecordDomainController _playerDC;
    [Inject]
    private Config _config;
    [Inject]
    private NetworkSystem _networkSystem;

    public Action<NetworkSystem.GameResponse> OnDataReceived;
    public Action OnNotEnoughGems;
    public Action OnNotEnoughCoins;

    private enum PlayerPrefs
    {
        PLAYER_RECORD
    }

    public class PlayerRecordRequest : NetworkSystem.Message
    {
        public string DeviceId { get; set; }
    }

    public void AddOnActionDataReceived(Action<NetworkSystem.GameResponse> onDataReceived)
    {
        _networkSystem.OnReceiveSuccess += onDataReceived;
    }

	public void GetCommonData(Action<string> getDataSuccesess, Action<ResponseCode> getDataFailed)
	{
		
	}

	public void GetLocalPlayerData(Action<string> getDataSuccesess, Action<string> getDataDefault, Action<ResponseCode> getDataFailed)
	{
		//		_localPrefs.DeleteAll();
		string data = _localPrefs.GetString(PlayerPrefs.PLAYER_RECORD.ToPrefsKey(), "");
		if (string.IsNullOrEmpty(data))
		{
			TextAsset txt = Resources.Load("GameData/PlayerRecord", typeof(TextAsset)) as TextAsset;
			getDataDefault(txt.text);
			return;
		}
		getDataSuccesess(data);
	}

	public void GetNetworkPlayerData(string deviceId, Action<string> getDataSuccesess, Action<ResponseCode> getDataFailed)
    {
        _networkSystem.SendRequest(NetworkSystem.GameRequests.LoginByDeviceId, NetworkSystem.GameResponse.LoginByDeviceId,
            getDataSuccesess, getDataFailed,
            new PlayerRecordRequest { DeviceId = deviceId });
    }

    public void SavePlayerRecord(string playerRcord)
    {
        _localPrefs.SetSharedString(PlayerPrefs.PLAYER_RECORD.ToPrefsKey(), playerRcord);
    }

    public long CurrentDateTime()
    {
        long unixTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        return unixTime;
    }

    public DateTime ConvertFromUnixTimestamp(long timestamp)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return origin.AddSeconds(timestamp);
    }

    public DateTime ConvertFromUnixTimestamp(double timestamp)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return origin.AddSeconds(timestamp);
    }

    public TimeSpan GetRemainingTime(long timestamp, int timeToOpen)
    {
        int totalSeconds = (int)(ConvertFromUnixTimestamp(CurrentDateTime()) - ConvertFromUnixTimestamp(timestamp)).TotalSeconds;
        if (totalSeconds > timeToOpen)
        {
            totalSeconds = timeToOpen;
        }
        return TimeSpan.FromSeconds(timeToOpen - totalSeconds);
    }

    public TimeSpan GetRemainingTimeMilliseconds(long timestamp, int timeToOpen)
    {
        double currentDateTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        double totalSeconds = (ConvertFromUnixTimestamp(currentDateTime) - ConvertFromUnixTimestamp(timestamp)).TotalSeconds;
        if (totalSeconds > timeToOpen)
        {
            totalSeconds = timeToOpen;
        }
        return TimeSpan.FromSeconds(timeToOpen - totalSeconds);
    }

    public string GetRemainingTime(TimeSpan timeSpan, string hour, string minute, string second)
    {
        string time = string.Empty;
        if ((int)timeSpan.TotalHours > 0)
        {
            time += string.Format("{0}{1} ", (int)timeSpan.TotalHours, hour);
        }
        if (timeSpan.Minutes > 0)
        {
            time += string.Format("{0}{1} ", timeSpan.Minutes, minute);
        }
        if (timeSpan.Hours <= 0 && timeSpan.Minutes <= 0 && timeSpan.Seconds > 0)
        {
            time += string.Format("{0}{1} ", timeSpan.Seconds, second);
        }
        return time;
    }

    public int GetCrystals()
    {
        return _playerDC.GetPlayerGems();
    }

    public void SetLastGameScore(long score)
    {
        _playerDC.SetLastGameScore(score);
        _playerDC.SaveRecord();

        if (_playerDC.GetCurrentMaxScore() < score)
        {
            SetCurrentMaxScore(score);
        }
    }

    public void SetCurrentMaxScore(long score)
    {
        _networkSystem.SendMaxScore((int)score);
        _playerDC.SetCurrentMaxScore(score);
        _playerDC.SaveRecord();
    }
}
