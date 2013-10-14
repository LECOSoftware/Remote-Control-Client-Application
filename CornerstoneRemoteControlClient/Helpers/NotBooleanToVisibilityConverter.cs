// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace CornerstoneRemoteControlClient.Helpers
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class NotBooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static NotBooleanToVisibilityConverter _converter;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new NotBooleanToVisibilityConverter();
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Converter.Convert(!(bool)value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValue = Converter.ConvertBack(value, targetType, parameter, culture);

            return (convertedValue is bool) ? !((bool)convertedValue) : convertedValue;
        }
        private static readonly BooleanToVisibilityConverter Converter = new BooleanToVisibilityConverter();
    }
}