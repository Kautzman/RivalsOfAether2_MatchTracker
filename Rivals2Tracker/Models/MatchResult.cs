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
        public long OpponentElo { get; set; }
        public long MyElo { get; set; }

        private string _opponent2 = String.Empty;
        public string Opponent2
        {
            get
            {
                if (_opponent2 is null)
                {
                    return String.Empty;
                }

                return _opponent2;
            }
            set { SetProperty(ref _opponent2, value); }
        }

        private string _opponent3 = String.Empty;
        public string Opponent3
        {
            get
            {
                if (_opponent3 is null)
                {
                    return String.Empty;
                }

                return _opponent3;
            }
            set { SetProperty(ref _opponent3, value); }
        }
        public string Result { get; set; }
        public string Patch { get; set; }
        public string Notes { get; set; }
    }
}
