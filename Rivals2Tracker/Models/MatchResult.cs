using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rivals2Tracker.Models
{
    class MatchResult : BindableBase
    {
        public long ID { get; set; }
        public string Date { get; set; }
        public string Opponent { get; set; }
        public string OpponentElo { get; set; }
        public string MyElo { get; set; }
        public string OppChar1 { get; set; }

        private string _oppChar2 = String.Empty;
        public string OppChar2
        {
            get
            {
                if (_oppChar2 is null)
                {
                    return String.Empty;
                }

                return _oppChar2;
            }
            set { SetProperty(ref _oppChar2, value); }
        }

        private string _oppChar3 = String.Empty;
        public string OppChar3
        {
            get
            {
                if (_oppChar3 is null)
                {
                    return String.Empty;
                }

                return _oppChar3;
            }
            set { SetProperty(ref _oppChar3, value); }
        }
        public string Result { get; set; }
        public string Patch { get; set; }
        public string Notes { get; set; }
    }
}
