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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kernel.BFHAdmin.Client.GUI.Views
{
    /// <summary>
    /// Interaction logic for PlayerList.xaml
    /// </summary>
    public partial class PlayerList : UserControl
    {
        private RconModel _rconModel;
        public PlayerList()
        {
            InitializeComponent();
        }

        public void Register(RconModel rconModel)
        {
            _rconModel = rconModel;
        }
    }
}
