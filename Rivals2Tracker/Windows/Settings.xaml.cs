using kWindows.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rivals2Tracker
{
    public partial class Settings : kWindow
    {
        public Settings()
        {
            InitializeComponent();
            if (DataContext is Settings_VM vm)
            {
                vm.Close = () => this.Close();
            }
        }
    }
}
