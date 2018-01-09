public class PieceDO
{
	public PieceType Type { get; set; }
	public int Col { get; set; }
	public int Row { get; set; }
	public string Sprite { get; set; }

	public PieceView View { get; set; }

	public bool Checked { get; set; }
    public bool Locked { get; set; }
}