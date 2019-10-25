using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GateAccessControl.ViewModels
{
    class DgvSelectedItemsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //Console.WriteLine(values[0].ToString() + "-" + values[1].ToString());
                IList IlistCardTypes = (IList)values[1];
                var collection = IlistCardTypes.Cast<CardType>();
                List<CardType> listReturn = collection.ToList();
                return listReturn;
            }
            catch
            {
                return new List<CardType>();
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
