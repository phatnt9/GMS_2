using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GateAccessControl.ViewModels
{
    internal class DeviceProfilesSelectedItemsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //Console.WriteLine(values[0].ToString() + "-" + values[1].ToString());
                IList IlistCardTypes = (IList)values[1];
                var collection = IlistCardTypes.Cast<DeviceProfile>();
                List<DeviceProfile> listReturn = collection.ToList();
                return listReturn;
            }
            catch
            {
                return new List<Profile>();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}