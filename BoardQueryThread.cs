using System;
using Newtonsoft.Json.Linq;
using System.Threading;
public class BoardQueryThread
{
    //int[,] _board;
    Object _lock;
    public BoardQueryThread(ref int[,] board, ref Object lock_obj)
    {
        //_board = board;
        _lock = lock_obj;
    }



    public static void QueryBoardRun(int[,] _board)
    {
    }
}
