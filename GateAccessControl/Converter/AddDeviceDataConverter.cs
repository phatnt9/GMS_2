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
                returnDevice.DEVICE_NAME = values[0].ToString();
                returnDevice.DEVICE_IP = values[1].ToString();
                returnDevice.DEVICE_NOTE = values[2].ToString();
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