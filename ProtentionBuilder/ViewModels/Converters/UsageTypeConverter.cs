using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Urbbox.AutoCAD.ProtentionBuilder.Building.Variations;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels.Converters
{
    [ValueConversion(typeof(UsageType), typeof(string))]
    public sealed class UsageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((UsageType) value).ToNameString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string) value).ToUsageType();
        }
    }
}
