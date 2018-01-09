/// <summary>
/// Player Data Record
/// </summary>
using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class PlayerRecordDO : ILoggable
{
    public long Id { get; set; }
	public int Level { get; set; }
	public string Name { get; set; }
	
	public int Coins { get; set; }
	public int Gems { get; set; }
	public long Experience { get; set; }

    public long CurrentMaxScore { get; set; }
    public long LastGameScore { get; set; }
}
