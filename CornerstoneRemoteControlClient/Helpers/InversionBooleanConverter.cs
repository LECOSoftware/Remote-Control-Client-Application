// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace CornerstoneRemoteControlClient.Helpers
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : MarkupExtension, IValueConverter
    {
        private static InverseBooleanConverter _converter;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new InverseBooleanConverter();
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
