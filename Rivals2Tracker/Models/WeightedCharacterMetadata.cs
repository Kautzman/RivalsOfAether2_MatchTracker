using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Rivals2Tracker.Models
{
    class WeightedCharacterMetadata : BindableBase
    {
        private string _character = String.Empty;
        public string Character
        {
            get { return _character; }
            set { SetProperty(ref _character, value); }
        }

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

        private string _adjustedEloSeasonal = String.Empty;
        public string AdjustedEloSeasonal
        {
            get { return _adjustedEloSeasonal; }
            set { SetProperty(ref _adjustedEloSeasonal, value); }
        }

        private string _adjustedEloTotal = String.Empty;
        public string AdjustedEloTotal
        {
            get { return _adjustedEloTotal; }
            set { SetProperty(ref _adjustedEloTotal, value); }
        }

        private string _adjustedMUSeasonal = String.Empty;
        public string AdjustedMUSeasonal
        {
            get { return _adjustedMUSeasonal; }
            set { SetProperty(ref _adjustedMUSeasonal, value); }
        }

        private string _adjustedMUTotal = String.Empty;
        public string AdjustedMUTotal
        {
            get { return _adjustedMUTotal; }
            set { SetProperty(ref _adjustedMUTotal, value); }
        }

        public List<WeightedMatchResult> MatchResults = new();

        public WeightedCharacterMetadata(string character)
        {
            Character = character;
        }

        public void AddResult(string myElo, string opponentElo, string result)
        {
            try
            {
                int myEloInt = Convert.ToInt32(myElo);

                if (opponentElo.Equals("U"))
                {
                    MatchResults.Add(new WeightedMatchResult(myEloInt, MatchProximity.Unranked, result));
                }

                int opponentEloInt = Convert.ToInt32(opponentElo);
                int delta = Math.Abs(myEloInt - opponentEloInt);

                if (delta <= 50)
                {
                    MatchResults.Add(new WeightedMatchResult(delta, myEloInt, opponentEloInt, MatchProximity.Close, result));
                }
                else if (delta <= 100)
                {
                    MatchResults.Add(new WeightedMatchResult(delta, myEloInt, opponentEloInt, MatchProximity.Medium, result));
                }
                else
                {
                    MatchResults.Add(new WeightedMatchResult(delta, myEloInt, opponentEloInt, MatchProximity.Far, result));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to parse a match: My Elo: {myElo}, Opponentn Elo: {opponentElo}");
            }
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
