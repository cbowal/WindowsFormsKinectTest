using System;

public class GameResetter
{
	public GameResetter()
	{
        MyWebRequest request = new MyWebRequest("http://107.170.71.135:8000/reset/");
	}
}
