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

namespace Slipstream.Resources.Components
{
    public partial class RivalsDialog : kWindow
    {
        RivalsDialog_VM thisVM;

        public RivalsDialog()
        {
            InitializeComponent();

            if (DataContext is RivalsDialog_VM vm)
            {
                thisVM = vm;
                vm.Close = () => this.Close();
            }
        }
    }
}
