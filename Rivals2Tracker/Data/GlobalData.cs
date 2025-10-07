using Slipstream.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Slipstream.Data
{
    public static class GlobalData
    {
        public static IntPtr MainWindowHandle { get; set; }
        public static RivalsSeasonEnum CurrentSeason = RivalsSeasonEnum.Season3;
        public static string MyName = "YOUR TAG HERE";
        public static bool IsSaveCaptures = false;
        public static bool IsPlayAudio = true;
        public static uint HotKeyCode = 0;
        public static uint ModifierCode = 0;
        public static int BestOf;
        public static RivalsSeason AllSeasonsSeason = new RivalsSeason { ID = -1, Patch = "-1", Label = "All Seasons", IsCurrentSeason = false };
        public static ObservableCollection<RivalsCharacter> AllRivals = new();
        public static RivalsCharacter UnknownCharacter = new RivalsCharacter { ID = -1, Name = "Unknown" };
        public static ObservableCollection<RivalsStage> AllStages = new();
        public static Dictionary<long, ObservableCollection<GameResult>> GameResultsRef = new();

        public static RivalsCharacter GetCharacterByID(string id)
        {
            if (String.IsNullOrEmpty(id) || id == "-1")
            {
                return UnknownCharacter;
            }

            long longId = Convert.ToInt64(id);

            return AllRivals.First(r => r.ID == longId);
        }

        public static RivalsCharacter GetCharacterByID(long id)
        {
            return AllRivals.First(r => r.ID == id);
        }

        public static RivalsStage GetStageByID(long id)
        {
            return AllStages.First(s => s.ID == id);
        }
    }

    public enum MatchHistoryView
    {
        Stages,
        Rivals,
        Players
    }

    public enum PlayerInformationView
    {
        Notes,
        Matches
    }

    public enum RivalsCharacterEnum
    {
        Absa,
        Clairen,
        Etalus,
        Fleet,
        Kragg,
        Loxodont,
        Maypul,
        Olympia,
        Orcane,
        Ranno,
        Wrastor,
        Zetterburn,
        Galvan
    }

    public enum GameResultEnum
    {
        Lose = 0,
        Win = 1,
        Draw = 2,
        InProgress = 3, // This is used to represent a game that will be played in a set - so game 1 or 2 in a Bo3, or 1, 2, or 3, in a Bo5
        Unplayed = 4,
    }
}
