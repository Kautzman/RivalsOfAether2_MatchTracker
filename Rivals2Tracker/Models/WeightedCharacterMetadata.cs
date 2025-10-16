using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Prism.Mvvm;
using Slipstream.Data;
using Windows.Security.Authentication.OnlineId;

namespace Slipstream.Models
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
                int myEloInt;
                int opponentEloInt;

                // Values of -1 indicate qualifying or Unranked matches, and will necessarily mean any match without a valid int value
                if (Int32.TryParse(opponentElo, out int parsedMyValue))
                    myEloInt = parsedMyValue;
                else
                    myEloInt = -1;

                if (Int32.TryParse(myElo, out int parsedOppValue))
                    opponentEloInt = parsedOppValue;
                else
                    opponentEloInt = -1;

                if (opponentEloInt.Equals("-1") && myEloInt.Equals("-1"))
                {
                    // If we are both qualifying, just drop the match
                    return;
                }

                // If the opponent is qualifying, just set their elo to mine and make the delta zero
                if (opponentEloInt.Equals("-1"))
                {
                    MatchResults.Add(new WeightedMatchResult(0, myEloInt, myEloInt, MatchProximity.Unranked, result, patch));
                    return;
                }

                // Same, but the other way around
                if (myEloInt.Equals("-1"))
                {
                    MatchResults.Add(new WeightedMatchResult(0, opponentEloInt, opponentEloInt, MatchProximity.Unranked, result, patch));
                    return;
                }

                // A negative delta number means an upset.  -50 on a win means I beat someone with 50 more Elo.  Negative 50 on a loss means I lost to someone with less Elo
                // Negative numbers are weighted higher as a result.
                int delta = result == "Win" ? myEloInt - opponentEloInt : opponentEloInt - myEloInt;

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
                Debug.WriteLine($"Ignoring Match for Elo calculations: My Elo: {myElo}, Opponent Elo: {opponentElo}");
            }
        }

        // Lambda is the strength knob for Elo adjustment here
        public void CalculateWeightedElo(double lambda = 1)
        {
            double totalScore = 0.0;
            double totalExpect = 0.0;

            foreach (WeightedMatchResult match in MatchResults)
            {
                double score = match.Result switch
                {
                    "Win" => 1.0,
                    "Lose" => 0.0,
                    _ => throw new ArgumentException($"Unknown result: {match.Result}")

                };

                int myElo = 0;
                int opponentElo = 0;

                if (match.MyElo == -1 && match.OpponentElo == -1)
                {
                    myElo = 1000;
                    opponentElo = 1000;
                }
                else if (match.MyElo == -1)
                {
                    myElo = match.OpponentElo;
                    opponentElo = match.OpponentElo;
                }
                else if (match.OpponentElo == -1)
                {
                    myElo = match.MyElo;
                    opponentElo = match.MyElo;
                }
                else
                {
                    myElo = match.MyElo;
                    opponentElo = match.OpponentElo;
                }

                double expected = 1.0 / (1.0 + Math.Pow(10.0, -(myElo - opponentElo) / 250.0));

                totalScore += score;
                totalExpect += expected;
            }

            double WR = totalScore / MatchResults.Count;
            double avgExpected = totalExpect / MatchResults.Count;

            double WWR = 0.5 + lambda * (WR - avgExpected);

            if (WWR < 0) WWR = 0;
            if (WWR > 1) WWR = 1;

            WWR = WWR * 100;
            WWR = Math.Round(WWR, 2);

            AdjustedMatchupRating = WWR.ToString() + "%";
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
