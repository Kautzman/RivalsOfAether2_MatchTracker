using kWindows.Core;

namespace Slipstream.Windows
{
    public partial class FirstStart : kWindow
    {
        public FirstStart()
        {
            InitializeComponent();

            if (DataContext is FirstStart_VM vm)
            {
                vm.Close = () => this.Close();
            }
        }
    }
}
