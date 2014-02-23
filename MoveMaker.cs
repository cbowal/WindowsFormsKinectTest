using System;

public class MoveMaker
{
    int _column;
    int _player = -1; //NOTE: THIS MUST BE 1 FOR CHRIS!
	public MoveMaker(int column)
	{
        _column = column;
	}

    public void makeMove()
    {
        MyWebRequest request = new MyWebRequest("http://107.170.71.135:8000/move/" + this._column + "/" + this._player);
    }
}