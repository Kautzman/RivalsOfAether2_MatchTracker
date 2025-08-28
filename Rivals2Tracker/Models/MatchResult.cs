using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rivals2Tracker.Models
{
    public class MatchResult : BindableBase
    {
        public long ID { get; set; }
        public string Date { get; set; }
        public string Opponent { get; set; }
        public string OpponentElo { get; set; }
        public string MyChar { get; set; }
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

        public DelegateCommand ShowMatchDetailsCommand { get; set; }

        public MatchResult()
        {
            ShowMatchDetailsCommand = new DelegateCommand(ShowMatchDetails);
        }

        private void ShowMatchDetails()
        {
            MatchDetails matchdDetails = new MatchDetails(this);

            matchdDetails.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            matchdDetails.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            matchdDetails.ShowDialog();
        }
    }
}
