using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace GateAccessControl.ViewModels
{
    internal class LoadTimeCheckParametersConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Profile p = values[0] as Profile;
            DateTime? d = values[1] as DateTime?;

            if (p != null && d != null)
            {
                return SqliteDataAccess.LoadTimeChecks(p.pinno, (DateTime)d);
            }
            else
            {
                return new List<TimeRecord>();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}