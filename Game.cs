using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using Newtonsoft.Json.Linq;

public class Game 
{
    private int[,] _board;
    private Rectangle[,]_dims;
    private Rectangle[] _rows;
    private int previousDropRow = -1;
    private int activatedColumn = -1;
    public bool hasWon = false;
    private Object _lock = new Object();
    private int player = 0;
    public int winningPlayer = 0;

    public void reset(int tPlayer)
    {
        player = tPlayer;

        lock (_lock)
        {
            _board = new int[6, 7] {
                            {0,0,0,0,0,0,0},
                            {0,0,0,0,0,0,0},
                            {0,0,0,0,0,0,0},
                            {0,0,0,0,0,0,0},
                            {0,0,0,0,0,0,0},
                            {0,0,0,0,0,0,0},
                                        };
        }
        previousDropRow = -1;
        activatedColumn = -1;
        hasWon = false;


        _dims = new Rectangle[6, 7] {
                                {new Rectangle(40, 50, 80, 61), new Rectangle(120, 50, 80, 61), new Rectangle(200, 50, 80, 61),
                                    new Rectangle(280, 50, 80, 61), new Rectangle(360, 50, 80, 61), new Rectangle(440, 50, 80, 61),
                                    new Rectangle(520, 50, 80, 61)},
                                {new Rectangle(40, 111, 80, 61), new Rectangle(120, 111, 80, 61), new Rectangle(200, 111, 80, 61),
                                    new Rectangle(280, 111, 80, 61), new Rectangle(360, 111, 80, 61), new Rectangle(440, 111, 80, 61),
                                    new Rectangle(520, 111, 80, 61)},
                                {new Rectangle(40, 172, 80, 61), new Rectangle(120, 172, 80, 61), new Rectangle(200, 172, 80, 61),
                                    new Rectangle(280, 172, 80, 61), new Rectangle(360, 172, 80, 61), new Rectangle(440, 172, 80, 61),
                                    new Rectangle(520, 172, 80, 61)},
                                {new Rectangle(40, 233, 80, 61), new Rectangle(120, 233, 80, 61), new Rectangle(200, 233, 80, 61),
                                    new Rectangle(280, 233, 80, 61), new Rectangle(360, 233, 80, 61), new Rectangle(440, 233, 80, 61),
                                    new Rectangle(520, 233, 80, 61)},
                                {new Rectangle(40, 294, 80, 61), new Rectangle(120, 294, 80, 61), new Rectangle(200, 294, 80, 61),
                                    new Rectangle(280, 294, 80, 61), new Rectangle(360, 294, 80, 61), new Rectangle(440, 294, 80, 61),
                                    new Rectangle(520, 294, 80, 61)},
                                {new Rectangle(40, 355, 80, 61), new Rectangle(120, 355, 80, 61), new Rectangle(200, 355, 80, 61),
                                    new Rectangle(280, 355, 80, 61), new Rectangle(360, 355, 80, 61), new Rectangle(440, 355, 80, 61),
                                    new Rectangle(520, 355, 80, 61)},

        };

        _rows = new Rectangle[] {
            new Rectangle(40, 50, 560, 80),    //activation row
            new Rectangle(20, 130, 600, 160),  //first drop row
            new Rectangle(0, 290, 640, 190),  //second drop row
        };


        WebClient wc = new WebClient();
        wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
        wc.DownloadStringAsync(new Uri("http://107.170.71.135:8000/reset/"));
    }

	public Game()
	{
        reset(1);
	}

    Pen arrowPen = new Pen(Color.Purple, 10);
    Pen borderPen = new Pen(Color.White, 2);
    Brush grayBrush = new SolidBrush(Color.Orange);
    Brush brush = new SolidBrush(Color.LightGreen);
    Brush p1Brush = new SolidBrush(Color.Green);
    Brush p2Brush = new SolidBrush(Color.Blue);

    private int sum4(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
    {
        lock (_lock)
        {
            return _board[x1, y1] + _board[x2, y2] + _board[x3, y3] + _board[x4, y4];
        }
    }

    private void someoneWon(int playerNum)
    {
        hasWon = true;
        winningPlayer = playerNum;

        if (player == playerNum)
            System.Console.WriteLine("YOU WON!!! Congrats :)");
        else
            System.Console.WriteLine("YOU LOST... :(");
    }

    private void didSomeoneWin()
    {
        //check vertical lines
        for (int x = 0; x < 6 - 3; x++)
            for (int y = 0; y < 7; y++)
            {
                int sum = sum4(x, y, x + 1, y, x + 2, y, x + 3, y) / 4;
                if (sum != 0)
                    someoneWon(sum);
            }

        //check horizontal lines
        for (int x = 0; x < 6; x++)
            for (int y = 0; y < 7 - 3; y++)
            {
                int sum = sum4(x, y, x, y + 1, x, y + 2, x, y + 3) / 4;
                if (sum != 0)
                    someoneWon(sum);
            }

        //check down-right diagonal lines
        for (int x = 0; x < 6 - 3; x++)
            for (int y = 0; y < 7 - 3; y++)
            {
                int sum = sum4(x, y, x + 1, y + 1, x + 2, y + 2, x + 3, y + 3) / 4;
                if (sum != 0)
                    someoneWon(sum);
            }

        //check down-left diagonal lines
        for (int x = 0; x < 6 - 3; x++)
            for (int y = 0; y < 7 - 3; y++)
            {
                int sum = sum4(x + 3, y + 3, x + 2, y + 2, x + 1, y + 1, x, y) / 4;
                if (sum != 0)
                    someoneWon(sum);
            }
    }


    public void DrawGridAndScore(Graphics g, AForge.Point p)
    {
        //draw the grid 
        lock (_lock)
        {
            for (int x = 0; x < 6; x++)
                for (int y = 0; y < 7; y++)
                {
                    if (_board[x, y] == 1)
                    {
                        g.FillRectangle(p1Brush, _dims[x, y]);
                    }
                    else if (_board[x, y] == -1)
                    {
                        g.FillRectangle(p2Brush, _dims[x, y]);
                    }
                    else if (x == 0 && _dims[x, y].Contains((int)p.X, (int)p.Y))
                    {
                        arrowPen.StartCap = LineCap.Round;
                        arrowPen.EndCap = LineCap.ArrowAnchor;
                        g.DrawLine(arrowPen, _dims[x, y].X + 40, _dims[x, y].Y - 35, _dims[x, y].X + 40, _dims[x, y].Y - 5);
                        activatedColumn = y;

                        g.FillRectangle(grayBrush, _dims[x, y]);
                    }
                    else
                    {
                        g.DrawRectangle(borderPen, _dims[x, y]);
                    }
                }
        }
        

        if (hasWon)
            return;

        //update drop status
        switch (previousDropRow)
        {
            case -1: //not valid - check if at top
                if (_rows[0].Contains((int)p.X, (int)p.Y))
                {
                    //g.FillRectangle(brush, _rows[0]);
                    previousDropRow++;
                }
                break;
            case 0: //was in top - check if in middle
                if (_rows[1].Contains((int)p.X, (int)p.Y))
                {
                    //g.FillRectangle(brush, _rows[1]);
                    previousDropRow++;
                }
                break;
            case 1: //was in middle - check if at bottom
                if (_rows[2].Contains((int)p.X, (int)p.Y))
                {
                    if (activatedColumn == -1)
                    {
                        //oops - didn't activate!
                        previousDropRow = -1;
                    }
                    else
                    {
                        //g.FillRectangle(brush, _rows[2]);
                        //woot - they dropped it!!!
                        
                        System.Console.WriteLine("DROPPED!!!");
                        //call web api
                        dropPiece(activatedColumn, player);

                        previousDropRow = -1;
                        activatedColumn = -1;
                    }
                }
                break;
            default:
                break;
        }

        updateBoard();

        didSomeoneWin();
    }

    private void dropPiece(int col, int player)
    {
        WebClient wc = new WebClient();
        wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
        wc.DownloadStringAsync(new Uri("http://107.170.71.135:8000/move/" + col.ToString() + "/" + player.ToString() + "/"));
    }


    private void updateBoard()
    {
        WebClient wc = new WebClient();
        wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
        wc.DownloadStringAsync(new Uri("http://107.170.71.135:8000/board/"));
    }

    private void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
        JObject json = JObject.Parse(e.Result);
        lock (_lock)
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    _board[i, j] = (int)json["board"][i][j];
                }
            }
        }
    }
}
