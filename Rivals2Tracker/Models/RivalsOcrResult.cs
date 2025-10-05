using System;

namespace Slipstream.Models
{
    class RivalsOcrResult
    {

        public RivalsMatch? Match { get; set; }
        public string RawData { get; set; } = String.Empty;
        public bool IsValid { get; set; } = false;
        public bool IsSalvagable { get; set; } = true;
        public string StatusText { get; set; } = String.Empty;
        public string ErrorText { get; set; } = String.Empty;

        public RivalsOcrResult(RivalsMatch rivalsMatch, MatchValidityFlag flag = MatchValidityFlag.Valid)
        {
            Match = rivalsMatch;
            IsValid = false;

            switch (flag)
            {
                case MatchValidityFlag.NoKad: ErrorText = "Can't match the Local Player name!"; break;
                case MatchValidityFlag.NoElo: ErrorText = "Failed to parse Elo"; break;
                case MatchValidityFlag.Valid: break;
                default: ErrorText = "Unknown Error in parsing Validity Flag"; break;
            }
        }

        public RivalsOcrResult(bool isValid, string errorText, bool isSalvagable = true)
        {
            IsValid = isValid;
            StatusText = errorText;
            IsSalvagable = isSalvagable;

        }
    }
}
