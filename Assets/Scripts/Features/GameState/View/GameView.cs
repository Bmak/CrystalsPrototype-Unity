using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameView : NguiView
{
    [SerializeField] private UIWidget _gameContainer;
    [SerializeField] private GameResultsView _resultsContainer;

	[SerializeField] private UIWidget _gridPieces;
	[SerializeField] private UIWidget _gridCells;
    [SerializeField] private GameObject _piecePrefab;
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private UILabel _scoreLabel;
    [SerializeField] private UIProgressBar _diamondProgressBar;
    [SerializeField] private UIProgressBar _timerProgress;
    [SerializeField] private UILabel _timerLabel;

    private int PIECE_WIDTH;
    private int PIECE_HEIGHT;
    private const int STEP = 20;

    private Vector2 _gridSize;
	private int _offsetX;
	private int _offsetY;

    private Action _addDiamondPiece;

	private List<List<PieceDO>> _piecePool;
    private readonly List<List<CellView>> _cells = new List<List<CellView>>(); 
    public List<List<CellView>> Cells { get { return _cells; } }

    private int _currentScore;
    public int GetGameScore { get { return _currentScore; } }
    private NguiLabelCounterInt _scoreCounter;

    private int _currentDiamondScore;
    public int GetDiamondScore { get { return _currentDiamondScore; } }

    public bool IsDropping { get; set; }

	private void Awake()
	{
		PIECE_WIDTH = _piecePrefab.GetComponent<UIWidget>().width;
		PIECE_HEIGHT = _piecePrefab.GetComponent<UIWidget>().height;

		_offsetX = _gridPieces.width / 2 - PIECE_WIDTH / 2;
		_offsetY = _gridPieces.height / 2 - PIECE_HEIGHT / 2;
    }

	public void InitializeViewData(GameRenderData data)
	{
        _resultsContainer.gameObject.SetActive(false);
	    _resultsContainer.InitializeViewData(data.OnGetToMenu);

        _gridSize = data.GridSize;
		_piecePool = data.GridList;

		for (int i = 0; i < _gridSize.x; i++)
		{
            List<CellView> cells = new List<CellView>();
			for (int j = 0; j < _gridSize.y; j++)
			{
                GameObject pieceGO = _gridPieces.gameObject.AddChild(_piecePrefab);
				pieceGO.transform.localScale = Vector3.one;
                PieceView piece = pieceGO.GetComponent<PieceView>();
				PieceDO pieceDo = data.GridList[i][j];

				if (piece != null)
				{
					piece.Initialize(pieceDo);
					piece.transform.localPosition = new Vector3(PIECE_WIDTH * i - _offsetX, PIECE_HEIGHT * j - _offsetY);
                }

			    GameObject cellGO = _gridCells.gameObject.AddChild(_cellPrefab);
                cellGO.transform.localScale = Vector3.one;
			    cellGO.transform.localPosition = piece.transform.localPosition;
			    cellGO.name = String.Format("cell {0}/{1}", i, j);
			    CellView cell = cellGO.GetComponent<CellView>();
                cell.Piece = piece;
                cell.Initialize(data.SelectCell);
                cells.Add(cell);
            }
            _cells.Add(cells);
		}
	    SetNeighbors();

        _piecePrefab.SetActive(false);
		_cellPrefab.SetActive(false);

	    _currentScore = 0;
        _scoreCounter = new NguiLabelCounterInt(_scoreLabel);
        _scoreCounter.SetInitialCounterValue(_currentScore);

	    _currentDiamondScore = 0;
        _diamondProgressBar.value = 0;
	    _addDiamondPiece = data.AddDiamondPiece;
	}

    private void SetNeighbors()
    {
        for (int i = 0; i < _gridSize.x; i++)
        {
            for (int j = 0; j < _gridSize.y; j++)
            {
                try
                {
                    if (i > 0)
                    {
                        _cells[i][j].AddCell(_cells[i - 1][j]);
                    }
                    if (i + 1 < _gridSize.x)
                    {
                        _cells[i][j].AddCell(_cells[i + 1][j]);
                    }

                    if (j > 0)
                    {
                        _cells[i][j].AddCell(_cells[i][j - 1]);
                    }
                    if (j + 1 < _gridSize.y)
                    {
                        _cells[i][j].AddCell(_cells[i][j + 1]);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new ArgumentOutOfRangeException(String.Format("Cell id is out of range: {0},{1}",i,j));
                }
                
            }
        }
    }

    public void RecreatePiece(PieceDO piece, int missingPieces)
	{
		piece.View.transform.localPosition = new Vector3(PIECE_WIDTH * piece.Col - _offsetX, PIECE_HEIGHT * missingPieces + _offsetY);
		piece.View.gameObject.SetActive(true);
        _cells[piece.Col][piece.Row].Piece = piece.View;
	}

    public void AddScore(int score)
    {
        _currentScore += score;
        _scoreCounter.AnimateToAmount(_currentScore, 0.3f);
    }

    public void AddDiamondProgress(int progress)
    {
        _currentDiamondScore += progress;
        if (_currentDiamondScore < 0)
            _currentDiamondScore = 0;

        _diamondProgressBar.value = (float)_currentDiamondScore / (float)DiamondBarState.Full;

        if (_currentDiamondScore >= (int) DiamondBarState.Full && _addDiamondPiece != null)
        {
            _addDiamondPiece();
            _currentDiamondScore = 0;
            _diamondProgressBar.value = 0f;
        }
    }

    public void UpdateTimer(long time)
    {
        _timerLabel.text = String.Format("{0}", time);
        _timerProgress.value = (float)time/ (float)_config.GetGameTime();
    }

    public void FinishedTimer()
    {
        _timerLabel.text = "0";
        _timerProgress.value = 0;

        _resultsContainer.SetScore(_currentScore);

        _gameContainer.gameObject.SetActive(false);
        _resultsContainer.gameObject.SetActive(true);
    }

    protected override void WireWidgets()
	{
		base.WireWidgets();
		//EventDelegate.Add(_button.onClick, Func);
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		//EventDelegate.Remove(_button.onClick, Func);
	}

	private void Update()
	{
		if (IsDropping == false) return;

		bool madeMove = false;
		for (int i = 0; i < _gridSize.x; i++)
		{
			for (int j = 0; j < _gridSize.y; j++)
			{
				PieceView piece = _piecePool[i][j].View;
				if (piece == null) continue;

                piece.IsMoving = false;
                //Move down
                if (piece.transform.localPosition.y > PIECE_HEIGHT * j - _offsetY)
				{
					piece.transform.localPosition = new Vector2(piece.transform.localPosition.x, piece.transform.localPosition.y - STEP);
					madeMove = true;
                    //_cells[i][j].Piece = piece;
				    piece.IsMoving = true;
				}
			}
		}

		if (IsDropping && !madeMove)
		{
			IsDropping = false;
		}
	}
}