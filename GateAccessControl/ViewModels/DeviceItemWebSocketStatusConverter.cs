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
                //DeviceItem di = value as DeviceItem;
                WebSocket vl = value as WebSocket;
                if (vl != null)
                {
                    Console.WriteLine(vl.IsAlive);
                    if (vl.IsAlive)
                    {
                        //di.WebSocketStatus = "Connected";
                        return "Connected";
                    }
                    else
                    {
                        //di.WebSocketStatus = "Connecting";
                        return "Disconnected";
                    }
                }
                else
                {
                    //di.WebSocketStatus = "Disconnected";
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
