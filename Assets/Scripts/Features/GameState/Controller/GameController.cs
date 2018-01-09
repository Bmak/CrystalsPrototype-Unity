using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : FeatureController
{
    private readonly PieceType[] _values = {PieceType.SIMPLE_1, PieceType.SIMPLE_2, PieceType.SIMPLE_3, PieceType.SIMPLE_4, PieceType.SIMPLE_5 };
    private readonly PieceType[] _diamondValues = {PieceType.FRIEND_DIAMOND, PieceType.MAGIC_DIAMOND};
	private readonly List<PieceDO> _currentPool = new List<PieceDO>();

	[Inject] private NetworkSystem _networkSystem;
	[Inject] private PlayerRecordDomainController _playerDC;
	[Inject] private PlayerService _playerService;
    [Inject] private IProvider<SpecificCountDownTimer> _timerProvider;

    private BonusController _bonusController;

    private SpecificCountDownTimer _roundTimer;

    private GameView _gameView;

	private List<List<PieceDO>> _grid;
    private List<PieceDO[]> _matches = new List<PieceDO[]>();

    private PieceType _selectedType;
	private GameTransitionInfo _transitionInfo;

    private bool _addDiamondPiece = false;
    private bool _setSuperCubeBonus = false;

	public void Initialize(GameTransitionInfo transitionInfo)
	{
		Initialize();

		_transitionInfo = transitionInfo;
		LoadResources();
	}

	private void LoadResources()
	{
        _viewProvider.Get<GameView>(view =>
		{
			_gameView = view;

			var data = new GameRenderData();

		    data.OnGetToMenu = OnGetToMenu;
			data.GridSize = new Vector2(_config.GetGridWidth(), _config.GetGridHeight());

			while (true)
			{
				var grid = new List<List<PieceDO>>();

				for (var i = 0; i < _config.GetGridWidth(); i++)
				{
					var col = new List<PieceDO>();

					for (var j = 0; j < _config.GetGridHeight(); j++)
					{
						col.Add(CreatePiece(i, j));
					}
					grid.Add(col);
				}

                _grid = grid;

                break;
			}

			data.SelectCell = OnSelectCell;
		    data.AddDiamondPiece = AddDiamondPiece;
			data.GridList = _grid;

			_gameView.InitializeViewData(data);

            CheckForMatches();

            _gameView.SetViewActive(true);

            ResourcesLoaded();
		});

		_networkSystem.OnDisconnect += OnDisconnect;
		_networkSystem.OnConnectionSuccess += OnReconnect;
	}

    private PieceDO CreatePiece(int col, int row, PieceType type = 0)
	{
		var piece = new PieceDO();
		piece.Col = col;
		piece.Row = row;
		piece.Type = type == 0 ? GetRandomType(_values) : type;
		piece.Sprite = GetTypeSprite(piece.Type);

		return piece;
	}

	private PieceDO ChangePiece(PieceDO piece, int col, int row, PieceType type = 0)
	{
		piece.Col = col;
		piece.Row = row;
		piece.Type = type == 0 ? GetRandomType(_values) : type;
		piece.Sprite = GetTypeSprite(piece.Type);
		piece.View.Change();

		return piece;
	}

    private void OnSelectCell(CellView cell)
    {
        foreach (PieceDO pieceDo in _currentPool)
        {
            pieceDo.Checked = false;
            pieceDo.Locked = false;
            pieceDo.View.Cell.IsMarked = false;
        }

        _currentPool.Clear();
        _selectedType = cell.Piece.GetData.Type;

        if (_selectedType == PieceType.MAGIC_DIAMOND)
        {
            for (int i = 0; i < _config.GetGridWidth(); i++)
            {
                _currentPool.Add(_grid[i][0]);
            }

            for (int i = 1; i < _config.GetGridHeight(); i++)
            {
                _currentPool.Add(_grid[cell.Piece.GetData.Col][i]);
            }

            _currentPool.FindAll(x => x.View.Cell != cell && (x.Type == PieceType.FRIEND_DIAMOND || x.Type == PieceType.MAGIC_DIAMOND)).ForEach((x) => _currentPool.Remove(x));
        }
        else if (_selectedType == PieceType.FRIEND_DIAMOND)
        {
            CheckForMatches();
            List<PieceDO[]> matches = new List<PieceDO[]>(_matches);
            _currentPool.Add(cell.Piece.GetData);

            _coroutineCreator.StartCoroutine(AnimateFriendDiamonds(matches));

            return;
        }
        else
        {
            LookForMatches(cell, _selectedType);

            if (_currentPool.Count <= 2)
            {
                //TODO show this match has failed
                _gameView.AddDiamondProgress((int)DiamondBarState.Miss);
                foreach (var pieceDo in _currentPool)
                {
                    pieceDo.Checked = false;
                    pieceDo.Locked = false;
                    pieceDo.View.Cell.IsMarked = false;
                }

                _currentPool.Clear();
                return;
            }
        }

        SetMatch();
    }

    private IEnumerator AnimateFriendDiamonds(List<PieceDO[]> matches)
    {
        while (matches.Count > 0)
        {
            foreach (PieceDO pieceDo in matches[0])
            {
                _currentPool.Add(pieceDo);
            }
            SetMatch();
            _currentPool.Clear();
            matches.RemoveAt(0);

            yield return new WaitForSeconds(0.05f);
        }
    }

    private void SetMatch()
    {
        List<PieceDO> bonusPieces = _currentPool.FindAll(p => p.View.HasBonus);

        foreach (PieceDO pieceDo in bonusPieces)
        {
            //TODO set pieces type of animation to destroy which depends on BonusType
            switch (pieceDo.View.BonusType)
            {
                case BonusType.Bomb:
                    ActivateBombBonus(pieceDo);
                    break;
                case BonusType.Color:
                    ActivateColorBonus(pieceDo);
                    break;
                case BonusType.Time:
                    ActivateTimeBonus();
                    break;
                case BonusType.BlackHole:
                    ActivateBlackHoleBonus(pieceDo.Type);
                    break;
                case BonusType.SuperCube:
                    _setSuperCubeBonus = true;
                    break;
            }
            _bonusController.StartBonus(pieceDo.View.BonusType);
        }

        foreach (var pieceDo in _currentPool)
        {
            pieceDo.View.ShowRemove();
            //pieceDo.View.gameObject.SetActive(false);
            _grid[pieceDo.Col][pieceDo.Row] = null;

            AffectAbove(pieceDo);
        }

        int score = _currentPool.Count * _config.GetMatchKoeff(_currentPool.Count, _selectedType);
        _gameView.AddScore(score);
        if (_selectedType != PieceType.MAGIC_DIAMOND && _selectedType != PieceType.FRIEND_DIAMOND)
        {
            _gameView.AddDiamondProgress(_currentPool.Count);
        }

        AddNewPieces();

        CheckForMatches();

        if (_setSuperCubeBonus)
        {
            ActivateSuperCubeBonus();
            _setSuperCubeBonus = false;
        }
    }
    
    private void AffectAbove(PieceDO piece)
	{
		var row = piece.Row + 1;
		while (row < _config.GetGridHeight())
		{
			if (_grid[piece.Col][row] != null)
			{
				_grid[piece.Col][row].Row--;
				_grid[piece.Col][row - 1] = _grid[piece.Col][row];
			    _gameView.Cells[piece.Col][row - 1].Piece = _grid[piece.Col][row - 1].View;
                _grid[piece.Col][row] = null;
			}
			row++;
		}
	}

	private void AddNewPieces()
	{
		for (var i = 0; i < _config.GetGridWidth(); i++)
		{
			var missingPieces = 0;
			for (var j = 0; j < _config.GetGridHeight(); j++)
			{
				if (_grid[i][j] == null)
				{
					PieceDO piece = _currentPool[_currentPool.Count - 1];
					ChangePiece(piece, i, j);
					_gameView.RecreatePiece(piece, ++missingPieces);
					_grid[i][j] = piece;

					_currentPool.Remove(piece);
				}
			}
		}

	    if (_addDiamondPiece)
	    {
            int col = Random.Range(0, _config.GetGridWidth());
            int row = Random.Range(0, _config.GetGridWidth());

	        PieceType type = GetRandomType(_diamondValues);
	        ChangePiece(_grid[col][row], col, row, type);

	        _addDiamondPiece = false;
	    }

		_gameView.IsDropping = true;
	}

	private void CheckForMatches()
	{
        _matches.Clear();
        ResetGridState();

        //1. Находим общее количество совпадений
        for (var j = 0; j < _config.GetGridHeight(); j++)
        {
            for (var i = 0; i < _config.GetGridWidth(); i++)
            {
                PieceDO piece = _grid[i][j];
                if (piece.Checked) continue;

                _currentPool.Clear();
                LookForMatches(piece.View.Cell, piece.Type);

                if (_currentPool.Count > 2)
                {
                    _matches.Add(_currentPool.ToArray());
                }
                CheckBonusLive();
            }
        }
	    ResetGridState();

        //2. Если совпадений < 2
        if (_matches.Count < 2)
	    {
            //3. Помечаем те ячейки, которые нельзя проверять (по вертикали, горизонтали и диагонали)
            LockPieces(_matches);
            //4. Начинаем снизу и ищем первую попавшуюся непомеченную ячейку, у которой соседи тоже непомеченные
            //5. Изменяем тип соседних ячеек на такой же тип
            AddMatches();
        }
    }

    private void CheckBonusLive()
    {
        PieceDO pieceDo = _currentPool.Find(p => p.View.HasBonus);
        if (pieceDo != null)
        {
            if (_currentPool.Count > 2)
            {
                _currentPool.ForEach(p => p.View.ActivateBonusBorder(true));
            }
            else
            {
                pieceDo.View.RemoveBonus(() => {
                    _bonusController.StartBonus(pieceDo.View.BonusType, true);
                    _currentPool.ForEach(p => p.View.ActivateBonusBorder(false));
                });
            }
        }
        else
        {
            _currentPool.ForEach(p => p.View.ActivateBonusBorder(false));
        }
    }

    private void ResetGridState()
    {
        for (var i = 0; i < _config.GetGridWidth(); i++)
        {
            for (var j = 0; j < _config.GetGridHeight(); j++)
            {
                PieceDO piece = _grid[i][j];
                piece.Checked = false;
                piece.Locked = false;
                piece.View.Cell.IsMarked = false;
            }
        }
    }

    private void LockPieces(List<PieceDO[]> matches)
    {
        foreach (PieceDO[] pool in matches)
        {
            foreach (PieceDO piece in pool)
            {
                piece.Locked = true;
                if (piece.Col > 0)
                {
                    _grid[piece.Col - 1][piece.Row].Locked = true;
                }
                if (piece.Col + 1 < _config.GetGridWidth())
                {
                    _grid[piece.Col + 1][piece.Row].Locked = true;
                }
                if (piece.Row > 0)
                {
                    _grid[piece.Col][piece.Row - 1].Locked = true;
                }
                if (piece.Row + 1 < _config.GetGridHeight())
                {
                    _grid[piece.Col][piece.Row + 1].Locked = true;
                }
                //
                if (piece.Col > 0 && piece.Row > 0)
                {
                    _grid[piece.Col - 1][piece.Row-1].Locked = true;
                }
                if (piece.Col > 0 && piece.Row + 1 < _config.GetGridHeight())
                {
                    _grid[piece.Col - 1][piece.Row + 1].Locked = true;
                }
                if (piece.Col + 1 < _config.GetGridWidth() && piece.Row + 1 < _config.GetGridHeight())
                {
                    _grid[piece.Col + 1][piece.Row + 1].Locked = true;
                }
                if (piece.Col + 1 < _config.GetGridWidth() && piece.Row > 0)
                {
                    _grid[piece.Col + 1][piece.Row - 1].Locked = true;
                }
            }
        }

        foreach (List<PieceDO> list in _grid)
        {
            foreach (PieceDO pieceDo in list)
            {
                if (pieceDo.Type == PieceType.MAGIC_DIAMOND || pieceDo.Type == PieceType.FRIEND_DIAMOND)
                {
                    pieceDo.Locked = true;
                }
            }
        }
    }

    private void AddMatches()
    {
        for (var j = 0; j < _config.GetGridHeight(); j++)
        {
            for (var i = 0; i < _config.GetGridWidth(); i++)
            {
                PieceDO piece = _grid[i][j];
                if (piece.Locked) continue;

                if (piece.Col > 0 && _grid[piece.Col - 1][piece.Row].Locked)
                    continue;
                if (piece.Col + 1 < _config.GetGridWidth() && _grid[piece.Col + 1][piece.Row].Locked)
                    continue;
                if (piece.Row > 0 && _grid[piece.Col][piece.Row - 1].Locked)
                    continue;
                if (piece.Row + 1 < _config.GetGridHeight() && _grid[piece.Col][piece.Row + 1].Locked)
                    continue;

                if (piece.Col > 0)
                {
                    ChangePieceType(_grid[piece.Col - 1][piece.Row], piece.Type);
                }
                if (piece.Col + 1 < _config.GetGridWidth())
                {
                    ChangePieceType(_grid[piece.Col + 1][piece.Row], piece.Type);
                }
                if (piece.Row > 0)
                {
                    ChangePieceType(_grid[piece.Col][piece.Row - 1], piece.Type);
                }
                if (piece.Row + 1 < _config.GetGridHeight())
                {
                    ChangePieceType(_grid[piece.Col][piece.Row + 1], piece.Type);
                }
                return;
            }
        }
    }

    private void ChangePieceType(PieceDO piece, PieceType type)
    {
        piece.Type = type;
        piece.Sprite = GetTypeSprite(piece.Type);
        if (piece.View != null)
            piece.View.Change();
    }

    private void LookForMatches(CellView cell, PieceType type)
    {
        if (cell == null) return;

        cell.CheckType(type, _currentPool);
    }

    private List<PieceDO> LookForMatchesToList(CellView cell, PieceType type)
    {
        List<PieceDO> result = new List<PieceDO>();

        if (cell == null) return result;

        cell.CheckType(type, result);

        return result;
    }
    
    private void AddDiamondPiece()
    {
        _addDiamondPiece = true;
    }

    private string GetTypeSprite(PieceType type)
	{
		return string.Format("ui.item{0}", (int)type);
	}

    private PieceType GetRandomType(PieceType[] values)
	{
		int id = Random.Range(0, values.Length);

		return (PieceType)values.GetValue(id);
	}

    private void UpdateTimer(long time)
    {
        _gameView.UpdateTimer(time);
        _gameView.AddDiamondProgress((int)DiamondBarState.Pause);
    }

    private void FinishTimer()
    {
        _bonusController.Stop();
        _gameView.FinishedTimer();
    }

    private void OnGetToMenu()
    {
        _playerService.SetLastGameScore(_gameView.GetGameScore);

        HomeBaseTransitionInfo transitionInfo = null;
        if (_transitionInfo != null)
        {
            transitionInfo = new HomeBaseTransitionInfo();
        }
        EnterFeature<HomeBaseState>(transitionInfo);
    }

    private bool OnSetBonus(BonusType type)
    {
        CheckForMatches();

        int rndMatch = -1;
        while (rndMatch == -1)
        {
            if (_matches.Count == 0)
            {
                return false;
            }

            rndMatch = Random.Range(0, _matches.Count);
            foreach (PieceDO pieceDo  in _matches[rndMatch])
            {
                if (pieceDo.View.HasBonus)
                {
                    _matches.RemoveAt(rndMatch);
                    rndMatch = -1;
                    break;
                }
            }
        }
        
        int rndPiece = Random.Range(0, _matches[rndMatch].Length);

        foreach (PieceDO pieceDo in _matches[rndMatch])
        {
            pieceDo.View.ActivateBonusBorder(true);
        }
        PieceDO piece = _matches[rndMatch][rndPiece];
        piece.View.SetBonus(type);

        return true;
    }

    private void OnRemoveBonus(BonusType type, Action finishCallBack)
    {
        foreach (List<PieceDO> list in _grid)
        {
            PieceDO piece = list.Find(p => p.View.HasBonus && p.View.BonusType == type);
            if (piece != null)
            {
                List<PieceDO> match = LookForMatchesToList(piece.View.Cell, piece.Type);
                piece.View.RemoveBonus(() =>
                {
                    if (finishCallBack != null)
                        finishCallBack();

                    foreach (PieceDO pieceDo in match)
                    {
                        pieceDo.Checked = false;
                        pieceDo.Locked = false;
                        pieceDo.View.Cell.IsMarked = false;
                        pieceDo.View.ActivateBonusBorder(false);
                    }
                });

                break;
            }
        }
    } 

    private void ActivateBombBonus(PieceDO pieceDo)
    {
        //reset states
        ResetGridState();

        List<PieceDO> bonusPool = LookForMatchesToList(pieceDo.View.Cell, pieceDo.Type);

        foreach (PieceDO piece in bonusPool)
        {
            foreach (CellView cell in piece.View.Cell.Neighbors)
            {
                if (_currentPool.IndexOf(cell.Piece.GetData) == -1 && cell.Piece.GetData.Type != PieceType.FRIEND_DIAMOND && cell.Piece.GetData.Type != PieceType.MAGIC_DIAMOND)
                {
                    _currentPool.Add(cell.Piece.GetData);
                }
            }
        }
    }

    private void ActivateColorBonus(PieceDO pieceDo)
    {
        int rndI = Random.Range(2, _config.GetGridWidth() - 2);
        int rndJ = Random.Range(2, _config.GetGridHeight() - 2);

        _grid[rndI][rndJ].View.Cell.ChangeType(pieceDo.Type, GetTypeSprite(pieceDo.Type), true, 3);
    }

    private void ActivateTimeBonus()
    {
        _roundTimer.EndTime += 2;
        _gameView.UpdateTimer(_roundTimer.RemainingTime);
    }

    private void ActivateBlackHoleBonus(PieceType type)
    {
        for (int i = 0; i < _config.GetGridWidth(); i++)
        {
            for (int j = 0; j < _config.GetGridHeight(); j++)
            {
                PieceDO piece = _grid[i][j];
                if (piece.Type == type && _currentPool.IndexOf(piece) == -1)
                {
                    _currentPool.Add(piece);
                }
            }
        }
    }

    private void ActivateSuperCubeBonus()
    {
        List<List<int>> colorPieces = new List<List<int>>();
        colorPieces.Add(new List<int>());
        colorPieces.Add(new List<int>());
        colorPieces.Add(new List<int>());
        colorPieces.Add(new List<int>());
        colorPieces.Add(new List<int>());

        PieceDO piece = null;
        for (int i = 0; i < _config.GetGridWidth(); i++)
        {
            for (int j = 0; j < _config.GetGridHeight(); j++)
            {
                piece = _grid[i][j];
                if (piece.Type == PieceType.FRIEND_DIAMOND || piece.Type == PieceType.MAGIC_DIAMOND) continue;
                colorPieces[(int)piece.Type-1].Add((int)piece.Type);
            }
        }

        colorPieces.Sort((p1, p2) => p1.Count.CompareTo(p2.Count));

        for (int i = 0; i < _config.GetGridWidth(); i++)
        {
            for (int j = 0; j < _config.GetGridHeight(); j++)
            {
                piece = _grid[i][j];
                if (piece.Type == PieceType.FRIEND_DIAMOND || piece.Type == PieceType.MAGIC_DIAMOND) continue;

                while (colorPieces[0].Count == 0)
                {
                    colorPieces.RemoveAt(0);
                }

                int len = colorPieces[0].Count;

                piece.Type = (PieceType) colorPieces[0][len - 1];
                piece.Sprite = GetTypeSprite(piece.Type);
                piece.View.Change();
                colorPieces[0].RemoveAt(len - 1);
            }
        }
        CheckForMatches();
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
            _bonusController = new BonusController();
            _bonusController.Initialize(_timerProvider, _transitionInfo.BonusTypes, OnSetBonus, OnRemoveBonus);
        }
        _roundTimer = _timerProvider.Get();

        FeatureInitializeFinish();

        _roundTimer.StartTimer(_config.GetGameTime(), UpdateTimer, FinishTimer);
        _bonusController.Start();
    }

	protected override void OnBackButtonClicked()
	{
        //TODO set pause game
		ExitUtil.MinimizeAndroidApplication();
	}

	public override void Shutdown()
	{
		base.Shutdown();
	    if (_bonusController != null)
	    {
            _bonusController.Destroy();
	        _bonusController = null;
	    }
        
        if (_gameView != null)
		{
			_gameView.DeactivateAndRelease();
			_gameView = null;
		}
	}
}