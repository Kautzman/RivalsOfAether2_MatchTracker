using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Rivals2Tracker.Models
{
    class RivalsOcrResult
    {

        public RivalsMatch? Match { get; set; }
        public string RawData { get; set; } = String.Empty;
        public bool IsValid { get; set; } = false;
        public string ErrorText { get; set; } = String.Empty;

        public RivalsOcrResult(RivalsMatch rivalsMatch)
        {
            Match = rivalsMatch;
            IsValid = true;
        }

        public RivalsOcrResult(bool isValid, MatchValidityFlag flag)
        {
            IsValid = isValid;

            switch (flag)
            {
                case MatchValidityFlag.NoKad: ErrorText = "Can't Find a Kadecgos!"; break;
                case MatchValidityFlag.NoElo: ErrorText = "Failed to parse Elo"; break;
                default: ErrorText = "Unknown Error in parsing Validity Flag"; break;
            }
        }

        public RivalsOcrResult(bool isValid, string errorText)
        {
            IsValid = isValid;
            ErrorText = errorText;

        }
    }
}
