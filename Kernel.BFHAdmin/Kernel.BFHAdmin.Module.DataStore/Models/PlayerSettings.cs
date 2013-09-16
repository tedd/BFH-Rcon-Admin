using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;

namespace Kernel.BFHAdmin.Module.DataStore.Models
{
    public class PlayerSettings : NotifyPropertyBase
    {
        private bool _stopSpam = false;

        public bool StopSpam
        {
            get { return _stopSpam; }
            set { _stopSpam = value; }
        }
    }
}
