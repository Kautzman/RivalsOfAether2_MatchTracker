using Prism.Mvvm;
using Slipstream.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;

namespace Slipstream.Models
{
    public class RivalsMatch : BindableBase
    {

        private string _id = "-1";
        public string ID
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private RivalsPlayer _me;
        public RivalsPlayer Me
        {
            get { return _me; }
            set { SetProperty(ref _me, value); }
        }

        private RivalsPlayer _opponent;
        public RivalsPlayer Opponent
        {
            get { return _opponent; }
            set { SetProperty(ref _opponent, value); }
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

        private RivalsSeasonEnum _season = GlobalData.CurrentSeason;
        public RivalsSeasonEnum Season
        {
            get { return _season; }
            set
            {
                SetProperty(ref _season, value);
            }
        }

        private MatchStatus _status = MatchStatus.InProgress;
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

        public Dictionary<RivalsCharacter, int> CharactersPlayed = new();

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

            for (int i = 1; i <= GlobalData.BestOf; i++)
            {
                GameResultEnum defaultGameState = GameResultEnum.Unplayed;

                if (i < 3 || (i == 3 && GlobalData.BestOf == 5))
                {
                    defaultGameState = GameResultEnum.InProgress;
                }

                RivalsGame newGame = new RivalsGame();
                newGame.BuildGame(i, Me.Character, Opponent.Character, defaultGameState, this);
                Games.Add(newGame);
            }
        }

        public bool AreCharactersSet()
        {
            foreach (RivalsGame game in Games)
            {
                if (game.ResultIsValid())
                {
                    if (String.IsNullOrEmpty(game.MyCharacter.Name) || String.IsNullOrEmpty(game.OppCharacter.Name))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // This handles a case where the OCR either didn't pick up a character or you are manually setting up matches so you don't have to select characters for every game.
        public void CascadeCharacterSelection(string player, RivalsCharacter character)
        {
            foreach (RivalsGame game in Games)
            {
                if (player == "Opponent")
                {
                    if (game.OppCharacter is null || game.OppCharacter.Name == "Unknown" || String.IsNullOrEmpty(game.OppCharacter.Name))
                    {
                        game.AutoSetOppCharacter(character);
                    }
                }

                if (player == "Me")
                {
                    if (game.MyCharacter is null || game.MyCharacter.Name == "Unknown" || String.IsNullOrEmpty(game.MyCharacter.Name))
                    {
                        game.AutoSetMyCharacter(character);
                    }
                }
            }
        }

        // This is an OCR test - if it doesn't pick up your tag in either player 1 or player 2, it can do something from that state.
        public bool LocalPlayerNameNotMatched()
        {
            if (Player1.IsLocalPlayer() || Player2.IsLocalPlayer())
            {
                return false;
            }

            return true;
        }

        public void Clean()
        {
            if (String.IsNullOrEmpty(Player1.PlayerTag) && Player2.IsLocalPlayer())
            {
                Player1.PlayerTag = "???";
            }

            if (String.IsNullOrEmpty(Player2.PlayerTag) && Player1.IsLocalPlayer())
            {
                Player1.PlayerTag = "???";
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

        public bool CanBeFlaggedWin()
        {
            int wins = Games.Where(g => g.Result == GameResultEnum.Win).Count();
            int losses = Games.Where(g => g.Result == GameResultEnum.Lose).Count();

            if (wins > losses)
            {
                if (wins == 1)
                {
                    DialogResult result = MessageBox.Show("There aren't enough games to played for this set without a forfeit - are you sure you want to record it as a 'Win'?", "Record Partial Match", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        return true;
                    }
                }

                return true;
            }

            return false;
        }
        public bool CanBeFlaggedLoss()
        {
            int wins = Games.Where(g => g.Result == GameResultEnum.Win).Count();
            int losses = Games.Where(g => g.Result == GameResultEnum.Lose).Count();

            if (losses > wins)
            {
                if (losses == 1)
                {
                    DialogResult result = MessageBox.Show("There aren't enough games to played for this set without a forfeit - are you sure you want to record it as a 'Loss'?", "Record Partial Match", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        return true;
                    }
                }

                return true;
            }

            return false;
        }

        public bool AreStagesPicked()
        {
            foreach (RivalsGame game in Games)
            {
                if (game.SelectedStage == null && game.ResultIsValid())
                {
                    return false;
                }
            }

            return true;
        }

        public void DetermineMatchCharacters()
        {
            CharactersPlayed = new();

            foreach (RivalsGame game in Games)
            {
                if (game.ResultIsValid())
                {
                    CharactersPlayed[game.OppCharacter] = CharactersPlayed.GetValueOrDefault(game.OppCharacter, 0) + 1;
                }
            }
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

    public enum RivalsSeasonEnum
    {
        Season3,
        Season4,
        Season5
    }
}