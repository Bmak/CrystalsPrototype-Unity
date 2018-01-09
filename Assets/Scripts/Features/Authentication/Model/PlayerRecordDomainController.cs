using System;
using System.Collections.Generic;
using System.Linq;
using JsonFx.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerRecordDomainController : ILoggable, ILifecycleAware, IDomainController
{
    [Inject]
    private PlayerService _playerService;
    [Inject]
    private Config _config;

    private PlayerRecordDO _playerRecord;

    public PlayerRecordDO PlayerRecord { get { return _playerRecord; } }


    public class PlayerRecordResponseMessage
    {
        public UserTicket UserTicket;
    }

    public class UserTicket
    {
        public long Id { get; set; }
        public int Level { get; set; }
        public string AuthorizedName { get; set; }
        public string Nickname { get; set; }
        public int Coins { get; set; }
        public int Gems { get; set; }
    }

    void ILifecycleAware.Reset()
    {
    }

    public PlayerRecordDomainController()
    {
        Init();
    }

    void IDomainController.Reset()
    {
        Init();
    }

    private void Init()
    {
    }

    public void SaveRecord()
    {
        string playerRecord = JsonWriter.Serialize(_playerRecord);

        _playerService.SavePlayerRecord(playerRecord);
    }

    private void GetDataDefault(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            this.LogError("Player Record Data Error: data is empty");
            return;
        }

        _playerRecord = JsonReader.Deserialize<PlayerRecordDO>(data);
    }

	public void GetLocalData(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			this.LogError("Player Record Data Error: data is empty");
			return;
		}

		_playerRecord = JsonReader.Deserialize<PlayerRecordDO>(data);
	}

	public void InitializeLocalPlayerRecord(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			this.LogError("Player Record Data Error: data is empty");
			return;
		}

		_playerRecord = JsonReader.Deserialize<PlayerRecordDO>(data);
	}

    public void InitializePlayerRecord(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            this.LogError("Player Record Data Error: data is empty");
            return;
        }

        PlayerRecordResponseMessage response = JsonReader.Deserialize<PlayerRecordResponseMessage>(data);
        UserTicket playerData = response.UserTicket;
        _playerRecord = new PlayerRecordDO
        {
            Id = playerData.Id,
            Level = playerData.Level,
            Name = playerData.Nickname,
            Coins = playerData.Coins,
            Gems = playerData.Gems,
        };
    }
    
    public long GetId()
    {
        return _playerRecord.Id;
    }

    public int GetPlayerGold()
    {
        return _playerRecord.Coins;
    }

    public int GetPlayerGems()
    {
        return _playerRecord.Gems;
    }

    public long GetPlayerExperience()
    {
        return _playerRecord.Experience;
    }

    public int GetPlayerLevel()
    {
        return _playerRecord.Level;
    }

    public string GetPlayerName()
    {
        return _playerRecord.Name;
    }

	//for local tests
	public PlayerRecordDO CreateDefaulEnemyRecord(PlayerRecordDO player, int avatar)
	{
		PlayerRecordDO enemy = new PlayerRecordDO();


		enemy.Id = 100;
		enemy.Name = "Bad Guy";
		enemy.Level = player.Level;
		enemy.Experience = player.Experience;

		enemy.Coins = 1000;
		enemy.Gems = 100;

		return enemy;
	}

    public void AddGold(int gold)
    {
        _playerRecord.Coins += gold;
    }

	public void AddExp(int diff)
    {
		_playerRecord.Experience += diff;
    }

    public void AddGems(int gems)
    {
        _playerRecord.Gems += gems;
    }

    public void SetLastGameScore(long score)
    {
        _playerRecord.LastGameScore = score;
    }
    public long GetLastGameScore()
    {
        return _playerRecord.LastGameScore;
    }
    public void SetCurrentMaxScore(long score)
    {
        _playerRecord.CurrentMaxScore = score;
    }
    public long GetCurrentMaxScore()
    {
        return _playerRecord.CurrentMaxScore;
    }
}
