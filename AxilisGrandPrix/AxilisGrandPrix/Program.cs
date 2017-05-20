using System;
using System.Collections.Generic;
using System.Data;
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

                while (true)
                {
                    var swap = SmarterSwapElements(board);

                    if (swap == null)
                    {
                        for (int i = 0; i < BoardSize - 1; i++)
                        {
                            for (int j = 0; j < BoardSize - 1; j++)
                            {
                                Console.Write(board[i, j] + " ");
                            }
                            Console.WriteLine();
                        }

                        SmarterSwapElements(board);
                    }

                    Console.WriteLine(JsonConvert.SerializeObject(new SwapSendModel
                    {
                        token = gameToken,
                        row1 = swap.Row1,
                        col1 = swap.Column1,
                        row2 = swap.Row2,
                        col2 = swap.Column2
                    }));


                    using (var newClient = new WebClient())
                    {
                        newClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                        result = newClient.UploadString(SwapItemsUri, "POST",
                            JsonConvert.SerializeObject(new SwapSendModel
                            {
                                token = gameToken,
                                row1 = swap.Row1,
                                col1 = swap.Column1,
                                row2 = swap.Row2,
                                col2 = swap.Column2
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

        private static SwapElement SmarterSwapElements(int[,] board)
        {
            var swaps = new List<SwapElement>();

            for (int i = BoardSize - 1; i >= 0; i--)
            {
                for (int j = BoardSize - 1; j >= 0; j--)
                {
                    var horizontalFront = CheckHorizontalFront(i, j, board);
                    var horizontalBack = CheckHorizontalBack(i, j, board);
                    var horizontalGap = CheckHorizontalGap(i, j, board);
                    var horizontalMiddle = CheckHorizontalMiddle(i, j, board);

                    var verticalBottom = CheckVerticalBottom(i, j, board);
                    var verticalTop = CheckVerticalTop(i, j, board);
                    var verticalGap = CheckVerticalGap(i, j, board);
                    var verticalMiddle = CheckVerticalMiddle(i, j, board);

                    if (horizontalBack != null)
                    {
                        Console.WriteLine("horizontalBack");
                        return horizontalBack;
                    }

                    if (horizontalFront != null)
                    {
                        Console.WriteLine("horizontalFront");
                        return horizontalFront;
                    }

                    if (horizontalGap != null)
                    {
                        Console.WriteLine("horizontalGap");
                        return horizontalGap;
                    }

                    if (horizontalMiddle != null)
                    {
                        Console.WriteLine("horizontalMiddle");
                        return horizontalMiddle;
                    }

                    if (verticalGap != null)
                    {
                        Console.WriteLine("verticalGap");
                        return verticalGap;
                    }

                    if (verticalBottom != null)
                    {
                        Console.WriteLine("verticalBottom");
                        return verticalBottom;
                    }

                    if (verticalTop != null)
                    {
                        Console.WriteLine("verticalTop");
                        return verticalTop;
                    }

                    if (verticalMiddle != null)
                    {
                        Console.WriteLine("verticalMiddle");
                        return verticalMiddle;
                    }
                }
            }

            return null;
        }

        private static SwapElement CheckHorizontalFront(int i, int j, int[,] board)
        {
            if (j <= BoardSize - 2)
            {
                if (board[i, j] == board[i, j + 1])
                {
                    if (i < BoardSize - 1 && j > 0)
                    {
                        if (board[i + 1, j - 1] == board[i, j])
                        {
                            return new SwapElement()
                            {
                                Row1 = i,
                                Column1 = j - 1,
                                Row2 = i + 1,
                                Column2 = j - 1
                            };
                        }
                    }

                    if (i > 0 && j > 0)
                    {
                        if (board[i - 1, j - 1] == board[i, j])
                        {
                            return new SwapElement()
                            {
                                Row1 = i,
                                Column1 = j - 1,
                                Row2 = i - 1,
                                Column2 = j - 1
                            };
                        }
                    }
                }
            }

            return null;
        }

        private static SwapElement CheckHorizontalMiddle(int i, int j, int[,] board)
        {
            if (j <= BoardSize - 3)
            {
                if (board[i, j] == board[i, j + 2])
                {
                    if (i < BoardSize - 1 && board[i, j] == board[i + 1, j + 1])
                    {
                        return new SwapElement()
                        {
                            Row1 = i,
                            Column1 = j + 1,
                            Row2 = i + 1,
                            Column2 = j + 1
                        };
                    }

                    if (i > 0 && board[i, j] == board[i - 1, j + 1])
                    {
                        return new SwapElement()
                        {
                            Row1 = i,
                            Column1 = j + 1,
                            Row2 = i - 1,
                            Column2 = j + 1
                        };
                    }
                }
            }

            return null;
        }

        private static SwapElement CheckHorizontalBack(int i, int j, int[,] board)
        {
            if (j <= BoardSize - 3)
            {
                if (board[i, j] == board[i, j + 1])
                {
                    if (i < BoardSize - 1)
                    {
                        if (board[i + 1, j + 2] == board[i, j])
                        {
                            return new SwapElement()
                            {
                                Row1 = i,
                                Column1 = j + 2,
                                Row2 = i + 1,
                                Column2 = j + 2
                            };
                        }
                    }

                    if (i > 0)
                    {
                        if (board[i - 1, j + 2] == board[i, j])
                        {
                            return new SwapElement()
                            {
                                Row1 = i,
                                Column1 = j + 2,
                                Row2 = i - 1,
                                Column2 = j + 2
                            };
                        }
                    }
                }
            }

            return null;
        }

        private static SwapElement CheckHorizontalGap(int i, int j, int[,] board)
        {
            if (j <= BoardSize - 4)
            {
                if (board[i, j] == board[i, j + 1] && board[i, j] == board[i, j + 3])
                {
                    return new SwapElement()
                    {
                        Row1 = i,
                        Column1 = j + 2,
                        Row2 = i,
                        Column2 = j + 3
                    };
                }
            }

            if (j >= 2 && j <= BoardSize - 2)
            {
                if (board[i, j] == board[i, j + 1] && board[i, j] == board[i, j - 2])
                {
                    return new SwapElement()
                    {
                        Row1 = i,
                        Column1 = j - 1,
                        Row2 = i,
                        Column2 = j - 2
                    };
                }
            }

            return null;
        }

        private static SwapElement CheckVerticalBottom(int i, int j, int[,] board)
        {
            if (i <= BoardSize - 3)
            {
                if (board[i, j] == board[i + 1, j])
                {
                    if (j >= 1 && board[i, j] == board[i + 2, j - 1])
                    {
                        return new SwapElement()
                        {
                            Row1 = i + 2,
                            Column1 = j,
                            Row2 = i + 2,
                            Column2 = j - 1
                        };
                    }

                    if (j < BoardSize - 1 && board[i, j] == board[i + 2, j + 1])
                    {
                        return new SwapElement()
                        {
                            Row1 = i + 2,
                            Column1 = j,
                            Row2 = i + 2,
                            Column2 = j + 1
                        };
                    }
                }
            }

            return null;
        }

        private static SwapElement CheckVerticalMiddle(int i, int j, int[,] board)
        {
            if (i <= BoardSize - 3)
            {
                if (board[i, j] == board[i + 2, j])
                {
                    if (j < BoardSize - 1 && board[i, j] == board[i + 1, j + 1])
                    {
                        return new SwapElement()
                        {
                            Row1 = i + 1,
                            Column1 = j,
                            Row2 = i + 1,
                            Column2 = j + 1
                        };
                    }

                    if (j > 0 && board[i, j] == board[i + 1, j - 1])
                    {
                        return new SwapElement()
                        {
                            Row1 = i + 1,
                            Column1 = j,
                            Row2 = i + 1,
                            Column2 = j - 1
                        };
                    }
                }
            }

            return null;
        }

        private static SwapElement CheckVerticalTop(int i, int j, int[,] board)
        {
            if (i >= 2)
            {
                if (board[i, j] == board[i - 1, j])
                {
                    if (j > 0 && board[i, j] == board[i - 2, j - 1])
                    {
                        return new SwapElement()
                        {
                            Row1 = i - 2,
                            Column1 = j,
                            Row2 = i - 2,
                            Column2 = j - 1
                        };
                    }

                    if (j <= BoardSize - 2 && board[i, j] == board[i - 2, j + 1])
                    {
                        return new SwapElement()
                        {
                            Row1 = i - 2,
                            Column1 = j,
                            Row2 = i - 2,
                            Column2 = j + 1
                        };
                    }
                }   
            }

            return null;
        }

        private static SwapElement CheckVerticalGap(int i, int j, int[,] board)
        {
            if (i > 2)
            {
                if (board[i, j] == board[i - 1, j] && board[i, j] == board[i - 3, j])
                {
                    return new SwapElement()
                    {
                        Row1 = i - 2,
                        Column1 = j,
                        Row2 = i - 3,
                        Column2 = j
                    };
                }
            }

            if (i <= BoardSize - 4)
            {
                if (board[i, j] == board[i + 1, j] && board[i, j] == board[i + 3, j])
                {
                    return new SwapElement()
                    {
                        Row1 = i + 2,
                        Column1 = j,
                        Row2 = i + 3,
                        Column2 = j
                    };
                }
            }

            return null;
        }
    }
}