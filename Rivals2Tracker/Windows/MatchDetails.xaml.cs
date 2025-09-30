using kWindows.Core;
using Prism.Events;
using Prism.Ioc;
using Slipstream.Models;
using System.ComponentModel;

namespace Slipstream
{
    public partial class MatchDetails : kWindow
    {
        public MatchDetails(MatchResult matchResult)
        {
            InitializeComponent();

            DataContext = new MatchDetails_VM(matchResult);

            if (DataContext is MatchDetails_VM vm)
            {
                vm.Close = () => this.Close();
            }
        }
    }
}
