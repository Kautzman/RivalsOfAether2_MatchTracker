using Prism.Mvvm;
using Slipstream.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slipstream.Models
{
    class RivalsMatch : BindableBase
    {

        private string _id = "-1";
        public string ID
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private RivalsPlayer _player1;
        public RivalsPlayer Player1
        {
            get { return _player1; }
            set { SetProperty(ref _player1, value); }
        }

        private RivalsPlayer _player2;
        public RivalsPlayer Player2
        {
            get { return _player2; }
            set { SetProperty(ref _player2, value); }
        }

        private RivalsPlayer _opponent;
        public RivalsPlayer Opponent
        {
            get { return _opponent; }
            set { SetProperty(ref _opponent, value); }
        }
        private RivalsPlayer _me;
        public RivalsPlayer Me
        {
            get { return _me; }
            set { SetProperty(ref _me, value); }
        }

        private ObservableCollection<RivalsGame> _games;
        public ObservableCollection<RivalsGame> Games
        {
            get { return _games; }
            set { SetProperty(ref _games, value); }
        }

        private string _notes = String.Empty;
        public string Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value); }
        }

        private string _patch = String.Empty;
        public string Patch
        {
            get { return _patch; }
            set
            {
                SetProperty(ref _patch, value);
            }
        }

        private RivalsSeason _season = GlobalData.CurrentSeason;
        public RivalsSeason Season
        {
            get { return _season; }
            set
            {
                SetProperty(ref _season, value);
            }
        }


        private MatchStatus _status = MatchStatus.New;
        public MatchStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public string StatusString
        {
            get
            {
                switch (Status)
                {
                    case MatchStatus.InProgress: return "In Progress";
                    case MatchStatus.Finished: return "Finished";
                    case MatchStatus.Invalid: return "Invalid";
                    default: return "Unknown";
                }
            }
        }
        private bool _isWon;
        public bool IsWon
        {
            get { return _isWon; }
            set { SetProperty(ref _isWon, value); }
        }

        public string MatchResult
        {
            get
            {
                if (Status == MatchStatus.Invalid)
                {
                    return "Invalid";
                }

                if (IsWon)
                {
                    return "Win";
                }
                else
                {
                    return "Lose";
                }
            }
        }

        public RivalsMatch(RivalsPlayer player1, RivalsPlayer player2)
        {
            Player1 = player1;
            Player2 = player2;

            if (player1.IsLocalPlayer())
            {
                Me = player1;
                Opponent = player2;
            }
            else if (player2.IsLocalPlayer())
            {
                Me = player2;
                Opponent = player1;
            }
            else
            {
                Me = player1;
                Opponent = player2;
            }

            BuildGames();
        }

        private void BuildGames()
        {
            Games = new();

            for (int i = 1; i <= 3; i++)
            {
                RivalsGame newGame = new RivalsGame(i, Me.Character);
                Games.Add(newGame);
            }
        }

        public bool HasNoKad()
        {
            if (Player1.IsLocalPlayer() || Player2.IsLocalPlayer())
            {
                return false;
            }

            return true;
        }

        public void Clean()
        {
            if (String.IsNullOrEmpty(Player1.Name) && Player2.IsLocalPlayer())
            {
                Player1.Name = "???";
            }

            if (String.IsNullOrEmpty(Player2.Name) && Player1.IsLocalPlayer())
            {
                Player1.Name = "???";
            }
        }

        public bool IsValid(out MatchValidityFlag validityFlag)
        {
            if (!Player1.IsLocalPlayer() && !Player2.IsLocalPlayer())
            {
                validityFlag = MatchValidityFlag.NoKad;
                return false;
            }

            if (Player1.Elo == "-1" || Player2.Elo == "-1")
            {
                validityFlag = MatchValidityFlag.NoElo;
                return false;
            }

            validityFlag = MatchValidityFlag.Valid;
            return true;
        }
    }

    public enum MatchStatus
    {
        InProgress = 1,
        Finished = 2,
        Invalid = 3,
        New = 4
    }

    public enum MatchValidityFlag
    {
        Valid = 1,
        NoKad = 2,
        NoElo = 3
    }

    public enum RivalsSeason
    {
        Season3,
        Season4,
        Season5
    }
}
