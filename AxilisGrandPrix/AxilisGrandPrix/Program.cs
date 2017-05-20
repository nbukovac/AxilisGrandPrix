using System;
using System.Net;
using Newtonsoft.Json;

namespace AxilisGrandPrix
{
    public class Program
    {
        private const string Token = "59201529bab30575fe2b7173";
        private const string NewGameUri = "http://api.jobfair.axilis.com/game/new";
        private const string SwapItemsUri = "http://api.jobfair.axilis.com/game/swap";
        private const int BoardSize = 9;

        public static void Main(string[] args)
        {
            var result = "";
            var gameToken = "";
            var random = new Random();

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                Console.WriteLine(JsonConvert.SerializeObject(new GameStartSendModel
                {
                    token = Token
                }));

                result = client.UploadString(NewGameUri, "POST", JsonConvert.SerializeObject(new GameStartSendModel
                {
                    token = Token
                }));


                var gameStart = JsonConvert.DeserializeObject(result, typeof(GameStartModel)) as GameStartModel;
                var board = gameStart.board;
                gameToken = gameStart.gameToken;

                int row1, row2, col1, col2 = 0;

                while (true)
                {
                    SwapElements(board, out row1, out col1, out row2, out col2);

                    Console.WriteLine(JsonConvert.SerializeObject(new SwapSendModel
                    {
                        token = gameToken,
                        row1 = row1,
                        col1 = col1,
                        row2 = row2,
                        col2 = col2
                    }));


                    using (var newClient = new WebClient())
                    {
                        newClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                        result = newClient.UploadString(SwapItemsUri, "POST",
                            JsonConvert.SerializeObject(new SwapSendModel
                            {
                                token = gameToken,
                                row1 = row1,
                                col1 = col1,
                                row2 = row2,
                                col2 = col2
                            }));
                    }

                    var swapReceived =
                        JsonConvert.DeserializeObject(result, typeof(SwapReceiveModel)) as SwapReceiveModel;

                    Console.WriteLine(swapReceived.totalScore + "\tturn " + swapReceived.round);

                    if (swapReceived.gameOver)
                    {
                        Console.WriteLine("Game ended " + swapReceived.totalScore);
                        break;
                    }

                    board = swapReceived.board;
                }
            }
        }

        private static void SwapElements(int[,] board, out int row1, out int col1, out int row2, out int col2)
        {
            row1 = col1 = 8;
            row2 = 8;
            col2 = 7;

            for (var i = BoardSize - 1; i >= 0; i--)
            {
                for (var j = BoardSize - 1; j >= 2; j--)
                {
                    if (board[i, j] == board[i, j - 1])
                    {
                        if ((i >= 1) && (board[i - 1, j - 2] == board[i, j]))
                        {
                            row1 = i;
                            col1 = j - 2;
                            row2 = i - 1;
                            col2 = j - 2;
                            return;
                        }
                        if ((i >= 1) && (j >= 3) && (board[i, j - 3] == board[i, j]))
                        {
                            row1 = i;
                            col1 = j - 2;
                            row2 = i;
                            col2 = j - 3;
                            return;
                        }
                        if ((i < BoardSize - 1) && (board[i + 1, j - 2] == board[i, j]))
                        {
                            row1 = i;
                            col1 = j - 2;
                            row2 = i + 1;
                            col2 = j - 2;
                            return;
                        }
                        if ((i < BoardSize - 1) && (j < BoardSize - 1) && (board[i + 1, j + 1] == board[i, j]))
                        {
                            row1 = i;
                            col1 = j + 1;
                            row2 = i + 1;
                            col2 = j + 1;
                            return;
                        }
                    }

                    if (board[i, j] == board[i, j - 2])
                    {
                        if ((i >= 1) && (board[i - 1, j - 1] == board[i, j]))
                        {
                            row1 = i;
                            col1 = j - 1;
                            row2 = i - 1;
                            col2 = j - 1;
                            return;
                        }
                    }

                    if (board[i, j - 1] == board[i, j - 2])
                    {
                        if ((i >= 1) && (board[i - 1, j] == board[i, j - 1]))
                        {
                            row1 = i;
                            col1 = j;
                            row2 = i - 1;
                            col2 = j;
                            return;
                        }
                    }

                    if ((i > 1) && (board[i, j] == board[i - 1, j]))
                    {
                        if ((j < BoardSize - 1) && (board[i, j] == board[i - 2, j + 1]))
                        {
                            row1 = i - 2;
                            col1 = j;
                            row2 = i - 2;
                            col2 = j + 1;
                            return;
                        }
                        if (board[i, j] == board[i - 2, j - 1])
                        {
                            row1 = i - 2;
                            col1 = j;
                            row2 = i - 2;
                            col2 = j - 1;
                            return;
                        }
                        if ((i > 2) && (board[i, j] == board[i - 3, j]))
                        {
                            row1 = i - 2;
                            col1 = j;
                            row2 = i - 3;
                            col2 = j;
                            return;
                        }
                    }

                    if ((i > 1) && (board[i, j] == board[i - 2, j]))
                    {
                        if (board[i, j] == board[i - 1, j - 1])
                        {
                            row1 = i - 1;
                            col1 = j;
                            row2 = i - 1;
                            col2 = j - 1;
                            return;
                        }
                        if ((j < BoardSize - 1) && (board[i, j] == board[i - 1, j + 1]))
                        {
                            row1 = i - 1;
                            col1 = j;
                            row2 = i - 1;
                            col2 = j + 1;
                            return;
                        }
                    }
                }
            }
        }
    }
}