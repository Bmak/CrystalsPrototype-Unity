using System;

public class HomeBaseRenderData
{
	public Action StartGame { get; set; }
	public Action ConnectFacebook { get; set; }
    public long MaxScore { get; set; }
    public long LastGameScore { get; set; }
    
}