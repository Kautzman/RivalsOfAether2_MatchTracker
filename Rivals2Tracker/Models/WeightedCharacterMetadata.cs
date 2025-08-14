using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Prism.Mvvm;
using Rivals2Tracker.Data;

namespace Rivals2Tracker.Models
{
    public class WeightedCharacterMetadata : BindableBase
    {
        private string _recordClose = String.Empty;
        public string RecordClose
        {
            get { return _recordClose; }
            set { SetProperty(ref _recordClose, value); }
        }

        private string _recordMedium = String.Empty;
        public string RecordMedium
        {
            get { return _recordMedium; }
            set { SetProperty(ref _recordMedium, value); }
        }

        private string _recordFar = String.Empty;
        public string RecordFar
        {
            get { return _recordFar; }
            set { SetProperty(ref _recordFar, value); }
        }

        private string _recordUnranked = String.Empty;
        public string RecordUnranked
        {
            get { return _recordUnranked; }
            set { SetProperty(ref _recordUnranked, value); }
        }

        private string _adjustedElo = String.Empty;
        public string AdjustedElo
        {
            get { return _adjustedElo; }
            set { SetProperty(ref _adjustedElo, value); }
        }


        private string _adjustedMatchupRating = String.Empty;
        public string AdjustedMatchupRating
        {
            get { return _adjustedMatchupRating; }
            set { SetProperty(ref _adjustedMatchupRating, value); }
        }

        public List<WeightedMatchResult> MatchResults = new();

        public WeightedCharacterMetadata()
        {
        }

        public void AddResult(string myElo, string opponentElo, string result, string patch)
        {
            try
            {
                int myEloInt = Convert.ToInt32(myElo);

                if (opponentElo.Equals("U"))
                {
                    MatchResults.Add(new WeightedMatchResult(myEloInt, MatchProximity.Unranked, result, patch));
                }

                int opponentEloInt = Convert.ToInt32(opponentElo);
                int delta = Math.Abs(myEloInt - opponentEloInt);

                if (delta <= 50)
                {
                    MatchResults.Add(new WeightedMatchResult(delta, myEloInt, opponentEloInt, MatchProximity.Close, result, patch));
                }
                else if (delta <= 100)
                {
                    MatchResults.Add(new WeightedMatchResult(delta, myEloInt, opponentEloInt, MatchProximity.Medium, result, patch));
                }
                else
                {
                    MatchResults.Add(new WeightedMatchResult(delta, myEloInt, opponentEloInt, MatchProximity.Far, result, patch));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to parse a match: My Elo: {myElo}, Opponentn Elo: {opponentElo}");
            }
        }

        public void DoTheMath()
        {
            float totalWinWeight = 0f;
            float totalLoseWeight = 0f;

            foreach (WeightedMatchResult match in MatchResults)
            {
                float weight;
                int matchDelta = match.EloDelta;

                matchDelta = Math.Clamp(0, matchDelta - 25, 500);

                if (matchDelta <= 80)
                {
                    weight = Math.Abs((((float)matchDelta / 80f) - 1f));
                    weight = Math.Clamp(0.20f, weight, 1);
                }
                else
                {
                    weight = Math.Abs(((((float)matchDelta / 200f) * 0.25f) - 0.25f));
                    weight = Math.Clamp(0.05f, weight, 0.15f);
                }

                if (match.Result == "Win")
                {
                    totalWinWeight += weight;
                }
                else if (match.Result == "Lose")
                {
                    totalLoseWeight += weight;
                }
            }

            AdjustedMatchupRating = ((totalWinWeight / (totalLoseWeight + totalWinWeight)) * 100).ToString("n2") + "%";
        }
    }

    public enum MatchProximity
    {
        Unranked,
        Close,
        Medium,
        Far
    }
}
