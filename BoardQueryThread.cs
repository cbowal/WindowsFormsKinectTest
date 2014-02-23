using System;
using Newtonsoft.Json.Linq;

public class BoardQueryThread
{
    int[,] _board;
    public BoardQueryThread(int[,] board)
    {
        _board = board;
    }

    public void QueryBoardRun()
    {
        while (true)
        {
            MyWebRequest request = MyWebRequest("http://107.170.71.135:8000/board/");
            string json_response = request.GetResponse();
            JObject json = JObject.Parse(json_response);
            for (int i = 0; i < 6; i++) {
                for (int j = 0; j < 7; j++) {
                    board[i][j] = (float)json["board"][i][j];
                }
            }
        }
    }
}
