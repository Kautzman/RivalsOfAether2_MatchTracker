using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rivals2Tracker.Models
{
    class CharacterMetadata
    {
        public string Character { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int TotalGames
        {
            get
            {
                return Wins + Loses;
            }
        }

        public float WinRatio
        {
            get
            {
                return ((float)Wins / (float)TotalGames) * 100.0f;
            }
        }

        public string WinRatioString
        {
            get => $"{WinRatio.ToString()}%";
        }
    }
}
