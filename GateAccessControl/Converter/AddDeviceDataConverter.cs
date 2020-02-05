using System;
using System.Globalization;
using System.Windows.Data;

namespace GateAccessControl.ViewModels
{
    internal class AddDeviceDataConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Device returnDevice = new Device();
                returnDevice.deviceName = values[0].ToString();
                returnDevice.deviceIp = values[1].ToString();
                returnDevice.deviceNote = values[2].ToString();
                return returnDevice;
            }
            catch
            {
                return new Device();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}