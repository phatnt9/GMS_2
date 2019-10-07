using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WebSocketSharp;

namespace GateAccessControl
{
    class DeviceItemWebSocketStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                WebSocket vl = value as WebSocket;
                if (vl != null)
                {
                    if (vl.IsAlive)
                    {
                        return "Connected";
                    }
                    else
                    {
                        return "Connecting";
                    }
                }
                else
                {
                    return "Disconnected";
                }
            }
            catch
            {
                return "Pending";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
