using System;
using System.Globalization;
using System.Windows.Data;
using Urbbox.SlabAssembler.Core.Variations;

namespace Urbbox.SlabAssembler.ViewModels.Converters
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
