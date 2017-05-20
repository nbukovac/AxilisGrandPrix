using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxilisGrandPrix
{
    public class SwapReceiveModel
    {
        public int score { get; set; }
        public int totalScore { get; set; }
        public int round { get; set; }
        public bool gameOver { get; set; }
        public bool gameReshuffled { get; set; }
        public int[,] board { get; set; }
    }
}
