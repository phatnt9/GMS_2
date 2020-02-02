using System;
using System.Globalization;
using System.Windows.Data;

namespace GateAccessControl.ViewModels
{
    internal class DeviceProfilesActiveTimeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string time1_from = (string)values[0];
            string time1_to = (string)values[1];
            string time2_from = (string)values[2];
            string time2_to = (string)values[3];
            foreach (var item in values)
            {
                if (!ValidateTime(item.ToString()))
                {
                    return null;
                }
            }
            return time1_from + "," + time1_to + "," + time2_from + "," + time2_to;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public bool ValidateTime(string time)
        {
            DateTime ignored;
            return DateTime.TryParseExact(time, "HH:mm",
                                          CultureInfo.InvariantCulture,
                                          DateTimeStyles.None,
                                          out ignored);
        }
    }
}