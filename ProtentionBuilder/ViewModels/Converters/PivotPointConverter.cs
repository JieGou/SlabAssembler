using System;
using System.Globalization;
using System.Windows.Data;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Converters
{
    [ValueConversion(typeof(PivotPoint), typeof(string))]
    class PivotPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((PivotPoint) value).ToNameString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string) value).ToPivotType();
        }
    }
}
