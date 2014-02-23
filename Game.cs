using System;

public class Game
{

    private Bitmap _bitmap;
    private int[,] _board;

	public Game(Bitmap bitmap)
	{
        _bitmap = bitmap;
        _board = new int[6, 7] {
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                            };
	}

    public Bitmap DrawGridAndScore()
    {
        //draw the grid 
        Pen whitePen = new Pen(Color.White, 2);
        Graphics g = Graphics.FromImage(_bitmap);
        // the grid is 640*480
        // there should be 7*6 spots for a dot
        // we need to draw 8 vertical lines and 7 horizontal
        for (int i = 0; i < 8; i++)
            g.DrawLine(whitePen, (i * 80), 0, (i * 80), 480);
        for (int i = 0; i < 7; i++)
            g.DrawLine(whitePen, 0, (i * 68), 640, (i * 68));
        

    }

    public bool placeMove(int col, int player)
    {
        for (int i = 0; i < 6; i++)
        {
            if (_board[i][col] == 0)
            {

            }
        }
    }
}
