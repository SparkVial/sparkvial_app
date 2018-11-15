using System;
using System.Globalization;
using Xamarin.Forms;

namespace sparkvial_app {
    public class IfTypeToIcon : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((string)value == "Serial") {
                return "\uf1e6";
            }
            if ((string)value == "Mock") {
                return "\uf140";
            }
            return "\uf059";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
