using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Kernel.BFHAdmin.Client.BFHRconProtocol.Models;

namespace Kernel.BFHAdmin.Client.GUI.Converters
{
    public abstract class BaseConverter : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    [ValueConversion(typeof(Team), typeof(Brush))]
    public class TeamToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
       object parameter, CultureInfo culture)
        {
            var v = (Team) value;
            if (v.TeamId == 1)
                return Brushes.Red.ToString();
            if (v.TeamId == 2)
                return Brushes.Blue.ToString();
            return Brushes.Black.ToString();

        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
