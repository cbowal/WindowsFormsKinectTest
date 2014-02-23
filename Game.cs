using System;
using System.Drawing;

public class Game
{

    private int[,] _board;
    private Rectangle[,]_dims;

	public Game()
	{
        _board = new int[6, 7] {
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                {0,0,0,0,0,0,0},
                                            };

        _dims = new Rectangle[1, 7] {
                                {new Rectangle(40, 50, 80, 61), new Rectangle(120, 50, 80, 61), new Rectangle(200, 50, 80, 61),
                                    new Rectangle(280, 50, 80, 61), new Rectangle(360, 50, 80, 61), new Rectangle(440, 50, 80, 61),
                                    new Rectangle(520, 50, 80, 61)}
        };
	}

   /* |50
    80
    ||||||||
    ||||||||
    ||||||||61
    ||||||||
    ||||||||
    */

    public void DrawGridAndScore(Graphics g, AForge.Point p)
    {
        //draw the grid 
        Pen borderPen = new Pen(Color.White, 2);
        Brush grayBrush = new SolidBrush(Color.Orange);
        for (int x = 0; x < 1; x++)
            for (int y = 0; y < 7; y++)
            {
                if (_dims[x, y].Contains((int)p.X, (int)p.Y))
                    g.FillRectangle(grayBrush, _dims[x, y]);
                else
                    g.DrawRectangle(borderPen, _dims[x, y]);
            }

        // the grid is 640*480
        // there should be 7*6 spots for a dot
        // we need to draw 8 vertical lines and 7 horizontal
        //for (int i = 0; i < 8; i++)
        //    g.DrawLine(whitePen, (i * 80), 50, (i * 80), 480);
        //for (int i = 0; i < 7; i++)
        //    g.DrawLine(whitePen, 0, (i * 61)+50, 640, (i * 61)+50);
    }

    /*public bool placeMove(int col, int player)
    {
        for (int i = 0; i < 6; i++)
        {
            if (_board[i][col] == 0)
            {

            }
        }
    }*/
}
