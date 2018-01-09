using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellView : MonoBehaviour, ILoggable
{
    public bool IsMarked { get; set; }
    private PieceView _piece;
    public PieceView Piece {
        get { return _piece; }
        set
        {
            _piece = value;
            _piece.Cell = this;
        }
    }

    //private BoxCollider _collider;
    private UIButton _button;
    private readonly List<CellView> _cells = new List<CellView>();
    public List<CellView> Neighbors { get { return _cells; } }

    private Action<CellView> _onSelectCell;

    private void Awake()
    {
        //_collider = GetComponent<BoxCollider>();
        _button = GetComponent<UIButton>();
    }

    public void Initialize(Action<CellView> onSelectCell)
    {
        _onSelectCell = onSelectCell;
        WireWidgets();
    }

    public void AddCell(CellView cell)
    {
        _cells.Add(cell);
    }

    public void CheckType(PieceType type, List<PieceDO> pool)
    {
        if (IsMarked) return;

        if (Piece.GetData.Checked) return;

        IsMarked = true;

        pool.Add(Piece.GetData);
        Piece.GetData.Checked = true;

        foreach (CellView cell in _cells)
        {
            if (!cell.IsMarked && cell.Piece.GetData.Type == type)
            {
                cell.CheckType(type, pool);
            }
        }
    }
    /// <summary>
    /// Change type of cell's piece and his neighbors until node count
    /// </summary>
    /// <param name="type"></param>
    /// <param name="sprite"></param>
    /// <param name="neighbors"></param>
    /// <param name="node"></param>
    public void ChangeType(PieceType type, string sprite, bool neighbors, int node = 1)
    {
        PieceType pieceType = Piece.GetData.Type;
        if (pieceType != PieceType.FRIEND_DIAMOND && pieceType != PieceType.MAGIC_DIAMOND && pieceType != type)
        {
            Piece.GetData.Type = type;
            Piece.GetData.Sprite = sprite;
            Piece.Change();
        }
        
        if (!neighbors) return;

        if (node <= 0) return;

        foreach (CellView cell in Neighbors)
        {
            cell.ChangeType(type, sprite, true, (node - 1));
        }
    }

    private void Update()
    {
        //if (Piece)
        //{
            //_collider.enabled = !Piece.IsMoving;
        //}
    }

    public void WireWidgets()
    {
        EventDelegate.Add(_button.onClick, OnSelectCell);
    }

    private void OnDestroy()
    {
        EventDelegate.Remove(_button.onClick, OnSelectCell);
    }

    private void OnSelectCell()
    {
        if (_onSelectCell != null)
        {
            _onSelectCell(this);
        }
    }

}
