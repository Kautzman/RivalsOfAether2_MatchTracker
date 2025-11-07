using Prism.Mvvm;
using Slipstream.Data;
using System;
using System.Windows;

namespace Slipstream.Models
{
    public class GameResult : BindableBase
    {
        public long MatchID { get; set; }
        public long GameID { get; set; }
        public GameResultEnum Result { get; set; }
        public RivalsCharacter MyCharacter { get; set; }
        public RivalsCharacter OppCharacter { get; set; }
        public RivalsStage PickedStage { get; set; }
        public RivalsStage BannedStage1 { get; set; }
        public RivalsStage BannedStage2 { get; set; }
        public Visibility BannedStage1Visibility { get; set; } = Visibility.Collapsed;
        public Visibility BannedStage2Visibility { get; set; } = Visibility.Collapsed;

        public GameResult(GameResultRecord record)
        {
            MatchID = record.MatchID;
            GameID = record.GameID;
            Result = record.Result == 1 ? GameResultEnum.Win : GameResultEnum.Lose;
            MyCharacter = GlobalData.GetCharacterByID(Convert.ToInt64(record.MyCharacter));
            OppCharacter = GlobalData.GetCharacterByID(Convert.ToInt64(record.OppCharacter));
            PickedStage = GlobalData.GetStageByID(Convert.ToInt64(record.PickedStage));


            if (!String.IsNullOrEmpty(record.BannedStagesList))
            {
                string[] stages = record.BannedStagesList.Split(',');

                if (stages.Length > 0 && !string.IsNullOrEmpty(stages[0]))
                {
                    BannedStage1 = GlobalData.GetStageByID(Convert.ToInt64(stages[0]));
                    BannedStage1Visibility = Visibility.Visible;
                }

                if (stages.Length > 1 && !string.IsNullOrEmpty(stages[1]))
                {
                    BannedStage2 = GlobalData.GetStageByID(Convert.ToInt64(stages[1]));
                    BannedStage2Visibility = Visibility.Visible;
                }
            }
        }
    }

    public record GameResultRecord()
    {
        public long MatchID { get; set; }
        public long GameID { get; set; }
        public long GameNumber { get; set; }
        public string MyCharacter { get; set; }
        public string OppCharacter { get; set; }
        public string PickedStage { get; set; }
        public long Result { get; set; }
        public string BannedStagesList { get; set; }
    }
}
