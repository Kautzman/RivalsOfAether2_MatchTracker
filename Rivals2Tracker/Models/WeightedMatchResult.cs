using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slipstream.Models
{
    public class WeightedMatchResult
    {
        public int EloDelta { get; set; }
        public int MyElo { get; set; }
        public int OpponentElo { get; set; }
        public MatchProximity Proximity { get; set; }
        public string Result { get; set; } = String.Empty;
        public string Patch { get; set; } = String.Empty;

        public WeightedMatchResult(int myElo, MatchProximity proximity, string result, string patch)
        {
            MyElo = myElo;
            Proximity = proximity;
            Result = result;
            Patch = patch;
        }

        public WeightedMatchResult(int eloDelta, int myElo, int opponentElo, MatchProximity proximity, string result, string patch)
        {
            EloDelta = eloDelta;
            MyElo = myElo;
            OpponentElo = opponentElo;
            Proximity = proximity;
            Result = result;
            Patch = patch;
        }
    }
}
