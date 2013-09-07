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
    public class ChatHistoryItem : NotifyPropertyBase, ITypeCloneable<ChatHistoryItem>
    {
        private string _message;
        private string _type;
        private string _what;
        private string _from;
        private int _number;
        private string _timeStamp;

        public int Number
        {
            get { return _number; }
            set
            {
                if (value == _number) return;
                _number = value;
                OnPropertyChanged();
            }
        }

        public string From
        {
            get { return _from; }
            set
            {
                if (value == _from) return;
                _from = value;
                OnPropertyChanged();
            }
        }

        public string What
        {
            get { return _what; }
            set
            {
                if (value == _what) return;
                _what = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                if (value == _type) return;
                _type = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public string TimeStamp
        {
            get { return _timeStamp; }
            set
            {
                if (value == _timeStamp) return;
                _timeStamp = value;
                OnPropertyChanged();
            }
        }


        public ChatHistoryItem Clone()
        {
            return (ChatHistoryItem)this.MemberwiseClone();
        }
    }
}
