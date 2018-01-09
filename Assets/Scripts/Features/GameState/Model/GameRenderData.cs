using System;
using System.Collections.Generic;
using UnityEngine;

public class GameRenderData
{
    public Action OnGetToMenu { get; set; }
    public Action<CellView> SelectCell { get; set; }
    public Action AddDiamondPiece { get; set; }
	public Vector2 GridSize { get; set; }
	public List<List<PieceDO>> GridList { get; set; }
}