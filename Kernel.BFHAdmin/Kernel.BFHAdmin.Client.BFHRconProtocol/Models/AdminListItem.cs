using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Interfaces;
using Newtonsoft.Json;
using PropertyChanging;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{
    public class AdminListItem : NotifyPropertyBase, ITypeCloneable<AdminListItem>
    {
        private int _port;
        private string _address;
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                if (value == _address) return;
                _address = value;
                OnPropertyChanged();
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                if (value == _port) return;
                _port = value;
                OnPropertyChanged();
            }
        }

        public AdminListItem Clone()
        {
            return (AdminListItem)this.MemberwiseClone();
        }
    }
}
