using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Interfaces;

namespace Kernel.BFHAdmin.Client.BFHRconProtocol.Models
{
    public class PlayerPosition : NotifyPropertyBase, ITypeCloneable<PlayerPosition>, IEqualityComparer<PlayerPosition>
    {
        private char[] coma = ",".ToCharArray();
        private string _text = "";
        private decimal _x;
        private decimal _y;
        private decimal _z;
        [Key]
        public string Text
        {
            get { return _text; }
            set { ParseFrom(value); }
        }
        public decimal X
        {
            get { return _x; }
            set
            {
                if (value == _x) return;
                _x = value;
                UpdateTextString();
                OnPropertyChanged();
                OnPropertyChanged("Text");
            }
        }

        public decimal Y
        {
            get { return _y; }
            set
            {
                if (value == _y) return;
                _y = value;
                UpdateTextString();
                OnPropertyChanged();
                OnPropertyChanged("Text");
            }
        }

        public decimal Z
        {
            get { return _z; }
            set
            {
                if (value == _z) return;
                _z = value;
                UpdateTextString();
                OnPropertyChanged();
                OnPropertyChanged("Text");
            }
        }

        public PlayerPosition(string position)
            : this()
        {
            ParseFrom(position);
        }
        public PlayerPosition(decimal x, decimal y, decimal z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }
        public PlayerPosition() { }

        public void ParseFrom(string text)
        {
            var s = text.Split(coma);
            X = decimal.Parse(s[0], NumberStyles.Float, CultureInfo.InvariantCulture);
            Y = decimal.Parse(s[1], NumberStyles.Float, CultureInfo.InvariantCulture);
            Z = decimal.Parse(s[2], NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private void UpdateTextString()
        {
            _text = X + "," + Y + "," + Z;
        }

        public PlayerPosition Clone()
        {
            return (PlayerPosition)this.MemberwiseClone();
        }

        public static PlayerPosition operator -(PlayerPosition a, PlayerPosition b)
        {
            // Produce a delta object containing differences
            var ret = new PlayerPosition();
            ret.X = a.X - b.X;
            ret.X = a.Y - b.Y;
            ret.X = a.Z - b.Z;
            return ret;
        }

        public override string ToString()
        {
            return _text;
        }

        public bool Equals(PlayerPosition firstObj, PlayerPosition secondObj)
        {
            var diff = Math.Abs(firstObj.X - secondObj.X)
               + Math.Abs(firstObj.Y - secondObj.Y)
               + Math.Abs(firstObj.Z - secondObj.Z);
            return diff == 0;
        }

        public int GetHashCode(PlayerPosition obj)
        {
            return _text.GetHashCode();
        }
    }
}
