using System;
using Newtonsoft.Json.Linq;
using System.Threading;
public class BoardQueryThread
{
    int[,] _board;
    Object _lock;
    public BoardQueryThread(ref int[,] board, ref Object lock_obj)
    {
        _board = board;
        _lock = lock_obj;
    }

    public void QueryBoardRun()
    {
        while (true)
        {
            Thread.Sleep(100);
            MyWebRequest request = new MyWebRequest("http://107.170.71.135:8000/board/");
            string json_response = request.GetResponse();
            JObject json = JObject.Parse(json_response);
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
}
